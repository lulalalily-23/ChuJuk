using UnityEngine;

// HealthManager를 쓰는 일반 적이 죽었었는지 여부를 전달하는 스크립트
[RequireComponent(typeof(HealthManager))]
public class EnemyDeathReporter : MonoBehaviour
{
    private EnemyIdentity identity;
    private HealthManager health;

    private void Awake()
    {
        identity = GetComponent<EnemyIdentity>();
        health = GetComponent<HealthManager>();

        if (identity == null || string.IsNullOrEmpty(identity.enemyId))
        {
            return;
        }

        // 씬이 다시 로드됐는데 세이브 파일 기준으로 이미 죽은 적이라면 스폰되자마자 제거
        if (EnemyDeathRegistry.Instance != null && EnemyDeathRegistry.Instance.IsDead(identity.enemyId))
        {
            gameObject.SetActive(false);
            return;
        }

        health.OnDeath += HandleDeath;
    }

    private void HandleDeath()
    {
        if (EnemyDeathRegistry.Instance != null)
            EnemyDeathRegistry.Instance.RegisterDeath(identity.enemyId);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }
}