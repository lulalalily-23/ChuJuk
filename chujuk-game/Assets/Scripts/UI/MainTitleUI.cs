using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainTitleUI : MonoBehaviour
{
    [Header("게임 시작 설정")]
    [SerializeField]
    private string firstSceneName = "1-1";

    [Header("버튼")]
    [SerializeField]
    private Button continueButton;

    private void Start()
    {
        Time.timeScale = 1f;

        UpdateContinueButton();
    }

    // 새 게임
    public void NewGame()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "[MainTitleUI] SaveManager가 없습니다."
            );

            return;
        }

        SaveManager.Instance.StartNewGame(
            firstSceneName
        );
    }

    // 이어서 플레이
    public void ContinueGame()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "[MainTitleUI] SaveManager가 없습니다."
            );

            return;
        }

        if (!SaveManager.Instance.HasSaveFile())
        {
            Debug.LogWarning(
                "[MainTitleUI] 저장된 게임이 없습니다."
            );

            return;
        }

        SaveManager.Instance.LoadGame();
    }

    // 게임 종료
    public void QuitGame()
    {
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying =
            false;

#else

        Application.Quit();

#endif
    }

    // 이어하기 버튼 상태

    private void UpdateContinueButton()
    {
        if (continueButton == null)
        {
            return;
        }

        bool hasSave =
            SaveManager.Instance != null &&
            SaveManager.Instance.HasSaveFile();

        continueButton.interactable = hasSave;
    }
}