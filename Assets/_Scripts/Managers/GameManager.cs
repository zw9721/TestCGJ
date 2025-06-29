using QFramework;
using System;
using UnityEngine;
using UnityEngine.Playables; // 新增
using Game; // 引入 Game 命名空间

public class GameManager : MonoSingleton<GameManager>
{
    public PlayableDirector introCutsceneDirector; // 引入 Timeline 播放器

    // 游戏事件
    public static event Action OnGameVictory;
    public static event Action OnGameDefeat;
    public static event Action<LivingObjectBase> OnObjectRecovered;

    public GameObject playerGameObject; // 主角 GameObject

    // 游戏状态变量
    public float gameDuration = 60f; // 游戏总时长 (秒)
    public float CurrentTime => mCurrentTime; // 当前剩余时间
    private float mCurrentTime;
    private int mScore = 0; // 玩家得分
    private int mTotalObjectsToRecover = 0; // 需要回收的物品总数
    private int mRecoveredObjectsCount = 0; // 已回收物品数

    public bool IsGameOver { get; private set; } = false;
    private bool isGameStarted = false; // 新增：控制游戏是否已开始

    public override void OnSingletonInit()
    {
        // 如果有开场动画，则播放动画并在动画结束后开始游戏
        if (introCutsceneDirector != null)
        {
            introCutsceneDirector.Play();
            introCutsceneDirector.stopped += HandleCutsceneStopped; // 订阅动画播放完成事件
        }
        else
        {
            // 没有开场动画，直接开始游戏
            StartGameLogic();
        }
    }

    private void HandleCutsceneStopped(PlayableDirector director)
    {
        // 动画播放完成后，开始游戏逻辑
        StartGameLogic();
        introCutsceneDirector.stopped -= HandleCutsceneStopped; // 取消订阅，避免重复调用
    }

    private void StartGameLogic()
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

        // 激活主角 GameObject
        if (playerGameObject != null)
        {
            playerGameObject.SetActive(true);
        }
        introCutsceneDirector.gameObject.SetActive(false); // 隐藏开场动画对象
        isGameStarted = true; // 游戏正式开始
        Debug.Log("游戏逻辑开始：计时器启动，物品已注册，主角已激活。");
        PixelGameJam.Audio.AudioManager.Instance.PlayMusic("游戏背景音乐"); // 播放游戏开始音效
    }

    private void Update()
    {
        if (IsGameOver || !isGameStarted) return; // 只有游戏开始且未结束时才执行计时

        mCurrentTime -= Time.deltaTime;
        if (mCurrentTime <= 0)
        {
            mCurrentTime = 0;
            CheckDefeatCondition();
        }
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
            Time.timeScale = 0; // 停止游戏时间
            PixelGameJam.Audio.AudioManager.Instance.PlaySound("游戏胜利"); // 播放胜利音效
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
            Time.timeScale = 0; // 停止游戏时间
            PixelGameJam.Audio.AudioManager.Instance.PlaySound("游戏失败"); // 播放失败音效
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
        if (introCutsceneDirector != null)
        {
            introCutsceneDirector.stopped -= HandleCutsceneStopped; // 取消订阅 Timeline 事件
        }
    }
}