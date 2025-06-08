using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private List<CharacterStats> enemies = new List<CharacterStats>();


    // 存储当前关卡中，玩家角色的「红条」和「白条实际值」，用来跨场景继承
    [HideInInspector] public int playerRed;
    [HideInInspector] public int playerWhiteReal;

    // 标记是否刚刚创建实例（用于区分「第一关」或后续关卡）
    private bool justCreated = false;

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

        // 初始化第一关红白条
        justCreated = true;
        playerRed = 100;
        playerWhiteReal = 100;
        SceneManager.sceneLoaded += OnLevelLoaded; // 监听场景加载完成事件，以便给新关卡的玩家传值
    }
    /// 每当有一个新场景加载完成，就会调用这个回调，把上次关卡保存的红白条，传到新场景里的玩家身上。
    private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] 场景 {scene.name} 加载完成，准备给玩家初始化红白条：({playerRed},{playerWhiteReal})");
        // 尝试找场景里 Tag="Player" 的物体，把红白条数据给他
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");   
        if (playerGO != null)
        {
            CharacterStats playerStats = playerGO.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                // 如果刚刚创建（即是第一关），justCreated=true，此时 playerRed/WhiteReal 已经是 (100,100)
                playerStats.InitializeRedWhite(playerRed, playerWhiteReal);
                Debug.Log($"[GameManager] 场景 “{scene.name}” 加载完成，传递给玩家 红={playerStats.redValue}, 白实际={playerStats.redValue} (whiteDisplay={playerStats.WhiteDisplay})");
            }
            else
            {
                Debug.LogWarning("[GameManager] 场景加载完成，但找不到 CharacterStats 组件！");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] 场景加载完成，但找不到 Tag=\"Player\" 的物体！");
        }

        // 进入下一个关卡后，justCreated 应该设为 false（后面都沿用继承值）
        justCreated = false;
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

    /// Execution:确认击杀
    public void OnEnemyKillConfirmed(CharacterStats enemyStats)
    {

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");//更新“玩家的红白条”
        if (playerGO != null)
        {
            CharacterStats playerStats = playerGO.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                playerStats.OnEnemyKilled(); // 红条+4，白条-5

                playerRed = playerStats.redValue;// 更新到 GameManager 里，以便关卡结束后继承
                playerWhiteReal = playerStats.whiteRealValue;
            }
            enemyStats.ConfirmKill();// 真正让敌人销毁
        }
    }
    /// Execution:确认治愈
    public void OnEnemyHealConfirmed(CharacterStats enemyStats)
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            CharacterStats playerStats = playerGO.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                playerStats.OnEnemyHealed(); // 红条-5，白条+12（可溢出）

                // 更新到 GameManager 里，用于继承
                playerRed = playerStats.redValue;
                playerWhiteReal = playerStats.whiteRealValue;
            }
        }
        enemyStats.ConfirmHeal();
    }
}

