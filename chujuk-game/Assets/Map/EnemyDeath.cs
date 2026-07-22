using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public int hp = 30;
    public RoomClearManager roomClearManager;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log("Enemy HP : " + hp);

        if (hp <= 0)
        {

            if (roomClearManager != null)
            {
                roomClearManager.EnemyKilled();
            }

            Destroy(gameObject);
        }
    }
}