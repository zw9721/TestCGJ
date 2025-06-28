using QFramework;
using System;
using UnityEngine;
using Game; // 引入 Game 命名空间

public class GameManager : MonoSingleton<GameManager>
{
    // 游戏事件
    public static event Action OnGameVictory;
    public static event Action OnGameDefeat;
    public static event Action<LivingObjectBase> OnObjectRecovered;

    // 游戏状态变量
    public float gameDuration = 120f; // 游戏总时长 (秒)
    public float CurrentTime => mCurrentTime; // 当前剩余时间
    private float mCurrentTime;
    private int mScore = 0; // 玩家得分
    private int mTotalObjectsToRecover = 0; // 需要回收的物品总数
    private int mRecoveredObjectsCount = 0; // 已回收物品数

    public bool IsGameOver { get; private set; } = false;

    public override void OnSingletonInit()
    {
        mCurrentTime = gameDuration;
        mScore = 0;
        mRecoveredObjectsCount = 0;
        IsGameOver = false;

        // 订阅物品回收事件
        OnObjectRecovered += HandleObjectRecovered;

        // 获取场景中所有 LivingObjectBase 并注册
        LivingObjectBase[] livingObjects = FindObjectsOfType<LivingObjectBase>();
        foreach (LivingObjectBase obj in livingObjects)
        {
            RegisterObject(obj);
        }
    }

    private void Update()
    {
        if (IsGameOver) return;

        mCurrentTime -= Time.deltaTime;
        if (mCurrentTime <= 0)
        {
            mCurrentTime = 0;
            CheckDefeatCondition();
        }

        // 可以通过事件通知 UIManager 更新计时器
        // UIManager.Instance.UpdateTimer(mCurrentTime); // 假设 UIManager 存在
    }

    /// <summary>
    /// 注册需要回收的物品
    /// </summary>
    /// <param name="obj"></param>
    public void RegisterObject(LivingObjectBase obj)
    {
        mTotalObjectsToRecover++;
        Debug.Log($"注册物品: {obj.name}. 当前需要回收总数: {mTotalObjectsToRecover}");
    }

    /// <summary>
    /// 当物品被回收时调用
    /// </summary>
    /// <param name="obj"></param>
    private void HandleObjectRecovered(LivingObjectBase obj)
    {
        mRecoveredObjectsCount++;
        mScore += 100; // 假设回收一个物品加 100 分
        Debug.Log($"物品 {obj.name} 已回收. 已回收: {mRecoveredObjectsCount}/{mTotalObjectsToRecover}. 得分: {mScore}");
        CheckWinCondition();
    }

    /// <summary>
    /// 检查胜利条件
    /// </summary>
    private void CheckWinCondition()
    {
        if (mRecoveredObjectsCount >= mTotalObjectsToRecover && mTotalObjectsToRecover > 0)
        {
            IsGameOver = true;
            OnGameVictory?.Invoke();
            Debug.Log("游戏胜利！");
        }
    }

    /// <summary>
    /// 检查失败条件 (时间耗尽)
    /// </summary>
    private void CheckDefeatCondition()
    {
        if (mCurrentTime <= 0 && !IsGameOver)
        {
            IsGameOver = true;
            OnGameDefeat?.Invoke();
            Debug.Log("游戏失败！时间耗尽。");
        }
    }

    /// <summary>
    /// 触发物品回收事件
    /// </summary>
    /// <param name="obj">被回收的物品</param>
    public void TriggerObjectRecovered(LivingObjectBase obj)
    {
        OnObjectRecovered?.Invoke(obj);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnObjectRecovered -= HandleObjectRecovered; // 取消订阅事件
    }
}