using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ShopItemDisplay : MonoBehaviour
{
    [Header("화면 표시")]
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject soldOutObject;

    private ItemData currentItem;
    private ShopManager shopManager;
    private bool isSold;

    public ItemData CurrentItem => currentItem;

    public bool CanPurchase =>
        currentItem != null &&
        !isSold;

    private void Awake()
    {
        if (iconRenderer == null)
        {
            iconRenderer =
                GetComponent<SpriteRenderer>();
        }

        Collider2D interactionCollider =
            GetComponent<Collider2D>();

        interactionCollider.isTrigger = true;

        ClearDisplay();
    }

    public void Setup(
        ShopManager owner,
        ItemData itemData)
    {
        shopManager = owner;
        currentItem = itemData;
        isSold = false;

        RefreshVisual();
    }

    public bool TryPurchase()
    {
        if (!CanPurchase)
            return false;

        if (shopManager == null)
        {
            Debug.LogError(
                $"{name}: ShopManager가 연결되지 않았습니다.",
                this
            );

            return false;
        }

        return shopManager.TryPurchase(this);
    }

    public void MarkSold()
    {
        isSold = true;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = null;
            iconRenderer.enabled = false;
        }

        if (priceText != null)
        {
            priceText.text = "판매 완료";
        }

        if (soldOutObject != null)
        {
            soldOutObject.SetActive(true);
        }
    }

    private void RefreshVisual()
    {
        if (currentItem == null)
        {
            ClearDisplay();
            return;
        }

        if (iconRenderer != null)
        {
            iconRenderer.sprite =
                currentItem.icon;

            iconRenderer.enabled =
                currentItem.icon != null;
        }

        if (priceText != null)
        {
            priceText.text =
                currentItem.buyPrice.ToString();
        }

        if (soldOutObject != null)
        {
            soldOutObject.SetActive(false);
        }
    }

    private void ClearDisplay()
    {
        currentItem = null;
        isSold = false;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = null;
            iconRenderer.enabled = false;
        }

        if (priceText != null)
        {
            priceText.text = "";
        }

        if (soldOutObject != null)
        {
            soldOutObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ShopInteractor interactor =
            other.GetComponentInParent<ShopInteractor>();

        if (interactor == null)
            return;

        interactor.RegisterShopItem(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ShopInteractor interactor =
            other.GetComponentInParent<ShopInteractor>();

        if (interactor == null)
            return;

        interactor.UnregisterShopItem(this);
    }

    private void OnDestroy()
    {
        ShopInteractor interactor =
            FindFirstObjectByType<ShopInteractor>();

        if (interactor != null)
        {
            interactor.UnregisterShopItem(this);
        }
    }
}