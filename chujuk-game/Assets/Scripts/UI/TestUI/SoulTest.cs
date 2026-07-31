using UnityEngine;
using UnityEngine.InputSystem;

public class SoulTest : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit9Key
            .wasPressedThisFrame)
        {
            GameManager.Instance?.AddSoul(10);
        }

        if (Keyboard.current.digit0Key
            .wasPressedThisFrame)
        {
            bool success =
                GameManager.Instance != null &&
                GameManager.Instance.UseSoul(5);

            Debug.Log(
                success
                    ? "Soul 5 사용 성공"
                    : "Soul이 부족합니다."
            );
        }
    }
}