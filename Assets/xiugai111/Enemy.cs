using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game; // 引入 Game 命名空间s

public class Enemy : LivingObjectBase
{
    public bool IsBe = false;
    public Vector3 myPosition;
    private void Awake()
    {
        myPosition = transform.position;
    }
    public override void ResetThis()
    {
        IsBe = false;
        // 恢复动画
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true; // 恢复动画组件
        }
        // transform.position = myPosition;
    }

    public override void Interact(PlayerInteraction interactor)
    {
        base.Interact(interactor);
        if (!IsBe)
        {
            IsBe = true;
            //动画组件禁用
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false; // 禁用动画组件
            }
        }
        // Debug.Log($"{gameObject.name} 被玩家交互了！");
    }
}
