#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class BatchPrefabInstallerWindow : EditorWindow
{
    // 원하는 만큼 프리팹을 넣을 수 있는 리스트
    public List<GameObject> prefabsToInstall = new List<GameObject>();

    private SerializedObject serializedObject;
    private SerializedProperty prefabsProperty;

    [MenuItem("Tools/씬 일괄 자동 배치기")]
    public static void ShowWindow()
    {
        GetWindow<BatchPrefabInstallerWindow>("씬 배치기");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        prefabsProperty = serializedObject.FindProperty("prefabsToInstall");
    }

    private void OnGUI()
    {
        serializedObject.Update();

        GUILayout.Label("모든 씬에 프리팹 일괄 배치 (다중 선택)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 유니티 인스펙터처럼 리스트 목록을 출력
        EditorGUILayout.PropertyField(prefabsProperty, new GUIContent("배치할 프리팹 목록"), true);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        if (GUILayout.Button("빌드 세팅의 모든 씬에 일괄 배치 실행", GUILayout.Height(35)))
        {
            if (prefabsToInstall == null || prefabsToInstall.Count == 0)
            {
                EditorUtility.DisplayDialog("오류", "배치할 프리팹을 최소 1개 이상 목록에 등록해주세요!", "확인");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            int updatedCount = 0;

            foreach (var sceneSetting in EditorBuildSettings.scenes)
            {
                if (!sceneSetting.enabled) continue;

                var scene = EditorSceneManager.OpenScene(sceneSetting.path);
                bool isModified = false;

                foreach (var prefab in prefabsToInstall)
                {
                    if (prefab == null) continue;

                    // 해당 씬에 똑같은 이름의 프리팹이 없을 때만 배치 (중복 생성 방지)
                    if (GameObject.Find(prefab.name) == null)
                    {
                        PrefabUtility.InstantiatePrefab(prefab);
                        isModified = true;
                    }
                }

                if (isModified)
                {
                    EditorSceneManager.SaveScene(scene);
                    updatedCount++;
                }
            }

            EditorUtility.DisplayDialog("작업 완료", $"총 {updatedCount}개 씬에 선택한 모든 프리팹 배치를 완료했습니다.", "확인");
        }
    }
}
#endif