using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class dingshuji : Enemy
{
    [SerializeField] public float speed=5f;
    private float attackTime=0;
    private float attackCD=5f;
    private float detectTime = 0f;
    private float InAttackTime = 0;
    private float InAttackTime1 = 1.5f;
    [SerializeField] private float detectCD = 1f;
    private Animator animator;
    private GameObject target;
    private NavMeshAgent navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsBe) 
        {
        if (attackTime > 0)
        {
            attackTime-=Time.deltaTime;
            
        }
        else 
        {
            DetectPlayer();
        }
        }
        if (Vector3.Distance(transform.position, myPosition) > 7f && !(InAttackTime > 0))
        {
            navMeshAgent.SetDestination(myPosition);
        }
        else 
        {
            navMeshAgent.ResetPath();
            if (InAttackTime > 0)
            {
                InAttackTime -= Time.deltaTime;
            }
        }
    }


    IEnumerator Attack(Vector3 targetPosition,float distance) 
    {
        animator.SetBool("Attack",true);
        InAttackTime = InAttackTime1;
        StartCoroutine(AttackShutDown());
        Vector3 direction = (targetPosition - transform.position).normalized;
        float currentDis = 0;
        while (currentDis<distance) 
        {
           transform.position += direction * speed * Time.deltaTime;
            currentDis+=speed*Time.deltaTime;
            if (IsPlayerInSight(AttackRadius,AttackAngle,AttackRayCount,Color.green)) 
            {
                Debug.Log("命中敌人");
                PlayerController PlayerController = target.GetComponent<PlayerController>();
                if (PlayerController != null)
                {
                    PlayerController.Stun(1.5f); // 触发击退
                    // PlayerController.Stun(1.5f); // 触发眩晕
                }
                else
                {
                    Debug.LogWarning("PlayerController component not found on target.");
                }
                break;
            }
            yield return null;
        }
    }
    IEnumerator AttackShutDown()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool("Attack", false);
    }
    private void DetectPlayer()
    {
        if (detectTime > 0)
        {
            detectTime -= Time.deltaTime;
        }
        else
        {
            if (IsPlayerInSight(sightRadius,sightAngle,rayCount,Color.red))
            {
                detectTime= detectCD;
                StartCoroutine(Attack(target.transform.position, 5));
                attackTime = attackCD;
            }
        }
    }
    [Header("视野参数")]
    private float sightRadius = 6f;       // 视野半径
    private float sightAngle = 360f;       // 视野角度（扇形张角）
    private int rayCount = 14;            // 射线数量
    public LayerMask targetMask;         // 目标层级（如Player）
    public LayerMask obstacleMask;
    private float AttackRadius = 0.5f;       // 视野半径
    private float AttackAngle = 360f;       // 视野角度（扇形张角）
    private int AttackRayCount = 14;            // 射线数量
    private bool IsPlayerInSight(float sightRadius,float sightAngle,int rayCount,Color color)
    {
        bool playerDetected = false;
        float halfAngle = sightAngle / 2f;
        float angleStep = sightAngle / (rayCount - 1); // 每条射线的角度间隔

        for (int i = 0; i < rayCount; i++)
        {
            // 计算当前射线的角度（从左侧到右侧）
            float currentAngle = -halfAngle + angleStep * i;

            // 将角度转换为方向向量（2D）
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) *transform.right;

            // 发射射线
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position+new Vector3 (0,0.5f,0),
                rayDirection,
                sightRadius,
                targetMask
            );

            // 绘制射线（Scene视图可视化）
            Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), rayDirection * sightRadius, color);

            // 检测是否命中玩家
            if (hit.collider != null) 
            {
                Debug.Log("发现玩家");
            }
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
            {
                playerDetected = true;
                target=hit.collider.gameObject;
            }
        }
        return playerDetected;
    }

}
