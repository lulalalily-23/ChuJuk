using UnityEngine;

public class RoomClearManager : MonoBehaviour
{
    public GameObject portal;

    private int enemyCount;

    private void Start()
    {
        enemyCount = FindObjectsByType<EnemyDeath>(FindObjectsSortMode.None).Length;

        portal.SetActive(false);
    }

    public void EnemyKilled()
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            portal.SetActive(true);
        }
    }
}