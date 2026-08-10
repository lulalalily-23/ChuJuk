using System.Collections;
using UnityEngine;

public class HazardZone : MonoBehaviour
{
    public enum HazardType
    {
        DamageOnly,
        FallAndRespawn
    }

    [Header("Hazard Settings")]
    [SerializeField] private HazardType hazardType;

    [SerializeField]
    [Min(0)]
    private int damageAmount = 10;

    [Header("Damage Cooldown")]
    [SerializeField]
    [Min(0f)]
    private float damageCooldown = 0.3f;

    private bool canActivate = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canActivate)
        {
            return;
        }

        // 자식 콜라이더에도 대응하기 위해 Rigidbody2D의 오브젝트를 찾음
        GameObject targetObject;

        if (other.attachedRigidbody != null)
        {
            targetObject = other.attachedRigidbody.gameObject;
        }
        else
        {
            targetObject = other.transform.root.gameObject;
        }

        if (!targetObject.CompareTag("Player"))
        {
            return;
        }

        canActivate = false;

        switch (hazardType)
        {
            case HazardType.DamageOnly:
                ApplyNormalDamage(targetObject);
                break;

            case HazardType.FallAndRespawn:
                ApplyFallDamage(targetObject);
                break;
        }

        StartCoroutine(ActivationCooldown());
    }

    private void ApplyNormalDamage(GameObject player)
    {
        HealthManager healthManager =
            player.GetComponent<HealthManager>();

        if (healthManager == null)
        {
            Debug.LogError("Player에서 HealthManager를 찾지 못했습니다.");
            return;
        }

        healthManager.TakeDamage(damageAmount);
    }

    private void ApplyFallDamage(GameObject player)
    {
        PlayerFallHandler fallHandler =
            player.GetComponent<PlayerFallHandler>();

        if (fallHandler == null)
        {
            Debug.LogError("Player에서 PlayerFallHandler를 찾지 못했습니다.");
            return;
        }

        fallHandler.HandleFall(damageAmount);
    }

    private IEnumerator ActivationCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        canActivate = true;
    }
}