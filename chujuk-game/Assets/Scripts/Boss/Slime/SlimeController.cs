using UnityEngine;
using System.Collections;

// 슬라임 보스의 페이즈 판정 + 분열 처리를 담당.
// 분열 전에는 HP 비율로 Phase1/Phase2를 판정하다가 50% 밑으로 내려가는 순간 분열을 실행.
public class SlimeController : MonoBehaviour, IPatternUser
{
    public enum BossPhase { Phase1, Phase2, Phase3 }

    [Tooltip("분열 시 생성할 자식 슬라임 프리팹")]
    public GameObject splitChildPrefab;
    public float splitOffsetX = 1.5f;

    [Tooltip("자신이 자식 슬라임인지 여부")]
    public bool isSplitChild = false;

    [Tooltip("분열로 함께 태어난 형제 슬라임 참조")]
    public SlimeController siblingChild;

    [Tooltip("낙하공격 중인지 여부")]
    public bool isPerformingFallAttack = false;

    [Tooltip("분열 시 갈라져 나오는 데 걸리는 시간")]
    public float splitMoveOutDuration = 0.3f;

    [Header("처치 실패 시 회복 타이머")]
    [Tooltip("이 시간 안에 소환수 죽이지 않으면 체력 일부 회복")]
    public float enrageHealInterval = 10f;
    [Range(0f, 1f)]
    public float enrageHealPercent = 0.1f;

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

        // 분열된 슬라임들이 서로를 형제로 등록
        leftChild.siblingChild = rightChild;
        rightChild.siblingChild = leftChild;

        Destroy(gameObject);
    }

    private SlimeController SpawnChild(Vector3 targetPosition)
    {
        GameObject child = Instantiate(splitChildPrefab, transform.position, Quaternion.identity);

        SlimeController childController = child.GetComponent<SlimeController>();
        childController.isSplitChild = true;
        childController.player = player;

        SlimeSkill_FallAttack fallAttack = child.GetComponent<SlimeSkill_FallAttack>();
        if (fallAttack != null)
            fallAttack.player = player;

        childController.StartCoroutine(childController.MoveOutRoutine(targetPosition));

        return childController;
    }

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