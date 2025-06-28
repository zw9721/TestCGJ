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
        PlayerFSM.AddState(EPlayerState.Idle, new PlayerIdleState(PlayerFSM, this));
        PlayerFSM.AddState(EPlayerState.Moving, new PlayerMovingState(PlayerFSM, this));
        PlayerFSM.AddState(EPlayerState.Carrying_Idle, new PlayerCarryingIdleState(PlayerFSM, this));
        PlayerFSM.AddState(EPlayerState.Carrying_Moving, new PlayerCarryingMovingState(PlayerFSM, this));
        PlayerFSM.AddState(EPlayerState.Struggling, new PlayerStrugglingState(PlayerFSM, this));
        PlayerFSM.AddState(EPlayerState.Stunned, new PlayerStunnedState(PlayerFSM, this));
        PlayerFSM.AddState(EPlayerState.KnockedBack, new PlayerKnockedBackState(PlayerFSM, this));

        PlayerFSM.OnStateChanged((previousState, nextState) =>
        {
            OnStateChanged?.Invoke(nextState);
            Debug.Log($"Player State Changed: {previousState} -> {nextState}");
        });

        // 初始状态
        PlayerFSM.StartState(EPlayerState.Idle);
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

public enum EPlayerState
{
    Idle,
    Moving,
    Carrying_Idle,
    Carrying_Moving,
    Struggling,
    Stunned,
    KnockedBack
}

// 玩家状态基类
public abstract class PlayerBaseState : AbstractState<EPlayerState, PlayerState>
{
    public PlayerBaseState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }

    protected override void OnEnter()
    {
        Debug.Log($"进入状态: {mFSM.CurrentStateId}");
    }

    protected override void OnExit()
    {
        Debug.Log($"退出状态: {mFSM.CurrentStateId}");
    }
}

// 具体状态实现
public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
}

public class PlayerMovingState : PlayerBaseState
{
    public PlayerMovingState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
}

public class PlayerCarryingIdleState : PlayerBaseState
{
    public PlayerCarryingIdleState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
}

public class PlayerCarryingMovingState : PlayerBaseState
{
    public PlayerCarryingMovingState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
}

public class PlayerStrugglingState : PlayerBaseState
{
    public PlayerStrugglingState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
}

public class PlayerStunnedState : PlayerBaseState
{
    public PlayerStunnedState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
}

public class PlayerKnockedBackState : PlayerBaseState
{
    public PlayerKnockedBackState(FSM<EPlayerState> fsm, PlayerState target) : base(fsm, target) { }
}