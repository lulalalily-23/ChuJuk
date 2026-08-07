using UnityEngine;
using System.Collections.Generic;

// 슬라임 보스의 페이즈별 스킬 순환 관리.
public class SlimePatternManager : MonoBehaviour
{
    public float decisionInterval = 1.5f;

    [Header("스킬 연결")]
    public GolemSkill_Charge charge;               // Phase1 - 기존 돌진 재활용
    public SlimeSkill_Missile missile;             // Phase1/Phase2 - 원거리 점액 공격
    public SlimeSkill_RandomCharge randomCharge;   // Phase2 - 무작위 방향 돌진 + 장판
    public SlimeSkill_FallAttack fallAttack;       // Phase3 - 낙하 공격 + 소환

    private SlimeController controller;
    private float lastDecisionTime;
    private bool isPatternRunning = false;

    void Start()
    {
        controller = GetComponent<SlimeController>();
        lastDecisionTime = Time.time;
    }

    void Update()
    {
        if (isPatternRunning) return;
        if (Time.time - lastDecisionTime < decisionInterval) return;
        lastDecisionTime = Time.time;

        TryExecuteSkill();
    }

    void TryExecuteSkill()
    {
        if (controller.IsPhase3)
        {
            bool siblingBusy = controller.siblingChild != null && controller.siblingChild.isPerformingFallAttack;

            if (!siblingBusy && fallAttack != null && fallAttack.CanUse())
            {
                fallAttack.Execute();
                isPatternRunning = true;
                return;
            }

            if (fallAttack != null && fallAttack.CanQuickSummon())
            {
                fallAttack.ExecuteQuickSummon();
            }

            return;
        }

        var candidates = new List<System.Action>();

        if (controller.IsPhase2)
        {
            // Phase2: 무작위 돌진 + 미사일 2배(4발)
            if (randomCharge != null && randomCharge.CanUse())
                candidates.Add(() => randomCharge.Execute());

            if (missile != null && missile.CanUse())
                candidates.Add(() => missile.Execute(8));
        }
        else
        {
            // Phase1: 돌진 + 미사일(2발)
            if (charge != null && charge.CanUse())
                candidates.Add(() => charge.Execute());

            if (missile != null && missile.CanUse())
                candidates.Add(() => missile.Execute(4));
        }

        if (candidates.Count == 0) return;

        int index = Random.Range(0, candidates.Count);
        candidates[index].Invoke();
        isPatternRunning = true;
    }

    public void NotifyPatternFinished()
    {
        isPatternRunning = false;
    }
}