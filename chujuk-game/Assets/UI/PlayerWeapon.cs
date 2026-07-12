using System;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public string currentWeaponName = "sword";

    public event Action<string> OnWeaponChanged;

    private void Start()
    {
        OnWeaponChanged?.Invoke(currentWeaponName);
    }

    public void ChangeWeapon(string weaponName)
    {
        currentWeaponName = weaponName;
        OnWeaponChanged?.Invoke(currentWeaponName);
    }
}