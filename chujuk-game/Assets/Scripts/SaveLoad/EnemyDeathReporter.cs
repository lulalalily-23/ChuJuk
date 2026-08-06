using UnityEngine;

// HealthManager를 쓰는 일반 적에 EnemyIdentity와 함께 붙이는 컴포넌트.
// 스폰(Awake) 시점에 "이미 죽은 것으로 기록되어 있는지" 확인해서 즉시 비활성화하고,
// 살아있다면 OnDeath 이벤트를 구독해뒀다가 죽는 순간 레지스트리에 등록한다.
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
            Debug.LogWarning($"[EnemyDeathReporter] {gameObject.name}에 EnemyIdentity가 없거나 enemyId가 비어있습니다. 저장/불러오기에서 제외됩니다.");
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