using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ShopItemDisplay : MonoBehaviour
{
    [Header("월드 아이템 이미지")]
    [SerializeField]
    private SpriteRenderer iconRenderer;

    [Header("월드 UI")]
    [SerializeField]
    private TMP_Text itemNameText;

    [SerializeField]
    private TMP_Text tierText;

    [SerializeField]
    private TMP_Text priceText;

    [SerializeField]
    private GameObject soldOutObject;

    [SerializeField]
    private GameObject interactionHint;

    private ShopManager shopManager;
    private ItemData currentItem;

    private int slotIndex = -1;
    private int price;
    private bool isSoldOut;

    private ShopInteractor currentInteractor;

    private readonly HashSet<Collider2D>
        playerColliders =
            new HashSet<Collider2D>();

    public bool CanPurchase =>
        shopManager != null &&
        currentItem != null &&
        slotIndex >= 0 &&
        !isSoldOut;

    public ItemData CurrentItem =>
        currentItem;

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

        if (soldOutObject != null)
        {
            soldOutObject.SetActive(false);
        }

        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    public void Setup(
        ShopManager owner,
        int newSlotIndex,
        ItemData itemData,
        string tierName,
        int itemPrice)
    {
        shopManager = owner;
        slotIndex = newSlotIndex;
        currentItem = itemData;
        price = Mathf.Max(0, itemPrice);
        isSoldOut = false;

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

        if (itemNameText != null)
        {
            itemNameText.text =
                currentItem.itemName;
        }

        if (tierText != null)
        {
            tierText.text =
                tierName;
        }

        if (priceText != null)
        {
            priceText.text =
                $"{price:N0} Soul";
        }

        if (soldOutObject != null)
        {
            soldOutObject.SetActive(false);
        }

        UpdateInteractionHint();
    }

    public bool TryPurchase()
    {
        if (!CanPurchase)
            return false;

        return shopManager.TryBuyItem(
            slotIndex
        );
    }

    public void MarkSoldOut()
    {
        isSoldOut = true;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = null;
            iconRenderer.enabled = false;
        }

        if (itemNameText != null)
        {
            itemNameText.text = "";
        }

        if (tierText != null)
        {
            tierText.text = "";
        }

        if (priceText != null)
        {
            priceText.text = "";
        }

        if (soldOutObject != null)
        {
            soldOutObject.SetActive(true);
        }

        UpdateInteractionHint();
    }

    private void ClearDisplay()
    {
        currentItem = null;
        isSoldOut = true;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = null;
            iconRenderer.enabled = false;
        }

        if (itemNameText != null)
        {
            itemNameText.text = "";
        }

        if (tierText != null)
        {
            tierText.text = "";
        }

        if (priceText != null)
        {
            priceText.text = "";
        }

        if (soldOutObject != null)
        {
            soldOutObject.SetActive(false);
        }

        UpdateInteractionHint();
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        ShopInteractor interactor =
            other.GetComponentInParent<
                ShopInteractor>();

        if (interactor == null)
            return;

        playerColliders.Add(other);

        currentInteractor = interactor;

        interactor.RegisterShopItem(this);

        UpdateInteractionHint();
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        ShopInteractor interactor =
            other.GetComponentInParent<
                ShopInteractor>();

        if (interactor == null)
            return;

        playerColliders.Remove(other);

        if (playerColliders.Count > 0)
            return;

        interactor.UnregisterShopItem(this);

        if (currentInteractor == interactor)
        {
            currentInteractor = null;
        }

        UpdateInteractionHint();
    }

    private void OnDisable()
    {
        if (currentInteractor != null)
        {
            currentInteractor
                .UnregisterShopItem(this);

            currentInteractor = null;
        }

        playerColliders.Clear();

        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    private void UpdateInteractionHint()
    {
        if (interactionHint == null)
            return;

        bool shouldShow =
            playerColliders.Count > 0 &&
            CanPurchase;

        interactionHint.SetActive(shouldShow);
    }
}