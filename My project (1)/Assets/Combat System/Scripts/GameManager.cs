using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Execution UI 预制件")]
    public GameObject executionUIPrefab; // 拖：Assets/Combat System/Prefabs/ExecutionUI.prefab

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // 如果你是在运行时才生成敌人，可以在敌人生成的地方把 OnZeroHP 绑定过去
        // 但更常见的是：在 EnemyHealth.Start() 里就会调用 RegisterEnemy(this)
        // 这里先留空
    }

    /// <summary>
    /// 让 GameManager 订阅一个刚生成或场景中已有的 EnemyHealth
    /// </summary>
    public void RegisterEnemy(CharacterStats stats)
    {
        stats.OnZeroHP += HandleEnemyZeroHP;
    }

    /// <summary>
    /// EnemyHealth 触发 OnZeroHP 时，会调用到这里：
    /// 负责弹出那个处决 UI 面板
    /// </summary>
    private void HandleEnemyZeroHP(CharacterStats stats)
    {
        // 1. 实例化 ExecutionUI
        GameObject uiGO = Instantiate(executionUIPrefab);

        // 2. 把它的 ExecutionUIController 拿到，调用 Init(enemy)
        ExecutionUIController uiCtrl = uiGO.GetComponent<ExecutionUIController>();
        uiCtrl.Init(stats);
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

