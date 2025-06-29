using QFramework;
using UnityEngine;
using Game; // 添加此行

public class CollectBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 尝试获取碰撞到的对象的 LivingObjectBase 组件
        LivingObjectBase livingObject = other.GetComponent<LivingObjectBase>();

        // 如果碰撞到的对象是 LivingObjectBase 并且不是玩家自身
        if (livingObject != null)
        {
            // 触发 GameManager 中的物品回收事件
            GameManager.Instance.TriggerObjectRecovered(livingObject);
            Debug.Log($"CollectBox: Detected {livingObject.name}. Triggering OnObjectRecovered event.");

            // 销毁被回收的物品
            Destroy(other.gameObject);
            PixelGameJam.Audio.AudioManager.Instance.PlaySound("物品收集"); // 播放物品回收音效
        }
    }
}