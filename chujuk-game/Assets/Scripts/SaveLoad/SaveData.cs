using System;
using System.Collections.Generic;

// JsonUtility로 직렬화할 순수 데이터 그릇.
// PlayerStat의 flat/percent 보너스 딕셔너리는 저장하지 않는다 - 인벤토리 아이템을
// 다시 AddItem()으로 적용하면 자동으로 재계산되므로, 굳이 따로 저장하면
// "저장된 스탯"과 "실제 인벤토리"가 어긋나는 버그 소지만 늘어난다.
[Serializable]
public class SaveData
{
    public string sceneName;

    // 재화
    public int soul;

    // 플레이어 체력/위치
    // 주의: 현재 프로젝트에 HealthManager / PlayerController 체력이 이원화되어 있는
    // 알려진 이슈가 있음 -> 여기서는 실제 게임플레이에 쓰이는 PlayerController 기준으로 저장.
    // 팀에서 체력 시스템을 하나로 통일하면 이 부분도 맞춰 수정 필요.
    public int currentHp;
    public int maxHp;
    public float posX;
    public float posY;
    public float posZ;

    // 인벤토리 - 아이템 자체가 아니라 이름만 저장, 불러올 때 ItemDatabase로 재조회
    public List<string> itemNames = new List<string>();

    // 죽은 적 목록 - EnemyIdentity.enemyId 기준. HealthManager를 쓰는 일반 적만 해당.
    // 골렘 보스처럼 별도 피격 로직을 쓰는 대상은 아직 포함되지 않음(추후 별도 작업 필요).
    public List<string> deadEnemyIds = new List<string>();

    // 디버그/표시용
    public string savedAtUtc;
}