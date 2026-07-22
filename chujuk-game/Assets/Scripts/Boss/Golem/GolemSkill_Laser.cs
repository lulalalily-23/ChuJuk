using UnityEngine;
using System.Collections;

//눈에서 레이저를 발사하는 공격.
//양 눈을 기준으로 아래에서 위로 레이저를 긁어올리도록 설계
public class GolemSkill_Laser : MonoBehaviour
{
    [Tooltip("레이저 길이")]
    public float laserLength = 15f;
    [Tooltip("레이저 텔레그래프/판정 두께")]
    public float laserThickness = 0.6f;
    [Tooltip("레이저 시작 각도")]
    public float startAngle = -50f;
    [Tooltip("레이저 끝 각도")]
    public float endAngle = 0f;
    [Tooltip("시작에서 끝까지 레이저로 휩쓰는 시간")]
    public float sweepDuration = 1.2f;
    [Tooltip("회전을 몇단계로 나눌지 (많을수록 부드러움)")]
    public int sweepSteps = 10;
    [Tooltip("쿨타임")]
    public float coolTime = 10f;
    [Tooltip("공격 전 표시되는 경고 지속시간")]
    public float warningDuration = 0.5f;
    [Tooltip("판정 대상 레이어 (Player)")]
    public LayerMask targetLayer;
    [Tooltip("눈 위치 기준점 (GolemHead에 연결)")]
    public Transform eyePosition;
    [Tooltip("두 레이저 시작점 사이의 거리")]
    public float eyeSpacing = 0.5f;

    private GolemController golemController;
    private ICombatStats myStats;
    private float lastUseTime = -999f;
    private bool isExecuting;

    void Start()
    {
        golemController = GetComponent<GolemController>();
        myStats = GetComponent<ICombatStats>();
    }

    public bool CanUse()
    {
        if (isExecuting) return false;
        if (Time.time - lastUseTime < coolTime) return false;
        return true;
    }

    public void Execute()
    {
        if (isExecuting) return;
        StartCoroutine(LaserRoutine());
    }

    IEnumerator LaserRoutine()
    {
        isExecuting = true;
        lastUseTime = Time.time;

        // 눈 위치에서 좌우로 eyeSpacing만큼 떨어진 두 시작점 계산
        Vector2 leftPivot = (Vector2)eyePosition.position + Vector2.left * (eyeSpacing / 2f);
        Vector2 rightPivot = (Vector2)eyePosition.position + Vector2.right * (eyeSpacing / 2f);

        // 예비 경고: 본 공격 전에 짧게 표시
        TelegraphIndicator.Instance.ShowCircle(eyePosition.position, 0.5f, warningDuration);
        yield return new WaitForSeconds(warningDuration);

        float stepDuration = sweepDuration / sweepSteps;
        bool alreadyHitLeft = false;
        bool alreadyHitRight = false;

        for (int i = 1; i <= sweepSteps; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, (float)i / sweepSteps);
            float angleRad = angle * Mathf.Deg2Rad;

            // 오른쪽 레이저
            Vector2 rightDir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector2 rightCenter = rightPivot + rightDir * (laserLength / 2f);
            TelegraphIndicator.Instance.ShowSquare(rightCenter, new Vector2(laserLength, laserThickness), stepDuration * 1.5f, angle);

            if (!alreadyHitRight)
            {
                Collider2D[] rightHits = Physics2D.OverlapBoxAll(rightCenter, new Vector2(laserLength, laserThickness), angle, targetLayer);
                if (rightHits.Length > 0)
                {
                    ApplyDamage(rightHits);
                    alreadyHitRight = true; // 회전 중 중복 판정 방지
                }
            }

            // 왼쪽 레이저
            Vector2 leftDir = new Vector2(-Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector2 leftCenter = leftPivot + leftDir * (laserLength / 2f);
            TelegraphIndicator.Instance.ShowSquare(leftCenter, new Vector2(laserLength, laserThickness), stepDuration * 1.5f, 180f - angle);

            if (!alreadyHitLeft)
            {
                Collider2D[] leftHits = Physics2D.OverlapBoxAll(leftCenter, new Vector2(laserLength, laserThickness), 180f - angle, targetLayer);
                if (leftHits.Length > 0)
                {
                    ApplyDamage(leftHits);
                    alreadyHitLeft = true; // 회전 중 중복 판정 방지
                }
            }

            yield return new WaitForSeconds(stepDuration);
        }

        isExecuting = false;
        golemController.OnPatternExecuted();
    }

    void ApplyDamage(Collider2D[] hits)
    {
        if (hits.Length == 0) return;
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