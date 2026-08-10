using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;

    // 재화
    public int soul;

    // 플레이어 체력/위치
    public int currentHp;
    public int maxHp;
    public float posX;
    public float posY;
    public float posZ;

    // 인벤토리 - 아이템 자체가 아니라 이름만 저장, 불러올 때 ItemDatabase로 재조회
    public List<string> itemNames = new List<string>();

    // 죽은 적 목록 - EnemyIdentity.enemyId 기준. HealthManager를 쓰는 일반 적만 해당.
    public List<string> deadEnemyIds = new List<string>();

    // 디버그/표시용
    public string savedAtUtc;
}