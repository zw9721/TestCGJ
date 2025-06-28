using QFramework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D mRigidbody;
    private PlayerState mPlayerState;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down; // 默认向下

    public float moveSpeed = 5f; // 移动速度

    private void Awake()
    {
        mRigidbody = GetComponent<Rigidbody2D>();
        if (mRigidbody == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D component on the same GameObject.");
        }
        mPlayerState = PlayerState.Instance;
    }

    private void FixedUpdate()
    {
        if (mRigidbody == null || mPlayerState == null) return;

        // 在 Struggling 状态下禁止移动
        if (mPlayerState.CurrentStateId == EPlayerState.Struggling || mPlayerState.CurrentStateId == EPlayerState.Stunned || mPlayerState.CurrentStateId == EPlayerState.KnockedBack)
        {
            mRigidbody.velocity = Vector2.zero;
            return; // 禁止移动，直接返回
        }

        // 获取输入
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 moveInput = new Vector2(horizontalInput, verticalInput);
        Vector2 moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        mRigidbody.velocity = moveDirection * moveSpeed;

        // 更新 LastMoveDirection
        if (moveDirection.magnitude > 0.1f)
        {
            LastMoveDirection = moveDirection;
        }

        // 根据速度检查并切换状态
        // 使用一个小的阈值来判断是否静止，避免浮点数误差
        if (mRigidbody.velocity.magnitude > 0.1f) // 玩家正在移动
        {
            if (mPlayerState.CurrentStateId == EPlayerState.Idle)
            {
                mPlayerState.ChangeState(EPlayerState.Moving);
            }
            else if (mPlayerState.CurrentStateId == EPlayerState.Carrying_Idle)
            {
                mPlayerState.ChangeState(EPlayerState.Carrying_Moving);
            }
        }
        else // 玩家静止
        {
            if (mPlayerState.CurrentStateId == EPlayerState.Moving)
            {
                mPlayerState.ChangeState(EPlayerState.Idle);
            }
            else if (mPlayerState.CurrentStateId == EPlayerState.Carrying_Moving)
            {
                mPlayerState.ChangeState(EPlayerState.Carrying_Idle);
            }
        }
    }

    /// <summary>
    /// 玩家被击退
    /// </summary>
    /// <param name="knockbackDirection">击退方向</param>
    /// <param name="knockbackForce">击退力</param>
    /// <param name="stunDuration">眩晕时长</param>
    public void KnockBack(Vector2 knockbackDirection, float knockbackForce, float stunDuration)
    {
        if (mRigidbody == null || mPlayerState == null) return;

        mPlayerState.ChangeState(EPlayerState.KnockedBack);
        mRigidbody.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        // 击退后进入眩晕状态，并在眩晕结束后恢复
        ActionKit.Delay(stunDuration, () =>
        {
            if (mPlayerState.CurrentStateId == EPlayerState.KnockedBack || mPlayerState.CurrentStateId == EPlayerState.Stunned)
            {
                // 击退或眩晕结束后，根据速度判断是 Idle 还是 Moving
                if (mRigidbody.velocity.magnitude > 0.1f)
                {
                    mPlayerState.ChangeState(EPlayerState.Moving);
                }
                else
                {
                    mPlayerState.ChangeState(EPlayerState.Idle);
                }
            }
        }).Start(this); // 使用 Start(this) 将 Action 绑定到 MonoBehaviour 的生命周期
    }

    /// <summary>
    /// 玩家进入眩晕状态
    /// </summary>
    /// <param name="stunDuration">眩晕时长</param>
    public void Stun(float stunDuration)
    {
        if (mPlayerState == null) return;

        mPlayerState.ChangeState(EPlayerState.Stunned);

        // 眩晕结束后恢复
        ActionKit.Delay(stunDuration, () =>
        {
            if (mPlayerState.CurrentStateId == EPlayerState.Stunned)
            {
                // 眩晕结束后，根据速度判断是 Idle 还是 Moving
                if (mRigidbody.velocity.magnitude > 0.1f)
                {
                    mPlayerState.ChangeState(EPlayerState.Moving);
                }
                else
                {
                    mPlayerState.ChangeState(EPlayerState.Idle);
                }
            }
        }).Start(this); // 使用 Start(this) 将 Action 绑定到 MonoBehaviour 的生命周期
    }
}