using UnityEngine;
using System.Collections.Generic;

// 골렘 페이즈에 따라 사용 가능한 스킬 목록에서 하나를 랜덤으로 골라 실행.
// 각 스킬의 쿨타임, 사거리를 충족하는 스킬들 기준.
// 현재 페이즈의 사용 가능한 스킬을 리스트로 묶어 모든 스킬을 한 바퀴 다 쓸 수 있게 수정함
public class GolemPatternManager : MonoBehaviour
{
    public float decisionInterval = 1.5f; 
    public float phase3IntervalMultiplier = 0.6f;  

    public GolemSkill_Charge charge;
    public GolemSkill_Slam slam;
    public GolemSkill_WideSlam wideSlam;
    public GolemSkill_Roar roar;
    public GolemSkill_Laser laser;

    private GolemController golemController;
    private float lastDecisionTime;
    private bool isPatternRunning = false;   
    private HashSet<object> usedSkillsThisCycle = new HashSet<object>();
    private object lastUsedSkill = null;

    void Start()
    {
        golemController = GetComponent<GolemController>();
    }

    void Update()
    {
        if (isPatternRunning) return;   // 이미 다른 패턴 실행 중이면 새 패턴을 고르지 않음

        float interval = golemController.IsPhase3 ? decisionInterval * phase3IntervalMultiplier : decisionInterval;

        if (Time.time - lastDecisionTime < interval) return;
        lastDecisionTime = Time.time;

        TryExecuteRandomSkill();
    }

    void TryExecuteRandomSkill()
    {
        // 현재 페이즈에서 CanUse()(쿨타임/사거리)를 통과한 스킬들만 후보로 모음
        var candidates = new List<(object skill, System.Action execute)>();

        if (golemController.IsPhase3)
        {
            // 3페이즈: 울부짖음, 레이저, 돌진
            if (roar.CanUse()) candidates.Add((roar, () => roar.Execute()));
            if (laser.CanUse()) candidates.Add((laser, () => laser.Execute()));
            if (charge.CanUse()) candidates.Add((charge, () => charge.Execute()));
        }
        else if (golemController.IsPhase2)
        {
            // 2페이즈: 넓은 범위 찍기, 돌진
            if (wideSlam.CanUse()) candidates.Add((wideSlam, () => wideSlam.Execute()));
            if (charge.CanUse()) candidates.Add((charge, () => charge.Execute()));
        }
        else
        {
            // 1페이즈: 내려찍기, 돌진 중 사용 가능한 것
            if (slam.CanUse()) candidates.Add((slam, () => slam.Execute()));
            if (charge.CanUse()) candidates.Add((charge, () => charge.Execute()));
        }

        if (candidates.Count == 0) return;   // 쓸 수 있는 스킬이 없으면 대기

        // 이번 세트에서 아직 안 쓴 스킬만 후보로 좁힘
        var notUsedYet = candidates.FindAll(c => !usedSkillsThisCycle.Contains(c.skill));

        // 이번 세트의 모든 스킬을 다 썼다면 자해딜 및 세트 초기화
        if (notUsedYet.Count == 0)
        {
            golemController.ApplySelfDamage();
            usedSkillsThisCycle.Clear();
            notUsedYet = candidates;   // 새 세트 시작
        }

        // 완전히 같은 스킬 연속 방지
        var filtered = notUsedYet.FindAll(c => c.skill != lastUsedSkill);
        var pool = filtered.Count > 0 ? filtered : notUsedYet;   // 필터링해서 아무것도 안 남으면 필터 없이 진행

        int randomIndex = Random.Range(0, pool.Count);
        pool[randomIndex].execute.Invoke();

        lastUsedSkill = pool[randomIndex].skill;
        usedSkillsThisCycle.Add(pool[randomIndex].skill);

        isPatternRunning = true; // 실행 시작했으니 다음 패턴 선택을 막음
    }

    public void NotifyPatternFinished()   
    {
        isPatternRunning = false;
    }
}