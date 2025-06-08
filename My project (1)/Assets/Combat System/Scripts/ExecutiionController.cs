using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ExecutionUIController：
/// 当某个敌人（CharacterStats）血量归零并且第一次触发时，由 GameManager 实例化此 UI。
/// 该 UI 固定在屏幕底部（由 Canvas 布局决定），只有当玩家靠近“倒地的敌人”一定距离内时才显示提示文字。
/// 提示文字为：“按 H 治愈 / 按 K 杀死”。
/// 玩家按下 H 或 K 时，调用 GameManager 对应接口，并销毁本 UI。
/// 
/// - 不再做世界坐标定位；UI 坐标由 Canvas 决定，且只需在 Inspector 手动微调文字位置。
/// - 只改本脚本，不动 GameManager、CharacterStats 逻辑。
/// </summary>
public class ExecutionUIController : MonoBehaviour
{  
    [Header("处决UI组件，这边请")]
    [Tooltip("在 Canvas 下的 Text 组件，用来显示 \"按 H 治愈 / 按 K 杀死\"")]
    public TextMeshProUGUI promptText;

    [Header("处决面触发距离")]
    [Tooltip("玩家与倒地敌人的最大显示距离，单位为世界单位")]
    public float showDistance = 3.0f;

    // 被处决的目标敌人 Stats
    private CharacterStats targetEnemy;

    // 玩家 Transform（通过 Tag=\"Player\" 获取）
    private Transform playerTransform;

    // 标记：是否已经执行过隐藏/显示逻辑后的初始化
    private bool isInitialized = false;

    void Awake()
    {
        // 一开始隐藏整个 UI，后面在 Init 中再激活。
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 由 GameManager 创建 prefab 后立即调用：
    /// 参数 enemyStats 是刚刚血量归零的敌人（CharacterStats 实例）。
    /// </summary>
    public void Init(CharacterStats enemyStats)
    {
        Debug.LogFormat(">> [ExecutionUIController] Init 被调用，targetEnemy={0}", enemyStats == null ? "NULL" : enemyStats.name);
        targetEnemy = enemyStats;
        Debug.LogFormat(">> [ExecutionUIController] Init 时目标 currentHP = {0}", targetEnemy.currentHP);

        // 先找到玩家 Transform（场景里需要给玩家打 Tag=\"Player\"）
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
        }
        else
        {
            Debug.LogError($"[ExecutionUIController] Init: 找不到 Tag=\"Player\" 的对象，请检查玩家是否打了 Tag。");
        }

        // 设置提示文字内容（如果要换语言或者格式，可以改这里）
        if (promptText != null)
        {
            promptText.text = "按 H 治愈\n按 K 杀死";
            // 一开始先隐藏文字，等玩家靠近再启用
            promptText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"[ExecutionUIController] Init: promptText 未关联，请在 Inspector 里拖入 Text 组件。");
        }

        // 激活整个 UI（但此时文字是隐藏的）
        gameObject.SetActive(true);

        isInitialized = true;
    }

    void Update()
    {
        if (promptText == null)
        {
            Debug.LogError(">> [ExecutionUIController] promptText 为 null，无法显示文字");
            return;
        }
        // 只有在初始化成功、且 targetEnemy 和 playerTransform 都不为空时才做后续逻辑
        if (!isInitialized || targetEnemy == null || playerTransform == null) return;
        Debug.Log(">> [ExecutionUIController] 正在 Update");

        // 如果目标敌人被二次确认过杀死/复活后 (Destroy 或 界面已经处理)，就直接隐藏并销毁 UI
        // 注意：CharacterStats.ConfirmHeal 会重置 currentHP 为 maxHP，但那时我们会在 GameManager 里先调用 ConfirmHeal，然后 Enemy 重置；
        // 如果“治愈”后我们不想再继续显示这个提示，UI 应该直接销毁。
        // 同理，“击杀”后敌人 GameObject 会被销毁，也要销毁 UI。
        // 所以这里检查一下 targetEnemy.currentHP != 0，就代表已经“治愈”过或者敌人被移除。
        if (targetEnemy.currentHP != 0)
        {
            Debug.Log(">> [ExecutionUIController] 发现 targetEnemy.currentHP != 0，将销毁 UI");
            Destroy(gameObject);
            return;
        }

        // 计算玩家与目标敌人的距离（世界坐标）
        float dist = Vector3.Distance(playerTransform.position, targetEnemy.transform.position);
        Debug.LogFormat(">> [ExecutionUIController] 当前距离 dist = {0}, showDistance = {1}", dist, showDistance);

        if (dist <= showDistance)
        {
            // 玩家接近，显示提示文字
            if (!promptText.gameObject.activeSelf)
                Debug.Log(">> [ExecutionUIController] 玩家进入范围，准备显示提示文字");
            promptText.gameObject.SetActive(true);

            // 监听键盘按键：H -> 治愈，K -> 杀死
            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log(">> [ExecutionUIController] 检测到玩家按下 H，准备调用 OnEnemyHealConfirmed");
                GameManager.Instance.OnEnemyHealConfirmed(targetEnemy);
                Destroy(gameObject);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.Instance.OnEnemyKillConfirmed(targetEnemy);
                Destroy(gameObject);
            }
        }
        else
        {
            // 玩家远离，隐藏提示文字
            if (promptText.gameObject.activeSelf)
                promptText.gameObject.SetActive(false);
        }
    }
}
