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
            item.UpdateCooldown(Time.deltaTime);
        }
    }



    // 아이템 획득
    public bool AddItem(ItemData data)
    {
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


        OnInventoryChanged?.Invoke();


        Debug.Log($"{data.itemName} 획득");

        Debug.Log($"현재 아이템 개수: {items.Count}");

        foreach (ItemInstance item in items)
        {
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

        RemoveItemStats(removedItem.data);

        items.RemoveAt(index);

        UpdateSet();
        OnInventoryChanged?.Invoke();

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


        items[index].Use();
    }



    // 모든 태그 가져오기
    public List<ItemTag> GetAllTags()
    {
        List<ItemTag> tags = new List<ItemTag>();


        foreach(ItemInstance item in items)
        {
            foreach(ItemTag tag in item.data.tags)
            {
                if(tag != ItemTag.None)
                    tags.Add(tag);
            }
        }


        return tags;
    }



    // 능력치 적용
    private void ApplyItemStats(ItemData data)
    {
        foreach(StatModifier modifier in data.modifiers)
        {
            PlayerStat.Instance.AddStat(modifier);
        }
    }



    // 능력치 제거
    private void RemoveItemStats(ItemData data)
    {
        foreach(StatModifier modifier in data.modifiers)
        {
            PlayerStat.Instance.RemoveStat(modifier);
        }
    }



    // 세트 효과 업데이트
    private void UpdateSet()
    {
        if(SetSystem.Instance == null)
            return;


        SetSystem.Instance.UpdateSetEffects(GetAllTags());
    }



    // 게임 재시작용
    public void ClearInventory()
    {
        foreach (ItemInstance item in items)
        {
            RemoveItemStats(item.data);
        }

        items.Clear();

        UpdateSet();
        OnInventoryChanged?.Invoke();
    }
}