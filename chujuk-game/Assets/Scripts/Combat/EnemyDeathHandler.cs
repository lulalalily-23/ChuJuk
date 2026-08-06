using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    //적의 죽음을 담당하는 핸들러
    private HealthManager healthManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("드랍 설정")]
    public GameObject moneyPrefab;

    public int dropAmount = 10;
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
        SpawnMoney();
        Destroy(gameObject);
    }
    private void SpawnMoney()
    {
        if (moneyPrefab == null)
        {
            Debug.LogError(
                "Money Prefab이 연결되지 않았습니다."
            );
            return;
        }
        for (int i = 0; i < dropAmount; i++)
        {
            Vector3 randomPosition =
                transform.position +
                new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0
                );
            Instantiate(
                moneyPrefab,
                randomPosition,
                Quaternion.identity
            );
        }
    }

}


