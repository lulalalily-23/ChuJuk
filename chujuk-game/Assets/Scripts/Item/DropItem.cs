using UnityEngine;

public class DropItem : MonoBehaviour
{
    [Header("Item Info")]
    public int amount = 10; // 금액
    public float magnetRadius = 3f; // 자석처럼 끌려가기 시작하는 거리
    public float moveSpeed = 10f; // 플레이어한테 끌려가는 속도

    private Transform player;
    private bool isMagnetic = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= magnetRadius)
        {
            isMagnetic = true;
        }

        if (isMagnetic)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerScript = other.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.AddMoney(amount);
            }

            Destroy(gameObject);
        }
    }
}