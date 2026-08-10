using UnityEngine;

public class ChestInteractor : MonoBehaviour
{
    public GameObject itemToDrop; // 드롭할 아이템 프리팹
    public Transform dropPoint;   // 아이템이 튀어나올 위치
    private bool isPlayerInRange = false;
    private bool isOpened = false;

    void Update()
    {
        // F키 누르면 상자 오픈
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !isOpened)
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;
        Debug.Log("보물상자를 엽니다!");

        // 아이템 생성
        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, dropPoint.position, Quaternion.identity);
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}