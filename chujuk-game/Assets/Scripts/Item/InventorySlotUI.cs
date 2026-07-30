using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("슬롯 구성")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject hoverFrame;
    [SerializeField] private Image holdFillImage;

    private InventoryUIController inventoryUI;
    private ItemData currentItem;
    private int slotIndex;

    public int SlotIndex => slotIndex;
    public ItemData CurrentItem => currentItem;
    public bool HasItem => currentItem != null;

    public void Initialize(
        InventoryUIController owner,
        int index)
    {
        inventoryUI = owner;
        slotIndex = index;

        if (hoverFrame != null)
            hoverFrame.SetActive(false);

        SetHoldProgress(0f);
    }

    public void SetItem(ItemData itemData)
    {
        currentItem = itemData;

        if (currentItem == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        else
        {
            iconImage.sprite = currentItem.icon;
            iconImage.enabled = true;
        }

        SetHoldProgress(0f);
    }

    public void SetHoldProgress(float progress)
    {
        if (holdFillImage == null)
            return;

        holdFillImage.fillAmount =
            Mathf.Clamp01(progress);
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (hoverFrame != null)
            hoverFrame.SetActive(true);

        inventoryUI.SetHoveredSlot(this);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (hoverFrame != null)
            hoverFrame.SetActive(false);

        inventoryUI.ClearHoveredSlot(this);
    }
}