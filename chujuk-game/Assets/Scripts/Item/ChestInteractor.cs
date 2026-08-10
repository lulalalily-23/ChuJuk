using UnityEngine;

public class ChestInteractor : MonoBehaviour
{
    [Header("아이템 데이터")]
    public ItemData itemToDrop;
    public Transform dropPoint; // 아이템이 튀어나올 위치

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

        if (itemToDrop != null)
        {
            // 빈 게임 오브젝트를 그 자리에서 즉석으로 생성
            GameObject droppedItemObj = new GameObject("Dropped_" + itemToDrop.itemName);
            droppedItemObj.transform.position = dropPoint.position;

            // 아이콘을 보여줄 SpriteRenderer 컴포넌트 추가하고 이미지 세팅
            SpriteRenderer sr = droppedItemObj.AddComponent<SpriteRenderer>();
            sr.sprite = itemToDrop.icon;

            // 아이템 먹는 콜라이더나 스크립트 추가
            // BoxCollider2D col = droppedItemObj.AddComponent<BoxCollider2D>();
            // col.isTrigger = true;

            Debug.Log($"[{itemToDrop.itemName}] 아이템이 생성되었습니다!");
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