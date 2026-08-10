using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    //적의 죽음을 담당하는 핸들러
    private HealthManager healthManager;

    [Header("드랍 설정")]
    public GameObject moneyPrefab;

    public int dropAmount = 10;

    void Start()
    {
        healthManager = GetComponent<HealthManager>();

        if (healthManager == null)
        {
            Debug.LogError(
                "[EnemyDeathHandler] HealthManager를 찾을 수 없습니다."
            );
            return;
        }

        healthManager.OnDeath += HandleEnemyDeath; //OnDeath이벤트가 호출될 경우 HandleEnemyDeath 이벤트를 호출하도록 함
    }

    void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath -= HandleEnemyDeath;
        }
    }

    void HandleEnemyDeath()
    {
        if (PlayerStat.Instance != null)
        {
            int healAmount =
                Mathf.RoundToInt(
                    PlayerStat.Instance.GetStat(StatType.HealOnKill)
                );

            if (healAmount > 0)
            {
                PlayerController player =
                    FindAnyObjectByType<PlayerController>();

                if (player != null)
                {
                    player.Heal(healAmount);
                }
                else
                {
                    Debug.LogWarning(
                        "[EnemyDeathHandler] PlayerController를 찾을 수 없습니다."
                    );
                }
            }

            if (SetSystem.Instance != null)
            {
                int savageSetCount =
                    SetSystem.Instance.GetSetCount(ItemTag.Savagery);

                if (savageSetCount >= 4)
                {
                    PlayerStat.Instance.AddRuntimeStat(
                        StatType.AttackSpeed,
                        ModifierType.Percent,
                        0.20f
                    );
                }
            }
        }

        if (healthManager != null && healthManager.data != null)
        {
            int baseSoul = healthManager.data.soulReward;

            if (baseSoul > 0 && GameManager.Instance != null)
            {
                float goldGain = 0f;

                if (PlayerStat.Instance != null)
                {
                    goldGain = PlayerStat.Instance.GetStat(StatType.GoldGain);
                }

                int finalSoul =
                    Mathf.RoundToInt(
                        baseSoul * (1f + goldGain)
                    );

                if (finalSoul > 0)
                {
                    GameManager.Instance.AddSoul(finalSoul); //적의 지급재화량에 GoldGain 보너스 반영해서 지급
                }
            }
        }

        SpawnMoney();
        Destroy(gameObject);
    }

    private void SpawnMoney()
    {
        if (moneyPrefab == null)
        {
            Debug.LogWarning(
                "[EnemyDeathHandler] moneyPrefab이 설정되지 않았습니다."
            );
            return;
        }

        if (dropAmount <= 0)
            return;

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