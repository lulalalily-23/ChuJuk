using System;
using System.Text;
using TMPro;
using UnityEngine;

public class SynergyUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text synergyText;

    [Header("색상")]
    [SerializeField]
    private Color activeColor = Color.white;

    [SerializeField]
    private Color inactiveColor =
        new Color(
            0.45f,
            0.45f,
            0.45f,
            1f
        );

    [SerializeField]
    private Color maxColor =
        new Color(
            1f,
            0.85f,
            0.35f,
            1f
        );

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
        if (synergyText == null)
            return;

        StringBuilder sb =
            new StringBuilder();

        foreach (
            ItemTag tag in
            Enum.GetValues(typeof(ItemTag)))
        {
            if (tag == ItemTag.None)
                continue;

            int count = 0;

            if (SetSystem.Instance != null)
            {
                count =
                    SetSystem.Instance
                        .GetSetCount(tag);
            }

            int nextTarget =
                GetNextTarget(count);

            bool active =
                count >= 2;

            bool max =
                count >= 6;

            Color color;

            if (max)
            {
                color = maxColor;
            }
            else if (active)
            {
                color = activeColor;
            }
            else
            {
                color = inactiveColor;
            }

            string colorHex =
                ColorUtility.ToHtmlStringRGBA(
                    color
                );

            sb.Append(
                $"<color=#{colorHex}>"
            );

            sb.Append(
                GetSynergyName(tag)
            );

            sb.Append("<pos=65%>");

            sb.Append(
                GetProgressText(
                    count,
                    nextTarget
                )
            );

            sb.Append("</color>");

            sb.AppendLine();
        }

        synergyText.text =
            sb.ToString();
    }

    private int GetNextTarget(
        int currentCount)
    {
        if (currentCount < 2)
            return 2;

        if (currentCount < 4)
            return 4;

        if (currentCount < 6)
            return 6;

        return 6;
    }

    private string GetProgressText(
        int currentCount,
        int target)
    {
        if (currentCount >= 6)
        {
            return $"{currentCount} / MAX";
        }

        return $"{currentCount} / {target}";
    }

    private string GetSynergyName(
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
                return "헤드헌터";

            case ItemTag.Explosion:
                return "폭발";

            default:
                return tag.ToString();
        }
    }
}