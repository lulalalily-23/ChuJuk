using UnityEngine;
using System.Collections.Generic;

// 골렘 페이즈에 따라 사용 가능한 스킬 목록에서 하나를 랜덤으로 골라 실행.
// 각 스킬의 쿨타임, 사거리를 충족하는 스킬들 기준.
// 한 번에 하나의 패턴만 실행되도록 isPatternRunning으로 중복 실행을 방지.

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

    void Start()
    {
        golemController = GetComponent<GolemController>();
    }

    void Update()
    {
        if (isPatternRunning) return; // 이미 다른 패턴 실행 중이면 새 패턴을 고르지 않음

        float interval = golemController.IsPhase3 ? decisionInterval * phase3IntervalMultiplier : decisionInterval;

        if (Time.time - lastDecisionTime < interval) return;
        lastDecisionTime = Time.time;

        TryExecuteRandomSkill();
    }

    // 현재 페이즈에 맞는 스킬 목록 중, CanUse를 통과한 것들만 후보로 모아 무작위 실행
    void TryExecuteRandomSkill()
    {
        List<System.Action> availableSkills = new List<System.Action>();

        if (golemController.IsPhase3)
        {
            // 3페이즈: 울부짖음, 레이저, 돌진
            if (roar.CanUse()) availableSkills.Add(() => roar.Execute());
            if (laser.CanUse()) availableSkills.Add(() => laser.Execute());
            if (charge.CanUse()) availableSkills.Add(() => charge.Execute());
        }
        else if (golemController.IsPhase2)
        {
            // 2페이즈: 넓은 범위 찍기, 돌진
            if (wideSlam.CanUse()) availableSkills.Add(() => wideSlam.Execute());
            if (charge.CanUse()) availableSkills.Add(() => charge.Execute());
        }
        else
        {
            // 1페이즈: 내려찍기, 돌진 중 사용 가능한 것
            if (slam.CanUse()) availableSkills.Add(() => slam.Execute());
            if (charge.CanUse()) availableSkills.Add(() => charge.Execute());
        }

        if (availableSkills.Count == 0) return; //사용가능한 스킬 없으면 대기

        int randomIndex = Random.Range(0, availableSkills.Count);
        availableSkills[randomIndex].Invoke();

        isPatternRunning = true; // 실행 시작했으니 다음 패턴 선택을 막음
    }

    public void NotifyPatternFinished()   
    {
        isPatternRunning = false;
    }
}