using QFramework;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator mAnimator;
    private Rigidbody2D mRigidbody;
    private SpriteRenderer mSpriteRenderer; // 添加 SpriteRenderer 引用
    private PlayerState mPlayerState;
    private PlayerController mPlayerController; // 添加 PlayerController 引用
    private Vector2 mLastMoveDirection = Vector2.down; // 默认向下，用于停止时面朝方向
    private Vector2 mMoveDirection = Vector2.zero;
    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mRigidbody = GetComponent<Rigidbody2D>();
        mSpriteRenderer = GetComponent<SpriteRenderer>(); // 获取 SpriteRenderer 引用
        mPlayerController = GetComponent<PlayerController>(); // 获取 PlayerController 引用
        if (mAnimator == null)
        {
            Debug.LogError("PlayerAnimator requires an Animator component on the same GameObject.");
        }
        if (mRigidbody == null)
        {
            Debug.LogError("PlayerAnimator requires a Rigidbody2D component on the same GameObject.");
        }
        if (mSpriteRenderer == null)
        {
            Debug.LogError("PlayerAnimator requires a SpriteRenderer component on the same GameObject.");
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
        mMoveDirection = mPlayerController.LastMoveDirection;

        if (mPlayerController.LastMoveDirection.magnitude > 0.1)
        {
            mLastMoveDirection = mPlayerController.LastMoveDirection;
        }

        // mAnimator.SetFloat("MoveX", mMoveDirection.x);
        // mAnimator.SetFloat("MoveY", mMoveDirection.y);

        // 根据水平移动方向翻转 Sprite
        if (mMoveDirection.x > 0.01f)
        {
            mSpriteRenderer.flipX = false;
        }
        else if (mMoveDirection.x < -0.01f)
        {
            mSpriteRenderer.flipX = true;
        }
        else if (mMoveDirection.x == 0) // 如果当前没有水平移动，则根据上次的水平移动方向决定翻转
        {
            if (mLastMoveDirection.x > 0.01f)
            {
                mSpriteRenderer.flipX = false;
            }
            else if (mLastMoveDirection.x < -0.01f)
            {
                mSpriteRenderer.flipX = true;
            }
        }

        // mAnimator.SetFloat("LastMoveX", mLastMoveDirection.x);
        // mAnimator.SetFloat("LastMoveY", mLastMoveDirection.y);
    }
}