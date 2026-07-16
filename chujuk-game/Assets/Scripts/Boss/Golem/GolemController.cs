using UnityEngine;

//골렘의 체력에따른 페이즈 관리와 한 패턴이 끝날때마다 5%의 데미지를 입도록 함
public class GolemController : MonoBehaviour
{
    enum BossPhase {Phase1, Phase2, Phase3};
    private HealthManager healthManager;
    BossPhase currentPhase = BossPhase.Phase1;
    float healthPercent;

    [Tooltip("Phase2 전환 기준 체력 비율")]
    float Phase2Threshold = 0.8f;
    [Tooltip("Phase3 전환 기준 체력 비율")]
    float Phase3Threshold = 0.5f;
    public bool IsPhase2 => currentPhase == BossPhase.Phase2;
    public bool IsPhase3 => currentPhase == BossPhase.Phase3;

    public HealthManager MainHealth => healthManager;
    public GolemPatternManager patternManager;

    void Start()
    {
        healthManager = GetComponent<HealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        healthPercent = (float)healthManager.currentHealth / (float)healthManager.data.maxHealth;
        if (healthPercent <= Phase3Threshold) {
            currentPhase = BossPhase.Phase3;
        }
        else if (healthPercent <= Phase2Threshold) {
            currentPhase = BossPhase.Phase2;
        }
        else {
            currentPhase = BossPhase.Phase1;
        }
    }

    public void OnPatternExecuted()
    {
        // 패턴(스킬) 하나가 끝날 때마다 호출됨.
        // 현재체력의 5%씩 자해딜 들어감.
        int selfDamage = Mathf.RoundToInt(healthManager.currentHealth * 0.05f);
        healthManager.TakeDamage(selfDamage);
        patternManager.NotifyPatternFinished();
    }
}
