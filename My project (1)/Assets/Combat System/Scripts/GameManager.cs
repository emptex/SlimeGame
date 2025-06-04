using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private List<CharacterStats> enemies = new List<CharacterStats>();

    [Header("Execution UI 预制件")]
    public GameObject executionUIPrefab; // 拖：Assets/Combat System/Prefabs/ExecutionUI.prefab

    // 保证场景里有且只有一个不随场景销毁的GameManager实例
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[GameManager] 发现重复的 GameManager 实例，销毁：{name}");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        Debug.Log($"[GameManager] 单例初始化完成，实例名：{name}");
    }


    /// 声明一个调用函数RegisterEnemy，让 GameManager 订阅一个刚生成或场景中已有的 CharacterStats：当后续 stats 的 ApplyHPChange(...) 中发现血量归零时，会触发 OnZeroHP 事件，届时自动调用 HandleEnemyZeroHP(stats)
    public void RegisterEnemy(CharacterStats stats)
    {
        if (!enemies.Contains(stats))
        {
            enemies.Add(stats);
            // 订阅它的零血事件
            stats.OnZeroHP += HandleEnemyZeroHP;
            Debug.Log($"[GameManager] 已订阅 {stats.gameObject.name} 的 OnZeroHP");
        }
    }

    /// <summary>
    /// EnemyHealth 触发 OnZeroHP 时，会调用到这里：
    /// 负责弹出那个处决 UI 面板
    /// </summary>
    private void HandleEnemyZeroHP(CharacterStats stats)
    {
        Debug.Log($"[GameManager] 收到 {stats.gameObject.name} 血量归零事件，弹出处决 UI");
        if (executionUIPrefab == null)
        {
            Debug.LogError("[GameManager] executionUIPrefab 为 null！请在 Inspector 中拖入 Prefab");
            return;

        }
        // 1. 实例化 ExecutionUI
        GameObject uiGO = Instantiate(executionUIPrefab);
        uiGO.SetActive(true); // 强制激活
        Debug.Log("[GameManager] Instantiate 已完成，uiGO = " + uiGO.name
              + "，activeSelf = " + uiGO.activeSelf);

        // 2. 把它的 ExecutionUIController 拿到，调用 Init(stats)
        ExecutionUIController uiCtrl = uiGO.GetComponent<ExecutionUIController>();
        if (uiCtrl == null)
        {
            Debug.LogError("[GameManager] 执行 Prefab 后没拿到 ExecutionUIController 组件！");
            return;
        }
        Debug.Log("[GameManager] 拿到 uiCtrl，准备调用 Init()，调用前 uiGO.activeSelf = " + uiGO.activeSelf);
        uiCtrl.Init(stats);
        Debug.Log("[GameManager] 调用完 Init(), uiGO.activeSelf = " + uiGO.activeSelf);
    }

    /// <summary>
    /// 当玩家在 UI 面板点“杀死”时，ExecutionUIController 会调用到这里
    /// </summary>
    public void OnEnemyKillConfirmed(CharacterStats enemyStats)
    {
        // 这里可以加“红白条增长”逻辑
        // 例如：RedWhiteManager.Instance.AddRed(1); RedWhiteManager.Instance.AddWhite(-1);

        // 真正让敌人销毁
        enemyStats.ConfirmKill();
    }

    /// <summary>
    /// 当玩家在 UI 面板点“治愈”时，ExecutionUIController 会调用到这里
    /// </summary>
    public void OnEnemyHealConfirmed(CharacterStats enemyStats)
    {
        // 这里可以加“红白条增长”逻辑
        // 例如：RedWhiteManager.Instance.AddWhite(1); RedWhiteManager.Instance.AddRed(-1);

        // 让敌人复活
        enemyStats.ConfirmHeal();
    }
}

