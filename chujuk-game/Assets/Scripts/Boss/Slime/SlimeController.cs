using UnityEngine;
using System.Collections;

// 슬라임 보스의 페이즈 판정 + 분열 처리를 담당.
// 분열 전(원본)에는 HP 비율로 Phase1/Phase2를 판정하다가 50% 밑으로 내려가는 순간 분열을 실행.
// 분열로 태어난 자식 슬라임(isSplitChild = true)은 항상 Phase3(낙하 패턴) 고정.
public class SlimeController : MonoBehaviour, IPatternUser
{
    public enum BossPhase { Phase1, Phase2, Phase3 }

    [Header("분열 설정")]
    [Tooltip("분열 시 생성할 '자식 슬라임' 프리팹 - 원본보다 작고 약하게 별도 디자인")]
    public GameObject splitChildPrefab;
    public float splitOffsetX = 1.5f;

    [Tooltip("이 인스턴스가 분열로 태어난 자식인지 여부. 자식 프리팹에서는 true로 설정")]
    public bool isSplitChild = false;

    [Tooltip("분열로 함께 태어난 형제 슬라임 참조 (자식일 때만 사용). 형제가 죽으면 자동으로 null이 됨")]
    public SlimeController siblingChild;

    [Tooltip("지금 낙하공격을 수행 중인지 여부 - 형제가 이걸 보고 자기 차례를 양보함")]
    public bool isPerformingFallAttack = false;

    [Header("분열 연출")]
    [Tooltip("분열 시 부모 위치에서 목표 위치까지 갈라져 나오는 데 걸리는 시간")]
    public float splitMoveOutDuration = 0.3f;

    [Header("자식 전용 - 처치 실패 시 회복 타이머")]
    [Tooltip("이 시간 안에 죽지 않으면 체력 일부 회복")]
    public float enrageHealInterval = 10f;
    [Range(0f, 1f)]
    public float enrageHealPercent = 0.1f;

    [Header("페이즈 기준")]
    [Tooltip("Phase2 전환 기준 체력 비율")]
    public float phase2Threshold = 0.8f;
    [Tooltip("분열 기준 체력 비율")]
    public float splitThreshold = 0.5f;

    private HealthManager healthManager;
    private Animator animator;
    private SpriteRenderer sr;
    private BossPhase currentPhase = BossPhase.Phase1;
    private bool hasSplit = false;

    public Transform player;
    public SlimePatternManager patternManager;

    public HealthManager MainHealth => healthManager;
    public bool IsPhase2 => currentPhase == BossPhase.Phase2;
    public bool IsPhase3 => currentPhase == BossPhase.Phase3;

    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        animator = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (isSplitChild)
        {
            currentPhase = BossPhase.Phase3;
            StartCoroutine(EnrageHealRoutine());
        }
    }

    void Update()
    {
        if (healthManager == null) return;

        if (!isSplitChild && !hasSplit)
        {
            float healthPercent = (float)healthManager.currentHealth / healthManager.data.maxHealth;

            if (healthPercent <= splitThreshold)
            {
                hasSplit = true;
                StartCoroutine(SplitRoutine());
                return;
            }

            currentPhase = healthPercent <= phase2Threshold ? BossPhase.Phase2 : BossPhase.Phase1;
        }

        FaceTarget();
    }

    void FaceTarget()
    {
        if (player == null || sr == null) return;
        sr.flipX = player.position.x >= transform.position.x;
    }

    // 스킬 하나 끝날 때마다 각 슬라임 스킬 컴포넌트에서 호출
    public void OnPatternExecuted()
    {
        patternManager.NotifyPatternFinished();
    }

    private IEnumerator SplitRoutine()
    {
        PlaySplit();
        yield return new WaitForSeconds(0.4f); // 분열 연출 대기 - 실제 애니메이션 길이에 맞게 조정 필요

        Vector3 leftPos = transform.position + Vector3.left * splitOffsetX;
        Vector3 rightPos = transform.position + Vector3.right * splitOffsetX;

        SlimeController leftChild = SpawnChild(leftPos);
        SlimeController rightChild = SpawnChild(rightPos);

        // 서로를 형제로 등록 - 한쪽이 죽으면 이 참조로 "내가 마지막인지" 판단함
        leftChild.siblingChild = rightChild;
        rightChild.siblingChild = leftChild;

        // 원본은 처치 판정(재화 지급) 없이 정리 - 진짜 처치는 자식 둘 다 죽었을 때 SlimeDeathHandler가 처리
        Destroy(gameObject);
    }

    private SlimeController SpawnChild(Vector3 targetPosition)
    {
        // 목표 위치(좌우로 벌어진 지점)가 아니라 부모 위치에서 스폰해서, 갈라져 나오는 것처럼 보이게 함
        GameObject child = Instantiate(splitChildPrefab, transform.position, Quaternion.identity);

        SlimeController childController = child.GetComponent<SlimeController>();
        childController.isSplitChild = true;
        childController.player = player;

        // SlimeSkill_FallAttack은 별도의 player 필드를 갖고 있어서 여기서 같이 연결해줘야 함.
        // 자식은 씬에 미리 배치되는 게 아니라 런타임에 스폰되기 때문에, 프리팹 인스펙터에서
        // 미리 연결해둘 수가 없음 - 반드시 코드로 넘겨줘야 함.
        SlimeSkill_FallAttack fallAttack = child.GetComponent<SlimeSkill_FallAttack>();
        if (fallAttack != null)
            fallAttack.player = player;

        childController.StartCoroutine(childController.MoveOutRoutine(targetPosition));

        return childController;
    }

    // 부모 위치에서 목표 위치(좌/우로 벌어진 자리)까지 짧게 이동하는 연출.
    // X축만 직접 옮기고 Y축은 건드리지 않음 - Rigidbody2D가 Dynamic이라 중력이 알아서
    // 바닥까지 떨어뜨려주기 때문에, 여기서 Y까지 고정시키면 공중에 뜬 채로 분열하는 것처럼 보임.
    // public으로 열어둔 이유: SpawnChild()가 "부모"의 SplitRoutine 안에서 호출하지만,
    // 실제로 이 코루틴은 갓 태어난 "자식" 자신의 컴포넌트에서 실행되어야 하기 때문.
    public IEnumerator MoveOutRoutine(Vector3 targetPos)
    {
        float startX = transform.position.x;
        float elapsed = 0f;

        while (elapsed < splitMoveOutDuration)
        {
            float x = Mathf.Lerp(startX, targetPos.x, elapsed / splitMoveOutDuration);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(targetPos.x, transform.position.y, transform.position.z);
    }

    private IEnumerator EnrageHealRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(enrageHealInterval);

            if (healthManager == null || healthManager.currentHealth <= 0)
                yield break;

            int healAmount = Mathf.RoundToInt(healthManager.data.maxHealth * enrageHealPercent);
            healthManager.Heal(healAmount);
            Debug.Log($"[SlimeBoss] {enrageHealInterval}초 내 처치 실패 - {healAmount} 회복");
        }
    }

    public void PlayDash() => animator?.SetTrigger("Dash");
    public void PlayMissile() => animator?.SetTrigger("Missile");
    public void PlaySplit() => animator?.SetTrigger("Split");
    public void PlayFall() => animator?.SetTrigger("Fall");
    public void PlaySummon() => animator?.SetTrigger("Summon");
    public void PlayDie() => animator?.SetTrigger("Die");
}