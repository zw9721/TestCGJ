using QFramework;
using UnityEngine;
using UnityEngine.UI; // 引入 UI 命名空间
using Game; // 引入 Game 命名空间
using TMPro; // 如果使用 TextMeshPro，需要引入此命名空间

public class UIManager : MonoSingleton<UIManager>
{
    // UI 元素引用
    public TextMeshProUGUI timerText; // 计时器文本
    public GameObject victoryScreen; // 胜利屏幕
    public GameObject defeatScreen; // 失败屏幕

    public override void OnSingletonInit()
    {
        // 初始化 UI 元素状态
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(false);
        }
        if (defeatScreen != null)
        {
            defeatScreen.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // 订阅 GameManager 事件
        GameManager.OnGameVictory += HandleGameVictory;
        GameManager.OnGameDefeat += HandleGameDefeat;
        GameManager.OnObjectRecovered += HandleObjectRecovered;
    }

    private void OnDisable()
    {
        // 取消订阅 GameManager 事件
        GameManager.OnGameVictory -= HandleGameVictory;
        GameManager.OnGameDefeat -= HandleGameDefeat;
        GameManager.OnObjectRecovered -= HandleObjectRecovered;
    }

    private void Update()
    {
        // 更新计时器 UI (如果 GameManager 提供了当前时间)
        if (timerText != null && GameManager.Instance != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(GameManager.Instance.CurrentTime)}s";
        }
    }

    /// <summary>
    /// 处理游戏胜利事件
    /// </summary>
    private void HandleGameVictory()
    {
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(true);
        }
        Debug.Log("UIManager: 显示胜利屏幕");
    }

    /// <summary>
    /// 处理游戏失败事件
    /// </summary>
    private void HandleGameDefeat()
    {
        if (defeatScreen != null)
        {
            defeatScreen.SetActive(true);
        }
        Debug.Log("UIManager: 显示失败屏幕");
    }

    /// <summary>
    /// 处理物品回收事件
    /// </summary>
    /// <param name="obj"></param>
    private void HandleObjectRecovered(LivingObjectBase obj)
    {
        // 可以更新计分 UI 或显示回收提示
        Debug.Log($"UIManager: 物品 {obj.name} 已回收.");
    }

    /// <summary>
    /// 根据玩家状态显示/隐藏挣扎条
    /// </summary>
    /// <param name="newState"></param>

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 在 OnDestroy 中取消订阅，以防 OnDisable 未被调用
        GameManager.OnGameVictory -= HandleGameVictory;
        GameManager.OnGameDefeat -= HandleGameDefeat;
        GameManager.OnObjectRecovered -= HandleObjectRecovered;
    }
}