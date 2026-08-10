using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField]
    private GameObject pausePanel;

    [Header("Other UI")]
    [SerializeField]
    private InventoryUIController inventoryUI;

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
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (GameManager.Instance == null)
            return;

        // GameOver에서는 ESC 무시
        if (GameManager.Instance.State ==
            GameManager.GameState.GameOver)
        {
            return;
        }

        // 1순위 : 인벤토리 닫기
        if (inventoryUI != null &&
            inventoryUI.IsOpen)
        {
            inventoryUI.CloseInventory();
            return;
        }

        // 2순위 : Pause가 열려 있으면 닫기
        if (GameManager.Instance.State ==
            GameManager.GameState.Paused)
        {
            ResumeGame();
            return;
        }

        // 3순위 : 일반 상태에서 Pause 열기
        if (GameManager.Instance.State ==
            GameManager.GameState.Playing)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (GameManager.Instance == null)
            return;

        // Playing 상태에서만 일시정지 가능
        if (GameManager.Instance.State !=
            GameManager.GameState.Playing)
        {
            return;
        }

        // 혹시 인벤토리가 열려 있으면 먼저 닫음
        if (inventoryUI != null &&
            inventoryUI.IsOpen)
        {
            inventoryUI.CloseInventory();
        }

        GameManager.Instance.Pause();

        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.State !=
            GameManager.GameState.Paused)
        {
            return;
        }

        pausePanel.SetActive(false);

        GameManager.Instance.Resume();
    }

    public void RestartGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "GameManager를 찾을 수 없습니다."
            );

            return;
        }

        pausePanel.SetActive(false);

        GameManager.Instance.RestartGame();
    }

    // 저장 후 메인 타이틀로 이동
    public void SaveAndQuitToTitle()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "[PauseUI] SaveManager를 찾을 수 없습니다."
            );

            return;
        }

        // 현재 플레이 기록 저장
        SaveManager.Instance.SaveGame();

        // 일시정지 상태 해제
        if (GameManager.Instance != null &&
            GameManager.Instance.State ==
            GameManager.GameState.Paused)
        {
            GameManager.Instance.Resume();
        }
        else
        {
            Time.timeScale = 1f;
        }

        pausePanel.SetActive(false);

        // 메인 타이틀 이동
        SceneManager.LoadScene("MainTitle");
    }
}