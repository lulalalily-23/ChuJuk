using UnityEngine;
using System.Collections;

public class PlayerActiveSkill : MonoBehaviour
{
    public static PlayerActiveSkill Instance { get; private set; }

    // 초기화

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 액티브 스킬 실행

    public void Activate()
    {
        Debug.Log(
            "[PlayerActiveSkill] 액티브 스킬 실행"
        );


        if (SetSystem.Instance == null)
        {
            Debug.LogWarning(
                "[PlayerActiveSkill] SetSystem.Instance가 없습니다."
            );

            return;
        }

        // 현재 세트 개수 확인

        int toughCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Tough
            );

        int rushCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Rush
            );

        int ambushCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Ambush
            );

        int savageryCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Savagery
            );

        int snipingCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Sniping
            );

        int treasureCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Treasure
            );

        int relicCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Relic
            );

        int swiftnessCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Swiftness
            );

        int headHunterCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.HeadHunter
            );

        int explosionCount =
            SetSystem.Instance.GetSetCount(
                ItemTag.Explosion
            );

        // 돌진 6세트

        if (rushCount >= 6)
        {
            ActivateRush();
        }

        // 매복 6세트

        if (ambushCount >= 6)
        {
            ActivateAmbush();
        }

        // 흉포 6세트

        if (savageryCount >= 6)
        {
            ActivateSavagery();
        }

        // 저격 6세트

        if (snipingCount >= 6)
        {
            ActivateSniping();
        }

        // 보화 6세트

        if (treasureCount >= 6)
        {
            ActivateTreasure();
        }


        // 유물 6세트

        if (relicCount >= 6)
        {
            ActivateRelic();
        }

        // 신속 6세트

        if (swiftnessCount >= 6)
        {
            ActivateSwiftness();
        }

        // 헤드헌터 6세트

        if (headHunterCount >= 6)
        {
            ActivateHeadHunter();
        }

        // 폭발 6세트

        if (explosionCount >= 6)
        {
            ActivateExplosion();
        }
    }

    // 돌진 6세트

    private void ActivateRush()
    {
        Debug.Log(
            "[액티브] 돌진 6세트 → 5초간 무적"
        );


        HealthManager healthManager =
            GetComponent<HealthManager>();


        if (healthManager == null)
        {
            healthManager =
                FindAnyObjectByType<HealthManager>();
        }


        if (healthManager == null)
        {
            Debug.LogWarning(
                "[PlayerActiveSkill] HealthManager를 찾을 수 없습니다."
            );

            return;
        }


        healthManager.StartActiveInvincibility(
            5f
        );
    }

    // 매복 6세트

    private void ActivateAmbush()
    {
        Debug.Log(
            "[액티브] 매복 6세트 → 다음 원거리 공격 데미지 +300%"
        );


        if (PlayerStat.Instance == null)
        {
            Debug.LogWarning(
                "[PlayerActiveSkill] PlayerStat.Instance가 없습니다."
            );

            return;
        }

        // 매복 액티브 버프 활성화

        bool activated =
            PlayerStat.Instance.ActivateAmbushActive();


        if (!activated)
        {
            Debug.Log(
                "[액티브] 매복 6세트 → 활성화 실패"
            );

            return;
        }


        Debug.Log(
            "[액티브] 매복 6세트 → 활성화 성공"
        );
    }

    // 흉포 6세트

    private void ActivateSavagery()
    {
        Debug.Log(
            "[액티브] 흉포 6세트 → 5초간 공격속도 +70%"
        );


        if (PlayerStat.Instance == null)
            return;


        PlayerStat.Instance.AddRuntimeStat(
            StatType.AttackSpeed,
            ModifierType.Percent,
            0.70f
        );


        StartCoroutine(
            RemoveSavageryBonus()
        );
    }


    // 흉포 공격속도 보너스 제거

    private IEnumerator RemoveSavageryBonus()
    {
        yield return new WaitForSeconds(
            5f
        );


        if (PlayerStat.Instance == null)
            yield break;


        PlayerStat.Instance.AddRuntimeStat(
            StatType.AttackSpeed,
            ModifierType.Percent,
            -0.70f
        );


        Debug.Log(
            "[액티브] 흉포 6세트 → 공격속도 +70% 종료"
        );
    }

    // 저격 6세트

    private void ActivateSniping()
    {
        Debug.Log(
            "[액티브] 저격 6세트 → 머신건 생성"
        );


        // 실제 머신건 프리팹 및 발사 시스템 연결 필요
    }


    // 보화 6세트

    private void ActivateTreasure()
    {
        Debug.Log(
            "[액티브] 보화 6세트 → 재화 300 소모 시도"
        );


        if (GameManager.Instance == null)
        {
            Debug.LogWarning(
                "[PlayerActiveSkill] GameManager.Instance가 없습니다."
            );

            return;
        }


        bool success =
            GameManager.Instance.UseSoul(
                300
            );


        if (!success)
        {
            Debug.Log(
                "[액티브] 보화 6세트 → 재화 부족"
            );

            return;
        }


        Debug.Log(
            "[액티브] 보화 6세트 → 재화 300 소모 성공 / 폭발 실행 예정"
        );


        // 실제 폭발 공격 시스템 연결 필요
    }

    // 유물 6세트

    private void ActivateRelic()
    {
        Debug.Log(
            "[액티브] 유물 6세트 → 과거의 인물 변신 효과 실행 예정"
        );


        // 유물 강화 시스템과 연결 필요
    }

    // 신속 6세트

    private void ActivateSwiftness()
    {
        Debug.Log(
            "[액티브] 신속 6세트 → 대쉬 강화 효과 확인"
        );


        // 대쉬 시스템과 연결 필요
    }

    // 헤드헌터 6세트


    private void ActivateHeadHunter()
    {
        Debug.Log(
            "[액티브] 헤드헌터 6세트 → 초과 치명타 확률을 치명타 데미지로 전환"
        );


        if (PlayerStat.Instance == null)
            return;


        float criticalChance =
            PlayerStat.Instance.GetStat(
                StatType.CriticalChance
            );


        // 치명타 확률 100%를 초과한 부분만 계산
        float excess =
            Mathf.Max(
                0f,
                criticalChance - 1f
            );


        if (excess <= 0f)
        {
            Debug.Log(
                "[헤드헌터] 초과 치명타 확률 없음"
            );

            return;
        }


        PlayerStat.Instance.AddRuntimeStat(
            StatType.CriticalDamage,
            ModifierType.Percent,
            excess
        );


        Debug.Log(
            "[헤드헌터] 초과 치명타 확률 " +
            excess +
            " → 치명타 데미지 보너스로 적용"
        );
    }

    // 폭발 6세트

    private void ActivateExplosion()
    {
        Debug.Log(
            "[액티브] 폭발 6세트 → 5초 후 폭발하는 폭탄 투척"
        );


        // 실제 폭탄 프리팹 및 폭발 시스템 연결 필요
    }
}