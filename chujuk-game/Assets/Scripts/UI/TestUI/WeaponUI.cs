using TMPro;
using UnityEngine;

public class WeaponUI : MonoBehaviour
{
    public PlayerController playerController;
    public TMP_Text weaponText;

    private void Start()
    {
        playerController.OnWeaponChanged += UpdateWeaponUI;
        UpdateWeaponUI(playerController.IsUsingGun);   
    }

    private void OnDestroy()
    {
        if (playerController != null)
            playerController.OnWeaponChanged -= UpdateWeaponUI;
    }

    private void UpdateWeaponUI(bool isGun)   
    {
        weaponText.text = isGun ? "Weapon : Gun" : "Weapon : Sword";
    }
}