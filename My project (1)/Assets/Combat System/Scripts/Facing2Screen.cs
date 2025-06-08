using UnityEngine;

/// <summary>
/// 让一个 World Space Canvas 下的 UI（比如血条）始终面向指定摄像机。
/// 如果没有在 Inspector 里手动指定 targetCamera，会尝试使用 Camera.main（场景中 Tag 为 MainCamera 的摄像机）。
/// 同时会插入 Debug.Log 帮助检查摄像机引用和旋转逻辑是否正确。
/// </summary>
public class FaceUICamera : MonoBehaviour
{
    [Header("—— 指定需要面朝的摄像机 ——")]
    [Tooltip("如果你的场景里摄像机不是默认的 MainCamera，可以在 Inspector 里把想要用的摄像机拖到这里。")]
    public Camera targetCamera;

    // 用来对比上一帧目标旋转，只有当角度变化时才打印日志
    private Quaternion lastTargetRotation;

    void Start()
    {
        // 如果没有在 Inspector 里赋值，就尝试用 Camera.main
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera != null)
            {
                Debug.Log($"[{nameof(FaceUICamera)}] Start(): 未指定 targetCamera，自动使用 Camera.main ({targetCamera.name})。");
            }
            else
            {
                Debug.LogError($"[{nameof(FaceUICamera)}] Start(): targetCamera 也没有（Camera.main 为 null）。请在 Inspector 里指定一个摄像机引用。");
                enabled = false;
                return;
            }
        }
        else
        {
            Debug.Log($"[{nameof(FaceUICamera)}] Start(): 使用 Inspector 里指定的摄像机：{targetCamera.name}。");
        }

        // 初始化 lastTargetRotation，方便后面比较是否有角度变化
        lastTargetRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            return;

        // 计算从当前物体指向摄像机的向量
        Vector3 dirToCamera = targetCamera.transform.position - transform.position;

        // 如果你只想让 UI 在水平面（XZ 平面）跟随摄像机转，不随着摄像机的俯仰角倾斜，就打开下一行：
        // dirToCamera.y = 0f;

        // 如果距离太小或者向量为零，就跳过本帧旋转
        if (dirToCamera.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning($"[{nameof(FaceUICamera)}] LateUpdate(): 物体与摄像机几乎重合，跳过本帧旋转。");
            return;
        }

        // 计算目标旋转，让本物体的本地“前方”（Z+ 朝向）对准摄像机
        Quaternion targetRot = Quaternion.LookRotation(dirToCamera);

        // 只有在旋转角度有明显变化的时候才打印调试信息
        float angleDelta = Quaternion.Angle(lastTargetRotation, targetRot);
        if (angleDelta > 0.01f)
        {
            Debug.Log($"[{nameof(FaceUICamera)}] LateUpdate(): dirToCamera.normalize = {dirToCamera.normalized}, 目标欧拉角 = {targetRot.eulerAngles}");
            lastTargetRotation = targetRot;
        }

        // 最终赋值，让物体面朝摄像机
        transform.rotation = targetRot;
    }
}
