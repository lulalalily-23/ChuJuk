#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class EnemySaveSetupTool
{
    [MenuItem("Tools/저장 시스템/씬의 모든 적에 저장 컴포넌트 추가")]
    public static void AddSaveComponentsToAllEnemies()
    {
        HealthManager[] allHealthManagers = Object.FindObjectsByType<HealthManager>(FindObjectsSortMode.None);

        int identityAdded = 0;
        int reporterAdded = 0;
        int skippedPlayer = 0;

        foreach (HealthManager hm in allHealthManagers)
        {
            GameObject go = hm.gameObject;

            if (go.GetComponent<PlayerController>() != null)
            {
                skippedPlayer++;
                continue;
            }

            if (go.GetComponent<EnemyIdentity>() == null)
            {
                Undo.AddComponent<EnemyIdentity>(go);
                identityAdded++;
            }

            if (go.GetComponent<EnemyDeathReporter>() == null)
            {
                Undo.AddComponent<EnemyDeathReporter>(go);
                reporterAdded++;
            }

            EditorUtility.SetDirty(go);
        }

        Debug.Log($"[EnemySaveSetupTool] EnemyIdentity {identityAdded}개, EnemyDeathReporter {reporterAdded}개 추가 완료. " +
                  $"(플레이어로 판단되어 건너뛴 오브젝트 {skippedPlayer}개)");
    }

    // 프리팹 원본에 ID가 이미 박혀있는 경우, 씬에 뿌려진 인스턴스들이 전부 같은 ID를
    // 공유하게 되는 문제가 생길 수 있다. 이 메뉴로 중복/빈 ID를 찾아서 재생성한다.
    [MenuItem("Tools/저장 시스템/중복된 EnemyId 재생성")]
    public static void FixDuplicateEnemyIds()
    {
        EnemyIdentity[] allIdentities = Object.FindObjectsByType<EnemyIdentity>(FindObjectsSortMode.None);
        System.Collections.Generic.HashSet<string> seenIds = new System.Collections.Generic.HashSet<string>();
        int fixedCount = 0;

        foreach (EnemyIdentity identity in allIdentities)
        {
            bool isEmpty = string.IsNullOrEmpty(identity.enemyId);
            bool isDuplicate = !isEmpty && !seenIds.Add(identity.enemyId);

            if (isEmpty || isDuplicate)
            {
                identity.enemyId = System.Guid.NewGuid().ToString();
                seenIds.Add(identity.enemyId);
                EditorUtility.SetDirty(identity);
                fixedCount++;
            }
        }

        Debug.Log($"[EnemySaveSetupTool] 중복되었거나 비어있던 ID {fixedCount}개를 새로 생성했습니다.");
    }
}
#endif