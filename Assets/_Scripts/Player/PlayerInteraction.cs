using QFramework;
using UnityEngine;
using Game; // 引入 Game 命名空间

public class PlayerInteraction : MonoBehaviour
{
    private PlayerState mPlayerState;
    private IInteractable mCurrentInteractable;
    private bool mIsCarrying = false;
    private LivingObjectBase mCarriedObject;

    // 挣扎进度条相关变量
    private float mStruggleProgress = 0f; // 当前挣扎进度 (0 到 100)
    public float struggleIncreasePerClick = 20f; // 每次点击交互键增加的进度
    public float struggleDecayRate = 10f; // 进度条每秒下降的基础速率
    public float struggleSuccessThreshold = 100f; // 挣扎成功的进度阈值
    public float struggleFailThreshold = 0f; // 挣扎失败的进度阈值


    public Transform carryPoint; // 玩家搬运物品的位置
    public float interactionRadius = 1.5f; // 交互检测半径
    public LayerMask interactableLayer; // 可交互对象的层

    private Rigidbody2D mRigidbody; // 新增 Rigidbody2D 引用

    private void Awake()
    {
        mRigidbody = GetComponent<Rigidbody2D>(); // 获取 Rigidbody2D 引用
        mPlayerState = PlayerState.Instance;
        if (carryPoint == null)
        {
            Debug.LogError("PlayerInteraction: CarryPoint is not assigned!");
        }
        if (mRigidbody == null)
        {
            Debug.LogError("PlayerInteraction requires a Rigidbody2D component on the same GameObject.");
        }
    }

    private void Update()
    {
        // 检查交互输入 (例如，按下 E 键)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        // 处理挣扎逻辑 (如果处于挣扎状态)
        if (mPlayerState.CurrentStateId == EPlayerState.Struggling)
        {
            HandleStruggle();
        }

        // 如果正在搬运，更新搬运物品的位置
        if (mIsCarrying && mCarriedObject != null && carryPoint != null)
        {
            mCarriedObject.transform.position = carryPoint.position;
            // 可以根据需要调整旋转等
        }
    }

    public void TryInteract()
    {
        if (mPlayerState.CurrentStateId == EPlayerState.Idle || mPlayerState.CurrentStateId == EPlayerState.Moving)
        {
            if (mIsCarrying)
            {
                DropCarriedObject();
            }
            else
            {
                DetectAndInteract();
            }
        }
        else if (mPlayerState.CurrentStateId == EPlayerState.Carrying_Idle || mPlayerState.CurrentStateId == EPlayerState.Carrying_Moving)
        {
            // 如果已经搬运，再次交互是放下
            DropCarriedObject();
        }
    }

    private void DetectAndInteract()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);

        IInteractable closestInteractable = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D hitCollider in hitColliders)
        {
            IInteractable interactable = hitCollider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector2.Distance(transform.position, hitCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        if (closestInteractable != null)
        {
            mCurrentInteractable = closestInteractable;
            mCurrentInteractable.Interact(this); // 调用可交互对象的 Interact 方法
            // 假设 Interact 方法会触发玩家进入挣扎状态
            // 如果 Interact 方法不直接触发挣扎，这里需要根据 Interact 的返回值或事件来判断
            // 暂时假设 Interact 会导致玩家进入 Struggling 状态
            mPlayerState.ChangeState(EPlayerState.Struggling);
        }
        else
        {
            Debug.Log("没有检测到可交互对象。");
        }
    }

    public void StartStruggle(LivingObjectBase objectToStruggleWith)
    {
        // 确保只有在合适的状态下才能开始挣扎
        if (mPlayerState.CurrentStateId == EPlayerState.Idle || mPlayerState.CurrentStateId == EPlayerState.Moving)
        {
            mCarriedObject = objectToStruggleWith;
            mPlayerState.ChangeState(EPlayerState.Struggling);
            mStruggleProgress = 0f; // 初始化挣扎进度
            Debug.Log($"开始与 {objectToStruggleWith.name} 挣扎！当前难度: {objectToStruggleWith.struggleDifficulty}");
        }
    }

    private void HandleStruggle()
    {
        // 挣扎进度条逻辑
        if (Input.GetKeyDown(KeyCode.E)) // 每次点击交互键增加进度
        {
            mStruggleProgress += struggleIncreasePerClick;
            Debug.Log($"挣扎进度: {mStruggleProgress}");
        }

        // 进度条持续下降，下降速度受物体难度影响
        mStruggleProgress -= struggleDecayRate * mCarriedObject.struggleDifficulty * Time.deltaTime;

        // 限制进度条范围
        mStruggleProgress = Mathf.Clamp(mStruggleProgress, struggleFailThreshold, struggleSuccessThreshold);

        // 判断挣扎结果
        if (mStruggleProgress >= struggleSuccessThreshold)
        {
            // 挣扎成功
            Debug.Log("挣扎成功！");
            mIsCarrying = true;
            if (mCarriedObject != null)
            {
                mCarriedObject.transform.SetParent(carryPoint); // 将物品设置为搬运点的子级
                mCarriedObject.transform.localPosition = Vector3.zero; // 重置本地位置
            }

            if (mRigidbody.velocity.magnitude > 0.1f)
            {
                mPlayerState.ChangeState(EPlayerState.Carrying_Moving);
            }
            else
            {
                mPlayerState.ChangeState(EPlayerState.Carrying_Idle);
            }
            mStruggleProgress = 0f; // 重置进度
            mCarriedObject = null; // 清除搬运对象，因为现在已经成功搬运
        }
        else if (mStruggleProgress <= struggleFailThreshold)
        {
            // 挣扎失败
            Debug.Log("挣扎失败！");
            mPlayerState.ChangeState(EPlayerState.Stunned);
            mIsCarrying = false;
            mCarriedObject = null;
            mStruggleProgress = 0f; // 重置进度
        }
    }

    private void DropCarriedObject()
    {
        if (mCarriedObject != null)
        {
            mCarriedObject.transform.SetParent(null); // 解除父级关系
            // 可以添加一个小的推力或放置动画
            Debug.Log($"放下 {mCarriedObject.name}");
            mCarriedObject = null;
            mIsCarrying = false;

            if (mRigidbody.velocity.magnitude > 0.1f)
            {
                mPlayerState.ChangeState(EPlayerState.Moving);
            }
            else
            {
                mPlayerState.ChangeState(EPlayerState.Idle);
            }
        }
    }

    // 可视化交互范围 (仅在编辑器中显示)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}