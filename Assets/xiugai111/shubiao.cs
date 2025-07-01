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
    // private void OnDrawGizmosSelected()
    // {
        
    //     if (MovePositions == null || MovePositions.Count == 0) return;
    //     GUIStyle style = new GUIStyle();
    //     style.normal.textColor = Color.red;
    //     style.fontSize = 14;
    //     style.fontStyle = FontStyle.Bold;
    //     for (int i = 0; i < MovePositions.Count; i++)
    //     {
    //         // �ڵ��Ϸ���������������
    //         Handles.Label(MovePositions[i] + Vector3.up * 0.3f, i.ToString(), style);
    //     }
    // }
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
        else
        {
          navMeshAgent.ResetPath(); 

         }
        
    }
    private void imageFilp() 
    {
        float angle = NormalizeAngle(eye.transform.eulerAngles.z);
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
            angle = angle % 360;  // ��ת��Ϊ (-360, 360)
            if (angle < 0)
            {
                angle += 360;     // �����Ƕ�ת��Ϊ [0, 360)
            }
            return angle;
        }
    

    private void move() 
    {
        Vector3 direction = (MovePositions[CurrentTarget] - transform.position).normalized;
        // �����ƶ�
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
    
    [Header("��Ұ����")]
    private float sightRadius = 3.5f;       // ��Ұ�뾶
    private float sightAngle = 60f;       // ��Ұ�Ƕȣ������Žǣ�
    private int rayCount = 10;            // ��������
    public LayerMask targetMask;         // Ŀ��㼶����Player��
    public LayerMask obstacleMask;
    private bool IsPlayerInSight()
    {
        bool playerDetected = false;
        float halfAngle = sightAngle / 2f;
        float angleStep = sightAngle / (rayCount - 1); // ÿ�����ߵĽǶȼ��

        for (int i = 0; i < rayCount; i++)
        {
            // ���㵱ǰ���ߵĽǶȣ�����ൽ�Ҳࣩ
            float currentAngle = -halfAngle + angleStep * i;

            // ���Ƕ�ת��Ϊ����������2D��
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * eye.transform.right;

            // ��������
            RaycastHit2D hit = Physics2D.Raycast(
                eye.transform.position,
                rayDirection,
                sightRadius,
                targetMask
            );

            // �������ߣ�Scene��ͼ���ӻ���
            Debug.DrawRay(eye.transform.position, rayDirection * sightRadius, Color.red);
                
            // ����Ƿ��������
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
            {
                playerDetected = true;
            }
        }
        return playerDetected;
    }
}
