using UnityEngine;
using UnityEngine.UI;
using QFramework; // 如果需要 MonoSingleton 或其他 QFramework 特性

public class StruggleSlider : MonoBehaviour
{
    public Slider slider; // 引用 Slider UI 组件
    public GameObject SpaceBar; // 引用 Struggle Bar GameObject

    private void Awake()
    {
        if (slider == null)
        {
            Debug.LogError("StruggleSlider: Slider component is not assigned!");
        }
    }

    private void OnEnable()
    {
        // 订阅 PlayerInteraction 的事件
        PlayerInteraction.OnStruggleProgressUpdated += UpdateSlider;
        PlayerInteraction.OnStruggleBarVisibilityChanged += SetVisibility;
    }

    private void OnDisable()
    {
        // 取消订阅事件
        PlayerInteraction.OnStruggleProgressUpdated -= UpdateSlider;
        PlayerInteraction.OnStruggleBarVisibilityChanged -= SetVisibility;
    }

    private void UpdateSlider(float progress)
    {
        if (slider != null)
        {
            slider.value = progress;
        }
    }

    private void SetVisibility(bool isVisible)
    {
        if (slider != null)
        {
            slider.gameObject.SetActive(isVisible);
            SpaceBar.SetActive(isVisible);
        }
    }
}