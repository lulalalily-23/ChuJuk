using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// 씬에 배치된 각 적 오브젝트를 저장/불러오기에서 구분하기 위한 고유 ID.
public class EnemyIdentity : MonoBehaviour
{
    [Tooltip("고유Id - 자동생성됨")]
    public string enemyId;

#if UNITY_EDITOR
    private void Reset()
    {
        if (IsEditingPrefabAsset()) return;

        AssignIdIfNeeded();
    }

    private void OnValidate()
    {
        // 플레이 모드 중이거나, 프리팹 에셋 자체를 편집 중일 때는 건드리지 않음
        if (Application.isPlaying) return;
        if (IsEditingPrefabAsset()) return;

        AssignIdIfNeeded();
    }

    private bool IsEditingPrefabAsset()
    {
        if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            return true;

        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && stage.scene == gameObject.scene)
            return true;

        return false;
    }

    private void AssignIdIfNeeded()
    {
        bool needsNewId = string.IsNullOrEmpty(enemyId) || IsDuplicateInScene();

        if (needsNewId)
        {
            enemyId = System.Guid.NewGuid().ToString();
        }
    }

    private bool IsDuplicateInScene()
    {
        EnemyIdentity[] all = FindObjectsByType<EnemyIdentity>(FindObjectsSortMode.None);

        foreach (EnemyIdentity other in all)
        {
            if (other != this && other.enemyId == enemyId)
                return true;
        }

        return false;
    }
#endif
}