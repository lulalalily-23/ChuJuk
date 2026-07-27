using UnityEngine;

public class GolemDeathHandler : MonoBehaviour
{
    private HealthManager healthManager;
    private GolemPatternManager patternManager;

    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        patternManager = GetComponent<GolemPatternManager>();
        healthManager.OnDeath += HandleGolemDeath;
    }

    void HandleGolemDeath()
    {
        patternManager.enabled = false;   // 더 이상 패턴 선택 안 함
        GetComponent<GolemController>().enabled = false;   // 페이즈 계산도 정지

        // 씬에 남아있는 소환수 전부 정리
        GolemSummon[] summons = FindObjectsOfType<GolemSummon>();
        foreach (GolemSummon summon in summons)
        {
            Destroy(summon.gameObject);
        }

        // 골렘 자신도 정리 (사망 애니메이션 등으로 업데이트 가능)
        Destroy(gameObject);
    }
}