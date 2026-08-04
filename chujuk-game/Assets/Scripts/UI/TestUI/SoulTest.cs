using UnityEngine;
using UnityEngine.InputSystem;

public class SoulTest : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "SoulTest: GameManager.Instance가 null입니다.",
                this
            );

            return;
        }

        Debug.Log(
            $"SoulTest 연결 성공 / 현재 Soul: " +
            $"{GameManager.Instance.Soul}",
            this
        );
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError(
                    "F1 입력은 됐지만 GameManager.Instance가 null입니다.",
                    this
                );

                return;
            }

            int beforeSoul =
                GameManager.Instance.Soul;

            GameManager.Instance.AddSoul(100);

            Debug.Log(
                $"Soul 증가 테스트: " +
                $"{beforeSoul} → {GameManager.Instance.Soul}"
            );
        }

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            if (GameManager.Instance == null)
                return;

            bool success =
                GameManager.Instance.UseSoul(50);

            Debug.Log(
                success
                    ? $"Soul 50 사용 / 현재: {GameManager.Instance.Soul}"
                    : "Soul 사용 실패"
            );
        }
    }
}