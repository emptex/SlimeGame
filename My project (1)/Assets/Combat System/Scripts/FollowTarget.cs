using UnityEngine;

/// <summary>
/// 第三人称自由视角摄像头控制器
/// 功能：跟随目标、鼠标控制视角、滚轮缩放、障碍物检测
/// </summary>
public class FollowTarget : MonoBehaviour
{
    [Header("跟踪目标")]
    [SerializeField] private string targetTag = "Player"; // 目标的标签
    [SerializeField] private Transform target; // 直接指定目标 (可选)

    [Header("视角控制 (鼠标)")]
    [SerializeField] private float mouseSensitivity = 200f; // 鼠标灵敏度
    [SerializeField] private float pitchMin = -40f; // 俯仰角限制 (向下)
    [SerializeField] private float pitchMax = 80f;  // 俯仰角限制 (向上)

    [Header("距离与偏移")]
    [SerializeField] private float initialDistance = 1f; // 初始距离
    [SerializeField] private float minDistance = 0.5f;   // 最小缩放距离
    [SerializeField] private float maxDistance = 10f;  // 最大缩放距离
    [SerializeField] private float zoomSpeed = 5f;     // 滚轮缩放速度
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 3.0f, -3.0f); // 摄像机聚焦在目标身上的高度偏移

    [Header("移动与平滑")]
    [SerializeField] private float followSmoothTime = 0.1f; // 跟随位置的平滑时间

    [Header("障碍物检测")]
    [SerializeField] private bool avoidObstacles = true; // 是否启用障碍物检测
    [SerializeField] private LayerMask obstacleLayers; // 障碍物的层
    [SerializeField] private float collisionPadding = 0.2f; // 碰撞检测的缓冲距离


    // 私有变量
    private float yaw = 0f;      // 水平旋转角度 (Y轴)
    private float pitch = 20f;   // 垂直旋转角度 (X轴)
    private float currentDistance;
    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        FindTarget();
        currentDistance = initialDistance;

        // 隐藏并锁定鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FindTarget()
    {
        if (target != null) return;

        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
        {
            target = targetObj.transform;
        }
        else
        {
            Debug.LogError($"未找到标签为 '{targetTag}' 的游戏对象!");
        }
    }

    // 使用LateUpdate可以确保摄像机在所有物体都完成移动后再更新
    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        HandleInputAndRotation();
        HandleZoom();
        HandlePositioning();
    }

    /// <summary>
    /// 处理鼠标输入和视角旋转
    /// </summary>
    private void HandleInputAndRotation()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 更新旋转角度
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax); // 限制俯仰角

        // 设置摄像机旋转
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    /// <summary>
    /// 处理滚轮缩放
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    /// <summary>
    /// 处理摄像机的位置和障碍物检测
    /// </summary>
    private void HandlePositioning()
    {
        // 1. 计算目标中心点
        Vector3 targetCenter = target.position + positionOffset;

        // 2. 计算理想的摄像机位置 (无障碍物时)
        // 从目标中心点，根据当前旋转和距离，向后移动
        Vector3 desiredPosition = targetCenter - transform.forward * currentDistance;

        // 3. (可选) 障碍物检测
        float finalDistance = currentDistance;
        if (avoidObstacles)
        {
            RaycastHit hit;
            // 从目标中心向理想的摄像机位置发射一条射线
            if (Physics.Raycast(targetCenter, -transform.forward, out hit, currentDistance, obstacleLayers))
            {
                // 如果射线撞到了障碍物，将摄像机移动到碰撞点前面一点的位置
                finalDistance = hit.distance - collisionPadding;
            }
        }
        
        // 4. 计算最终位置
        Vector3 finalPosition = targetCenter - transform.forward * finalDistance;

        // 5. 使用平滑阻尼移动摄像机到最终位置
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, followSmoothTime);
    }
}