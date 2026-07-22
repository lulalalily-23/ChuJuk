using UnityEngine;

//골렘의 체력에따른 페이즈 관리와 한 패턴 세트가 끝날때마다 5%의 데미지를 입도록 함
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

    // 스킬 하나가 끝날 때마다 각 GolemSkill에서 호출됨.
    // GolemPatternManager에게 다음 패턴 선택 및 세트 완료 여부 판단을 맡김.
    public void OnPatternExecuted()
    {
        patternManager.NotifyPatternFinished();
    }

    // GolemPatternManager가 "스킬 세트를 한 바퀴 다 썼다"고 판단했을 때만 호출.
    // 최대체력(고정값) 기준 5% 고정 데미지 
    public void ApplySelfDamage()
    {
        int selfDamage = Mathf.RoundToInt(healthManager.data.maxHealth * 0.05f);
        healthManager.TakeDamage(selfDamage);
    }
}
