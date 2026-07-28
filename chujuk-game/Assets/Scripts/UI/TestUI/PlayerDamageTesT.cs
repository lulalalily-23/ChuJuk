using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDamageTesT : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private HealthManager targetHealth;

    [Header("Test")]
    [SerializeField] private int damageAmount = 10;

    private void Awake()
    {
        if (targetHealth == null)
        {
            targetHealth = GetComponent<HealthManager>();
        }

        if (targetHealth == null)
        {
            Debug.LogError(
                "PlayerDamageTest가 HealthManager를 찾지 못했습니다.",
                gameObject
            );
        }
    }

    private void Update()
    {
        if (targetHealth == null)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        // K 키: 데미지
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            Debug.Log($"테스트 데미지 {damageAmount} 적용");

            targetHealth.TakeDamage(damageAmount);
        }

        // L 키: 회복
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log($"테스트 회복 {damageAmount} 적용");

            targetHealth.Heal(damageAmount);
        }
    }
}