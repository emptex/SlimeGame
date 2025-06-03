using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用于控制处决 UI 的显示与按钮回调。
/// 这个脚本需要挂在 ExecutionUI.prefab 的根节点（挂在那个 World Space Canvas 上）。
/// </summary>
public class ExecutionUIController : MonoBehaviour
{
    [Header("UI 引用")]
    public Button killButton;    // 拖：ExecutionUI.prefab 里“杀死”按钮
    public Button healButton;    // 拖：ExecutionUI.prefab 里“治愈”按钮

    private CharacterStats targetEnemy;   // 当前等待玩家选择的 Enemy

    void Awake()
    {
        // 先把 UI 隐藏，只有 Init 时才显示
        gameObject.SetActive(false);

        // 绑定按钮回调
        killButton.onClick.AddListener(OnKillClicked);
        healButton.onClick.AddListener(OnHealClicked);
    }

    /// <summary>
    /// 初始化这个 UI，告诉它“现在要对哪一个敌人做处决选择”。
    /// </summary>
    /// <param name="enemy">当前处于待处决状态的 EnemyHealth</param>
    public void Init(CharacterStats enemyStats)
    {
        targetEnemy = enemyStats;
        gameObject.SetActive(true);

        // 把 UI 定位到敌人头顶：假设敌人的根节点在地面高度，是 0，在上面加个偏移
        Vector3 headPos = targetEnemy.transform.position + Vector3.up * 2.0f; // 你可以根据敌人模型高度调整偏移量
        transform.position = headPos;

        // 让 UI 面朝主摄像机，保持可视
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        }
    }

    private void LateUpdate()
    {
        // 如果玩家和摄像机位置变化、敌人还没被销毁，保持 UI 跟随头顶
        if (targetEnemy != null && gameObject.activeSelf)
        {
            Vector3 headPos = targetEnemy.transform.position + Vector3.up * 2.0f;
            transform.position = headPos;
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
            }
        }
    }

    private void OnKillClicked()
    {
        if (targetEnemy != null)
        {
            // 通知 GameManager：玩家选择“杀死”这个敌人
            GameManager.Instance.OnEnemyKillConfirmed(targetEnemy);
        }
        Destroy(gameObject);
    }

    private void OnHealClicked()
    {
        if (targetEnemy != null)
        {
            // 通知 GameManager：玩家选择“治愈”这个敌人
            GameManager.Instance.OnEnemyHealConfirmed(targetEnemy);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // 注意：解绑事件，避免引用泄露
        killButton.onClick.RemoveListener(OnKillClicked);
        healButton.onClick.RemoveListener(OnHealClicked);
    }
}
