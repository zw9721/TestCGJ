# QFramework 项目开发规则

本文档旨在为基于 QFramework 框架的项目开发提供指导原则和规范，以确保代码的一致性、可维护性和可扩展性。

## 1. 架构 (Architecture)

*   项目应使用 `Architecture<T>` 作为应用的顶层架构，负责注册和管理 System、Model 和 Utility。
*   在 `Architecture` 的 `Init()` 方法中进行模块的注册。

## 2. MVC/MVVM 模式

*   推荐采用 MVC (Model-View-Controller) 或 MVVM (Model-View-ViewModel) 模式来组织代码。
*   **Model:** 继承 `AbstractModel`，定义应用的数据和状态。数据变更应通过事件或 `BindableProperty` 通知。
*   **View:** 负责 UI 的显示和用户输入的捕获。
*   **Controller:** 继承 `IController`，处理用户输入，发送 Command，并根据 Model 的变化更新 View。
*   使用 `BindableProperty<T>` 来实现数据的绑定和响应式更新，避免手动调用 `UpdateView()`。

## 3. 命令 (Command)

*   使用 `AbstractCommand` 来封装用户交互或业务逻辑操作。
*   Command 负责修改 Model 的状态。
*   Command 执行完成后，如果导致 Model 状态变化，应发送相应的 Event 或更新 `BindableProperty`。

## 4. 查询 (Query)

*   使用 `AbstractQuery<TResult>` 来封装复杂的只读数据获取操作。
*   Query 不应修改 Model 的状态。

## 5. 事件 (Event)

*   定义结构体作为事件类型，用于模块间的解耦通信。
*   使用 `SendEvent<T>()` 发送事件，使用 `RegisterEvent<T>()` 注册事件监听。
*   对于频繁变更的数据，优先考虑使用 `BindableProperty`。

## 6. 工具层 (Utility)

*   将外部依赖或通用的功能（如数据存储、网络请求等）抽象为实现 `IUtility` 接口的 Utility 类。
*   在 `Architecture` 中注册 Utility，并通过 `GetUtility<T>()` 获取实例。

## 7. UI 管理 (UIKit)

*   使用 `UIKit` 管理 UI 面板的生命周期。
*   自定义 UI 面板应继承 `UIPanel`，并实现 `OnInit`, `OnOpen`, `OnShow`, `OnHide`, `OnClose` 等生命周期方法。
*   使用 `ResLoader` 管理 UI 相关的资源加载和释放。

## 8. 资源管理 (ResKit)

*   使用 `ResLoader` 进行资源的加载和管理。
*   利用 `ResLoader` 的关联对象管理功能，确保资源释放时，相关的对象也能被正确销毁。

## 9. 对象池 (PoolKit)

*   对于需要频繁创建和销毁的对象（如子弹、特效等），使用 `SimpleObjectPool` 或 `SafeObjectPool` 进行管理，以优化性能和减少 GC 开销。
*   实现 `IPoolable` 接口以更好地控制对象的生命周期。

## 10. 时序动作 (ActionKit)

*   使用 `ActionKit` 来编排和执行一系列顺序或并行的动作。
*   支持与 DOTween 等第三方动画库集成。
* 如果有时序任务需求，使用ActionKit，而不是协程

## 11. 状态机 (FSMKit)

*   使用 `FSM<TStateId>` 来管理游戏对象或系统在不同状态间的切换逻辑。
*   可以使用链式 API 或类来定义状态。

## 12. JSON 处理 (JsonKit)

*   使用 `JSONObject` 类来创建、解析和访问 JSON 数据。

## 13. 单例 (SingletonKit)

*   对于需要在全局访问且只有一个实例的 MonoBehaviour 类，使用 `MonoSingleton<T>`。

## 14. 代码规范

*   遵循 C# 命名规范和 Unity 项目的最佳实践。
*   保持代码简洁、可读性高。
*   添加必要的注释，解释代码的意图和复杂逻辑。