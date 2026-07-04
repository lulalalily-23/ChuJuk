using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 게임 상태
    public enum GameState
    {
        Playing,
        GameOver,
        Paused
    }

    public GameState State { get; private set; }

    // 재화 (영혼)
    public int Soul { get; private set; }

    public event Action<int> OnSoulChanged;
    public event Action OnGameOver;
    public event Action OnGameRestart;

    // 초기화
    private void Awake()
    {
        // 싱글톤
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬 재로드 시 이전 씬 오브젝트들과의 이벤트 연결 누수를 막기 위함
        SceneManager.sceneLoaded += OnSceneLoaded;

        State = GameState.Playing;
    }

    private void Start()
    {
        ResetGameSession();
    }

    private void ResetGameSession()
    {
        Soul = 0;
        OnSoulChanged?.Invoke(Soul);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearEvents();
        OnSoulChanged?.Invoke(Soul);
    }

    // Soul 시스템
    public void AddSoul(int amount)
    {
        if (amount <= 0) return;

        Soul += amount;
        OnSoulChanged?.Invoke(Soul);
    }

    public bool UseSoul(int amount)
    {
        if (amount <= 0 || Soul < amount) return false;

        Soul -= amount;
        OnSoulChanged?.Invoke(Soul);
        return true;
    }

    // 게임 상태 제어
    public void SetGameOver()
    {
        if (State == GameState.GameOver) return;

        State = GameState.GameOver;
        OnGameOver?.Invoke();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;

        ResetGameSession();
        OnGameRestart?.Invoke();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    private void ClearEvents()
    {
        OnSoulChanged = null;
        OnGameOver = null;
        OnGameRestart = null;
    }

    // 안전 정리
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ClearEvents();
    }
}