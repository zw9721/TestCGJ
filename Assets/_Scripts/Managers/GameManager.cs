using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Game;

public class GameManager : MonoBehaviour
{
    // 手动实现单例模式
    private static GameManager mInstance;
    public static GameManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<GameManager>();
                if (mInstance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    mInstance = obj.AddComponent<GameManager>();
                }
            }
            return mInstance;
        }
    }

    public PlayableDirector introCutsceneDirector; // 引入 Timeline 播放器
    public GameObject playerGameObject; // 主角 GameObject

    // 游戏事件
    public static event Action OnGameVictory;
    public static event Action OnGameDefeat;
    public static event Action<LivingObjectBase> OnObjectRecovered;

    // 游戏状态变量
    public float gameDuration = 60f; // 游戏总时长 (秒)
    public float CurrentTime => mCurrentTime; // 当前剩余时间
    private float mCurrentTime;
    private int mScore = 0; // 玩家得分
    private int mTotalObjectsToRecover = 0; // 需要回收的物品总数
    private int mRecoveredObjectsCount = 0; // 已回收物品数

    public bool IsGameOver { get; private set; } = false;
    private bool isGameStarted = false;
    private HashSet<LivingObjectBase> registeredObjects = new HashSet<LivingObjectBase>();

    private void Awake()
    {
        // 实现健壮的单例模式，兼容手动放置在场景中的情况
        if (mInstance == null)
        {
            mInstance = this;
            DontDestroyOnLoad(gameObject); // 确保在场景切换时不被销毁
        }
        else if (mInstance != this)
        {
            Destroy(gameObject); // 如果已存在实例，则销毁自己
            return;
        }

        // 初始化逻辑
        InitializeGame();
    }

    private void InitializeGame()
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
        if (isGameStarted)
        {
            Debug.LogWarning("游戏逻辑已开始，无法重复启动。");
            return;
        }
        
        // 重置游戏状态
        mCurrentTime = gameDuration;
        mScore = 0;
        mRecoveredObjectsCount = 0;
        mTotalObjectsToRecover = 0;
        registeredObjects.Clear();
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
        isGameStarted = true;
        Debug.Log("游戏逻辑开始：计时器启动，物品已注册，主角已激活。");
    }

    private void Update()
    {
        if (IsGameOver || !isGameStarted) return;

        mCurrentTime -= Time.deltaTime;
        if (mCurrentTime <= 0)
        {
            mCurrentTime = 0;
            CheckDefeatCondition();
        }
    }

    public void RegisterObject(LivingObjectBase obj)
    {
        if (registeredObjects.Add(obj))
        {
            mTotalObjectsToRecover++;
            Debug.Log($"注册物品: {obj.name}. 当前需要回收总数: {mTotalObjectsToRecover}");
        }
        else
        {
            Debug.LogWarning($"尝试重复注册物品: {obj.name}");
        }
    }

    private void HandleObjectRecovered(LivingObjectBase obj)
    {
        mRecoveredObjectsCount++;
        mScore += 100;
        Debug.Log($"物品 {obj.name} 已回收. 已回收: {mRecoveredObjectsCount}/{mTotalObjectsToRecover}. 得分: {mScore}");
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (mRecoveredObjectsCount >= mTotalObjectsToRecover && mTotalObjectsToRecover > 0)
        {
            IsGameOver = true;
            OnGameVictory?.Invoke();
            Debug.Log("游戏胜利！");
            Time.timeScale = 0;
            // PixelGameJam.Audio.AudioManager.Instance.PlaySound("游戏胜利");
        }
    }

    private void CheckDefeatCondition()
    {
        if (mCurrentTime <= 0 && !IsGameOver)
        {
            IsGameOver = true;
            OnGameDefeat?.Invoke();
            Debug.Log("游戏失败！时间耗尽。");
            Time.timeScale = 0;
            // PixelGameJam.Audio.AudioManager.Instance.PlaySound("游戏失败");
        }
    }

    public void TriggerObjectRecovered(LivingObjectBase obj)
    {
        OnObjectRecovered?.Invoke(obj);
    }

    private void OnDestroy()
    {
        OnObjectRecovered -= HandleObjectRecovered;
        if (introCutsceneDirector != null)
        {
            introCutsceneDirector.stopped -= HandleCutsceneStopped;
        }
    }
}