using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// 씬에 배치된 각 적 오브젝트를 저장/불러오기에서 구분하기 위한 고유 ID.
// 에디터에서 자동으로 GUID를 생성해서 채워주기 때문에 직접 입력할 필요 없음.
// 주의: 코드로 런타임에 Instantiate()하는 적(예: 소환수)에는 이 자동 생성이 적용되지 않음 -
// 이런 경우는 애초에 저장 대상으로 다루지 않는 게 맞음 (일시적인 오브젝트이므로).
public class EnemyIdentity : MonoBehaviour
{
    [Tooltip("자동 생성되는 고유 ID입니다. 직접 수정하지 않아도 됩니다.")]
    public string enemyId;

#if UNITY_EDITOR
    // 컴포넌트를 처음 추가했을 때 (또는 인스펙터에서 Reset을 눌렀을 때) 한 번 채워줌
    private void Reset()
    {
        if (IsEditingPrefabAsset()) return;

        AssignIdIfNeeded();
    }

    // 인스펙터 값이 바뀌거나 오브젝트가 복제/저장될 때마다 체크
    private void OnValidate()
    {
        // 플레이 모드 중이거나, 프리팹 에셋 자체를 편집 중일 때는 건드리지 않음
        if (Application.isPlaying) return;
        if (IsEditingPrefabAsset()) return;

        AssignIdIfNeeded();
    }

    // 프리팹 "에셋"을 편집 중인지 확인.
    // PrefabUtility.IsPartOfPrefabAsset()만으로는 Prefab Mode(더블클릭해서 들어간 편집 화면) 안에서
    // 편집 중인 오브젝트를 제대로 못 잡아내는 경우가 있어서, PrefabStage 체크를 추가로 병행함.
    // 단, PrefabStage.IsPartOfPrefabContents()는 내부적으로 prefabContentsRoot를 참조하는데
    // 이게 Awake/OnEnable/OnValidate 초반 시점엔 호출이 막혀있어 예외가 나므로,
    // 대신 "오브젝트가 속한 씬 == 프리팹 스테이지의 씬"인지만 가볍게 비교한다.
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

    // 씬 안에 같은 ID를 가진 다른 EnemyIdentity가 있는지 확인 (Ctrl+D 복제 대비)
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