using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string SavePath => Path.Combine(Application.persistentDataPath, "savefile.json");

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

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    // ===================== 저장 =====================

    public void SaveGame()
    {
        PlayerController player = FindPlayer();

        if (player == null)
        {
            Debug.LogWarning("[SaveManager] PlayerController를 찾을 수 없어 저장을 취소합니다.");
            return;
        }

        SaveData data = new SaveData();

        data.sceneName = SceneManager.GetActiveScene().name;
        data.soul = GameManager.Instance != null ? GameManager.Instance.Soul : 0;

        data.currentHp = player.currentHp;
        data.maxHp = player.maxHp;
        data.posX = player.transform.position.x;
        data.posY = player.transform.position.y;
        data.posZ = player.transform.position.z;

        data.savedAtUtc = DateTime.UtcNow.ToString("o");

        if (Inventory.Instance != null)
        {
            foreach (ItemInstance item in Inventory.Instance.GetItems())
            {
                data.itemNames.Add(item.data.itemName);
            }
        }

        data.deadEnemyIds = EnemyDeathRegistry.Instance != null
            ? EnemyDeathRegistry.Instance.GetDeadIds()
            : new System.Collections.Generic.List<string>();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"[SaveManager] 저장 완료: {SavePath}");
    }

    // ===================== 불러오기 =====================

    public void LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveManager] 세이브 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        StartCoroutine(LoadRoutine(data));
    }

    private IEnumerator LoadRoutine(SaveData data)
    {
        // 씬을 다시 불러오기 전에 죽은 적 목록부터 레지스트리에 반영해야
        // 새로 스폰되는 적들이 자기 Awake 시점에 "나 이미 죽었었나?"를 정확히 판단할 수 있다.
        if (EnemyDeathRegistry.Instance != null)
            EnemyDeathRegistry.Instance.LoadDeadIds(data.deadEnemyIds);

        // 같은 씬이어도 항상 다시 로드한다 - 죽인 적, 상호작용한 오브젝트 등
        // 씬 자체의 상태를 저장 시점으로 되돌리려면 리로드가 필요하기 때문.
        if (!string.IsNullOrEmpty(data.sceneName))
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(data.sceneName);
            while (op != null && !op.isDone)
                yield return null;
        }

        // 씬 내 오브젝트들의 Awake/Start가 끝나도록 한 프레임 대기
        yield return null;

        ApplyData(data);
    }

    private void ApplyData(SaveData data)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetSoul(data.soul);

        if (Inventory.Instance != null)
        {
            Inventory.Instance.ClearInventory();

            foreach (string itemName in data.itemNames)
            {
                ItemData itemData = ItemDatabase.Instance != null
                    ? ItemDatabase.Instance.GetItemByName(itemName)
                    : null;

                if (itemData != null)
                    Inventory.Instance.AddItem(itemData);
                else
                    Debug.LogWarning($"[SaveManager] '{itemName}' 아이템을 찾지 못해 복원하지 못했습니다.");
            }
        }

        PlayerController player = FindPlayer();

        if (player != null)
        {
            // Die()로 비활성화된 상태였을 수 있으니 복원 시 다시 활성화
            if (!player.gameObject.activeSelf)
                player.gameObject.SetActive(true);

            player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
            player.LoadHealth(data.currentHp, data.maxHp);
        }

        Debug.Log("[SaveManager] 불러오기 완료");
    }

    private PlayerController FindPlayer()
    {
        // 씬에 플레이어가 한 명뿐이라는 전제. 추후 구조가 바뀌면 태그/직접 참조로 교체 필요
        // includeInactive: 사망 처리(Die())로 SetActive(false)된 상태에서도 찾을 수 있어야
        // 죽은 직후에 로드하는 시나리오가 정상 동작함
        return FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
    }

    // 세이브 파일 삭제 (예: "새 게임 시작" 버튼용)
    public void DeleteSaveFile()
    {
        if (HasSaveFile())
            File.Delete(SavePath);
    }
}