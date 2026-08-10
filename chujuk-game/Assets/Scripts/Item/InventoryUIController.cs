using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    private enum HoldAction
    {
        None,
        Discard,
        Sell
    }

    [Header("UI")]
    [SerializeField]
    private GameObject inventoryPanel;

    [SerializeField]
    private Transform slotParent;

    [Header("아이템 버리기")]
    [SerializeField]
    private ItemDropSpawner itemDropSpawner;

    [Header("홀드 설정")]
    [SerializeField]
    private float holdDuration = 2f;

    [Header("아이템 설명창")]
    [SerializeField]
    private ItemDescriptionUI itemDescriptionUI;

    private InventorySlotUI[] slotUIs;
    private Inventory boundInventory;

    private InventorySlotUI hoveredSlot;
    private InventorySlotUI holdingSlot;

    private HoldAction currentHoldAction =
        HoldAction.None;

    private float holdTimer;
    private bool isShopStage;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (slotParent == null)
        {
            Debug.LogError(
                "InventoryUIController에 Slot Parent가 없습니다."
            );

            return;
        }

        slotUIs =
            slotParent.GetComponentsInChildren
                <InventorySlotUI>(true);

        // Hierarchy에서 위에 있는 슬롯부터
        // 0, 1, 2... 순서로 정렬
        Array.Sort(
            slotUIs,
            (left, right) =>
                left.transform
                    .GetSiblingIndex()
                    .CompareTo(
                        right.transform
                            .GetSiblingIndex()
                    )
        );

        for (int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i].Initialize(this, i);
        }

        if (slotUIs.Length != 8)
        {
            Debug.LogWarning(
                $"현재 슬롯 개수는 {slotUIs.Length}개입니다. " +
                "8개를 만들어야 합니다."
            );
        }
    }

    private void Start()
    {
        BindInventory();
        SetInventoryOpen(false);
    }

    private void OnDisable()
    {
        UnbindInventory();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Inventory가 늦게 생성된 경우 다시 연결
        if (boundInventory == null)
            BindInventory();

        // GameManager가 없으면 인벤토리 입력 금지
        if (GameManager.Instance == null)
        {
            if (IsOpen)
                CloseInventory();

            return;
        }

        // Playing 상태가 아니면 인벤토리를 사용할 수 없음
        if (GameManager.Instance.State != GameManager.GameState.Playing)
        {
            // GameOver나 Pause로 넘어갔는데
            // 인벤토리가 열려 있었다면 자동으로 닫기
            if (IsOpen)
                CloseInventory();

            return;
        }

        // Playing 상태에서만 I키 사용 가능
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            SetInventoryOpen(!IsOpen);
        }

        if (!IsOpen)
            return;

        HandleHoldInput();
    }

    private void BindInventory()
    {
        if (Inventory.Instance == null)
            return;

        if (boundInventory == Inventory.Instance)
            return;

        UnbindInventory();

        boundInventory = Inventory.Instance;

        boundInventory.OnInventoryChanged +=
            RefreshUI;

        RefreshUI();
    }

    private void UnbindInventory()
    {
        if (boundInventory == null)
            return;

        boundInventory.OnInventoryChanged -=
            RefreshUI;

        boundInventory = null;
    }

    private void SetInventoryOpen(bool open)
    {
        IsOpen = open;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(open);

        if (open)
        {
            // 현재 씬이 상점인지 다시 검사
            isShopStage =
                FindFirstObjectByType<ShopStageMarker>()
                != null;

            RefreshUI();
        }
        else
        {
            hoveredSlot = null;
            ResetHold();

            // 인벤토리를 닫으면 아이템 설명창도 숨김
            if (itemDescriptionUI != null)
            {
                itemDescriptionUI.Hide();
            }
        }
    }

    public void CloseInventory()
    {
        SetInventoryOpen(false);
    }

    public void SetHoveredSlot(
    InventorySlotUI slot)
    {
        hoveredSlot = slot;
        ResetHold();

        if (itemDescriptionUI == null)
            return;

        if (slot != null &&
            slot.HasItem)
        {
            itemDescriptionUI.Show(
                slot.CurrentItem
            );
        }
        else
        {
            itemDescriptionUI.Hide();
        }
    }

    public void ClearHoveredSlot(
    InventorySlotUI slot)
    {
        if (hoveredSlot != slot)
            return;

        hoveredSlot = null;

        ResetHold();

        if (itemDescriptionUI != null)
        {
            itemDescriptionUI.Hide();
        }
    }

    private void RefreshUI()
    {
        if (boundInventory == null ||
            slotUIs == null)
        {
            return;
        }

        List<ItemInstance> items =
            boundInventory.GetItems();

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (i < items.Count)
            {
                slotUIs[i].SetItem(
                    items[i].data
                );
            }
            else
            {
                slotUIs[i].SetItem(null);
            }
        }
    }

    private void HandleHoldInput()
    {
        HoldAction requestedAction =
            GetRequestedAction();

        if (requestedAction == HoldAction.None)
        {
            ResetHold();
            return;
        }

        // 슬롯 또는 행동 종류가 바뀌면
        // 홀드 시간을 처음부터 시작
        if (holdingSlot != hoveredSlot ||
            currentHoldAction != requestedAction)
        {
            BeginHold(
                hoveredSlot,
                requestedAction
            );
        }

        holdTimer += Time.unscaledDeltaTime;

        holdingSlot.SetHoldProgress(
            holdTimer / holdDuration
        );

        if (holdTimer < holdDuration)
            return;

        ExecuteHoldAction();
        ResetHold();
    }

    private HoldAction GetRequestedAction()
    {
        if (hoveredSlot == null ||
            !hoveredSlot.HasItem)
        {
            return HoldAction.None;
        }

        if (!Keyboard.current.fKey.isPressed)
            return HoldAction.None;

        bool shiftPressed =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        // Shift + F
        if (shiftPressed)
        {
            // 상점이 아니면 아무 행동도 하지 않음
            return isShopStage
                ? HoldAction.Sell
                : HoldAction.None;
        }

        // 일반 F
        return HoldAction.Discard;
    }

    private void BeginHold(
        InventorySlotUI slot,
        HoldAction action)
    {
        ResetHold();

        holdingSlot = slot;
        currentHoldAction = action;
        holdTimer = 0f;
    }

    private void ExecuteHoldAction()
    {
        if (holdingSlot == null ||
            boundInventory == null)
        {
            return;
        }

        List<ItemInstance> items =
            boundInventory.GetItems();

        int index = holdingSlot.SlotIndex;

        if (index < 0 ||
            index >= items.Count)
        {
            return;
        }

        ItemData itemData =
            items[index].data;

        switch (currentHoldAction)
        {
            case HoldAction.Discard:
                DiscardItem(index, itemData);
                break;

            case HoldAction.Sell:
                SellItem(index, itemData);
                break;
        }
    }

    private void DiscardItem(
    int index,
    ItemData itemData)
    {
        if (boundInventory == null)
        {
            Debug.LogError(
                "[버리기 실패] Inventory가 연결되지 않았습니다."
            );

            return;
        }

        if (itemDropSpawner == null)
        {
            Debug.LogError(
                "[버리기 실패] InventoryUIController의 " +
                "Item Drop Spawner가 연결되지 않았습니다."
            );

            return;
        }

        if (itemData == null)
        {
            Debug.LogError(
                "[버리기 실패] 슬롯의 ItemData가 null입니다."
            );

            return;
        }

        // 먼저 바닥에 아이템 생성
        bool spawnSucceeded =
            itemDropSpawner.TrySpawn(itemData);

        if (!spawnSucceeded)
        {
            Debug.LogError(
                $"[버리기 실패] {itemData.itemName}을 " +
                "바닥에 생성하지 못했습니다."
            );

            return;
        }

        // 생성된 이후 인벤토리에서 제거
        bool removeSucceeded =
            boundInventory.RemoveItem(
                index,
                out ItemInstance removedItem
            );

        if (!removeSucceeded)
        {
            Debug.LogError(
                $"[버리기 실패] {itemData.itemName}을 " +
                "인벤토리에서 제거하지 못했습니다."
            );

            return;
        }

        Debug.Log(
            $"[버리기 성공] " +
            $"{removedItem.data.itemName}을 버렸습니다."
        );
    }

    private void SellItem(int index, ItemData itemData)
    {
        if (!isShopStage)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "GameManager.Instance가 없습니다."
            );

            return;
        }

        bool removed = boundInventory.RemoveItem(
            index,
            out ItemInstance removedItem
        );

        if (!removed ||
            removedItem == null ||
            removedItem.data == null)
        {
            return;
        }

        int sellPrice =
            Mathf.Max(0, removedItem.data.sellPrice);

        GameManager.Instance.AddSoul(sellPrice);

        Debug.Log(
            $"{removedItem.data.itemName} 판매 완료: " +
            $"{sellPrice} Soul"
        );
    }

    private void ResetHold()
    {
        if (holdingSlot != null)
        {
            holdingSlot.SetHoldProgress(0f);
        }

        holdingSlot = null;
        currentHoldAction = HoldAction.None;
        holdTimer = 0f;
    }
}