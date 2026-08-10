using UnityEngine;

public class ItemInstance
{
    // 원본 아이템 데이터
    public ItemData data;

    // 액티브 아이템용 현재 쿨타임
    private float currentCooldown;


    public ItemInstance(ItemData data)
    {
        this.data = data;
        currentCooldown = 0f;
    }


    // 액티브 아이템 여부
    public bool IsActiveItem()
    {
        if (data == null)
            return false;

        return data.effectType == ItemEffectType.Active;
    }


    // 액티브 아이템 최대 쿨타임
    public float GetMaxCooldown()
    {
        if (data == null)
            return 0f;

        return Mathf.Max(
            0f,
            data.cooldown
        );
    }


    // 액티브 사용 가능 여부
    public bool CanUse()
    {
        if (data == null)
            return false;

        if (!IsActiveItem())
            return false;

        return currentCooldown <= 0f;
    }



    // 매 프레임 쿨타임 감소
    public void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown <= 0f)
            return;

        currentCooldown -= deltaTime;

        if (currentCooldown < 0f)
            currentCooldown = 0f;
    }



    // 아이템 사용
    // 액티브 아이템 사용 성공 여부를 반환
    public bool Use()
    {
        if (!CanUse())
        {
            Debug.Log(
                "[ItemInstance] 현재 사용할 수 없는 아이템입니다."
            );

            return false;
        }


        float cooldownReduction = 0f;

        if (PlayerStat.Instance != null)
        {
            cooldownReduction =
                PlayerStat.Instance.GetStat(
                    StatType.ActiveCoolDown
                );
        }


        cooldownReduction =
            Mathf.Clamp01(
                cooldownReduction
            );


        float reducedCooldown =
            data.cooldown *
            (1f - cooldownReduction);


        // 쿨타임이 음수로 내려가지 않도록 최소값 보장
        currentCooldown =
            Mathf.Max(
                reducedCooldown,
                0f
            );


        Debug.Log(
            "[ItemInstance] 액티브 사용 / " +
            data.itemName +
            " / 쿨타임: " +
            currentCooldown
        );


        return true;
    }



    // 현재 쿨타임 확인
    public float GetCurrentCooldown()
    {
        return currentCooldown;
    }



    // 쿨타임 초기화
    public void ResetCooldown()
    {
        currentCooldown = 0f;
    }
}
