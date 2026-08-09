using UnityEngine;

public class ChestSpawnManager : MonoBehaviour
{
    public RoomClearManager roomClearManager; // 기존 매니저 연결
    public GameObject chest; // 상자 오브젝트

    void Start()
    {
        if (chest != null) chest.SetActive(false); // 처음엔 숨김
    }

    // 클리어되었을 때 외부에서 호출할 함수
    public void SpawnChest()
    {
        if (chest != null) chest.SetActive(true);
    }
}