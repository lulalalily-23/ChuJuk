using System.Collections;
using UnityEngine;

public class PlayerFallHandler : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private HealthManager healthManager;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField]
    [Min(0.01f)]
    private float groundCheckRadius = 0.2f;

    [Header("Safe Position")]
    [Tooltip("최근 안전 위치를 저장하는 시간 간격입니다.")]
    [SerializeField]
    [Min(0.01f)]
    private float saveInterval = 0.2f;

    [Tooltip("복귀할 때 바닥에 끼지 않도록 위로 더해주는 값입니다.")]
    [SerializeField]
    [Min(0f)]
    private float respawnYOffset = 0.1f;

    [Header("Fall Recovery")]
    [SerializeField]
    [Min(0f)]
    private float recoveryTime = 0.3f;

    private Vector2 lastSafePosition;
    private float nextSaveTime;
    private bool isRecovering;

    private void Awake()
    {
        if (healthManager == null)
        {
            healthManager = GetComponent<HealthManager>();
        }

        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        // 게임 시작 위치를 최초 안전 위치로 저장
        lastSafePosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (isRecovering)
        {
            return;
        }

        SaveSafePosition();
    }

    private void SaveSafePosition()
    {
        if (groundCheck == null)
        {
            return;
        }

        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;

        if (!isGrounded)
        {
            return;
        }

        // 위아래로 빠르게 이동 중인 상태는 저장하지 않음
        if (playerRigidbody != null &&
            Mathf.Abs(playerRigidbody.linearVelocity.y) > 0.1f)
        {
            return;
        }

        if (Time.time < nextSaveTime)
        {
            return;
        }

        lastSafePosition = transform.position;
        nextSaveTime = Time.time + saveInterval;
    }

    public void HandleFall(int damageAmount)
    {
        if (isRecovering)
        {
            return;
        }

        StartCoroutine(FallRecoveryRoutine(damageAmount));
    }

    private IEnumerator FallRecoveryRoutine(int damageAmount)
    {
        isRecovering = true;

        // 낙사 피해
        if (healthManager != null)
        {
            healthManager.TakeDamage(damageAmount);
        }
        else
        {
            Debug.LogError(
                "PlayerFallHandler가 HealthManager를 찾지 못했습니다.",
                this
            );
        }

        // 낙하 속도 제거
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        // 최근 안전 위치로 이동
        transform.position =
            lastSafePosition + Vector2.up * respawnYOffset;

        // 위치 변경을 물리 시스템에 즉시 반영
        Physics2D.SyncTransforms();

        yield return new WaitForSeconds(recoveryTime);

        isRecovering = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}