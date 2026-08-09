using UnityEngine;
using System;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public bool HasSpace =>
        items.Count < maxSlots;

    [Header("인벤토리 설정")]
    public int maxSlots = 8;


    // 실제 보유 아이템
    private List<ItemInstance> items = new List<ItemInstance>();


    // UI 갱신 등에 사용
    public event Action OnInventoryChanged;

    // 액티브 아이템 사용 성공 시 호출
    public event Action<ItemInstance> OnActiveItemUsed;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    private void Update()
    {
        // 액티브 아이템 쿨타임 감소
        foreach (ItemInstance item in items)
        {
            if (item == null)
                continue;

            item.UpdateCooldown(Time.deltaTime);
        }
    }



    // 아이템 획득
    public bool AddItem(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "[Inventory] 추가하려는 ItemData가 null입니다."
            );

            return false;
        }


        if (items.Count >= maxSlots)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return false;
        }


        ItemInstance newItem = new ItemInstance(data);

        items.Add(newItem);


        // 능력치 적용
        ApplyItemStats(data);


        // 세트 효과 갱신
        UpdateSet();


        // 최대 체력 변경 시 체력 갱신
        RefreshPlayerHealth();


        OnInventoryChanged?.Invoke();


        Debug.Log($"{data.itemName} 획득");

        Debug.Log($"현재 아이템 개수: {items.Count}");

        foreach (ItemInstance item in items)
        {
            if (item == null || item.data == null)
                continue;

            Debug.Log($"보유 아이템 : {item.data.itemName}");
        }

        return true;
    }



    // 아이템 제거
    //
    public bool RemoveItem(int index, out ItemInstance removedItem)
    {
        removedItem = null;

        if (index < 0 || index >= items.Count)
            return false;


        removedItem = items[index];


        if (removedItem == null ||
            removedItem.data == null)
        {
            items.RemoveAt(index);

            UpdateSet();
            RefreshPlayerHealth();
            OnInventoryChanged?.Invoke();

            return true;
        }


        RemoveItemStats(removedItem.data);

        items.RemoveAt(index);

        UpdateSet();

        // 최대 체력 변경 시 체력 갱신
        RefreshPlayerHealth();

        OnInventoryChanged?.Invoke();


        Debug.Log(
            $"{removedItem.data.itemName} 제거"
        );

        return true;
    }



    // 아이템 목록 반환
    public List<ItemInstance> GetItems()
    {
        return items;
    }



    // 특정 아이템 사용
    public void UseItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return;


        ItemInstance item = items[index];


        if (item == null)
        {
            Debug.LogWarning(
                "[Inventory] 사용하려는 아이템이 null입니다."
            );

            return;
        }


        if (!item.IsActiveItem())
        {
            Debug.Log(
                "[Inventory] 액티브 아이템이 아닙니다."
            );

            return;
        }


        // 실제 사용 판정
        bool used = item.Use();


        if (!used)
        {
            Debug.Log(
                "[Inventory] 액티브 아이템을 사용할 수 없습니다."
            );

            return;
        }


        // 액티브 아이템 사용 성공 알림
        OnActiveItemUsed?.Invoke(item);
    }



    // 특정 아이템 사용 가능 여부
    public bool CanUseItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return false;


        ItemInstance item = items[index];


        if (item == null)
            return false;


        return item.CanUse();
    }



    // 특정 아이템 현재 쿨타임
    public float GetItemCooldown(int index)
    {
        if (index < 0 || index >= items.Count)
            return 0f;


        ItemInstance item = items[index];


        if (item == null)
            return 0f;


        return item.GetCurrentCooldown();
    }



    // 특정 아이템 최대 쿨타임
    public float GetItemMaxCooldown(int index)
    {
        if (index < 0 || index >= items.Count)
            return 0f;


        ItemInstance item = items[index];


        if (item == null)
            return 0f;


        return item.GetMaxCooldown();
    }



    // 모든 태그 가져오기
    public List<ItemTag> GetAllTags()
    {
        List<ItemTag> tags = new List<ItemTag>();


        foreach (ItemInstance item in items)
        {
            if (item == null ||
                item.data == null ||
                item.data.tags == null)
            {
                continue;
            }


            foreach (ItemTag tag in item.data.tags)
            {
                if (tag != ItemTag.None)
                    tags.Add(tag);
            }
        }


        return tags;
    }



    // 능력치 적용
    private void ApplyItemStats(ItemData data)
    {
        if (PlayerStat.Instance == null)
        {
            Debug.LogError(
                "[Inventory] PlayerStat.Instance가 없습니다."
            );

            return;
        }


        if (data == null ||
            data.modifiers == null)
        {
            return;
        }


        foreach (StatModifier modifier in data.modifiers)
        {
            if (modifier == null)
                continue;

            PlayerStat.Instance.AddStat(modifier);
        }
    }



    // 능력치 제거
    private void RemoveItemStats(ItemData data)
    {
        if (PlayerStat.Instance == null)
        {
            Debug.LogError(
                "[Inventory] PlayerStat.Instance가 없습니다."
            );

            return;
        }


        if (data == null ||
            data.modifiers == null)
        {
            return;
        }


        foreach (StatModifier modifier in data.modifiers)
        {
            if (modifier == null)
                continue;

            PlayerStat.Instance.RemoveStat(modifier);
        }
    }



    // 세트 효과 업데이트
    private void UpdateSet()
    {
        if (SetSystem.Instance == null)
        {
            Debug.LogError(
                "[Inventory] SetSystem.Instance가 없습니다."
            );

            return;
        }


        List<ItemTag> tags = GetAllTags();

        SetSystem.Instance.UpdateSetEffects(tags);
    }



    // 플레이어 최대 체력 갱신
    private void RefreshPlayerHealth()
    {
        PlayerController player =
            FindAnyObjectByType<PlayerController>();


        if (player == null)
            return;


        HealthManager healthManager =
            player.GetComponent<HealthManager>();


        if (healthManager == null)
            return;


        healthManager.RefreshMaxHealth();
    }



    // 게임 재시작용
    public void ClearInventory()
    {
        foreach (ItemInstance item in items)
        {
            if (item == null ||
                item.data == null)
            {
                continue;
            }

            RemoveItemStats(item.data);
        }


        items.Clear();


        UpdateSet();


        // 최대 체력 변경 시 체력 갱신
        RefreshPlayerHealth();


        // 런타임 능력치 초기화
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.ResetAllRuntimeStats();
        }


        OnInventoryChanged?.Invoke();


        Debug.Log(
            "[Inventory] 인벤토리 전체 초기화 완료"
        );
    }
}