using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 몬스터의 상태(Idle/Chase/Attack)를 거리 기반으로 판단하고,
    // 상태가 바뀔 때만 해당 행동 스크립트를 활성화/비활성화하는 컨트롤러
    // Attack 상태에는 attackExitRange를 적용해 경계선에서 상태 떨림을 방지하도록 수정함(쿨타임 없는 무한 공격 방지용).
    enum EnemyState {Idle, Chase, Attack};
    EnemyState currentState = EnemyState.Idle;
    EnemyState newState;
    EnemyChase chaseScript;
    EnemyAttack attackScript;
    EnemyIdle idleScript;

    [Tooltip("player Transform값 참조 변수, 현재는 플레이어 오브젝트와 연결 필요 (나중에 연결구조 수정 가능)")]
    public Transform player; 
    [Tooltip("몬스터가 플레이어를 감지하는 거리. 이 범위 안에 들어오면 Chase 상태로 전환됨")]
    public float detectRange; 
    [Tooltip("플레이어에게 공격을 시작하는 거리. detectRange보다 작은 값이어야 함")]
    public float attackRange; 
    private float attackExitRange; //Attack 상태에서 벗어나는 거리. attackRange보다 0.5크게 설정됨 (경계선에서 상태가 떨리는 것 방지)

    void Start()
    {
        chaseScript = GetComponent<EnemyChase>();
        attackScript = GetComponent<EnemyAttack>();
        idleScript = GetComponent<EnemyIdle>();
        attackExitRange = attackRange + 0.5f;

        // 씬 시작 시 인스펙터의 enabled 상태와 currentState가 불일치할 수 있으므로,
        // 강제로 모든 행동 스크립트를 끄고 Idle만 켜서 안전하게 수정함.
        chaseScript.enabled = false;
        attackScript.enabled = false;
        idleScript.enabled = false;
        idleScript.enabled = true;

        currentState = EnemyState.Idle;
    }

    // Update is called once per frame
    void Update()
    {   
        float distance = Vector2.Distance(player.position, transform.position); //몬스터와 플레이어간의 거리
        
        if (currentState == EnemyState.Attack)
        // 이미 Attack 중이면 "나가는 기준(attackExitRange)"으로 판단.
        // attackRange와 attackExitRange 사이에 여유 구간을 두어 플레이어가 경계선 근처에서 미세하게 움직일 때 상태 떨림을 막도록 수정 (쿨타임 없는 무한 공격 방지)
        {
            if (distance > attackExitRange)
            {
                newState = (distance < detectRange) ? EnemyState.Chase : EnemyState.Idle;
            }
            else
            {
                newState = EnemyState.Attack;
            }
        }
        else
        {
            if (distance < attackRange){
                newState = EnemyState.Attack;
            }
            else if (distance < detectRange){
                newState = EnemyState.Chase;
            }
            else {
                newState = EnemyState.Idle;
            }
        }

        if (newState != currentState){   
            chaseScript.enabled = false;
            attackScript.enabled = false;
            idleScript.enabled = false;  //초기 상태의 경우 모든 script는 비활성화된 상태임.

            switch(newState){ //현재상태에 따라 각각의 script를 활성화.
                case EnemyState.Idle:
                    idleScript.enabled = true;
                break;
                case EnemyState.Chase:
                    chaseScript.enabled = true;
                break;
                case EnemyState.Attack:
                    attackScript.enabled = true;
                break;
            }
        }

        currentState = newState;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어 쪽으로 튕겨나가게 방향 설정
            Vector2 dir = (collision.transform.position - transform.position).normalized * 5f;

            // 플레이어의 TakeDamage 호출
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(10, dir);
        }
    }
}
