using UnityEngine;
using System.Collections;

// 원거리 점액 미사일 발사. 발사 시점의 플레이어 위치를 목표로 고정하고
// 그 이후엔 유도하지 않기 때문에, 플레이어가 이동해서 피할 수 있음.
public class SlimeSkill_Missile : MonoBehaviour
{
    public GameObject missilePrefab;
    public Transform player;
    public Transform firePoint;
    public float missileSpeed = 6f;
    public float coolTime = 4f;
    public float telegraphRadius = 0.8f;
    public float telegraphDuration = 0.5f;
    public float intervalBetweenMissiles = 0.3f;

    private SlimeController controller;
    private float lastUseTime = -999f;

    void Start()
    {
        controller = GetComponent<SlimeController>();
    }

    public bool CanUse()
    {
        return Time.time - lastUseTime >= coolTime;
    }

    // missileCount: Phase1은 2발, Phase2는 4발(2배) - 패턴매니저에서 전달
    public void Execute(int missileCount)
    {
        StartCoroutine(MissileRoutine(missileCount));
    }

    private IEnumerator MissileRoutine(int missileCount)
    {
        lastUseTime = Time.time;
        controller.PlayMissile();

        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        ICombatStats myStats = GetComponent<ICombatStats>();

        for (int i = 0; i < missileCount; i++)
        {
            // 발사 시점 플레이어 위치를 고정 목표로 사용 (유도 아님 - 이동해서 피할 수 있음)
            Vector2 targetPos = player.position;

            TelegraphIndicator.Instance.ShowCircle(targetPos, telegraphRadius, telegraphDuration);

            GameObject missile = Instantiate(missilePrefab, origin, Quaternion.identity);
            SlimeProjectile projectile = missile.GetComponent<SlimeProjectile>();

            if (projectile != null)
                projectile.Launch(origin, targetPos, missileSpeed, myStats);

            yield return new WaitForSeconds(intervalBetweenMissiles);
        }

        controller.OnPatternExecuted();
    }
}