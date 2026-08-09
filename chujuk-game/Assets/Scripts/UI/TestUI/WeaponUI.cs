using TMPro;
using UnityEngine;

public class WeaponUI : MonoBehaviour
{
    public PlayerController playerController;
    public TMP_Text weaponText;

    private void Start()
    {
        playerController.OnWeaponChanged += UpdateWeaponUI;

        // 시작 시 한 번 표시
        UpdateWeaponUI();
    }

    private void OnDestroy()
    {
        if (playerController != null)
            playerController.OnWeaponChanged -= UpdateWeaponUI;
    }

    private void UpdateWeaponUI()
    {
        bool isGun = playerController.IsUsingGun;

        weaponText.text = isGun ? "Weapon : Gun" : "Weapon : Sword";
    }
}