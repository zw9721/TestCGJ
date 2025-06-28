using UnityEngine;

namespace Game
{
    public class LivingObjectBase : MonoBehaviour, IInteractable
    {
        public float struggleDifficulty = 1.0f; // 挣扎难度，默认1.0

        public virtual void Interact(PlayerInteraction interactor)
        {
            Debug.Log($"{gameObject.name} 被玩家交互了！");
            // 触发玩家进入挣扎状态的逻辑
            interactor.StartStruggle(this); // 调用 PlayerInteraction 的 StartStruggle 方法
        }
    }
}