using System.Collections.Generic;
using UnityEngine;

// 죽은 적들의 EnemyIdentity.enemyId를 모아두는 창고.
// SaveManager가 저장 시 여기서 목록을 가져가고, 불러오기 시 여기로 목록을 다시 채워넣는다.
// GameManager/ItemDatabase/SaveManager와 같은 오브젝트에 붙여서 같이 관리하면 된다.
public class EnemyDeathRegistry : MonoBehaviour
{
    public static EnemyDeathRegistry Instance;

    private HashSet<string> deadEnemyIds = new HashSet<string>();

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

    // 적이 죽었을 때 EnemyDeathReporter가 호출
    public void RegisterDeath(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId))
            return;

        deadEnemyIds.Add(enemyId);
    }

    // 적이 스폰될 때 "나 이미 죽었었나?" 확인용
    public bool IsDead(string enemyId)
    {
        return !string.IsNullOrEmpty(enemyId) && deadEnemyIds.Contains(enemyId);
    }

    // 저장 시 SaveManager가 호출
    public List<string> GetDeadIds()
    {
        return new List<string>(deadEnemyIds);
    }

    // 불러오기 시 SaveManager가 호출 (씬을 다시 로드하기 전에 먼저 호출해야 함)
    public void LoadDeadIds(List<string> ids)
    {
        deadEnemyIds.Clear();

        if (ids != null)
        {
            foreach (string id in ids)
                deadEnemyIds.Add(id);
        }
    }

    // 새 게임 시작(RestartGame 등)에서 초기화용
    public void ClearAll()
    {
        deadEnemyIds.Clear();
    }
}