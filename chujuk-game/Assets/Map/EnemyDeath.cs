using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public RoomClearManager roomClearManager;
    private HealthManager healthManager;

    void Awake()
    {
        healthManager = GetComponent<HealthManager>();
        healthManager.OnDeath += Die;
    }

    private void Die()
    {
        if (roomClearManager != null)
        {
            roomClearManager.EnemyKilled();
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath -= Die;
        }
    }
}
