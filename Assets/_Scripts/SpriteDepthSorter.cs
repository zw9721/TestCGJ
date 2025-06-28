using UnityEngine;

/// <summary>
/// 根据Y轴坐标动态调整SpriteRenderer的Sorting Order，实现2D俯视角层级排序。
/// Y轴坐标越小（越靠下），Sorting Order越大，从而显示在前方。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteDepthSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private const int SortingOrderMultiplier = -100; // 乘以一个负数，确保Y值越小，Sorting Order越大

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 根据Y轴坐标设置Sorting Order
        // 乘以一个负数，使得Y值越小（越靠屏幕下方），Sorting Order越大，从而显示在更前面
        spriteRenderer.sortingOrder = (int)(transform.position.y * SortingOrderMultiplier);
    }
}