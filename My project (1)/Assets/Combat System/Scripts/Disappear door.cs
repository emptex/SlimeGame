using UnityEngine;

public class DistanceVisibility : MonoBehaviour
{
    public float visibilityDistance = 2f; // 可见性切换距离
    public Transform player; // 玩家的Transform引用
    public bool useRenderer = true; // 是否使用Renderer控制可见性
    public bool useCollider = false; // 是否使用Collider控制碰撞

    private Renderer objectRenderer;
    private Collider objectCollider;
    private bool isVisible = true;

    void Start()
    {
        // 如果未指定玩家，尝试查找标签为"Player"的对象
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // 获取物件的Renderer组件
        objectRenderer = GetComponent<Renderer>();

        // 获取物件的Collider组件
        objectCollider = GetComponent<Collider>();
    }

    void Update()
    {
        // 如果没有玩家引用，不执行后续逻辑
        if (player == null)
            return;

        // 计算与玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 根据距离决定物件的可见性
        if (distanceToPlayer <= visibilityDistance && isVisible)
        {
            // 距离小于等于阈值且当前可见，则隐藏物件
            HideObject();
        }
        else if (distanceToPlayer > visibilityDistance && !isVisible)
        {
            // 距离大于阈值且当前不可见，则显示物件
            ShowObject();
        }
    }

    // 隐藏物件
    void HideObject()
    {
        isVisible = false;

        // 如果启用了Renderer控制，则设置为不可见
        if (useRenderer && objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }

        // 如果启用了Collider控制，则禁用碰撞
        if (useCollider && objectCollider != null)
        {
            objectCollider.enabled = false;
        }
    }

    // 显示物件
    void ShowObject()
    {
        isVisible = true;

        // 如果启用了Renderer控制，则设置为可见
        if (useRenderer && objectRenderer != null)
        {
            objectRenderer.enabled = true;
        }

        // 如果启用了Collider控制，则启用碰撞
        if (useCollider && objectCollider != null)
        {
            objectCollider.enabled = true;
        }
    }

    // 当脚本在Inspector中被修改时调用
    void OnValidate()
    {
        // 确保距离值为正数
        visibilityDistance = Mathf.Max(0, visibilityDistance);
    }
}
