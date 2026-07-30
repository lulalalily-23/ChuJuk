using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 데이터")]
    [SerializeField] private ItemData itemData;

    [Header("화면 표시")]
    [SerializeField] private SpriteRenderer iconRenderer;

    private PlayerItemInteractor currentInteractor;

    public ItemData Data => itemData;

    private void Awake()
    {
        Collider2D itemCollider =
            GetComponent<Collider2D>();

        itemCollider.isTrigger = true;

        RefreshVisual();
    }

    /// 버린 아이템을 생성할 때 데이터를 넣어주는 함수
    public void SetItemData(ItemData newItemData)
    {
        itemData = newItemData;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (iconRenderer == null ||
            itemData == null)
        {
            return;
        }

        iconRenderer.sprite = itemData.icon;
    }

    /// 플레이어가 F키를 눌렀을 때 호출
    public bool TryPickup()
    {
        if (itemData == null)
        {
            Debug.LogWarning(
                $"{name}에 ItemData가 없습니다."
            );

            return false;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError(
                "씬에서 Inventory.Instance를 찾지 못했습니다."
            );

            return false;
        }

        bool added =
            Inventory.Instance.AddItem(itemData);

        // 인벤토리가 가득 찼으면 아이템을 제거하지 않음
        if (!added)
            return false;

        Destroy(gameObject);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerItemInteractor interactor =
            other.GetComponentInParent<PlayerItemInteractor>();

        if (interactor == null)
            return;

        currentInteractor = interactor;
        interactor.RegisterPickup(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerItemInteractor interactor =
            other.GetComponentInParent<PlayerItemInteractor>();

        if (interactor == null)
            return;

        interactor.UnregisterPickup(this);

        if (currentInteractor == interactor)
            currentInteractor = null;
    }

    private void OnDestroy()
    {
        if (currentInteractor != null)
        {
            currentInteractor.UnregisterPickup(this);
        }
    }
}