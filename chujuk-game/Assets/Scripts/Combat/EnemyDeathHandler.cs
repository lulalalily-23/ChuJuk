using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    //적의 죽음을 담당하는 핸들러
    private HealthManager healthManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        healthManager.OnDeath += HandleEnemyDeath; //OnDeath이벤트가 호출될 경우 HandleEnemyDeath 이벤트를 호출하도록 함
    }

    void HandleEnemyDeath()
    {
        int baseSoul = healthManager.data.soulReward;
        float goldGain = PlayerStat.Instance.GetStat(StatType.GoldGain);
        int finalSoul = Mathf.RoundToInt(baseSoul * (1f + goldGain));

        GameManager.Instance.AddSoul(finalSoul); //적의 지급재화량에 GoldGain 보너스 반영해서 지급
        Destroy(gameObject);
    }
}
