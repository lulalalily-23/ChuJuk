using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 몬스터의 상태(Idle/Chase/Attack)를 거리 기반으로 판단하고,
    // 상태가 바뀔 때만 해당 행동 스크립트를 활성화/비활성화하는 컨트롤러
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

    void Start()
    {
        chaseScript = GetComponent<EnemyChase>();
        attackScript = GetComponent<EnemyAttack>();
        idleScript = GetComponent<EnemyIdle>();
    }

    // Update is called once per frame
    void Update()
    {   
        float distance = Vector2.Distance(player.position, transform.position); //몬스터와 플레이어간의 거리

        if (distance < attackRange){ //공격거리보다 가까울 경우 Attack 상태로 변환
            newState = EnemyState.Attack;
        }
        else if (distance < detectRange){ //공격거리보다는 멀고 감지거리보다 가까울 경우 Chase 상태로 변환
            newState = EnemyState.Chase;
        }
        else { //둘 다 아닐경우 Idle 상태.
            newState = EnemyState.Idle;
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
}
