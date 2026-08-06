using UnityEngine;

// 무작위 돌진이 지나간 시작점~도착점을 잇는 직사각형 장판.
// SlimeSkill_RandomCharge가 스폰 직후 SetSize()를 호출해서 실제 돌진 거리/폭/회전에 맞춤.
// 범위 안에 플레이어가 있으면 일정 주기로 데미지.
public class SlimeHazardZone : MonoBehaviour
{
    public int damagePerTick = 5;
    public float tickInterval = 1f;

    [Tooltip("길이(돌진 방향) x 폭. SetSize()로 런타임에 덮어씀 - 프리팹 기본값은 씬에서 미리보기용")]
    public Vector2 size = new Vector2(3f, 1.5f);

    public LayerMask targetLayer;

    private float tickTimer;

    // SlimeSkill_RandomCharge가 스폰 직후 호출
    public void SetSize(float length, float width)
    {
        size = new Vector2(length, width);

        // 장판 스프라이트가 실제 판정 범위와 일치하도록 스케일도 같이 맞춤
        // (스프라이트가 단순 사각형 이미지라는 전제 - 늘어나는 그림이 어색하면 9-slice나 별도 연출로 교체 필요)
        transform.localScale = new Vector3(length, width, 1f);
    }

    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        tickTimer = 0f;

        // 장판 자체의 회전(돌진 방향)까지 반영해서 박스 판정
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