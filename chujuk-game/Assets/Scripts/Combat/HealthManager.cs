using UnityEngine;
using System;

// 플레이어/몬스터 공용 체력 관리 컴포넌트.
// TakeDamage/Heal로 체력을 조작하고, 체력이 0 이하가 되면 OnDeath 이벤트를 호출.
// OnDeath를 가지는 별도 스크립트(EnemyDeathHandler, PlayerDeathHandler)가 대상에 따라 다른 후속 처리(재화 지급 / 게임오버)를 담당함.
public class HealthManager : MonoBehaviour
{

    public CharacterData data; // 설계값 참조용. 인스펙터에서 CharacterData 에셋 연결 필요
    public int currentHealth; // 실시간 현재 체력
    public event Action OnDeath; // 사망 시 호출되는 이벤트.
    public event Action<int, int> OnHealthChanged; // UI 연동용 이벤트 추가
    private bool isDead; //중복 사망 방지용

    void Awake()
    {
        currentHealth = data.maxHealth;
        isDead = false;
        OnHealthChanged?.Invoke(currentHealth, data.maxHealth); // 초기 UI 동기화
    }

    // 외부에서 호출하여 데미지를 적용하는 함수.
    // 이미 사망한 상태면 무시. 체력이 0 이하가 되면 OnDeath 이벤트 발행
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;

        OnHealthChanged?.Invoke(currentHealth, data.maxHealth); // 데이터 변경 시 알림

        Debug.Log($"{gameObject.name} 이(가) {amount} 데미지를 입음. 남은 체력: {currentHealth}/{data.maxHealth}"); //디버그용 로그, 추후 삭제 필요함.
        if (currentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    // 외부에서 호출하여 체력을 회복하는 함수.
    // 이미 사망한 상태면 무시. 회복한 후의 체력이 maxHealth를 초과하지 않도록 설계함.
    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, data.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, data.maxHealth); // 데이터 변경 시 알림
    }

}