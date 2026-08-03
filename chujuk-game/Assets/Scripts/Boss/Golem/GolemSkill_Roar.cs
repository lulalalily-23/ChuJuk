using UnityEngine;
using System.Collections;

// 머리를 중심으로 원형 음파가 점점 퍼져나가는 공격
public class GolemSkill_Roar : MonoBehaviour
{
   [Tooltip("최종 판정 반경")]
    public float roarRadius = 5f;
    [Tooltip("음파가 최종 크기까지 퍼지는 데 걸리는 시간")]
    public float telegraphDuration = 1f;
    [Tooltip("공격 전 표시되는 경고 지속시간")]
    public float warningDuration = 0.5f;
    [Tooltip("쿨타임")]
    public float coolTime = 12f;
    [Tooltip("판정 대상 레이어 (Player)")]
    public LayerMask targetLayer;
    [Tooltip("음파의 중심이 되는 머리 위치 (GolemHead 연결)")]
    public Transform headPosition;

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
        StartCoroutine(RoarRoutine());
        golemController.PlayAttack();
    }

    IEnumerator RoarRoutine()
    {
        isExecuting = true;
        lastUseTime = Time.time;

        Vector2 roarCenter = headPosition.position;

        // 예비 경고
        TelegraphIndicator.Instance.ShowCircle(headPosition.position, 0.5f, warningDuration);
        yield return new WaitForSeconds(warningDuration);
        
        // 음파가 퍼지는 연출: 여러 단계로 나눠서 점점 커지는 원을 이어서 표시
        int waveSteps = 8;
        float stepDuration = telegraphDuration / waveSteps;

        for (int i = 1; i <= waveSteps; i++)
        {
            float currentRadius = roarRadius * i / waveSteps;
            TelegraphIndicator.Instance.ShowCircle(roarCenter, currentRadius, stepDuration * 1.5f);
            yield return new WaitForSeconds(stepDuration);
        }

        // 예고가 끝난 뒤 최종 크기로 한 번만 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(roarCenter, roarRadius, targetLayer);
        if (hits.Length > 0)
        {
            Collider2D hit = hits[0];
            ICombatStats[] statsArray = hit.GetComponentsInParent<ICombatStats>();
            ICombatStats targetStats = statsArray.Length > 0 ? statsArray[0] : null;

            if (targetStats != null)
            {
                HealthManager targetHealth = hit.GetComponentInParent<HealthManager>();
                if (targetHealth != null)
                {
                    int damage = DamageCalculator.CalculateDamage(myStats, targetStats);
                    targetHealth.TakeDamage(damage);
                }
            }
        }

        isExecuting = false;
        golemController.OnPatternExecuted();
    }
}