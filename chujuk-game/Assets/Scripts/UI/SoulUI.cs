using TMPro;
using UnityEngine;

public class SoulUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text soulText;

    [Header("표시 설정")]
    [SerializeField]
    private string prefix = "";

    private GameManager boundGameManager;

    private void OnEnable()
    {
        TryBindGameManager();
    }

    private void Start()
    {
        // 실행 순서 때문에 OnEnable 시점에
        // GameManager가 없었던 경우 다시 시도
        TryBindGameManager();
    }

    private void OnDisable()
    {
        UnbindGameManager();
    }

    private void TryBindGameManager()
    {
        if (boundGameManager != null)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning(
                "SoulUI가 GameManager.Instance를 찾지 못했습니다.",
                this
            );

            return;
        }

        boundGameManager = GameManager.Instance;

        boundGameManager.OnSoulChanged +=
            UpdateSoulText;

        // 이벤트를 기다리지 않고 현재 값을 즉시 표시
        UpdateSoulText(boundGameManager.Soul);
    }

    private void UnbindGameManager()
    {
        if (boundGameManager == null)
            return;

        boundGameManager.OnSoulChanged -=
            UpdateSoulText;

        boundGameManager = null;
    }

    private void UpdateSoulText(int soul)
    {
        if (soulText == null)
        {
            Debug.LogError(
                "SoulUI에 Soul Text가 연결되지 않았습니다.",
                this
            );

            return;
        }

        soulText.text = $"{prefix}{soul:N0}";
    }
}