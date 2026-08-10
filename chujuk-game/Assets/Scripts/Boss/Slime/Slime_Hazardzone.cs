using UnityEngine;

// 돌진이 지나간 범위를 덮는 장판.
// 범위 안에 플레이어가 있으면 일정 주기로 데미지.
public class SlimeHazardZone : MonoBehaviour
{
    public int damagePerTick = 5;
    public float tickInterval = 1f;

    [Tooltip("길이(돌진 방향) x 폭")]
    public Vector2 size = new Vector2(3f, 1.5f);

    public LayerMask targetLayer;

    private float tickTimer;

    public void SetSize(float length, float width)
    {
        size = new Vector2(length, width);

        transform.localScale = new Vector3(length, width, 1f);
    }

    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        tickTimer = 0f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, size, transform.eulerAngles.z, targetLayer);

        foreach (Collider2D hit in hits)
        {
            hit.GetComponentInParent<HealthManager>()?.TakeDamage(damagePerTick);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0f));
    }
}