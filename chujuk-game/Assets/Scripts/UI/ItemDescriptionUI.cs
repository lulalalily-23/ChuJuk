using System.Text;
using TMPro;
using UnityEngine;

public class ItemDescriptionUI : MonoBehaviour
{
    [Header("설명창")]
    [SerializeField]
    private GameObject descriptionPanel;

    [Header("기본 정보")]
    [SerializeField]
    private TMP_Text itemNameText;

    [SerializeField]
    private TMP_Text gradeText;

    [SerializeField]
    private TMP_Text typeText;

    [SerializeField]
    private TMP_Text descriptionText;

    [Header("능력치 효과")]
    [SerializeField]
    private TMP_Text modifierText;

    [Header("시너지")]
    [SerializeField]
    private TMP_Text synergyText;

    [Header("액티브 아이템")]
    [SerializeField]
    private GameObject cooldownGroup;

    [SerializeField]
    private TMP_Text cooldownText;

    [Header("가격")]
    [SerializeField]
    private TMP_Text sellPriceText;

    [SerializeField]
    private TMP_Text buyPriceText;

    private void Awake()
    {
        Hide();
    }

    public void Show(ItemData itemData)
    {
        if (itemData == null)
        {
            Hide();
            return;
        }

        if (itemNameText != null)
        {
            itemNameText.text =
                itemData.itemName;
        }

        if (gradeText != null)
        {
            gradeText.text =
                GetGradeName(itemData.grade);
        }

        if (typeText != null)
        {
            typeText.text =
                GetEffectTypeName(
                    itemData.effectType
                );
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                itemData.description;
        }

        if (modifierText != null)
        {
            modifierText.text =
                BuildModifierText(itemData);
        }

        if (synergyText != null)
        {
            synergyText.text =
                BuildSynergyText(itemData);
        }

        UpdateCooldown(itemData);
        UpdatePrice(itemData);

        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(true);
        }
    }

    public void Hide()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);
        }
    }

    private string BuildModifierText(
        ItemData itemData)
    {
        if (itemData.modifiers == null ||
            itemData.modifiers.Count == 0)
        {
            return "- 없음";
        }

        StringBuilder builder =
            new StringBuilder();

        foreach (StatModifier modifier
                 in itemData.modifiers)
        {
            if (modifier == null ||
                modifier.statType ==
                StatType.None)
            {
                continue;
            }

            string statName =
                GetStatName(
                    modifier.statType
                );

            string valueText =
                GetModifierValueText(
                    modifier
                );

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(
                $"- {statName} {valueText}"
            );
        }

        if (builder.Length == 0)
        {
            return "- 없음";
        }

        return builder.ToString();
    }

    private string BuildSynergyText(
        ItemData itemData)
    {
        if (itemData.tags == null ||
            itemData.tags.Count == 0)
        {
            return "- 없음";
        }

        StringBuilder builder =
            new StringBuilder();

        foreach (ItemTag tag
                 in itemData.tags)
        {
            if (tag == ItemTag.None)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(
                $"- {GetTagName(tag)}"
            );
        }

        if (builder.Length == 0)
        {
            return "- 없음";
        }

        return builder.ToString();
    }

    private string GetModifierValueText(
        StatModifier modifier)
    {
        float value = modifier.value;

        if (modifier.modifierType ==
            ModifierType.Percent)
        {
            float percent =
                value * 100f;

            string sign =
                percent > 0f ? "+" : "";

            return
                $"{sign}{FormatNumber(percent)}%";
        }

        string flatSign =
            value > 0f ? "+" : "";

        string suffix =
            GetFlatSuffix(
                modifier.statType
            );

        return
            $"{flatSign}{FormatNumber(value)}{suffix}";
    }

    private string FormatNumber(float value)
    {
        if (Mathf.Approximately(
                value,
                Mathf.Round(value)))
        {
            return
                Mathf.RoundToInt(value)
                    .ToString();
        }

        return value.ToString("0.##");
    }

    private string GetFlatSuffix(
        StatType statType)
    {
        switch (statType)
        {
            case StatType.InvincibilityDuration:
            case StatType.ActiveInvincibilityDuration:
            case StatType.DashCoolDown:
            case StatType.ActiveCoolDown:
                return "초";

            default:
                return "";
        }
    }

    private string GetGradeName(
        ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common:
                return "일반";

            case ItemGrade.Rare:
                return "희귀";

            case ItemGrade.Epic:
                return "에픽";

            case ItemGrade.Legendary:
                return "전설";

            default:
                return "-";
        }
    }

    private string GetEffectTypeName(
        ItemEffectType type)
    {
        switch (type)
        {
            case ItemEffectType.Passive:
                return "패시브";

            case ItemEffectType.Active:
                return "액티브";

            default:
                return "-";
        }
    }

    private string GetStatName(
        StatType statType)
    {
        switch (statType)
        {
            case StatType.Attack:
                return "공격력";

            case StatType.Defense:
                return "방어력";

            case StatType.MaxHP:
                return "최대 체력";

            case StatType.AttackSpeed:
                return "공격 속도";

            case StatType.MoveSpeed:
                return "이동 속도";

            case StatType.GoldGain:
                return "재화 획득량";

            case StatType.DamageIncrease:
                return "피해량";

            case StatType.ActiveCoolDown:
                return "액티브 쿨타임";

            case StatType.CriticalChance:
                return "치명타 확률";

            case StatType.CriticalDamage:
                return "치명타 피해";

            case StatType.DamageReduction:
                return "받는 피해";

            case StatType.InvincibilityDuration:
                return "피격 무적 시간";

            case StatType.RangedDamage:
                return "원거리 피해";

            case StatType.DashDamage:
                return "대쉬 피해";

            case StatType.DashCoolDown:
                return "대쉬 쿨타임";

            case StatType.HealOnKill:
                return "처치 시 체력 회복";

            case StatType.AttackSpeedOnKill:
                return "처치 시 공격 속도";

            case StatType.GoldAttackBonus:
                return "재화 비례 공격력";

            case StatType.ActiveInvincibilityDuration:
                return "액티브 무적 시간";

            case StatType.Evasion:
                return "회피";

            case StatType.TrapAvoidance:
                return "함정 회피";

            default:
                return statType.ToString();
        }
    }

    private string GetTagName(
        ItemTag tag)
    {
        switch (tag)
        {
            case ItemTag.Tough:
                return "강인";

            case ItemTag.Rush:
                return "돌진";

            case ItemTag.Ambush:
                return "매복";

            case ItemTag.Savagery:
                return "흉포";

            case ItemTag.Sniping:
                return "저격";

            case ItemTag.Pursuit:
                return "추격";

            case ItemTag.Treasure:
                return "보화";

            case ItemTag.Relic:
                return "유물";

            case ItemTag.Swiftness:
                return "신속";

            case ItemTag.HeadHunter:
                return "헤드 헌터";

            case ItemTag.Explosion:
                return "폭발";

            default:
                return tag.ToString();
        }
    }

    private void UpdateCooldown(ItemData itemData)
    {
        bool isActive =
            itemData.effectType ==
            ItemEffectType.Active;

        if (cooldownGroup != null)
        {
            cooldownGroup.SetActive(isActive);
        }

        if (!isActive)
            return;

        if (cooldownText != null)
        {
            cooldownText.text =
                $"{FormatNumber(itemData.cooldown)}초";
        }
    }

    private void UpdatePrice(ItemData itemData)
    {
        if (sellPriceText != null)
        {
            sellPriceText.text =
                $"판매가 {itemData.sellPrice:N0}";
        }

        if (buyPriceText != null)
        {
            buyPriceText.text =
                $"구매가 {itemData.buyPrice:N0}";
        }
    }
}