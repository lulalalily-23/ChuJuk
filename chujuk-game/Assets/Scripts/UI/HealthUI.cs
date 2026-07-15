using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public HealthManager targetHealth; // 인스펙터에서 플레이어/몬스터의 HealthManager를 연결
    public Slider hpSlider;

    private void Start()
    {
        if (targetHealth != null)
        {
            // 이벤트 구독
            targetHealth.OnHealthChanged += UpdateHpBar;
            // 초기값 설정
            UpdateHpBar(targetHealth.currentHealth, targetHealth.data.maxHealth);
        }
    }

    private void UpdateHpBar(int currentHp, int maxHp)
    {
        if (hpSlider != null)
        {
            hpSlider.value = (float)currentHp / maxHp;
        }
    }

    private void OnDestroy()
    {
        // 구독 해제
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateHpBar;
        }
    }
}