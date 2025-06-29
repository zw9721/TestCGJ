using QFramework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class shubiao : Enemy
{
    [SerializeField]private List<Vector3> MovePositions;
    [SerializeField] private int CurrentTarget;
    [SerializeField] private float speed;
    public float rotationSpeed = 20f;
    public GameObject eye;
    [SerializeField]private bool IsHead;
    public SpriteRenderer sprite;
    [SerializeField]private Vector3 currentDir;
    private float eyetime = 0f;
    [SerializeField]private float eyecd = 1f;
    private NavMeshAgent navMeshAgent;
    // Start is called before the first frame update
    private void OnDrawGizmosSelected()
    {
        
        if (MovePositions == null || MovePositions.Count == 0) return;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        for (int i = 0; i < MovePositions.Count; i++)
        {
            // 在点上方绘制索引和坐标
            Handles.Label(MovePositions[i] + Vector3.up * 0.3f, i.ToString(), style);
        }
    }
    void Start()
    {
        if (MovePositions!=null) 
        {
            MovePositions[0]=transform.position;
        }
        CurrentTarget = 1;
        IsHead = true;
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsBe) 
        {
        move();
        TransformDirction();
        DetectPlayer();
        imageFilp();
        }
        
    }
    private void imageFilp() 
    {
        float angle = NormalizeAngle(eye.transform.rotation.z);
        if (angle > 180f && angle < 360f)
        {
            sprite.flipX = false;
        }
        else 
        {
            sprite.flipX = true;
        }
    }
    
        float NormalizeAngle(float angle)
        {
            angle = angle % 360;  // 先转换为 (-360, 360)
            if (angle < 0)
            {
                angle += 360;     // 将负角度转换为 [0, 360)
            }
            return angle;
        }
    

    private void move() 
    {
        Vector3 direction = (MovePositions[CurrentTarget] - transform.position).normalized;
        // 匀速移动
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            eye.transform.rotation = Quaternion.Slerp(
                eye.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        //transform.position += direction * speed * Time.deltaTime;
        navMeshAgent.SetDestination(MovePositions[CurrentTarget]);
    }
    private void TransformDirction() 
    {
             if (Vector3.Distance(transform.position, MovePositions[CurrentTarget])<1f) 
            {
                 
                 int a = IsHead ? MovePositions.Count-1 : 0;
                 bool b=IsHead?false:true;
                if (CurrentTarget == a)
               {
                IsHead = b;
               }
            CurrentTarget = IsHead ? CurrentTarget + 1 : CurrentTarget - 1;
            }
    }
    private void DetectPlayer() 
    {
        if (eyetime > 0)
        {
            eyetime -= Time.deltaTime;
        }
        else 
        {
         if (IsPlayerInSight()) 
        {
            bool b=IsHead?false:true;
            int a=IsHead?-1:1;
            IsHead = b;
            CurrentTarget = CurrentTarget + a;
                eyetime = eyecd;
        }
            
        }
       
    }
    
    [Header("视野参数")]
    private float sightRadius = 3.5f;       // 视野半径
    private float sightAngle = 60f;       // 视野角度（扇形张角）
    private int rayCount = 10;            // 射线数量
    public LayerMask targetMask;         // 目标层级（如Player）
    public LayerMask obstacleMask;
    private bool IsPlayerInSight()
    {
        bool playerDetected = false;
        float halfAngle = sightAngle / 2f;
        float angleStep = sightAngle / (rayCount - 1); // 每条射线的角度间隔

        for (int i = 0; i < rayCount; i++)
        {
            // 计算当前射线的角度（从左侧到右侧）
            float currentAngle = -halfAngle + angleStep * i;

            // 将角度转换为方向向量（2D）
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * eye.transform.right;

            // 发射射线
            RaycastHit2D hit = Physics2D.Raycast(
                eye.transform.position,
                rayDirection,
                sightRadius,
                targetMask
            );

            // 绘制射线（Scene视图可视化）
            Debug.DrawRay(eye.transform.position, rayDirection * sightRadius, Color.red);
                
            // 检测是否命中玩家
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
            {
                playerDetected = true;
            }
        }
        return playerDetected;
    }
}
