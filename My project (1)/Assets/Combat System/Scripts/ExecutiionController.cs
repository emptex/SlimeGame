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

    [Header("与怪物的偏移设置")]
    [Tooltip("X 表示沿怪物本地右侧偏移多少，Y 表示抬高多少，Z 表示沿怪物本地前方向偏移多少")]
    public Vector3 localOffset = new Vector3(1.0f, 1.5f, 0.0f);

    private CharacterStats targetEnemy;   // 当前等待玩家选择的 Enemy

    void Awake()
    {
        Debug.Log("[ExecutionUIController] Awake, killButton=" + (killButton ? killButton.name : "null") + ", healButton=" + (healButton ? healButton.name : "null"));
        // 先把 UI 隐藏，只有 Init 时才显示
        gameObject.SetActive(false);

        // 绑定按钮回调
        killButton.onClick.AddListener(OnKillClicked);
        healButton.onClick.AddListener(OnHealClicked);
    }

    /// <summary>
    /// 初始化这个 UI，告诉它“现在要对哪一个敌人做处决选择”。
    /// 并把它放在怪物身侧 + 一点抬高，始终面朝摄像机。
    /// </summary>
    /// <param name="enemy">当前处于待处决状态的敌人 CharacterStats</param>
    public void Init(CharacterStats enemyStats)
    {
        targetEnemy = enemyStats;
        Debug.Log("[ExecutionUIController] Init(): 切换成可见 (SetActive(true))，Init 之前 activeSelf = " + gameObject.activeSelf);
        gameObject.SetActive(true);
        Debug.Log("[ExecutionUIController] Init(): 切换后 activeSelf = " + gameObject.activeSelf);

        // 计算“怪物身侧 + 抬高”的世界坐标：
        //    首先，拿到怪物的世界位置
        Vector3 basePos = targetEnemy.transform.position;
        //    然后，将 localOffset 从怪物的本地空间转换到世界空间：
        Vector3 worldOffset =
            targetEnemy.transform.right * localOffset.x   // 向本地右侧偏移
          + Vector3.up * localOffset.y   // 向世界 Y 轴方向抬高
          + targetEnemy.transform.forward * localOffset.z;  // 向本地“前方”偏移（如果需要）

        transform.position = basePos + worldOffset;

        // 让 UI 面朝主摄像机，保持可视
        if (Camera.main != null)
        {
            Vector3 toCamera = Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-toCamera, Vector3.up);
        }
    }

    void LateUpdate()
    {
        if (targetEnemy != null && gameObject.activeSelf)
        {
            // 每一帧都跟随怪物：重新计算世界坐标
            Vector3 basePos = targetEnemy.transform.position;
            Vector3 worldOffset =
                targetEnemy.transform.right * localOffset.x
              + Vector3.up * localOffset.y
              + targetEnemy.transform.forward * localOffset.z;

            transform.position = basePos + worldOffset;

            if (Camera.main != null)
            {
                Vector3 toCamera = Camera.main.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(-toCamera, Vector3.up);
            }
        }
    }

    private void OnKillClicked()
    {
        Debug.Log("[ExecutionUIController] OnKillClicked 被调用");
        if (targetEnemy != null)
        {
            Debug.Log("[ExecutionUIController] targetEnemy 不为 null，调用 GameManager.Instance.OnEnemyKillConfirmed");
            // 通知 GameManager：玩家选择“杀死”这个敌人
            GameManager.Instance.OnEnemyKillConfirmed(targetEnemy);
        }
        else
        {
            Debug.LogWarning("[ExecutionUIController] targetEnemy 为 null");
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
