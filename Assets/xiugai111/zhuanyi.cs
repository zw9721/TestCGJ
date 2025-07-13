using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zhuanyi : Enemy
{
    private float attackTime = 0;
    private float attackCD = 4f;
    private float detectTime = 0f;
    private float detectCD = 1f;
    public GameObject target;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsBe)
        {
            if (attackTime > 0)
            {
                attackTime -= Time.deltaTime;
            }
            else
            {
                DetectPlayer();
            } 
        }
    }
    IEnumerator Attack(Vector3 targetPosition)
    {
        animator.SetBool("Attack", true);
        StartCoroutine(AttackShutDown());
        Vector3 direction = (targetPosition - transform.position).normalized;
        float time = 0;
        while (time<1f) 
        {
            time += Time.deltaTime;
            if (IsPlayerInSight(AttackRadius, AttackAngle, AttackRayCount, Color.green))
            {
                Debug.Log("���е���");
                PlayerController PlayerController = target.GetComponent<PlayerController>();
                if (PlayerController != null)
                {
                    PlayerController.KnockBack(Vector2.down, 10f, 0.2f); // 触发击退
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
            if (IsPlayerInSight(sightRadius, sightAngle, rayCount, Color.red))
            {
                detectTime = detectCD;
                StartCoroutine(Attack(target.transform.position));
                attackTime = attackCD;
            }
        }
    }
    [Header("��Ұ����")]
    private float sightRadius = 1.5f;       // ��Ұ�뾶
    private float sightAngle = 360f;       // ��Ұ�Ƕȣ������Žǣ�
    private int rayCount = 14;            // ��������
    public LayerMask targetMask;         // Ŀ��㼶����Player��
    public LayerMask obstacleMask;
    private float AttackRadius = 1.2f;       // �����뾶
    private float AttackAngle = 360f;       // ��Ұ�Ƕȣ������Žǣ�
    private int AttackRayCount = 14;            // ��������
    private bool IsPlayerInSight(float sightRadius, float sightAngle, int rayCount, Color color)
    {
        bool playerDetected = false;
        float halfAngle = sightAngle / 2f;
        float angleStep = sightAngle / (rayCount - 1); // ÿ�����ߵĽǶȼ��

        for (int i = 0; i < rayCount; i++)
        {
            // ���㵱ǰ���ߵĽǶȣ�����ൽ�Ҳࣩ
            float currentAngle = -halfAngle + angleStep * i;

            // ���Ƕ�ת��Ϊ����������2D��
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * transform.right;

            // ��������
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position + new Vector3(0, 0.5f, 0),
                rayDirection,
                sightRadius,
                targetMask
            );

            // �������ߣ�Scene��ͼ���ӻ���
            Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), rayDirection * sightRadius, color);

            // ����Ƿ��������
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
            {
                playerDetected = true;
                target = hit.collider.gameObject;
            }
        }
        return playerDetected;
    }
}
