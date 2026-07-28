using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopItemData
{
    public string tierName;
    public int price;
    public int weight;
}

public class ShopManager : MonoBehaviour
{
    public int playerMoney = 5000;
    public int currentRerollCost = 300;

    public TextMeshProUGUI[] itemTexts;
    public TextMeshProUGUI moneyText;

    public Button[] itemButtons;

    public TextAsset itemTableCSV;

    private List<ShopItemData> itemDatabase = new List<ShopItemData>();

    private ShopItemData[] currentShopItems = new ShopItemData[4];
    private bool[] isSoldOut = new bool[4];

    void Start()
    {
        LoadCSV();
        RollItems();
        UpdateMoneyUI();
    }

    void LoadCSV()
    {
        if (itemTableCSV == null) return;

        string[] rows = itemTableCSV.text.Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            string[] columns = row.Split(',');
            ShopItemData newItem = new ShopItemData();
            newItem.tierName = columns[0];
            newItem.price = int.Parse(columns[1]);
            newItem.weight = int.Parse(columns[2]);
            itemDatabase.Add(newItem);
        }
    }

    public void RollItems()
    {
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, itemDatabase.Count);

            currentShopItems[i] = itemDatabase[randomIndex];
            isSoldOut[i] = false;

            itemTexts[i].text = $"{currentShopItems[i].tierName}\n{currentShopItems[i].price} G";

            if (itemButtons.Length > i && itemButtons[i] != null)
                itemButtons[i].interactable = true;
        }
    }

    public void BuyItem(int slotIndex)
    {
        if (isSoldOut[slotIndex]) return;

        ShopItemData itemToBuy = currentShopItems[slotIndex];

        if (playerMoney >= itemToBuy.price)
        {
            playerMoney -= itemToBuy.price;
            isSoldOut[slotIndex] = true;

            itemTexts[slotIndex].text = "SOLD OUT";
            itemButtons[slotIndex].interactable = false;

            UpdateMoneyUI();
            Debug.Log($"[{itemToBuy.tierName}] 구매 완료! 남은 돈: {playerMoney} G");

        }
        else
        {
            Debug.Log("돈이 부족합니다.");
        }
    }

    public void OnClickReroll()
    {
        if (playerMoney >= currentRerollCost)
        {
            playerMoney -= currentRerollCost;
            currentRerollCost = Mathf.RoundToInt(currentRerollCost * 1.2f);
            RollItems();
            UpdateMoneyUI();
        }
    }

    void UpdateMoneyUI()
    {
        moneyText.text = $"Monye: {playerMoney} G\n(Reroll: {currentRerollCost})";
    }
}