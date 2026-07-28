using UnityEngine;
using UnityEngine.InputSystem;

public class PauseUI : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;

    private void Awake()
    {
        if (pausePanel == null)
        {
            Debug.LogError(
                "PauseUI에 PausePanel이 연결되지 않았습니다.",
                this
            );

            return;
        }

        pausePanel.SetActive(false);
        isPaused = false;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        // 일시정지 상태에서 씬 이동하기 전에 시간 복구
        Time.timeScale = 1f;

        isPaused = false;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "GameManager를 찾을 수 없습니다."
            );

            return;
        }

        GameManager.Instance.RestartGame();
    }
}