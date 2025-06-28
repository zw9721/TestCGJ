using UnityEngine;
using QFramework; // 引入 QFramework 命名空间

public class TestPlayerStates : MonoBehaviour
{
    private PlayerController mPlayerController;

    private void Awake()
    {
        mPlayerController = FindObjectOfType<PlayerController>(); // 查找场景中的 PlayerController 实例
        if (mPlayerController == null)
        {
            Debug.LogError("TestPlayerStates: PlayerController not found in scene!");
        }
    }

    /// <summary>
    /// 测试玩家被击退
    /// </summary>
    public void TestKnockBack()
    {
        if (mPlayerController != null)
        {
            // 示例参数：向右击退，力量 10，眩晕 1.5 秒
            mPlayerController.KnockBack(Vector2.right, 10f, 0.2f);
            Debug.Log("触发 KnockBack 测试！");
        }
    }

    /// <summary>
    /// 测试玩家进入眩晕状态
    /// </summary>
    public void TestStun()
    {
        if (mPlayerController != null)
        {
            // 示例参数：眩晕 2.0 秒
            mPlayerController.Stun(2.0f);
            Debug.Log("触发 Stun 测试！");
        }
    }
}