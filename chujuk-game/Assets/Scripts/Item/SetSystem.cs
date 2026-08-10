using UnityEngine;
using System.Collections.Generic;

public class SetSystem : MonoBehaviour
{
    public static SetSystem Instance;

    // 현재 적용 중인 세트 개수 저장
    private Dictionary<ItemTag, int> activeSets
        = new Dictionary<ItemTag, int>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    // Inventory에서 호출
    public void UpdateSetEffects(List<ItemTag> tags)
    {
        if (tags == null)
        {
            Debug.LogWarning("[SetSystem] 전달된 태그 목록이 null입니다.");
            return;
        }


        // 기존 세트 능력치 초기화
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.ResetSetStats();
        }


        // 기존 세트 초기화
        activeSets.Clear();


        // 태그 개수 계산
        foreach (ItemTag tag in tags)
        {
            if (tag == ItemTag.None)
                continue;


            if (activeSets.ContainsKey(tag))
                activeSets[tag]++;
            else
                activeSets[tag] = 1;
        }


        // 세트 효과 적용
        foreach (KeyValuePair<ItemTag, int> set in activeSets)
        {
            ApplySetEffect(set.Key, set.Value);
        }


        // 세트가 변경되었으므로 런타임 세트 효과 확인
        ValidateRuntimeEffects();


        // 최대 체력 변경사항 반영
        RefreshPlayerHealth();
    }



    // 세트 변경에 따른 런타임 효과 확인
    private void ValidateRuntimeEffects()
    {
        if (PlayerStat.Instance == null)
            return;


        // 매복 6세트가 사라진 경우
        // 매복 액티브 상태를 제거한다.
        int ambushCount = GetSetCount(ItemTag.Ambush);


        if (ambushCount < 6 &&
            PlayerStat.Instance.IsAmbushActive())
        {
            PlayerStat.Instance.ConsumeAmbushActive();

            Debug.Log(
                "[SetSystem] 매복 6세트 해제 → 매복 액티브 효과 제거"
            );
        }
    }



    private void ApplySetEffect(ItemTag tag, int count)
    {
        if (PlayerStat.Instance == null)
        {
            Debug.LogError("[SetSystem] PlayerStat.Instance가 없습니다.");
            return;
        }


        switch (tag)
        {
            case ItemTag.Tough:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.MaxHP,
                        ModifierType.Percent,
                        0.10f
                    );

                    Debug.Log(
                        "강인 2세트 : 최대 체력 증가 +10%"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.MaxHP,
                        ModifierType.Percent,
                        0.20f
                    );

                    AddSetStat(
                        StatType.DamageReduction,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "강인 4세트 : 피격 데미지 -30%, 최대체력 증가 +20%"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.MaxHP,
                        ModifierType.Percent,
                        0.40f
                    );

                    AddSetStat(
                        StatType.DamageReduction,
                        ModifierType.Percent,
                        0.50f
                    );

                    AddSetStat(
                        StatType.InvincibilityDuration,
                        ModifierType.Flat,
                        0.20f
                    );

                    Debug.Log(
                        "강인 6세트 : 피격 데미지 -50%, 최대체력 증가 +40%, 피격시 무적 시간 증가 +0.2초"
                    );
                }

                break;



            case ItemTag.Rush:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.InvincibilityDuration,
                        ModifierType.Flat,
                        0.10f
                    );

                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "돌진 2세트 : 피격시 무적 시간 증가 +0.1초, 공격력 증가 +20%"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.InvincibilityDuration,
                        ModifierType.Flat,
                        0.10f
                    );

                    AddSetStat(
                        StatType.HealOnKill,
                        ModifierType.Flat,
                        10f
                    );

                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.40f
                    );

                    Debug.Log(
                        "돌진 4세트 : 피격시 무적 시간 증가 +0.1초, 적 처치시 피회복 +10, 공격력 증가 +40%"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.InvincibilityDuration,
                        ModifierType.Flat,
                        0.30f
                    );

                    AddSetStat(
                        StatType.HealOnKill,
                        ModifierType.Flat,
                        20f
                    );

                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.40f
                    );

                    Debug.Log(
                        "돌진 6세트 : 피격시 무적 시간 증가 +0.3초, 적 처치시 피회복 +20, 공격력 증가 +40% 액티브 활성화시 5초간 무적"
                    );
                }

                break;



            case ItemTag.Ambush:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.DashCoolDown,
                        ModifierType.Flat,
                        -0.10f
                    );

                    Debug.Log(
                        "매복 2세트 : 함정 회피 가능, 대쉬 쿨타임 -0.1초"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.DashCoolDown,
                        ModifierType.Flat,
                        -0.10f
                    );

                    AddSetStat(
                        StatType.RangedDamage,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "매복 4세트 : 함정 회피 가능, 대쉬 쿨타임 -0.1초, 원거리 공격 데미지 증가 +30%"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.DashCoolDown,
                        ModifierType.Flat,
                        -0.20f
                    );

                    AddSetStat(
                        StatType.RangedDamage,
                        ModifierType.Percent,
                        0.50f
                    );

                    Debug.Log(
                        "매복 6세트 : 함정 회피 가능, 대쉬 쿨타임 -0.2초, 원거리 공격 데미지 증가 +50%, 액티브 활성화시, 원거리 공격 데미지 +300% 1회"
                    );
                }

                break;



            case ItemTag.Savagery:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.MoveSpeed,
                        ModifierType.Percent,
                        0.05f
                    );

                    AddSetStat(
                        StatType.AttackSpeed,
                        ModifierType.Percent,
                        0.10f
                    );

                    Debug.Log(
                        "흉포 2세트 : 이동속도 +5%, 공격 속도 증가 +10%"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.MoveSpeed,
                        ModifierType.Percent,
                        0.10f
                    );

                    AddSetStat(
                        StatType.AttackSpeed,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "흉포 4세트 : 이동속도 +10%, 공격 속도 증가 +30%, 적 처치시 공격 속도 증가 +20%"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.MoveSpeed,
                        ModifierType.Percent,
                        0.10f
                    );

                    AddSetStat(
                        StatType.AttackSpeed,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "흉포 6세트 : 함정 회피 가능, 대쉬 쿨타임 -0.2초, 원거리 공격 데미지 증가 +50%, 액티브 활성화시, 원거리 공격 데미지 +300% 1회"
                    );
                }

                break;



            case ItemTag.Sniping:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.RangedDamage,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "저격 2세트 : 원거리 데미지 증가 +20%, 패시브로 적을 공격하는 권총 생성"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.RangedDamage,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "저격 4세트 : 원거리 데미지 증가 +30%, 패시브로 적을 공격하는 권총, 기관단총 생성"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.RangedDamage,
                        ModifierType.Percent,
                        0.50f
                    );

                    Debug.Log(
                        "저격 6세트 : 원거리 데미지 증가 +50%, 패시브로 적을 공격하는 권총, 기관단총 생성, 액티브 활성화시 머신건 생성"
                    );
                }

                break;



            case ItemTag.Pursuit:

                if (count >= 2)
                {
                    PlayerStat.Instance.SetChaseMaxStack(4);

                    Debug.Log(
                        "추격 2세트 : 적 공격시 이동속도 증가 +10%, 피격시 이동속도 증가 +20%, 스택당 추가 피해를 스택 생성 (최대 4스택, 스택당 추가 피해량 +10%)"
                    );
                }

                if (count >= 4)
                {
                    PlayerStat.Instance.SetChaseMaxStack(6);

                    Debug.Log(
                        "추격 4세트 : 적 공격시 이동속도 증가 +30%, 피격시 이동속도 증가 +20%, 스택당 추가 피해를 스택 생성 (최대 6스택, 스택당 추가 피해량 +10%)"
                    );
                }

                if (count >= 6)
                {
                    PlayerStat.Instance.SetChaseMaxStack(8);

                    Debug.Log(
                        "추격 6세트 : 적 공격시 이동속도 증가 +30%, 피격시 이동속도 증가 +20%, 스택당 추가 피해를 스택 생성 (최대 8스택, 스택당 추가 피해량 +10%)"
                    );
                }

                break;



            case ItemTag.Treasure:

                if (count >= 2)
                {
                    Debug.Log(
                        "보화 2세트 : 스테이지 진입시 보유한 재화 +10%"
                    );
                }

                if (count >= 4)
                {
                    Debug.Log(
                        "보화 4세트 : 스테이지 진입시 보유한 재화 +30%, 보유한 재화에 비례해 공격력 증가"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "보화 6세트 : 공격력 증가 +20%, 유물 아이템 강화시 공격력 +20% 추가 강화, 6세트 이상 강화시 과거의 인물로 변신하여 모든 능력치 강화 +40%"
                    );
                }

                break;



            case ItemTag.Relic:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "유물 2세트 : 공격력 증가 +20%"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "유물 4세트 : 공격력 증가 +20%, 유물 아이템 강화시 공격력 +20% 추가 강화"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "유물 6세트 : 공격력 증가 +20%, 유물 아이템 강화시 공격력 +20% 추가 강화, 6세트 이상 강화시 과거의 인물로 변신하여 모든 능력치 강화 +40%"
                    );
                }

                break;



            case ItemTag.Swiftness:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.DashCoolDown,
                        ModifierType.Flat,
                        -0.20f
                    );

                    Debug.Log(
                        "신속 2세트 : 대쉬 쿨타임 감소 -0.2초"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.DashCoolDown,
                        ModifierType.Flat,
                        -0.40f
                    );

                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "신속 4세트 : 대쉬 쿨타임 감소 -0.4초, 대쉬 사용시 공격력 증가 +20%, 대쉬에 데미지 추가"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.DashCoolDown,
                        ModifierType.Flat,
                        -0.60f
                    );

                    AddSetStat(
                        StatType.Attack,
                        ModifierType.Percent,
                        0.40f
                    );

                    Debug.Log(
                        "신속 6세트 : 대쉬 쿨타임 감소 -0.6초, 대쉬 사용시 공격력 증가 +40%, 대쉬 데미지 강화"
                    );
                }

                break;



            case ItemTag.HeadHunter:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.CriticalChance,
                        ModifierType.Percent,
                        0.10f
                    );

                    Debug.Log(
                        "헤드헌터 2세트 : 치명타 확률 증가 +10%"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.CriticalChance,
                        ModifierType.Percent,
                        0.30f
                    );

                    AddSetStat(
                        StatType.CriticalDamage,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "헤드헌터 4세트 : 치명타 확률 증가 +30%, 치명타 피해 증가 +30%"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.CriticalChance,
                        ModifierType.Percent,
                        0.70f
                    );

                    AddSetStat(
                        StatType.CriticalDamage,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "헤드헌터 6세트 : 치명타 확률 증가 +70%, 치명타 피해 증가 +30%"
                    );
                }

                break;



            case ItemTag.Explosion:

                if (count >= 2)
                {
                    AddSetStat(
                        StatType.AttackSpeed,
                        ModifierType.Percent,
                        0.20f
                    );

                    Debug.Log(
                        "폭발 2세트 : 공격속도 증가 +20%"
                    );
                }

                if (count >= 4)
                {
                    AddSetStat(
                        StatType.AttackSpeed,
                        ModifierType.Percent,
                        0.30f
                    );

                    Debug.Log(
                        "폭발 4세트 : 공격속도 증가 +30%"
                    );
                }

                if (count >= 6)
                {
                    AddSetStat(
                        StatType.AttackSpeed,
                        ModifierType.Percent,
                        0.40f
                    );

                    Debug.Log(
                        "폭발 6세트 : 공격속도 증가 +40%"
                    );
                }

                break;
        }
    }



    // 세트 스탯 추가
    private void AddSetStat(
        StatType statType,
        ModifierType modifierType,
        float value)
    {
        if (PlayerStat.Instance == null)
            return;


        PlayerStat.Instance.AddSetStat(
            new StatModifier
            {
                statType = statType,
                modifierType = modifierType,
                value = value
            }
        );
    }



    // 현재 적용된 세트 확인용
    public int GetSetCount(ItemTag tag)
    {
        if (activeSets.ContainsKey(tag))
            return activeSets[tag];

        return 0;
    }



    // 특정 세트가 필요한 개수 이상인지 확인
    public bool HasSet(
        ItemTag tag,
        int requiredCount)
    {
        return GetSetCount(tag) >= requiredCount;
    }



    // 플레이어 최대 체력 갱신
    private void RefreshPlayerHealth()
    {
        PlayerController player =
            FindAnyObjectByType<PlayerController>();

        if (player == null)
            return;


        HealthManager health =
            player.GetComponent<HealthManager>();

        if (health == null)
            return;


        health.RefreshMaxHealth();
    }
}