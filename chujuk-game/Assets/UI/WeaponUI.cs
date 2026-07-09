using TMPro;
using UnityEngine;

public class WeaponUI : MonoBehaviour
{
    public PlayerWeapon playerWeapon;
    public TMP_Text weaponText;

    private void Start()
    {
        playerWeapon.OnWeaponChanged += UpdateWeaponUI;
        UpdateWeaponUI(playerWeapon.currentWeaponName);
    }

    private void UpdateWeaponUI(string weaponName)
    {
        weaponText.text = "Weapon: " + weaponName;
    }
}