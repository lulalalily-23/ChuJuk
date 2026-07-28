using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthManager targetHealth;
    [SerializeField] private Slider hpSlider;

    private void Awake()
    {
        if (hpSlider == null)
        {
            hpSlider = GetComponent<Slider>();
        }

        if (targetHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                targetHealth = player.GetComponent<HealthManager>();
            }
        }
    }

    private void OnEnable()
    {
        if (targetHealth == null)
        {
            Debug.LogError(
                "HealthUI가 Player의 HealthManager를 찾지 못했습니다.",
                gameObject
            );

            return;
        }

        if (hpSlider == null)
        {
            Debug.LogError(
                "HealthUI가 Slider를 찾지 못했습니다.",
                gameObject
            );

            return;
        }

        targetHealth.OnHealthChanged += UpdateHpBar;
    }

    private void Start()
    {
        if (targetHealth == null || hpSlider == null)
        {
            return;
        }

        if (targetHealth.data == null)
        {
            Debug.LogError(
                "HealthManager에 CharacterData가 연결되지 않았습니다.",
                targetHealth.gameObject
            );

            return;
        }

        hpSlider.minValue = 0f;
        hpSlider.maxValue = 1f;

        UpdateHpBar(
            targetHealth.currentHealth,
            targetHealth.data.maxHealth
        );
    }

    private void UpdateHpBar(int currentHp, int maxHp)
    {
        if (hpSlider == null)
        {
            Debug.LogError("UpdateHpBar: Slider가 없습니다.");
            return;
        }

        if (maxHp <= 0)
        {
            Debug.LogError($"UpdateHpBar: 최대 체력이 잘못됐습니다. maxHp = {maxHp}");
            return;
        }

        float hpRatio = (float)currentHp / maxHp;

        hpSlider.value = hpRatio;

        Debug.Log(
            $"HP Bar 갱신: {currentHp}/{maxHp}, Slider Value: {hpSlider.value}",
            gameObject
        );
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateHpBar;
        }
    }
}