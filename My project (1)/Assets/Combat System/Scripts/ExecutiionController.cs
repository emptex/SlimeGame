using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 精简版 ExecutionUI：
/// - GameManager 在敌人血量归零时实例化本 Prefab，并调用 Init(enemyStats)  
/// - 本脚本挂在 Prefab 根节点上，Inspector 指定一个 Image（或 TextMeshProUGUI）作为提示组件  
/// - 玩家靠近时显示提示，按 H/K 触发治愈/处决并销毁 UI；玩家走远也自动销毁 UI  
/// </summary>
public class ExecutionUIController : MonoBehaviour
{
    [Header("提示组件（Image 或 TextMeshProUGUI）")]
    [Tooltip("拖入一个 UI Image 或 TextMeshProUGUI，用于显示“按 H 治愈 / K 杀死”")]
    public Graphic promptGraphic;

    [Header("提示文字（仅 TextMeshProUGUI 有效）")]
    [Tooltip("如果用 TextMeshProUGUI，请拖入此字段；否则留空，使用 Image 表示")]
    public TextMeshProUGUI promptText;

    [Header("玩家靠近显示距离")]
    [Tooltip("玩家与倒地敌人的最大显示距离")]
    public float showDistance = 3f;

    private CharacterStats targetEnemy;
    private Transform playerTransform;

    /// <summary>
    /// 由 GameManager 实例化 Prefab 后调用
    /// </summary>
    public void Init(CharacterStats enemyStats)
    {
        targetEnemy = enemyStats;
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerGO ? playerGO.transform : null;

        // 配置文字（如果有）
        if (promptText != null)
            promptText.text = "按 H 治愈\n按 K 杀死";

        // 初始隐藏
        if (promptGraphic != null)
            promptGraphic.enabled = false;

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (targetEnemy == null || playerTransform == null) return;

        // 如果敌人已复活或被移除，销毁 UI
        if (targetEnemy.currentHP != 0)
        {
            Destroy(gameObject);
            return;
        }

        float dist = Vector3.Distance(playerTransform.position, targetEnemy.transform.position);

        if (dist <= showDistance)
        {
            // 显示提示
            promptGraphic.enabled = true;

            // 输入检测
            if (Input.GetKeyDown(KeyCode.H))
            {
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
            // 走远后隐藏并销毁
            Destroy(gameObject);
        }
    }
}