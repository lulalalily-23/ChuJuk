using System.Collections.Generic;
using UnityEngine;

// 세이브 파일에는 ItemData(ScriptableObject)를 직접 담을 수 없기 때문에,
// itemName 문자열만 저장해두고 불러올 때 이 데이터베이스로 실제 에셋을 다시 찾아온다.
// 인스펙터에 존재하는 모든 ItemData 에셋을 등록해두면 된다.
public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("게임에 존재하는 모든 ItemData 등록")]
    public List<ItemData> allItems = new List<ItemData>();

    private Dictionary<string, ItemData> itemDict = new Dictionary<string, ItemData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildDictionary();
    }

    private void BuildDictionary()
    {
        itemDict.Clear();

        foreach (ItemData item in allItems)
        {
            if (item == null)
                continue;

            if (itemDict.ContainsKey(item.itemName))
            {
                // itemName이 저장/복원의 유일한 키이기 때문에 중복되면 세이브 시스템이
                // 엉뚱한 아이템을 복원할 수 있다. 중복 발견 시 반드시 이름을 고쳐야 한다.
                Debug.LogWarning($"[ItemDatabase] itemName 중복: '{item.itemName}' ({item.name}) - 저장/불러오기 오류를 유발할 수 있습니다.");
                continue;
            }

            itemDict.Add(item.itemName, item);
        }
    }

    public ItemData GetItemByName(string itemName)
    {
        if (itemDict.TryGetValue(itemName, out ItemData data))
            return data;

        Debug.LogWarning($"[ItemDatabase] '{itemName}' 아이템을 찾을 수 없습니다. allItems 리스트 등록을 확인하세요.");
        return null;
    }
}