using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    //플레이어의 죽음을 담당하는 핸들러
    private HealthManager healthManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {     
        healthManager = GetComponent<HealthManager>();
        healthManager.OnDeath += HandlePlayerDeath;
    }

    void HandlePlayerDeath()
    {
        GameManager.Instance.SetGameOver();
        //우선 임의로 GameManager.cs의 SetGameOver을 가져오는 식으로 제작함 (나중에 수정 필요)
    }
}
