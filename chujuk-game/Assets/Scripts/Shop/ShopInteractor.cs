using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//플레이어가 상점 아이템과 상호작용할 수 있도록 하는 클래스
public class ShopInteractor : MonoBehaviour
{
    [Header("인벤토리 UI가 열렸을 때 상점 입력 방지")]
    [SerializeField]
    private GameObject inventoryPanel;

    private readonly HashSet<ShopItemDisplay>
        nearbyShopItems =
            new HashSet<ShopItemDisplay>();

    private readonly HashSet<ShopManager>
        nearbyRerolls =
            new HashSet<ShopManager>();

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.State !=
            GameManager.GameState.Playing)
        {
            return;
        }

        // 인벤토리가 열려 있을 때는
        // F키 버리기 기능과 충돌하지 않도록 막음
        if (inventoryPanel != null &&
            inventoryPanel.activeInHierarchy)
        {
            return;
        }

        if (!Keyboard.current.fKey
                .wasPressedThisFrame)
        {
            return;
        }

        TryInteract();
    }

    private void TryInteract()
    {
        nearbyShopItems.RemoveWhere(
            item => item == null
        );

        nearbyRerolls.RemoveWhere(
            reroll => reroll == null
        );

        ShopItemDisplay closestItem =
            FindClosestShopItem(
                out float itemDistance
            );

        ShopManager closestReroll =
            FindClosestReroll(
                out float rerollDistance
            );

        if (closestItem == null &&
            closestReroll == null)
        {
            return;
        }

        // 둘 다 범위에 있으면 더 가까운 대상 선택
        if (closestItem != null &&
            (closestReroll == null ||
             itemDistance <= rerollDistance))
        {
            closestItem.TryPurchase();
            return;
        }

        closestReroll?.TryReroll();
    }

    public void RegisterShopItem(
        ShopItemDisplay shopItem)
    {
        if (shopItem != null)
        {
            nearbyShopItems.Add(shopItem);
        }
    }

    public void UnregisterShopItem(
        ShopItemDisplay shopItem)
    {
        if (shopItem != null)
        {
            nearbyShopItems.Remove(shopItem);
        }
    }

    public void RegisterReroll(
        ShopManager shopManager)
    {
        if (shopManager != null)
        {
            nearbyRerolls.Add(shopManager);
        }
    }

    public void UnregisterReroll(
        ShopManager shopManager)
    {
        if (shopManager != null)
        {
            nearbyRerolls.Remove(shopManager);
        }
    }

    private ShopItemDisplay
        FindClosestShopItem(
            out float closestDistance)
    {
        ShopItemDisplay closest = null;

        closestDistance =
            float.MaxValue;

        foreach (ShopItemDisplay shopItem
                 in nearbyShopItems)
        {
            if (shopItem == null ||
                !shopItem.CanPurchase)
            {
                continue;
            }

            float distance =
                (
                    (Vector2)shopItem
                        .transform.position -
                    (Vector2)transform.position
                ).sqrMagnitude;

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closest = shopItem;
        }

        return closest;
    }

    private ShopManager FindClosestReroll(
        out float closestDistance)
    {
        ShopManager closest = null;

        closestDistance =
            float.MaxValue;

        foreach (ShopManager reroll
                 in nearbyRerolls)
        {
            if (reroll == null ||
                !reroll.CanRerollInteract)
            {
                continue;
            }

            float distance =
                (
                    (Vector2)reroll
                        .transform.position -
                    (Vector2)transform.position
                ).sqrMagnitude;

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closest = reroll;
        }

        return closest;
    }
}