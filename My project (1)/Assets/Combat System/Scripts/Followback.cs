using UnityEngine;

public class Followtarget : MonoBehaviour
{
    // 相机跟随参数
    public Vector3 offset = new Vector3(0, 2f, -5f); // 相对目标的偏移量
    public float rotationDamping = 3f; // 旋转跟随速度
    public float positionDamping = 3f; // 位置跟随速度

    // 可在Inspector中指定的跟随目标
    [SerializeField] private Transform player;
    [SerializeField] private Transform currentTarget;
    private Vector3 currentOffset;

    void Start()
    {
        // 如果在Inspector中未指定目标，则尝试查找标签为"Player"的对象
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        currentTarget = player;
        currentOffset = offset;
    }

    // 当脚本在Inspector中被修改时调用，确保offset变化时更新currentOffset
    void OnValidate()
    {
        currentOffset = offset;
    }

    void LateUpdate()
    {
        if (currentTarget == null)
        {
            ResetToPlayer();
            return;
        }

        // 计算目标位置（考虑目标的旋转）
        Vector3 targetPosition = currentTarget.position +
            currentTarget.rotation * currentOffset;

        // 平滑移动相机到目标位置
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * positionDamping
        );

        // 平滑旋转相机朝向目标
        Quaternion targetRotation = Quaternion.LookRotation(
            currentTarget.position - transform.position,
            currentTarget.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationDamping
        );
    }

    // 设置新目标并使用默认偏移量
    public void SetTarget(Transform newTarget)
    {
        SetTarget(newTarget, offset);
    }

    // 设置新目标并使用自定义偏移量
    public void SetTarget(Transform newTarget, Vector3 newOffset)
    {
        if (newTarget != null)
        {
            currentTarget = newTarget;
            currentOffset = newOffset;
        }
    }

    // 重置到玩家并使用默认偏移量
    public void ResetToPlayer()
    {
        ResetToPlayer(offset);
    }

    // 重置到玩家并使用自定义偏移量
    public void ResetToPlayer(Vector3 newOffset)
    {
        if (player != null)
        {
            currentTarget = player;
            currentOffset = newOffset;
        }
    }
}
