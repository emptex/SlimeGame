using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TeleportOnTrigger : MonoBehaviour
{
    [Header("传送目标")]
    [Tooltip("玩家进入触发器后要被传送到此 Transform 的位置")]
    public Transform teleportTarget;

    [Header("玩家标记")]
    [Tooltip("只有带有此 Tag 的物体才会被传送")]
    public string playerTag = "Player";

    void Awake()
    {
        // 确保本物体的 Collider 是 Trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[{nameof(TeleportOnTrigger)}] 自动将 Collider 设置为 Trigger。");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 只处理指定 Tag 的玩家
        if (other.CompareTag(playerTag) && teleportTarget != null)
        {
            other.transform.position = teleportTarget.position;
        }
    }
}