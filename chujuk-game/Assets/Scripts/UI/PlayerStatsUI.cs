using System.Text;
using TMPro;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text statText;

    private Inventory boundInventory;

    private void OnEnable()
    {
        BindInventory();
        Refresh();
    }

    private void Start()
    {
        BindInventory();
        Refresh();
    }

    private void Update()
    {
        // Inventory가 나중에 생성되는 경우 대비
        if (boundInventory == null &&
            Inventory.Instance != null)
        {
            BindInventory();
            Refresh();
        }
    }

    private void OnDisable()
    {
        UnbindInventory();
    }

    private void BindInventory()
    {
        if (boundInventory == Inventory.Instance)
            return;

        UnbindInventory();

        boundInventory = Inventory.Instance;

        if (boundInventory != null)
        {
            boundInventory.OnInventoryChanged += Refresh;
        }
    }

    private void UnbindInventory()
    {
        if (boundInventory != null)
        {
            boundInventory.OnInventoryChanged -= Refresh;
        }

        boundInventory = null;
    }

    public void Refresh()
    {
        if (statText == null)
            return;

        if (PlayerStat.Instance == null)
        {
            statText.text = "PlayerStat 없음";
            return;
        }

        PlayerStat stat = PlayerStat.Instance;

        StringBuilder sb = new StringBuilder();

        AppendLine(
            sb,
            "공격력",
            stat.GetStat(StatType.Attack).ToString("0.##")
        );

        AppendLine(
            sb,
            "방어력",
            stat.GetStat(StatType.Defense).ToString("0.##")
        );

        AppendLine(
            sb,
            "최대 체력",
            stat.GetStat(StatType.MaxHP).ToString("0")
        );

        AppendLine(
            sb,
            "공격 속도",
            stat.GetStat(StatType.AttackSpeed).ToString("0.##")
        );

        AppendLine(
            sb,
            "이동 속도",
            stat.GetStat(StatType.MoveSpeed).ToString("0.##")
        );

        AppendLine(
            sb,
            "치명타 확률",
            FormatPercent(
                stat.GetStat(StatType.CriticalChance)
            )
        );

        AppendLine(
            sb,
            "치명타 피해",
            FormatPercent(
                stat.GetStat(StatType.CriticalDamage)
            )
        );

        sb.AppendLine();

        AppendLine(
            sb,
            "피해 감소",
            FormatPercent(
                stat.GetStat(StatType.DamageReduction)
            )
        );

        AppendLine(
            sb,
            "원거리 피해 증가",
            FormatPercent(
                stat.GetStat(StatType.RangedDamage)
            )
        );

        AppendLine(
            sb,
            "피격 무적 증가",
            $"{stat.GetStat(StatType.InvincibilityDuration):0.##}초"
        );

        AppendLine(
            sb,
            "처치 시 회복",
            stat.GetStat(StatType.HealOnKill).ToString("0.##")
        );

        float dashCooldown =
            stat.GetStat(StatType.DashCoolDown);

        AppendLine(
            sb,
            "대시 쿨타임",
            FormatCooldownChange(dashCooldown)
        );

        statText.text = sb.ToString();
    }

    private void AppendLine(
        StringBuilder sb,
        string statName,
        string value)
    {
        sb.Append(statName);

        // TMP 기준 텍스트 가로 70% 지점에 값 배치
        sb.Append("<pos=70%>");

        sb.Append(value);

        sb.AppendLine();
    }

    private string FormatPercent(float value)
    {
        return $"{value * 100f:0.#}%";
    }

    private string FormatCooldownChange(float value)
    {
        if (value < 0f)
        {
            return $"{-value:0.##}초 감소";
        }

        if (value > 0f)
        {
            return $"{value:0.##}초 증가";
        }

        return "0초";
    }
}