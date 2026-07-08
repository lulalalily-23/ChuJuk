using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public int hp = 30;
    public RoomClearManager roomClearManager;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            roomClearManager.EnemyKilled();
            Destroy(gameObject);
        }
    }
}