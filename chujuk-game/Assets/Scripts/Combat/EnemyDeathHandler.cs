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
        GameManager.Instance.AddSoul(healthManager.data.soulReward); //적의 지급재화량만큼 재화 지급
        Destroy(gameObject); //Destroy로 삭제됨
    }
}
