using UnityEngine;
using System.Collections;

// 무작위 방향으로 돌진하고, 멈춘 자리에 일정 시간 유지되는 장판을 남김.
public class SlimeSkill_RandomCharge : MonoBehaviour
{
    public float chargeDistance = 6f;
    public float chargeSpeed = 12f;
    public float telegraphDuration = 0.8f;
    public float coolTime = 5f;
    public GameObject hazardZonePrefab;
    public float hazardDuration = 5f;
    public float hazardWidth = 1.5f;  
    public LayerMask wallLayer;

    public float telegraphYOffset = 0f;

    private SlimeController controller;
    private Rigidbody2D rb;
    private float lastUseTime = -999f;
    private bool isCharging;
    private Vector2 chargeDirection;
    private bool hitWall = false;

    void Start()
    {
        controller = GetComponent<SlimeController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isCharging)
            rb.linearVelocity = chargeDirection * chargeSpeed;
    }

    public bool CanUse()
    {
        return !isCharging && Time.time - lastUseTime >= coolTime;
    }

    public void Execute()
    {
        if (isCharging) return;
        StartCoroutine(RandomChargeRoutine());
    }

    private IEnumerator RandomChargeRoutine()
    {
        lastUseTime = Time.time;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.3f).normalized;

        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + direction * chargeDistance;

        Vector2 telegraphCenter = (startPos + targetPos) / 2f + Vector2.down * telegraphYOffset;
        TelegraphIndicator.Instance.ShowSquare(telegraphCenter, new Vector2(chargeDistance, 1f), telegraphDuration);
        yield return new WaitForSeconds(telegraphDuration);

        controller.PlayDash();
        chargeDirection = direction;
        isCharging = true;

        float elapsed = 0f;
        float duration = chargeDistance / chargeSpeed;

        while (elapsed < duration && !hitWall)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        hitWall = false;
        isCharging = false;
        rb.linearVelocity = Vector2.zero;

        if (hazardZonePrefab != null)
        {
            Vector2 endPos = transform.position;
            float actualLength = Vector2.Distance(startPos, endPos);
            Vector2 mid = (startPos + endPos) / 2f;
            float rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject hazard = Instantiate(hazardZonePrefab, mid, Quaternion.Euler(0f, 0f, rotationAngle));

            SlimeHazardZone zone = hazard.GetComponent<SlimeHazardZone>();
            zone?.SetSize(actualLength, hazardWidth);

            Destroy(hazard, hazardDuration);
        }

        controller.OnPatternExecuted();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && ((1 << collision.gameObject.layer) & wallLayer) != 0)
            hitWall = true;
    }
}