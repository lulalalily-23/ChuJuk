using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    //Awake에서 CharacterStats를 받아 실시간으로 관리하는 스크립트
    //CharacterData가 원본 수치면 각 프리팹의 데이터를 저장함
    //추후에 구현될 수 있는 버프나 디버프등으로 변할 수 있는 공격 계수, 방어 계수 조절은 여기서 이루어짐.
    public CharacterData data;
    public int currentAttackPower;
    public int currentDefense;
    public float attackMultiplier = 1f;
    public float defenseMultiplier = 1f;

    void Awake()
    {
        currentAttackPower = data.attackPower;
        currentDefense = data.defense;
    }

}
