using UnityEngine;

/// <summary>
/// 挂载此脚本的 GameObject 在启动后等待指定秒数，然后自动销毁自身。
/// </summary>
public class AutoDestroyAfterDelay : MonoBehaviour
{
    [Tooltip("延迟多少秒后销毁自己（单位：秒）")]
    public float delaySeconds = 5.3f;

    void Start()
    {
        // 在 delaySeconds 秒后销毁该 GameObject
        Destroy(gameObject, delaySeconds);
    }
}