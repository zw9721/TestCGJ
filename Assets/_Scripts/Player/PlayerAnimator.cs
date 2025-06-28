using QFramework;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator mAnimator;
    private Rigidbody2D mRigidbody;
    private PlayerState mPlayerState;
    private Vector2 mLastMoveDirection = Vector2.down; // 默认向下，用于停止时面朝方向
    private Vector2 mMoveDirection = Vector2.zero;
    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mRigidbody = GetComponent<Rigidbody2D>();
        if (mAnimator == null)
        {
            Debug.LogError("PlayerAnimator requires an Animator component on the same GameObject.");
        }
        if (mRigidbody == null)
        {
            Debug.LogError("PlayerAnimator requires a Rigidbody2D component on the same GameObject.");
        }
    }

    private void OnEnable()
    {
        mPlayerState = PlayerState.Instance;
        if (mPlayerState != null)
        {
            mPlayerState.OnStateChanged += HandleStateChange;
        }
    }

    private void OnDisable()
    {
        if (mPlayerState != null)
        {
            mPlayerState.OnStateChanged -= HandleStateChange;
        }
    }

    private void HandleStateChange(EPlayerState newState)
    {
        if (mAnimator != null)
        {
            mAnimator.SetInteger("StateID", (int)newState);
        }
    }

    private void Update()
    {
        if (mAnimator == null || mRigidbody == null) return;
        Vector2 currentMove = mRigidbody.velocity;

        mMoveDirection = currentMove.normalized;

        if (currentMove.magnitude > 0.1)
        {
            mLastMoveDirection = currentMove.normalized;
        }

        mAnimator.SetFloat("MoveX", mMoveDirection.x);
        mAnimator.SetFloat("MoveY", mMoveDirection.y);

        mAnimator.SetFloat("LastMoveX", mLastMoveDirection.x);
        mAnimator.SetFloat("LastMoveY", mLastMoveDirection.y);
    }
}