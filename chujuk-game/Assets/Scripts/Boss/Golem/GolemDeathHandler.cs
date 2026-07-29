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
        // 골렘 처치 재화 지급 (GoldGain 보너스 반영)
        int baseSoul = healthManager.data.soulReward;
        float goldGain = PlayerStat.Instance.GetStat(StatType.GoldGain);
        int finalSoul = Mathf.RoundToInt(baseSoul * (1f + goldGain));

        GameManager.Instance.AddSoul(finalSoul);

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