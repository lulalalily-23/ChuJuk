using UnityEngine;
[CreateAssetMenu(fileName = "New Character Data", menuName = "Combat/CharacterData")]

public class CharacterData : ScriptableObject
{
    //캐릭터 설계(기본 능력치값)을 저장하는 데이터, ScriptableObject로 구현됨
    //플레이어와 몬스터 전부 해당 구조로 구현됨
    public int maxHealth;
    public int attackPower;
    public int defense;
    public int soulReward; //지급되는 재화량(플레이어의 경우 0)
}