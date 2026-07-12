using UnityEngine;

public class EnemyAutoDie : MonoBehaviour
{
    public float lifeTime = 10f;
    public RoomClearManager roomClearManager;

    private bool isDead = false;

    private void Start()
    {
        Invoke(nameof(Die), lifeTime);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (roomClearManager != null)
        {
            roomClearManager.EnemyKilled();
        }

        Destroy(gameObject);
    }
}