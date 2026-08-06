using UnityEngine;

// 분열된 자식 슬라임 프리팹에 붙이는 사망 처리.
// 마지막으로 남은 자식이 죽는 순간에만 진짜 처치 보상(영혼)을 지급한다.
public class SlimeDeathHandler : MonoBehaviour
{
    [Tooltip("사망 애니메이션 재생 후 오브젝트 정리까지의 대기 시간")]
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

        // 형제가 이미 없다(=null)면 내가 마지막 남은 슬라임이라는 뜻
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
            // 형제에게 "나 죽었다"고 알려서, 형제 쪽에서도 참조가 끊기게 함
            controller.siblingChild.siblingChild = null;
        }

        Destroy(gameObject, destroyDelay);
    }
}