### 1. 核心设计哲学与架构

为了在48小时内高效地构建一个稳定且有趣的游戏，我们将严格遵循以下架构原则：

1. **状态驱动 (State-Driven):** **玩家的一切行为和表现都由一个唯一的`PlayerState`状态机驱动。** 这是整个架构的核心，确保了行为的统一性和可预测性，从根本上杜绝了逻辑冲突。
    
2. **数据与逻辑分离 (Decoupling Data from Logic):** 大量使用`ScriptableObject`来存储配置数据（如物品属性、关卡配置）。这使得游戏设计师可以在不修改代码的情况下，通过编辑资产来调整游戏平衡，极大提升迭代效率。
    
3. **事件驱动通信 (Event-Driven Communication):** 系统间通过一个轻量级的全局事件管理器进行通信，而不是直接相互引用。例如，`GameManager`广播`OnGameVictory`事件，`UIManager`和`AudioManager`订阅并响应。这实现了模块间的低耦合，便于并行开发和独立测试。
    
4. **组件化与单一职责 (Component-Based & Single Responsibility):** 每个`MonoBehaviour`脚本都应只负责一件事情。`PlayerController`只管移动，`PlayerAnimator`只管动画，`PlayerInteraction`只管交互。这使得代码更易于理解、维护和复用。
    

### 2. 状态驱动的玩家系统

这是玩家角色的核心。所有与玩家相关的系统都围绕`PlayerState`来运作。

#### 2.1. 核心状态机: `PlayerState.cs`

- **职责:** 作为玩家状态的“唯一真理之源”。管理当前状态，并在状态变更时通知所有其他系统。
    
- **设计思路:** 包含一个私有的`EPlayerState`枚举变量和一​​个公共的`OnStateChanged`事件。提供一个唯一的公共方法`ChangeState()`来修改状态。
    
- **`public enum EPlayerState`**:
    
    - `Idle` (静止)
        
    - `Moving` (移动中)
        
    - `Carrying_Idle` (搬运静止)
        
    - `Carrying_Moving` (搬运移动中)
        
    - `Struggling` (挣扎中)
        
    - `Stunned` (被眩晕)
        
    - `KnockedBack` (被撞飞)
        
- **关键成员:**
    
    - `public EPlayerState CurrentState { get; }`
        
    - `public event Action<EPlayerState> OnStateChanged;`
        
    - `public void ChangeState(EPlayerState newState)`
        

#### 2.2. 行为控制

- **`PlayerController.cs`**
    
    - **职责:** 处理物理移动，并**负责管理静止与移动间的状态切换**。
        
    - **设计思路:**
        
        - 在`FixedUpdate()`中，首先检查当前是否处于允许移动的状态（`Idle`, `Moving`, `Carrying_Idle`, `Carrying_Moving`）。
            
        - 在应用移动逻辑后，**必须检查玩家的当前速度**。
            
        - 如果速度接近于零，且当前状态是`Moving`或`Carrying_Moving`，则调用`PlayerState.ChangeState()`切换到对应的`Idle`状态。
            
        - 如果速度大于零，且当前状态是`Idle`或`Carrying_Idle`，则调用`PlayerState.ChangeState()`切换到对应的`Moving`状态。
            
    - **关键函数:** `void FixedUpdate()`
        
- **`PlayerInteraction.cs`**
    
    - **职责:** 处理抓取、挣扎、放下等交互逻辑。
        
    - **设计思路:**
        
        - 发起交互前，检查状态是否为`Idle`或`Moving`。
            
        - 在`Update()`中，检查状态是否为`Struggling`，如果是，则处理挣扎逻辑。
            
        - 挣扎成功后，根据玩家是否在移动，调用`PlayerState.ChangeState()`切换到`Carrying_Idle`或`Carrying_Moving`。
            
        - 挣扎失败后，调用`PlayerState.ChangeState(EPlayerState.Stunned)`。
            
    - **关键函数:** `void TryInteract()`, `void HandleStruggle()`
        

#### 2.3. 表现控制

- **`PlayerAnimator.cs`**
    
    - **职责:** 同步玩家状态与Animator Controller。
        
    - **设计思路:**
        
        1. 在`Awake()`或`OnEnable()`中，订阅`PlayerState.OnStateChanged`事件。
            
        2. 当事件触发时，在回调函数中获取新的状态，并将其转换为一个整数ID。
            
        3. 调用`Animator.SetInteger("StateID", (int)newState)`来驱动Animator Controller中的状态切换。现在Animator中将有更明确的状态，如`Idle`动画状态、`Moving`动画状态等，而不是一个大的混合树。
            
    - **关键函数:** `void HandleStateChange(EPlayerState newState)`
        
- **`PlayerAudio.cs`**
    
    - **职责:** 根据玩家状态播放对应的音效。
        
    - **设计思路:** 与`PlayerAnimator`完全相同。订阅`PlayerState.OnStateChanged`事件，并在回调函数中根据新状态，通过`AudioManager`播放对应的音效（如移动时播放脚步声，静止时停止）。
        
    - **关键函数:** `void HandleStateChange(EPlayerState newState)`
        

### 3. 系统架构与类设计

#### 3.1. 活物 (Living Objects)

- **`LivingObjectData.cs` (ScriptableObject)**
    
    - **职责:** 存储活物的静态数据。
        
    - **关键字段:** `string objectName`, `float struggleDifficulty`, `float playerStrugglePower`, 以及下述AI行为设计中提到的所有可配置参数。
        
- **`IInteractable.cs` (Interface)**
    
    - **职责:** 定义一个所有可交互对象必须遵守的契约。
        
    - **关键函数:** `void Interact(PlayerInteraction interactor)`
        
- **`LivingObjectBase.cs` (Abstract Class, implements `IInteractable`)**
    
    - **职责:** 所有活物的基类，处理被抓取、被制服、被回收等通用逻辑。
        
    - **关键函数:**
        
        - `public virtual void Interact(...)`: 被玩家交互时调用，通常会触发`PlayerState`进入挣扎状态。
            
        - `protected abstract void AIBehaviour()`: 留给子类实现的独特AI逻辑。
            
- **`MouseObject.cs`, `ChairObject.cs`, `StaplerObject.cs`**
    
    - **职责:** 继承自`LivingObjectBase`，并重写`AIBehaviour()`来实现各自独特的AI。
        

##### 3.1.1. AI行为设计详述

###### 1. 电脑鼠标 (回避型 - The Skittering Coward)

- **核心理念:** 它的目标不是对抗，而是生存。它通过快速、不可预测的移动来消耗玩家的时间和耐心。抓住它考验的是玩家的预判和围堵能力。
    
- **AI状态机:**
    
    - `STATE_WANDERING` (闲逛): 默认状态。无威胁，以低速在小范围内随机移动，偶尔停顿。
        
    - `STATE_FLEEING` (逃窜): 核心行为状态。当玩家靠近时，它会以高速向玩家相反的方向逃跑。
        
    - `STATE_HIDING` (躲藏): 逃窜一段时间后，它会尝试寻找一个藏身之处并静止不动。
        
- **关键参数 (`LivingObjectData.cs` 中定义):**
    
    - `float detectionRadius = 5f;`
        
    - `float fleeSpeed = 8f;`
        
    - `float wanderSpeed = 2f;`
        
    - `float fleeDuration = 3f;`
        
    - `float hideDuration = 4f;`
        
- **逻辑流程 (`MouseObject.cs` 中的 `AIBehaviour()`):**
    
    1. **Wandering State:** 随机闲逛，持续检测玩家距离，若小于`detectionRadius`则切换到`Fleeing`。
        
    2. **Fleeing State:** 启动`fleeDuration`计时器，沿玩家的反方向以`fleeSpeed`高速移动（可加入随机偏移增加不确定性）。计时器结束后，寻找藏身点，找到则切换到`Hiding`，否则继续`Fleeing`。
        
    3. **Hiding State:** 停止移动，启动`hideDuration`计时器。结束后切换回`Wandering`。
        

###### 2. 转椅 (区域封锁型 - The Territorial Spinner)

- **核心理念:** 一个可预测的、基于时序的陷阱。它不追逐玩家，而是惩罚那些侵入其领地且时机不当的玩家。击败它考验的是玩家的耐心和节奏感。
    
- **AI状态机:**
    
    - `STATE_IDLE` (静止): 安全状态。
        
    - `STATE_WIND_UP` (蓄力): 预警状态，有视觉和听觉警告。
        
    - `STATE_SPINNING` (旋转): 攻击状态，碰撞体可击退玩家。
        
    - `STATE_COOLDOWN` (冷却): 疲劳状态，可被抓捕的主要窗口。
        
- **关键参数 (`LivingObjectData.cs` 中定义):**
    
    - `float proximityRadius = 4f;`
        
    - `float windUpDuration = 1.0f;`
        
    - `float spinDuration = 3.0f;`
        
    - `float cooldownDuration = 2.5f;`
        
- **逻辑流程 (`ChairObject.cs` 中的 `AIBehaviour()`):**
    
    1. **Idle State:** 静止，检测玩家是否进入`proximityRadius`，是则切换到`Wind-up`。
        
    2. **Wind-up State:** 播放预警效果，启动`windUpDuration`计时器，结束后切换到`Spinning`。
        
    3. **Spinning State:** 激活击退碰撞体，高速旋转，启动`spinDuration`计时器，结束后切换到`Cooldown`。
        
    4. **Cooldown State:** 停止旋转，禁用碰撞体，启动`cooldownDuration`计时器，结束后切换回`Idle`。
        

###### 3. 订书机 (埋伏攻击型 - The Pouncing Predator)

- **核心理念:** 一个机会主义的攻击者。它大部分时间静止不动，等待玩家进入其攻击范围并发动突袭，旨在打断玩家的节奏。应对它考验的是玩家的反应速度和空间感知能力。
    
- **AI状态机:**
    
    - `STATE_IDLE` (潜伏): 默认状态，静止。
        
    - `STATE_TARGETING` (锁定): 攻击预备状态，短暂瞄准。
        
    - `STATE_LEAPING` (猛扑): 攻击执行状态，向锁定位置进行一次弹道式跳跃。
        
    - `STATE_RECOVERY` (恢复): 攻击后的硬直状态，可被抓捕的窗口。
        
- **关键参数 (`LivingObjectData.cs` 中定义):**
    
    - `float attackRange = 6f;`
        
    - `float targetingDuration = 0.5f;`
        
    - `float leapForce = 100f;`
        
    - `float recoveryDuration = 1.5f;`
        
- **逻辑流程 (`StaplerObject.cs` 中的 `AIBehaviour()`):**
    
    1. **Idle State:** 静止，持续检测玩家是否在`attackRange`内且无障碍物（Line of Sight），是则切换到`Targeting`。
        
    2. **Targeting State:** 播放预备动画，记录当前玩家位置，启动`targetingDuration`计时器，结束后切换到`Leaping`。
        
    3. **Leaping State:** 向记录的位置施加一次瞬时力进行跳跃，跳跃中可击中玩家。
        
    4. **Recovery State:** 落地后进入硬直，启动`recoveryDuration`计时器，结束后切换回`Idle`。
        

#### 3.2. 核心管理器 (Managers)

- **`GameManager.cs` (Singleton)**
    
    - **职责:** 游戏流程控制。管理计时器、计分、胜利/失败条件、跟踪需要回收的物品总数。
        
    - **关键事件:**
        
        - `public static event Action OnGameVictory;`
            
        - `public static event Action OnGameDefeat;`
            
        - `public static event Action<LivingObjectBase> OnObjectRecovered;`
            
    - **关键函数:** `void RegisterObject(...)`, `void CheckWinCondition()`
        
- **`UIManager.cs` (Singleton)**
    
    - **职责:** 响应游戏事件，更新所有UI元素。
        
    - **设计思路:** 订阅`GameManager`和`PlayerState`的事件。
        
    - **关键函数:** `void UpdateTimer(...)`, `void ShowStruggleBar(...)`, `void ShowVictoryScreen()`
        
- **`AudioManager.cs` (Singleton)**
    
    - **职责:** 集中管理和播放所有音效和音乐。
        
    - **设计思路:** 提供简单的静态接口供全局调用。内部可以使用一个字典或`ScriptableObject`来映射音效名称和`AudioClip`。
        
    - **关键函数:** `public static void PlaySound(string soundName)`, `public static void PlayMusic(string musicName)`
        

### 4. 48小时执行计划

| 阶段           | 时间 (小时) | 核心模块          | 关键任务                                                                                                                                                                                          | 产出                                            |
| ------------ | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------- |
| **1. 基础框架**  | 1 - 8   | **玩家状态 & 移动** | 1. 实现`PlayerState`和完整的`EPlayerState`枚举。<br>2. 实现`PlayerController`，**负责根据速度切换`Idle`/`Moving`状态**。  <br>3. 搭建基础场景和碰撞体。                                                                         | 一个可以移动，并且其`Idle`/`Moving`状态会根据其实际移动情况自动切换的角色。 |
| **2. 核心循环**  | 9 - 20  | **交互 & 游戏流程** | 1. 实现`PlayerInteraction`的抓取和挣扎逻辑，并能正确改变玩家状态至`Struggling`和`Carrying`系列状态。<br>2. 实现`LivingObjectBase`和`LivingObjectData`。<br>3. 实现`GameManager`的计时和胜负逻辑。<br>4. 实现`UIManager`的计时器和挣扎条。           | 玩家可以抓住一个物品，与之挣扎，并将其放入回收区。游戏可以正常胜利或失败。         |
| **3. 冲突与挑战** | 21 - 32 | **AI & 动画**   | 1. **根据AI详述**，分别实现`MouseObject`, `ChairObject`, `StaplerObject`的AI状态机和行为。<br>2. 实现`PlayerAnimator`，订阅状态事件，并驱动一个**包含所有精细状态**的Animator Controller。  <br>3. 完整实现`Stunned`和`KnockedBack`状态的行为和动画。 | 游戏具备完整的核心玩法，活物会根据详细设计攻击和躲避，玩家有精确匹配其行为的动画表现。   |
| **4. 打磨与整合** | 33 - 44 | **视听 & 游戏感**  | 1. `AudioManager`整合所有音效，并**根据精细状态播放**（如脚步声）。  <br>2. 添加粒子效果、屏幕震动等Juice效果。  <br>3. 大量测试和Bug修复，重点调整AI参数、挣扎难度等数值。                                                                                | 游戏具备完整的声画表现，操作手感和反馈良好，接近最终成品。                 |
| **5. 发布**    | 45 - 48 | **构建 & 收尾**   | 1. 最终的致命Bug修复。  <br>2. 构建Windows和Mac版本。  <br>3. 准备itch.io页面。                                                                                                                                  | 一个可发布、可玩的Game Jam游戏。                          |
