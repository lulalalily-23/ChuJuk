using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider hpSlider;

    private void Start()
    {
        playerHealth.OnHealthChanged += UpdateHpBar;
        UpdateHpBar(playerHealth.currentHp, playerHealth.maxHp);
    }

    private void UpdateHpBar(int currentHp, int maxHp)
    {
        hpSlider.value = (float)currentHp / maxHp;
    }
}