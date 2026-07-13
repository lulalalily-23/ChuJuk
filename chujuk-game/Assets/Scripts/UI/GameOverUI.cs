using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    private bool isSubscribed;

    private void Start()
    {
        // 게임 시작 시 게임오버 화면 숨기기
        gameOverPanel.SetActive(false);

        SubscribeGameManager();
    }

    private void SubscribeGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager를 찾을 수 없습니다.");
            return;
        }

        GameManager.Instance.OnGameOver += ShowGameOver;
        isSubscribed = true;
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    // RestartButton의 OnClick에 연결할 함수
    public void RestartGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager를 찾을 수 없습니다.");
            return;
        }

        GameManager.Instance.RestartGame();
    }

    private void OnDestroy()
    {
        if (!isSubscribed || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnGameOver -= ShowGameOver;
    }
}