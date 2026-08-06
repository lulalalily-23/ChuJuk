using UnityEngine;
using System.Collections.Generic;

// 슬라임 보스의 페이즈별 스킬 순환 관리.
// Phase3(분열 후)는 낙하 패턴 하나만 반복하고, Phase1/Phase2는 후보 중 무작위 선택.
public class SlimePatternManager : MonoBehaviour
{
    public float decisionInterval = 1.5f;

    [Header("스킬 연결")]
    public GolemSkill_Charge charge;               // Phase1 - 기존 돌진 재활용
    public SlimeSkill_Missile missile;             // Phase1/Phase2 - 원거리 점액 공격
    public SlimeSkill_RandomCharge randomCharge;   // Phase2 - 무작위 방향 돌진 + 장판
    public SlimeSkill_FallAttack fallAttack;       // Phase3(분열 후) - 낙하 공격 + 소환

    private SlimeController controller;
    private float lastDecisionTime;
    private bool isPatternRunning = false;

    void Start()
    {
        controller = GetComponent<SlimeController>();

        // Time.time 기준 절대값(0)에서 시작하면, 게임이 이미 한참 진행된 뒤에
        // 스폰된 오브젝트(분열 자식 등)는 스폰 즉시 첫 스킬이 나가버림.
        // 스폰된 "지금" 시점부터 decisionInterval만큼은 대기하도록 초기화.
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

            // 형제가 지금 낙하공격 중이 아니면 평소대로 낙하공격 시도
            if (!siblingBusy && fallAttack != null && fallAttack.CanUse())
            {
                fallAttack.Execute();
                isPatternRunning = true;
                return;
            }

            // 형제가 바쁘거나(또는 내 낙하공격이 아직 쿨타임이면), 그냥 서있지 않고 소환수라도 소환
            // 즉시 끝나는 짧은 액션이라 isPatternRunning은 걸지 않음 - 다음 decisionInterval에 바로 재판단
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
                candidates.Add(() => missile.Execute(4));
        }
        else
        {
            // Phase1: 기존 돌진 + 미사일(2발)
            if (charge != null && charge.CanUse())
                candidates.Add(() => charge.Execute());

            if (missile != null && missile.CanUse())
                candidates.Add(() => missile.Execute(2));
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