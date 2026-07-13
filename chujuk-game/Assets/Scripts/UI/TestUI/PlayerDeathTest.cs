using UnityEngine;

public class PlayerDeathTest : MonoBehaviour
{
    private HealthManager healthManager;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            healthManager.TakeDamage(9999);
        }
    }
}