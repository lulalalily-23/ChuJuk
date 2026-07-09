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


    // 액티브 사용 가능 여부
    public bool CanUse()
    {
        if (data.effectType != ItemEffectType.Active)
            return false;

        return currentCooldown <= 0f;
    }


    // 매 프레임 쿨타임 감소
    public void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= deltaTime;

            if (currentCooldown < 0)
                currentCooldown = 0;
        }
    }


    // 아이템 사용
    public void Use()
    {
        if (!CanUse())
            return;


        currentCooldown = data.cooldown;


        Debug.Log($"{data.itemName} 사용");

        // 액티브 효과 구현 예정
    }


    // 현재 쿨타임 확인
    public float GetCurrentCooldown()
    {
        return currentCooldown;
    }
}