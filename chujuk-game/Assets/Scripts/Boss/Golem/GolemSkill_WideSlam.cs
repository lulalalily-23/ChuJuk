using UnityEngine;
using System.Collections;

// 플레이어가 있는 점 기준으로 여러번 넓은 x좌표 내려찍기. 타격 지점 간 간격은 매번 좁아짐(0.7배).
// 각 타격마다 GolemSkill_Slam과 동일한 "들어올림-정지-내려찍음" 반복.

public class GolemSkill_WideSlam : MonoBehaviour
{
    [Tooltip("총 타격 횟수")]
    public int hitCount = 4;
    [Tooltip("타격 지점 간 초기 간격 (매 타격마다 0.7배씩 좁아짐)")]
    public float spacing = 2f;
    [Tooltip("각 타격 판정 반경")]
    public float slamRadius = 1.5f;
    [Tooltip("얼마나 더 들어올릴지 각도")]
    public float raiseAmount = 110f;
    [Tooltip("얼마나 내려찍을지 각도 (음수)")]
    public float slamAmount = -30f;
    [Tooltip("팔을 들어올리는 데 걸리는 시간")]
    public float raiseDuration = 0.2f;
    [Tooltip("들어올린 자세로 멈춰서 예고하는 시간")]
    public float holdDuration = 0.15f;
    [Tooltip("내려찍는 데 걸리는 시간")]
    public float slamDuration = 0.1f;
    [Tooltip("각 타격 사이의 대기 시간")]
    public float intervalBetweenHits = 0.1f;
    [Tooltip("쿨타임")]
    public float coolTime = 8f;
    [Tooltip("사거리")]
    public float maxRange = 8f;
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
        StartCoroutine(WideSlamRoutine());
    }

    IEnumerator WideSlamRoutine()
    {
        isExecuting = true;
        lastUseTime = Time.time;

        // 플레이어가 오른쪽이면 1, 왼쪽이면 -1
        int side = player.position.x >= transform.position.x ? 1 : -1;

        Quaternion leftRaisedRot = leftArmOriginalRot * Quaternion.Euler(0f, 0f, raiseAmount * side);
        Quaternion rightRaisedRot = rightArmOriginalRot * Quaternion.Euler(0f, 0f, raiseAmount * side);
        Quaternion leftSlamRot = leftArmOriginalRot * Quaternion.Euler(0f, 0f, slamAmount * side);
        Quaternion rightSlamRot = rightArmOriginalRot * Quaternion.Euler(0f, 0f, slamAmount * side);

        // 방향에 맞는 팔(어깨) 위치를 첫 타격 지점의 기준으로 사용
        Transform startArm = side >= 0 ? rightArmPivot : leftArmPivot;
        float currentX = startArm.position.x;
        float currentSpacing = spacing;

        yield return RotateArms(leftArmOriginalRot, leftRaisedRot, rightArmOriginalRot, rightRaisedRot, raiseDuration);

        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            Vector2 hitPos = new Vector2(currentX, groundReference.position.y);

            TelegraphIndicator.Instance.ShowCircle(hitPos, slamRadius, holdDuration);
            yield return new WaitForSeconds(holdDuration);

            yield return RotateArms(leftRaisedRot, leftSlamRot, rightRaisedRot, rightSlamRot, slamDuration);

            Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, slamRadius, targetLayer);
            if (hits.Length > 0)
            {
                ApplyDamage(hits);
            }

            // 다음 타격 지점 계산 (간격은 매번 70%로 좁아짐)
            currentX += side * currentSpacing;
            currentSpacing *= 0.7f;

            if (hitIndex < hitCount - 1)
            {
                yield return RotateArms(leftSlamRot, leftRaisedRot, rightSlamRot, rightRaisedRot, raiseDuration);
            }

            yield return new WaitForSeconds(intervalBetweenHits);
        }

        // 전체 타격이 끝나면 원래 자세로 복귀
        yield return RotateArms(leftSlamRot, leftArmOriginalRot, rightSlamRot, rightArmOriginalRot, raiseDuration);

        isExecuting = false;
        golemController.OnPatternExecuted();
    }

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