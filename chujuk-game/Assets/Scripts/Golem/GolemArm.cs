using UnityEngine;
// 골렘 팔 부위 전용 스크립트.
// 1-2페이즈 - 팔이 맞아도 본체(GolemController.MainHealth) 체력이 깎임, 팔 자체는 파괴되지 않음.
// 3페이즈 - 팔 자신의 체력(healthManager)이 깎이고, 0이 되면 파괴됨.

public class GolemArm : MonoBehaviour
{
    private HealthManager healthManager;    
    private GolemController golemController;
    
    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        golemController = GetComponentInParent<GolemController>();
        healthManager.OnDeath += HandleArmDestroyed; // 팔 체력이 0이 되면 HandleArmDestroyed 호출
    }

    public void OnHit(int damage) {
        if (golemController.IsPhase3) {
            healthManager.TakeDamage(damage); // 3페이즈: 팔 자체 체력 차감
        }
        else {
            golemController.MainHealth.TakeDamage(damage); // 1~2페이즈: 본체 체력 차감
        }
    }

    void HandleArmDestroyed() {
        gameObject.SetActive(false);
        golemController.PlayBrokenArm();
    }
}
