using UnityEngine;

// 분열된 자식 슬라임 프리팹에 붙이는 사망 처리.
public class SlimeDeathHandler : MonoBehaviour
{
    public float destroyDelay = 0.5f;

    private HealthManager healthManager;
    private SlimeController controller;

    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        controller = GetComponent<SlimeController>();
        healthManager.OnDeath += HandleDeath;
    }

    void HandleDeath()
    {
        controller?.PlayDie();

        bool isLastOne = controller == null || controller.siblingChild == null;

        if (isLastOne)
        {
            int baseSoul = healthManager.data.soulReward;
            float goldGain = PlayerStat.Instance.GetStat(StatType.GoldGain);
            int finalSoul = Mathf.RoundToInt(baseSoul * (1f + goldGain));

            GameManager.Instance.AddSoul(finalSoul);

            Debug.Log("[SlimeBoss] 슬라임 보스 최종 처치 - 보상 지급 완료");
        }
        else
        {
            controller.siblingChild.siblingChild = null;
        }

        Destroy(gameObject, destroyDelay);
    }
}