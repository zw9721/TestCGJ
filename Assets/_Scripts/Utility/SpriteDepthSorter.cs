using UnityEngine;

/// <summary>
/// 根据Y轴坐标动态调整SpriteRenderer的Sorting Order，实现2D俯视角层级排序。
/// Y轴坐标越小（越靠下），Sorting Order越大，从而显示在前方。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] // 添加对Collider2D的依赖
public class SpriteDepthSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider; // 新增Collider2D引用
    private const int SortingOrderMultiplier = -100; // 乘以一个负数，确保Y值越小，Sorting Order越大

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>(); // 获取Collider2D组件
    }

    void Update()
    {
        // 根据物体碰撞盒中心点Y轴坐标设置Sorting Order
        // 乘以一个负数，使得Y值越小（越靠屏幕下方），Sorting Order越大，从而显示在更前面
        spriteRenderer.sortingOrder = (int)(objectCollider.bounds.center.y * SortingOrderMultiplier);
    }
}