using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItemInteractor : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField]
    private InventoryUIController inventoryUI;

    private readonly HashSet<ItemPickup> nearbyPickups =
        new HashSet<ItemPickup>();

    private void Start()
    {
        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUIController>();
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // 인벤토리가 열려 있으면
        // F키는 버리기/판매 기능에만 사용
        if (inventoryUI != null &&
            inventoryUI.IsOpen)
        {
            return;
        }

        if (!Keyboard.current.fKey
                .wasPressedThisFrame)
        {
            return;
        }

        ItemPickup closestPickup =
            FindClosestPickup();

        if (closestPickup == null)
            return;

        closestPickup.TryPickup();
    }

    public void RegisterPickup(ItemPickup pickup)
    {
        if (pickup != null)
            nearbyPickups.Add(pickup);
    }

    public void UnregisterPickup(ItemPickup pickup)
    {
        if (pickup != null)
            nearbyPickups.Remove(pickup);
    }

    private ItemPickup FindClosestPickup()
    {
        // 이미 파괴된 아이템 정리
        nearbyPickups.RemoveWhere(
            pickup => pickup == null
        );

        ItemPickup closestPickup = null;
        float closestDistance = float.MaxValue;

        foreach (ItemPickup pickup in nearbyPickups)
        {
            float distance =
                ((Vector2)pickup.transform.position -
                 (Vector2)transform.position)
                .sqrMagnitude;

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestPickup = pickup;
        }

        return closestPickup;
    }
}