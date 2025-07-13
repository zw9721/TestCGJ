# 玩家状态和控制器实现计划

## 目标
根据《胡闹办公室》最终版程序设计文档 (PDD) V1.md 的要求，实现 `Assets/_Scripts/Player/PlayerController.cs` 和 `Assets/_Scripts/Player/PlayerState.cs`，并使用 QFramework 的 `FSMKit`。

## 核心设计原则
*   **状态驱动 (State-Driven):** 玩家的一切行为和表现都由一个唯一的 `PlayerState` 状态机驱动。
*   **数据与逻辑分离 (Decoupling Data from Logic):** 状态数据和行为逻辑分离。
*   **事件驱动通信 (Event-Driven Communication):** 状态变更通过事件通知。
*   **组件化与单一职责 (Component-Based & Single Responsibility):** `PlayerState` 负责状态管理，`PlayerController` 负责物理移动和状态切换。

## 计划详情

### 1. 定义 `EPlayerState` 枚举
在 `Assets/_Scripts/Player/PlayerState.cs` 中定义 `public enum EPlayerState`，包含以下状态：
*   `Idle` (静止)
*   `Moving` (移动中)
*   `Carrying_Idle` (搬运静止)
*   `Carrying_Moving` (搬运移动中)
*   `Struggling` (挣扎中)
*   `Stunned` (被眩晕)
*   `KnockedBack` (被撞飞)

### 2. 实现 `PlayerState.cs`
`PlayerState` 将作为玩家状态的“唯一真理之源”，管理当前状态并在状态变更时通知其他系统。

*   **类定义:**
    ```csharp
    using QFramework;
    using System;
    using UnityEngine;

    public class PlayerState : MonoSingleton<PlayerState>
    {
        public FSM<EPlayerState> PlayerFSM = new FSM<EPlayerState>();

        public EPlayerState CurrentStateId => PlayerFSM.CurrentStateId;
        public event Action<EPlayerState> OnStateChanged;

        public void ChangeState(EPlayerState newState)
        {
            PlayerFSM.ChangeState(newState);
        }

        public override void OnSingletonInit()
        {
            // 初始化 FSM 并添加所有状态
            // ... (具体状态添加在步骤3中实现)
            PlayerFSM.OnStateChanged((previousState, nextState) =>
            {
                OnStateChanged?.Invoke(nextState);
            });
        }

        private void Update()
        {
            PlayerFSM.Update();
        }

        private void FixedUpdate()
        {
            PlayerFSM.FixedUpdate();
        }

        protected override void OnDestroy()
        {
            PlayerFSM.Clear();
        }
    }
    ```

### 3. 为每个 `EPlayerState` 定义状态类
在 `PlayerState.cs` 内部，为每个 `EPlayerState` 定义一个继承自 `AbstractState<EPlayerState, PlayerState>` 的内部类。这些类将处理各自状态的进入、更新和退出逻辑。

*   **示例 (Idle 状态):**
    ```csharp
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
    }
    ```
*   所有状态类都将以类似方式定义，并在 `PlayerState.OnSingletonInit()` 中添加到 `PlayerFSM`。

### 4. 实现 `PlayerController.cs`
`PlayerController` 负责处理物理移动，并根据玩家速度管理 `Idle`/`Moving` 状态的切换。

*   **类定义:**
    ```csharp
    using QFramework;
    using UnityEngine;

    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D mRigidbody;
        private PlayerState mPlayerState;

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

            // 获取输入
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector2 moveInput = new Vector2(horizontalInput, verticalInput);
            Vector2 targetPosition = mRigidbody.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
            mRigidbody.MovePosition(targetPosition);

            // 如果没有输入，确保速度为零，以完全消除惯性
            if (moveInput.magnitude == 0)
            {
                mRigidbody.velocity = Vector2.zero;
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
    }
    ```

### 5. 实现 `PlayerAnimator.cs`
`PlayerAnimator` 负责同步玩家状态与 Animator Controller，并处理人物的四向行走动画及停止时面朝的方向。

*   **类定义:**
    ```csharp
    using QFramework;
    using UnityEngine;

    public class PlayerAnimator : MonoBehaviour
    {
        private Animator mAnimator;
        private Rigidbody2D mRigidbody;
        private PlayerState mPlayerState;
        private Vector2 mLastMoveDirection = Vector2.down; // 默认向下，用于停止时面朝方向

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

            if (currentMove.magnitude > 0.1f) // 正在移动
            {
                mLastMoveDirection = currentMove.normalized;
            }

            // 设置 Animator 的方向参数
            mAnimator.SetFloat("MoveX", mLastMoveDirection.x);
            mAnimator.SetFloat("MoveY", mLastMoveDirection.y);
        }
    }
    ```

### Mermaid 图示 (修正后)

```mermaid
graph TD
    A[PlayerAnimator (MonoBehaviour)] --> B[获取 Animator 组件];
    A --> G[获取 Rigidbody2D 组件];
    A --> C[获取 PlayerState 实例];
    C -- 订阅 OnStateChanged 事件 --> D[HandleStateChange(EPlayerState newState)];
    D -- 设置 Animator 状态参数 --> E[Animator.SetInteger("StateID", (int)newState)];
    G -- 在 Update/FixedUpdate 中获取速度 --> H[计算移动方向];
    H -- 设置 Animator 方向参数 --> I[Animator.SetFloat("MoveX/Y", direction)];
    A -- OnDisable/OnDestroy --> F[取消订阅 OnStateChanged 事件];
```

## Animator Controller 设置指南

为了让 Animator Controller 适用于 `PlayerAnimator.cs` 中的代码，您需要进行以下设置：

1.  **创建 Animator Controller**：
    *   在 Unity 项目视图中，右键点击 -> Create -> Animator Controller。
    *   将其命名为 `PlayerAnimatorController` (或您喜欢的任何名称)。
    *   将此 Animator Controller 拖拽到玩家 GameObject 的 Animator 组件上。

2.  **添加参数 (Parameters)**：
    *   在 Animator 窗口中，切换到 "Parameters" 选项卡。
    *   点击 "+" 按钮，添加以下参数：
        *   `StateID` (类型：`Int`)
        *   `MoveX` (类型：`Float`)
        *   `MoveY` (类型：`Float`)

3.  **创建动画状态 (Animation States)**：
    *   为 `EPlayerState` 中的每个状态创建对应的动画状态。例如：
        *   `Idle`
        *   `Moving` (这将是一个 Blend Tree)
        *   `Carrying_Idle`
        *   `Carrying_Moving` (这将是一个 Blend Tree)
        *   `Struggling`
        *   `Stunned`
        *   `KnockedBack`
    *   将相应的动画剪辑 (Animation Clips) 拖拽到这些状态中。

4.  **设置 Blend Tree (用于四向移动)**：
    *   对于 `Moving` 和 `Carrying_Moving` 状态，您需要创建一个 Blend Tree 来处理四向行走动画。
    *   在 Animator 窗口中，右键点击 -> Create State -> From New Blend Tree。
    *   双击进入 Blend Tree。
    *   在 Inspector 窗口中，将 Blend Type 设置为 `2D Freeform Directional`。
    *   将 Parameters 设置为 `MoveX` 和 `MoveY`。
    *   添加四个 Motion 字段，分别对应向上、向下、向左、向右的行走动画。
    *   为每个 Motion 设置其对应的 Position (例如：向上 (0, 1), 向下 (0, -1), 向左 (-1, 0), 向右 (1, 0))。
    *   如果需要，可以添加对角线方向的动画。

5.  **创建状态转换 (Transitions)**：
    *   从 `Any State` 到每个主要状态（如 `Idle`, `Moving`, `Struggling`, `Stunned`, `KnockedBack`）创建转换。
    *   这些转换的条件将基于 `StateID` 参数。例如：
        *   从 `Any State` 到 `Idle`：条件 `StateID == 0` (假设 `EPlayerState.Idle` 的值为 0)
        *   从 `Any State` 到 `Moving`：条件 `StateID == 1` (假设 `EPlayerState.Moving` 的值为 1)
        *   以此类推，为所有 `EPlayerState` 定义对应的 `StateID` 值。
    *   确保 `Has Exit Time` 被取消勾选，以便状态可以立即切换。
    *   调整 Transition Duration 以控制动画过渡的平滑度。

6.  **处理停止时面朝方向**：
    *   由于 `PlayerAnimator.cs` 会在玩家静止时保持 `mLastMoveDirection`，并将其 `x` 和 `y` 值传递给 `MoveX` 和 `MoveY` 参数，因此您的 Blend Tree 会自动处理停止时面朝的方向。当玩家停止时，`MoveX` 和 `MoveY` 会保持最后一次移动的方向，从而使动画停留在该方向的帧。

### 示例 Animator Controller 结构 (简化)

```mermaid
graph TD
    A[Entry] --> B[Idle];
    A --> C[Moving (Blend Tree)];
    A --> D[Carrying_Idle];
    A --> E[Carrying_Moving (Blend Tree)];
    A --> F[Struggling];
    A --> G[Stunned];
    A --> H[KnockedBack];

    subgraph Blend Tree (Moving)
        I[Walk_Up]
        J[Walk_Down]
        K[Walk_Left]
        L[Walk_Right]
    end

    subgraph Blend Tree (Carrying_Moving)
        M[CarryWalk_Up]
        N[CarryWalk_Down]
        O[CarryWalk_Left]
        P[CarryWalk_Right]
    end

    AnyState -- StateID == 0 --> B;
    AnyState -- StateID == 1 --> C;
    AnyState -- StateID == 2 --> D;
    AnyState -- StateID == 3 --> E;
    AnyState -- StateID == 4 --> F;
    AnyState -- StateID == 5 --> G;
    AnyState -- StateID == 6 --> H;

    C -- MoveX, MoveY --> I,J,K,L;
    E -- MoveX, MoveY --> M,N,O,P;