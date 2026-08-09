using UnityEngine;
using System.Collections;

// 액티브 아이템 효과 관리자
// Inventory의 OnActiveItemUsed 이벤트를 받아
// 현재 보유 세트에 따라 실제 액티브 효과를 실행한다.

public class ActiveItemEffectManager : MonoBehaviour
{
    public static ActiveItemEffectManager Instance;

    // 설정

    [Header("액티브 공통 설정")]
    [SerializeField]
    private float invincibilityDuration = 5f;

    [SerializeField]
    private float savageDuration = 5f;

    [SerializeField]
    private float savageAttackSpeedBonus = 0.70f;

    [SerializeField]
    private float ambushRangedDamageBonus = 3.00f;

    [SerializeField]
    private int treasureExplosionCost = 300;

    // 상태

    private bool ambushNextRangedAttack;

    private Coroutine savageCoroutine;

    // 초기화

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void OnEnable()
    {
        Subscribe();
    }


    private void OnDisable()
    {
        Unsubscribe();
    }

    // 이벤트 연결

    private void Subscribe()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.OnActiveItemUsed -=
            HandleActiveItemUsed;

        Inventory.Instance.OnActiveItemUsed +=
            HandleActiveItemUsed;
    }


    private void Unsubscribe()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.OnActiveItemUsed -=
            HandleActiveItemUsed;
    }

    // 액티브 사용 처리

    private void HandleActiveItemUsed(
        ItemInstance item)
    {
        if (item == null)
            return;

        if (item.data == null)
            return;

        Debug.Log(
            "[ActiveItemEffectManager] 액티브 사용 처리 / " +
            item.data.itemName
        );

        // 현재 세트 개수 확인

        int toughCount =
            GetSetCount(ItemTag.Tough);

        int rushCount =
            GetSetCount(ItemTag.Rush);

        int ambushCount =
            GetSetCount(ItemTag.Ambush);

        int savageCount =
            GetSetCount(ItemTag.Savagery);

        int snipingCount =
            GetSetCount(ItemTag.Sniping);

        int treasureCount =
            GetSetCount(ItemTag.Treasure);

        int explosionCount =
            GetSetCount(ItemTag.Explosion);

        // 강인 6세트

        if (toughCount >= 6)
        {
            ActivateTough();
        }

        // 돌진 6세트

        if (rushCount >= 6)
        {
            ActivateRush();
        }

        // 매복 6세트

        if (ambushCount >= 6)
        {
            ActivateAmbush();
        }

        // 흉포 6세트

        if (savageCount >= 6)
        {
            ActivateSavage();
        }

        // 저격 6세트

        if (snipingCount >= 6)
        {
            ActivateSniping();
        }

        // 보화 6세트

        if (treasureCount >= 6)
        {
            ActivateTreasure();
        }

        // 폭발 6세트

        if (explosionCount >= 6)
        {
            ActivateExplosion();
        }
    }

    // 강인 액티브
    private void ActivateTough()
    {
        HealthManager health =
            GetPlayerHealthManager();

        if (health == null)
        {
            Debug.LogWarning(
                "[강인 액티브] HealthManager를 찾을 수 없습니다."
            );

            return;
        }


        health.StartExternalInvincibility(
            invincibilityDuration
        );


        Debug.Log(
            "[강인 6세트 액티브] " +
            invincibilityDuration +
            "초간 무적"
        );
    }

    // 돌진 액티브

    private void ActivateRush()
    {
        HealthManager health =
            GetPlayerHealthManager();

        if (health == null)
        {
            Debug.LogWarning(
                "[돌진 액티브] HealthManager를 찾을 수 없습니다."
            );

            return;
        }


        health.StartExternalInvincibility(
            invincibilityDuration
        );


        Debug.Log(
            "[돌진 6세트 액티브] " + invincibilityDuration + "초간 무적"
        );
    }

    // 매복 액티브
    private void ActivateAmbush()
    {
        ambushNextRangedAttack = true;


        Debug.Log(
            "[매복 6세트 액티브] " +
            "다음 원거리 공격 1회 데미지 +300%"
        );
    }

    // 매복 다음 원거리 공격 보너스 확인

    public float GetRangedDamageMultiplier()
    {
        if (!ambushNextRangedAttack)
            return 1f;


        return 1f + ambushRangedDamageBonus;
    }

    // 매복 원거리 공격 사용 처리
    // 1.0 = 일반 데미지
    // 4.0 = +300% 데미지

    public float ConsumeAmbushRangedDamage()
    {
        if (!ambushNextRangedAttack)
            return 1f;


        ambushNextRangedAttack = false;


        Debug.Log(
            "[매복 6세트] " +
            "원거리 공격 +300% 효과 사용 완료"
        );


        return 1f +
               ambushRangedDamageBonus;
    }

    // 흉포 액티브

    private void ActivateSavage()
    {
        if (PlayerStat.Instance == null)
        {
            Debug.LogWarning(
                "[흉포 액티브] PlayerStat.Instance가 없습니다."
            );

            return;
        }

        if (savageCoroutine != null)
        {
            StopCoroutine(
                savageCoroutine
            );

            PlayerStat.Instance.AddRuntimeStat(
                StatType.AttackSpeed,
                ModifierType.Percent,
                -savageAttackSpeedBonus
            );

            savageCoroutine = null;
        }


        savageCoroutine =
            StartCoroutine(
                SavageAttackSpeedRoutine()
            );


        Debug.Log(
            "[흉포 6세트 액티브] " +
            savageDuration +
            "초간 공격속도 +" +
            (savageAttackSpeedBonus * 100f) +
            "%"
        );
    }

    // 흉포 공격속도 지속

    private IEnumerator SavageAttackSpeedRoutine()
    {
        PlayerStat.Instance.AddRuntimeStat(
            StatType.AttackSpeed,
            ModifierType.Percent,
            savageAttackSpeedBonus
        );


        yield return new WaitForSeconds(
            savageDuration
        );


        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.AddRuntimeStat(
                StatType.AttackSpeed,
                ModifierType.Percent,
                -savageAttackSpeedBonus
            );
        }


        savageCoroutine = null;


        Debug.Log(
            "[흉포 6세트 액티브] 공격속도 효과 종료"
        );
    }

    // 저격 액티브

    private void ActivateSniping()
    {
        Debug.Log(
            "[저격 6세트 액티브] " +
            "머신건 생성 요청"
        );


        // 실제 머신건 생성은 무기, 투사체 시스템이 완성된 뒤 연결.
    }

    // 보화 액티브

    private void ActivateTreasure()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning(
                "[보화 6세트 액티브] GameManager.Instance가 없습니다."
            );

            return;
        }


        if (!GameManager.Instance.UseSoul(
            treasureExplosionCost))
        {
            Debug.Log(
                "[보화 6세트 액티브] " +
                "영혼이 부족합니다. 필요 영혼: " +
                treasureExplosionCost
            );

            return;
        }


        Debug.Log(
            "[보화 6세트 액티브] " +
            treasureExplosionCost +
            " 영혼 소모 → 폭발 공격"
        );


        PerformTreasureExplosion();
    }

    // 보화 폭발

    private void PerformTreasureExplosion()
    {
        PlayerStat playerStat = PlayerStat.Instance;

        if (playerStat == null)
            return;


        float attack = playerStat.GetStat(StatType.Attack);

        Debug.Log(
            "[보화 폭발] 플레이어 공격력 기반 폭발 데미지 : " + attack
        );


        Collider2D[] targets =
            Physics2D.OverlapCircleAll(
                GetPlayerPosition(),
                3f
            );


        foreach (Collider2D target in targets)
        {
            if (target == null)
                continue;


            if (target.CompareTag("Player"))
                continue;


            HealthManager health =
                target.GetComponentInParent<HealthManager>();


            if (health == null)
                continue;


            health.TakeDamage(
                Mathf.RoundToInt(attack)
            );
        }
    }

    // 폭발 액티브

    private void ActivateExplosion()
    {
        Debug.Log(
            "[폭발 6세트 액티브] " +
            "5초 후 폭발하는 폭탄 투척 요청"
        );


        // 폭탄 프리펩 필요
    }

    // 현재 세트 개수

    private int GetSetCount(ItemTag tag)
    {
        if (SetSystem.Instance == null)
            return 0;


        return SetSystem.Instance.GetSetCount(
            tag
        );
    }

    // 플레이어 HealthManager

    private HealthManager GetPlayerHealthManager()
    {
        PlayerController player =
            FindAnyObjectByType<PlayerController>();


        if (player == null)
            return null;


        return player.GetComponent<HealthManager>();
    }

    // 플레이어 위치

    private Vector2 GetPlayerPosition()
    {
        PlayerController player =
            FindAnyObjectByType<PlayerController>();


        if (player == null)
            return Vector2.zero;


        return player.transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;


        Gizmos.DrawWireSphere(
            GetPlayerPosition(),
            3f
        );
    }
}
