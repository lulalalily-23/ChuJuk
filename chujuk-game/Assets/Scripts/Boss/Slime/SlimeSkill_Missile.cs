using UnityEngine;
using System.Collections;

// 원거리 미사일 발사. 발사 시점의 플레이어 위치를 목표로 고정함
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

    // 페이즈1은 4발, 페이즈2는 8발(2배) 
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
            // 발사 시점 플레이어 위치를 고정 목표로 사용
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