using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [Header("Reference")]
    public PlayerController playerController;
    public Image weaponImage;

    [Header("Weapon Sprites")]
    public Sprite swordSprite;
    public Sprite gunSprite;

    private void Awake()
    {
        if (weaponImage == null)
        {
            weaponImage = GetComponent<Image>();
        }
    }

    private void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("WeaponUI에 PlayerController가 연결되지 않았습니다.");
            return;
        }

        if (weaponImage == null)
        {
            Debug.LogError("WeaponUI에 Image가 연결되지 않았습니다.");
            return;
        }

        playerController.OnWeaponChanged += UpdateWeaponUI;

        // 시작 시 현재 무기 상태 반영
        UpdateWeaponUI(playerController.IsUsingGun);
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnWeaponChanged -= UpdateWeaponUI;
        }
    }

    private void UpdateWeaponUI(bool isGun)   
    {
        weaponImage.sprite = isGun ? gunSprite : swordSprite;
    }
}