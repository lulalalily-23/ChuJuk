using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopItemData
{
    public string tierName;
    public int price;
    public int weight;
}

/// CSV의 tierName과 해당 등급에 속하는 실제 ItemData들을 연결합니다.

[Serializable]
public class ShopTierItemPool
{
    [Tooltip("CSV의 tierName과 정확히 같아야 합니다.")]
    public string tierName;

    [Tooltip("이 등급에 속하는 실제 아이템들")]
    public List<ItemData> items = new List<ItemData>();
}

[RequireComponent(typeof(Collider2D))]
public class ShopManager : MonoBehaviour
{
    private const int ShopSlotCount = 4;

    [Header("리롤 설정")]
    [SerializeField]
    private int currentRerollCost = 300;

    [SerializeField]
    private float rerollCostMultiplier = 1.2f;

    [SerializeField]
    private bool preventDuplicates = true;

    [Header("리롤 가격 표시")]
    [SerializeField]
    private TMP_Text rerollPriceText;

    [SerializeField]
    private GameObject rerollInteractionHint;

    [Header("상점 진열대 4개")]
    [SerializeField]
    private ShopItemDisplay[] itemDisplays =
        new ShopItemDisplay[ShopSlotCount];

    [Header("CSV")]
    [SerializeField]
    private TextAsset itemTableCSV;

    [Header("등급별 실제 ItemData")]
    [SerializeField]
    private List<ShopTierItemPool> tierItemPools =
        new List<ShopTierItemPool>();

    private readonly List<ShopItemData> itemDatabase =
        new List<ShopItemData>();

    private readonly ShopItemData[] currentShopItems =
        new ShopItemData[ShopSlotCount];

    private readonly ItemData[] currentItemDatas =
        new ItemData[ShopSlotCount];

    private readonly bool[] isSoldOut =
        new bool[ShopSlotCount];

    private ShopInteractor currentInteractor;

    public bool CanRerollInteract =>
        enabled &&
        gameObject.activeInHierarchy;

    public int CurrentRerollCost =>
        currentRerollCost;

    private void Awake()
    {
        Collider2D rerollCollider =
            GetComponent<Collider2D>();

        rerollCollider.isTrigger = true;

        if (rerollInteractionHint != null)
        {
            rerollInteractionHint.SetActive(false);
        }
    }

    private void Start()
    {
        LoadCSV();

        bool rolled = RollItems();

        if (!rolled)
        {
            Debug.LogError(
                "상점 최초 아이템 생성에 실패했습니다.",
                this
            );
        }

        UpdateRerollUI();
    }

    private void LoadCSV()
    {
        itemDatabase.Clear();

        if (itemTableCSV == null)
        {
            Debug.LogError(
                "ShopManager의 Item Table CSV가 비어 있습니다.",
                this
            );

            return;
        }

        string[] rows =
            itemTableCSV.text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries
            );

        // 첫 줄 헤더 -> 1부터 시작
        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim();

            if (string.IsNullOrEmpty(row))
                continue;

            string[] columns = row.Split(',');

            if (columns.Length < 3)
            {
                Debug.LogWarning(
                    $"CSV {i + 1}번째 줄의 열이 부족합니다: {row}",
                    this
                );

                continue;
            }

            string tierName =
                columns[0]
                    .Trim()
                    .Trim('\uFEFF')
                    .Trim('"');

            bool priceParsed =
                int.TryParse(
                    columns[1].Trim(),
                    out int price
                );

            bool weightParsed =
                int.TryParse(
                    columns[2].Trim(),
                    out int weight
                );

            if (!priceParsed || !weightParsed)
            {
                Debug.LogWarning(
                    $"CSV {i + 1}번째 줄의 가격 또는 가중치가 잘못됐습니다: {row}",
                    this
                );

                continue;
            }

            ShopItemData newItem =
                new ShopItemData
                {
                    tierName = tierName,
                    price = Mathf.Max(0, price),
                    weight = Mathf.Max(0, weight)
                };

            itemDatabase.Add(newItem);
        }

        Debug.Log(
            $"상점 CSV 로드 완료: {itemDatabase.Count}개 등급",
            this
        );
    }

  
    // 리롤

    public bool TryReroll()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "리롤 실패: GameManager.Instance가 없습니다.",
                this
            );

            return false;
        }

        if (!ValidateShopSettings())
            return false;

        int cost =
            Mathf.Max(0, currentRerollCost);

        if (cost > 0)
        {
            bool paid =
                GameManager.Instance.UseSoul(cost);

            if (!paid)
            {
                Debug.Log(
                    $"리롤 실패: Soul이 부족합니다. " +
                    $"필요: {cost}, " +
                    $"보유: {GameManager.Instance.Soul}"
                );

                return false;
            }
        }

        bool succeeded = RollItems();

        if (!succeeded)
        {
            if (cost > 0)
            {
                GameManager.Instance.AddSoul(cost);
            }

            Debug.LogError(
                "리롤에 실패하여 Soul을 환불했습니다.",
                this
            );

            return false;
        }

        currentRerollCost =
            Mathf.RoundToInt(
                currentRerollCost *
                Mathf.Max(1f, rerollCostMultiplier)
            );

        UpdateRerollUI();

        Debug.Log(
            $"상점 리롤 완료. 다음 가격: " +
            $"{currentRerollCost} Soul"
        );

        return true;
    }

    private bool RollItems()
    {
        if (!ValidateShopSettings())
            return false;

        bool useUniqueItems =
            preventDuplicates &&
            GetUniqueItemCount() >= ShopSlotCount;

        if (preventDuplicates &&
            !useUniqueItems)
        {
            Debug.LogWarning(
                "실제 ItemData가 4개보다 적어서 " +
                "이번 리롤에서는 아이템 중복이 허용됩니다.",
                this
            );
        }

        HashSet<ItemData> selectedItems =
            new HashSet<ItemData>();

        // 전부 성공한 다음 화면에 적용하기 위한 임시 배열
        ShopItemData[] rolledShopData =
            new ShopItemData[ShopSlotCount];

        ItemData[] rolledItemData =
            new ItemData[ShopSlotCount];

        for (int i = 0; i < ShopSlotCount; i++)
        {
            bool selected =
                TrySelectRandomItem(
                    selectedItems,
                    useUniqueItems,
                    out ShopItemData shopData,
                    out ItemData itemData
                );

            if (!selected)
            {
                Debug.LogError(
                    $"{i}번 상점 슬롯 추첨에 실패했습니다.",
                    this
                );

                return false;
            }

            rolledShopData[i] = shopData;
            rolledItemData[i] = itemData;

            if (useUniqueItems)
            {
                selectedItems.Add(itemData);
            }
        }

        // 4칸 추첨이 모두 성공한 뒤 실제 상태에 적용
        for (int i = 0; i < ShopSlotCount; i++)
        {
            currentShopItems[i] =
                rolledShopData[i];

            currentItemDatas[i] =
                rolledItemData[i];

            isSoldOut[i] = false;

            itemDisplays[i].Setup(
                this,
                i,
                currentItemDatas[i],
                currentShopItems[i].tierName,
                currentShopItems[i].price
            );
        }

        return true;
    }

    private bool TrySelectRandomItem(
        HashSet<ItemData> alreadySelected,
        bool preventItemDuplicate,
        out ShopItemData selectedShopData,
        out ItemData selectedItemData)
    {
        selectedShopData = null;
        selectedItemData = null;

        List<ShopItemData> availableTiers =
            new List<ShopItemData>();

        foreach (ShopItemData shopData
                 in itemDatabase)
        {
            ShopTierItemPool pool =
                FindTierPool(shopData.tierName);

            if (pool == null)
                continue;

            bool hasAvailableItem =
                HasAvailableItem(
                    pool,
                    alreadySelected,
                    preventItemDuplicate
                );

            if (hasAvailableItem)
            {
                availableTiers.Add(shopData);
            }
        }

        if (availableTiers.Count == 0)
            return false;

        selectedShopData =
            GetWeightedRandomTier(availableTiers);

        if (selectedShopData == null)
            return false;

        ShopTierItemPool selectedPool =
            FindTierPool(
                selectedShopData.tierName
            );

        if (selectedPool == null)
            return false;

        List<ItemData> availableItems =
            GetAvailableItems(
                selectedPool,
                alreadySelected,
                preventItemDuplicate
            );

        if (availableItems.Count == 0)
            return false;

        int randomIndex =
            UnityEngine.Random.Range(
                0,
                availableItems.Count
            );

        selectedItemData =
            availableItems[randomIndex];

        return selectedItemData != null;
    }

    private ShopItemData GetWeightedRandomTier(
        List<ShopItemData> candidates)
    {
        if (candidates == null ||
            candidates.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;

        foreach (ShopItemData item in candidates)
        {
            totalWeight +=
                Mathf.Max(0, item.weight);
        }

        // 모든 Weight가 0이면 균등 랜덤
        if (totalWeight <= 0)
        {
            int index =
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                );

            return candidates[index];
        }

        int randomValue =
            UnityEngine.Random.Range(
                0,
                totalWeight
            );

        int accumulatedWeight = 0;

        foreach (ShopItemData item in candidates)
        {
            accumulatedWeight +=
                Mathf.Max(0, item.weight);

            if (randomValue <
                accumulatedWeight)
            {
                return item;
            }
        }

        return candidates[
            candidates.Count - 1
        ];
    }

    public bool TryBuyItem(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.LogError(
                $"잘못된 상점 슬롯 번호입니다: {slotIndex}",
                this
            );

            return false;
        }

        if (isSoldOut[slotIndex])
            return false;

        ShopItemData shopData =
            currentShopItems[slotIndex];

        ItemData itemData =
            currentItemDatas[slotIndex];

        if (shopData == null ||
            itemData == null)
        {
            Debug.LogError(
                $"{slotIndex}번 슬롯의 데이터가 없습니다.",
                this
            );

            return false;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError(
                "구매 실패: Inventory.Instance가 없습니다.",
                this
            );

            return false;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "구매 실패: GameManager.Instance가 없습니다.",
                this
            );

            return false;
        }

        // 실제 저장소 Inventory 구조를 그대로 사용
        int currentItemCount =
            Inventory.Instance.GetItems().Count;

        int maxSlots =
            Inventory.Instance.maxSlots;

        if (currentItemCount >= maxSlots)
        {
            Debug.Log(
                "인벤토리가 가득 차서 구매할 수 없습니다."
            );

            return false;
        }

        int price =
            Mathf.Max(0, shopData.price);

        if (price > 0)
        {
            bool paid =
                GameManager.Instance.UseSoul(price);

            if (!paid)
            {
                Debug.Log(
                    $"구매 실패: Soul이 부족합니다. " +
                    $"필요: {price}, " +
                    $"보유: {GameManager.Instance.Soul}"
                );

                return false;
            }
        }

        bool added =
            Inventory.Instance.AddItem(itemData);

        if (!added)
        {
            // 예상하지 못한 인벤토리 추가 실패 시 환불
            if (price > 0)
            {
                GameManager.Instance.AddSoul(price);
            }

            Debug.LogError(
                "아이템 추가 실패로 Soul을 환불했습니다.",
                this
            );

            return false;
        }

        isSoldOut[slotIndex] = true;

        itemDisplays[slotIndex].MarkSoldOut();

        Debug.Log(
            $"[{itemData.itemName}] 구매 완료. " +
            $"남은 Soul: {GameManager.Instance.Soul}"
        );

        return true;
    }

 

    private ShopTierItemPool FindTierPool(
        string tierName)
    {
        if (string.IsNullOrWhiteSpace(tierName))
            return null;

        foreach (ShopTierItemPool pool
                 in tierItemPools)
        {
            if (pool == null)
                continue;

            bool sameTier =
                string.Equals(
                    pool.tierName?.Trim(),
                    tierName.Trim(),
                    StringComparison.OrdinalIgnoreCase
                );

            if (sameTier)
            {
                return pool;
            }
        }

        return null;
    }

    private bool HasAvailableItem(
        ShopTierItemPool pool,
        HashSet<ItemData> alreadySelected,
        bool preventItemDuplicate)
    {
        if (pool == null ||
            pool.items == null)
        {
            return false;
        }

        foreach (ItemData item in pool.items)
        {
            if (item == null)
                continue;

            if (preventItemDuplicate &&
                alreadySelected.Contains(item))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private List<ItemData> GetAvailableItems(
        ShopTierItemPool pool,
        HashSet<ItemData> alreadySelected,
        bool preventItemDuplicate)
    {
        List<ItemData> result =
            new List<ItemData>();

        if (pool == null ||
            pool.items == null)
        {
            return result;
        }

        foreach (ItemData item in pool.items)
        {
            if (item == null)
                continue;

            if (preventItemDuplicate &&
                alreadySelected.Contains(item))
            {
                continue;
            }

            result.Add(item);
        }

        return result;
    }

    private int GetUniqueItemCount()
    {
        HashSet<ItemData> uniqueItems =
            new HashSet<ItemData>();

        foreach (ShopTierItemPool pool
                 in tierItemPools)
        {
            if (pool == null ||
                pool.items == null)
            {
                continue;
            }

            foreach (ItemData item in pool.items)
            {
                if (item != null)
                {
                    uniqueItems.Add(item);
                }
            }
        }

        return uniqueItems.Count;
    }

    private bool ValidateShopSettings()
    {
        if (itemDatabase.Count == 0)
        {
            Debug.LogError(
                "상점 CSV 데이터가 비어 있습니다.",
                this
            );

            return false;
        }

        if (itemDisplays == null ||
            itemDisplays.Length < ShopSlotCount)
        {
            Debug.LogError(
                "Item Displays에 진열대 4개를 연결해야 합니다.",
                this
            );

            return false;
        }

        for (int i = 0; i < ShopSlotCount; i++)
        {
            if (itemDisplays[i] == null)
            {
                Debug.LogError(
                    $"Item Displays의 Element {i}가 비어 있습니다.",
                    this
                );

                return false;
            }
        }

        if (tierItemPools == null ||
            tierItemPools.Count == 0)
        {
            Debug.LogError(
                "Tier Item Pools가 비어 있습니다.",
                this
            );

            return false;
        }

        foreach (ShopItemData shopData
                 in itemDatabase)
        {
            if (FindTierPool(shopData.tierName) ==
                null)
            {
                Debug.LogError(
                    $"CSV의 [{shopData.tierName}]과 연결된 " +
                    "Tier Item Pool이 없습니다.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    private bool IsValidSlotIndex(
        int slotIndex)
    {
        return slotIndex >= 0 &&
               slotIndex < ShopSlotCount;
    }

  

    //리롤 상호작용

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        ShopInteractor interactor =
            other.GetComponentInParent<
                ShopInteractor>();

        if (interactor == null)
            return;

        currentInteractor = interactor;

        interactor.RegisterReroll(this);

        if (rerollInteractionHint != null)
        {
            rerollInteractionHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        ShopInteractor interactor =
            other.GetComponentInParent<
                ShopInteractor>();

        if (interactor == null)
            return;

        interactor.UnregisterReroll(this);

        if (currentInteractor == interactor)
        {
            currentInteractor = null;
        }

        if (rerollInteractionHint != null)
        {
            rerollInteractionHint.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (currentInteractor != null)
        {
            currentInteractor.UnregisterReroll(this);
            currentInteractor = null;
        }

        if (rerollInteractionHint != null)
        {
            rerollInteractionHint.SetActive(false);
        }
    }

    private void UpdateRerollUI()
    {
        if (rerollPriceText != null)
        {
            rerollPriceText.text =
                $"리롤\n{currentRerollCost:N0} Soul";
        }
    }
}