using System.Collections.Generic;
using UnityEngine;

// 죽은 적들의 EnemyIdentity.enemyId를 모아두는 스크립트
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

    // 적이 불러오기 후 스폰될 때 자신이 죽어있었는지 확인용
    public bool IsDead(string enemyId)
    {
        return !string.IsNullOrEmpty(enemyId) && deadEnemyIds.Contains(enemyId);
    }

    // 저장 시 SaveManager가 호출
    public List<string> GetDeadIds()
    {
        return new List<string>(deadEnemyIds);
    }

    // 불러오기 시 SaveManager가 호출 
    public void LoadDeadIds(List<string> ids)
    {
        deadEnemyIds.Clear();

        if (ids != null)
        {
            foreach (string id in ids)
                deadEnemyIds.Add(id);
        }
    }

    // 재시작에서 초기화용
    public void ClearAll()
    {
        deadEnemyIds.Clear();
    }
}