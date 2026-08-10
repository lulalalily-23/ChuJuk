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
            return;
        }

        SaveData data = new SaveData();

        data.sceneName = SceneManager.GetActiveScene().name;
        data.soul = GameManager.Instance != null ? GameManager.Instance.Soul : 0;

        data.currentHp = player.currentHp;
        data.maxHp = player.MaxHp;
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

        Debug.Log($"SaveManager 저장 완료: {SavePath}");
    }

    public void LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("세이브 파일이 없음");
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        StartCoroutine(LoadRoutine(data));
    }

    private IEnumerator LoadRoutine(SaveData data)
    {
        if (EnemyDeathRegistry.Instance != null)
            EnemyDeathRegistry.Instance.LoadDeadIds(data.deadEnemyIds);

        if (!string.IsNullOrEmpty(data.sceneName))
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(data.sceneName);
            while (op != null && !op.isDone)
                yield return null;
        }

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
                    Debug.LogWarning($"[SaveManager] '{itemName}' 아이템을 찾지 못해 복원 실패");
            }
        }

        PlayerController player = FindPlayer();

        if (player != null)
        {
            if (!player.gameObject.activeSelf)
                player.gameObject.SetActive(true);

            player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
            player.LoadHealth(data.currentHp);
        }

        Debug.Log("[SaveManager] 불러오기 완료");
    }

    private PlayerController FindPlayer()
    {
        return FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
    }

    // 세이브 파일 삭제
    public void DeleteSaveFile()
    {
        if (HasSaveFile())
            File.Delete(SavePath);
    }
    public void StartNewGame(string firstSceneName)
    {
        // 혹시 이전 화면에서 게임이 멈춰 있었다면 해제
        Time.timeScale = 1f;

        // 기존 세이브 파일 삭제
        DeleteSaveFile();

        // 기존에 죽었던 적 기록 삭제
        if (EnemyDeathRegistry.Instance != null)
        {
            EnemyDeathRegistry.Instance.ClearAll();
        }

        // 이전 인벤토리 데이터가 메모리에 남아 있다면 초기화
        if (Inventory.Instance != null)
        {
            Inventory.Instance.ClearInventory();
        }

        // 이전 Soul이 메모리에 남아 있다면 초기화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSoul(0);
        }

        // 런타임 능력치 초기화
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.ResetAllRuntimeStats();
        }

        // 첫 번째 게임 씬으로 이동
        SceneManager.LoadScene(firstSceneName);
    }
}