using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Game/Item Data"
)]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;

    public Sprite icon;

    [TextArea(1, 10)]
    public string description;

    [Header("아이템 등급")]
    public ItemGrade grade;

    [Header("아이템 분류")]
    public ItemEffectType effectType;

    [Header("아이템 태그")]
    public List<ItemTag> tags = new List<ItemTag>();

    [Header("능력치 효과")]
    public List<StatModifier> modifiers = new List<StatModifier>();

    [Header("액티브 설정")]
    public float cooldown;
    
    //임의 판매&구매 가격 설정용
    [Header("판매 가격")]
    [Min(0)]
    public int sellPrice = 10;

    [Header("구매 가격")]
    [Min(0)]
    public int buyPrice = 30;
}
