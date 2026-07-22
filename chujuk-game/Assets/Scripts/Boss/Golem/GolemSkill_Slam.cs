using UnityEngine;
using System.Collections;

// 플레이어 위치를 노려 양팔을 들었다가 내려찍는 단일 타격.
public class GolemSkill_Slam : MonoBehaviour
{
    [Tooltip("내려찍기 판정 반경")]
    public float slamRadius = 2f;
    [Tooltip("얼마나 더 들어올릴지 각도")]
    public float raiseAmount = 110f;
    [Tooltip("얼마나 내려찍을지 각도 (음수)")]
    public float slamAmount = -30f;
    [Tooltip("팔을 들어올리는 데 걸리는 시간")]
    public float raiseDuration = 0.3f;
    [Tooltip("들어올린 자세로 멈춰서 예고하는 시간")]
    public float holdDuration = 0.5f;
    [Tooltip("내려찍는 데 걸리는 시간")]
    public float slamDuration = 0.15f;
    [Tooltip("원래 자세로 복귀하는 데 걸리는 시간")]
    public float returnDuration = 0.3f;
    [Tooltip("쿨타임")]
    public float coolTime = 4f;
    [Tooltip("사거리")]
    public float maxRange = 4f;
    [Tooltip("판정 대상 레이어 (Player)")]
    public LayerMask targetLayer;
    public Transform player;
    [Tooltip("왼팔 회전축 (어깨 위치)")]
    public Transform leftArmPivot;
    [Tooltip("오른팔 회전축 (어깨 위치)")]
    public Transform rightArmPivot;
    public Transform groundReference;

    private GolemController golemController;
    private ICombatStats myStats;
    private float lastUseTime = -999f;
    private bool isExecuting;
    private Quaternion leftArmOriginalRot;
    private Quaternion rightArmOriginalRot;

    void Start()
    {
        golemController = GetComponent<GolemController>();
        myStats = GetComponent<ICombatStats>();
        leftArmOriginalRot = leftArmPivot.localRotation;
        rightArmOriginalRot = rightArmPivot.localRotation;
    }

    public bool CanUse()
    {
        if (isExecuting) return false;
        if (Time.time - lastUseTime < coolTime) return false;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > maxRange) return false;
        return true;
    }

    public void Execute()
    {
        if (isExecuting) return;
        StartCoroutine(SlamRoutine());
    }

    IEnumerator SlamRoutine()
    {
        isExecuting = true;
        lastUseTime = Time.time;

        // 플레이어가 오른쪽이면 1, 왼쪽이면 -1. 이 방향으로 양팔이 함께 기울어짐
        int side = player.position.x >= transform.position.x ? 1 : -1;

        Quaternion leftRaisedRot = leftArmOriginalRot * Quaternion.Euler(0f, 0f, raiseAmount * side);
        Quaternion rightRaisedRot = rightArmOriginalRot * Quaternion.Euler(0f, 0f, raiseAmount * side);
        Quaternion leftSlamRot = leftArmOriginalRot * Quaternion.Euler(0f, 0f, slamAmount * side);
        Quaternion rightSlamRot = rightArmOriginalRot * Quaternion.Euler(0f, 0f, slamAmount * side);

        Vector2 slamPos = new Vector2(player.position.x, groundReference.position.y);

        // 1단계: 팔을 들어올림
        yield return RotateArms(leftArmOriginalRot, leftRaisedRot, rightArmOriginalRot, rightRaisedRot, raiseDuration);

        // 2단계: 정지 (예고)
        TelegraphIndicator.Instance.ShowCircle(slamPos, slamRadius, holdDuration);
        yield return new WaitForSeconds(holdDuration);

        // 3단계: 내려찍기
        yield return RotateArms(leftRaisedRot, leftSlamRot, rightRaisedRot, rightSlamRot, slamDuration);

        // 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(slamPos, slamRadius, targetLayer);
        if (hits.Length > 0)
        {
            ApplyDamage(hits);
        }

        // 4단계: 원위치 복귀
        yield return RotateArms(leftSlamRot, leftArmOriginalRot, rightSlamRot, rightArmOriginalRot, returnDuration);

        isExecuting = false;
        golemController.OnPatternExecuted();
    }

    // 왼팔/오른팔을 각자 독립적인 시작-종료 회전값으로 자연스럽게 이동시킴
    IEnumerator RotateArms(Quaternion leftFrom, Quaternion leftTo, Quaternion rightFrom, Quaternion rightTo, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            leftArmPivot.localRotation = Quaternion.Lerp(leftFrom, leftTo, t);
            rightArmPivot.localRotation = Quaternion.Lerp(rightFrom, rightTo, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        leftArmPivot.localRotation = leftTo;
        rightArmPivot.localRotation = rightTo;
    }

    void ApplyDamage(Collider2D[] hits)
    {
        Collider2D hit = hits[0];
        ICombatStats[] statsArray = hit.GetComponentsInParent<ICombatStats>();
        ICombatStats targetStats = statsArray.Length > 0 ? statsArray[0] : null;
        if (targetStats == null) return;
        HealthManager targetHealth = hit.GetComponentInParent<HealthManager>();
        if (targetHealth == null) return;
        int damage = DamageCalculator.CalculateDamage(myStats, targetStats);
        targetHealth.TakeDamage(damage);
    }
}