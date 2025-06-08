using UnityEngine;
using System;

/// <summary>
/// CharacterStats：
/// 只负责存储和维护一个角色的基础数值（最大血量、当前血量、攻击力、防御等）
/// 提供一个 ApplyHPChange(int) 方法，供外部 CombatSystem 调用来扣血或回血。
/// 
/// 现在新增：当 currentHP 从正数变为 0 时，触发 OnZeroHP 事件，供 GameManager 去弹出“处决 UI”
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("角色基础属性")]
    public int maxHP = 100;            // 最大生命值，可在 Inspector 里调整
    [HideInInspector] public int currentHP; // 当前生命值，由 Awake 初始化为 maxHP

    public int attackPower = 20;       // 攻击力
    public int defense = 5;            // 防御

    // 事件：血量从正数变为 0 的那一刻；flag一个血量归零（死亡）次数
    public event Action<CharacterStats> OnZeroHP;
    private bool hasTriggeredZero = false;

    [Header("善恶值")]
    [Tooltip("当前红条数值，范围 [0,200]，超出 200 不会保留，直接卡最大值 200")]
    public int redValue;            // 当前红条（Attack Meter），范围：[0,200]
    [Tooltip("当前实际白条数值，可以超过 200，显示时取 min(200, whiteRealValue)")]
    public int whiteRealValue;      // 当前实际白条（Heal Meter），可 >200，溢出部分保留给下一关

    /// 只读：红条的可显示值（始终 ≤ 200）
    public int RedDisplay
    {
        get
        {
            return Mathf.Clamp(redValue, 0, 200);
        }
    }
    /// 只读：白条的可显示值（显示上限 200，实际可能 >200 且溢出）
    public int WhiteDisplay
    {
        get
        {
            return Mathf.Min(200, whiteRealValue);
        }
    }
    // 标记是否已初始化过红白条（避免重复调用）
    private bool rwInitialized = false;
    void Awake()
    {
        if (gameObject.CompareTag("Player") && rwInitialized == false)
        {
            // 先给一个占位值，具体真正的初始值要在 Start 或者场景加载回调时由 GameManager 传进来
            // 这里先不设置，等下一帧 Start 里再调用 InitializeRedWhite()
        }
        // 游戏开始时，让 currentHP 等于 maxHP；血量归零（死亡）触发次数为“否”
        currentHP = maxHP;
        hasTriggeredZero = false;

    }

    private void Start()
    {     

        // 如果这是一个敌人（带有Enemy Tag),就在 Awake 里把自己注册给 GameManager
        if (gameObject.CompareTag("Enemy"))
        {
            // 保证 GameManager.Instance 已经初始化
            if (GameManager.Instance != null)
            {
                Debug.Log($"[CharacterStats] Awake: 敌人 “{gameObject.name}” 正在注册到 GameManager");
                GameManager.Instance.RegisterEnemy(this);
                Debug.Log($"[CharacterStats] Awake: 敌人 “{gameObject.name}” 已注册到 GameManager");
            }
            else
            {
                Debug.LogWarning($"[CharacterStats] Awake: 敌人 “{gameObject.name}” 试图注册到 GameManager，但 GameManager.Instance 为 null");
            }
        }
        if (gameObject.CompareTag("Player") && rwInitialized == false && GameManager.Instance != null)
        {
            // 从 GameManager 拿到继承值；如果是第一关，GameManager 会给默认的 (100,100)
            int inheritedRed = GameManager.Instance.playerRed;
            int inheritedWhiteReal = GameManager.Instance.playerWhiteReal;
            InitializeRedWhite(inheritedRed, inheritedWhiteReal);
        }
    }
    /// <summary>
    /// 外部调用：对当前血量进行一次增减，并将结果限制在 [0, maxHP] 区间。
    /// value 为正时表示扣血，value 为负时表示回血。
    /// </summary>
    public void ApplyHPChange(int value)
    {
        // 扣血/加血逻辑
        currentHP -= value;
        if (currentHP < 0) currentHP = 0;
        if (currentHP > maxHP) currentHP = maxHP;

        Debug.Log($"{gameObject.name}.ApplyHPChange({value}) 实施后，currentHP = {currentHP}/{maxHP}");

        Debug.Log($"[DEBUG] 判断前：currentHP = {currentHP}, hasTriggeredZero = {hasTriggeredZero}");
        // 如果血量为 0，且还没触发过“从正数变到 0”这一事件，就触发
        if (currentHP == 0 && hasTriggeredZero == false)
        {
            hasTriggeredZero = true;  // 标记已经触发过，避免重复
            Debug.Log($"[CharacterStats] {gameObject.name} 血量归零，准备触发 OnZeroHP");
            // 触发事件：只要别人（GameManager）订阅了，就会被调用
            OnZeroHP?.Invoke(this);
        }

    }

    public void InitializeRedWhite(int redInit, int whiteRealInit)
    {
        if (rwInitialized) return; // 只初始化一次
        rwInitialized = true;

        // Clamp 红条到 [0,200]
        redValue = Mathf.Clamp(redInit, 0, 200);
        // 白条实际值至少 ≥ 0（不会有负数），不在此处上限截断（可溢出留到下一关）
        whiteRealValue = Mathf.Max(0, whiteRealInit);

        Debug.Log($"[CharacterStats] 玩家红白条初始化：red={redValue}, whiteReal={whiteRealValue} (whiteDisplay={WhiteDisplay})");


    }
    /// —— 新增方法：当玩家「杀死」一个史莱姆时被调用 ——  
    /// 符合策划文档：红条 +4（上限 200），白条真实值 -5（下限 0）
    /// </summary>
    public void OnEnemyKilled()
    {
        // 红条更新
        redValue = Mathf.Min(200, redValue + 4);
        // 白条更新
        whiteRealValue = Mathf.Max(0, whiteRealValue - 5);

        Debug.Log($"[CharacterStats] OnEnemyKilled(): red->{redValue}, whiteReal->{whiteRealValue} (whiteDisplay={WhiteDisplay})");
    }
    /// —— 新增方法：当玩家「治愈」一个史莱姆时被调用 ——  
    /// 符合策划文档：红条 -5（下限 0），白条真实值 +12（可溢出，显示取 min(200, real)）
    /// </summary>
    public void OnEnemyHealed()
    {
        // 红条更新
        redValue = Mathf.Max(0, redValue - 5);
        // 白条更新（不做上限截断）
        whiteRealValue += 12;

        Debug.Log($"[CharacterStats] OnEnemyHealed(): red->{redValue}, whiteReal->{whiteRealValue} (whiteDisplay={WhiteDisplay})");
    }





    /// <summary>
    /// 在 GameManager 确认“杀死”之后，才真正销毁敌人
    /// </summary>
    public void ConfirmKill()
    {
        // 如果你要播放一个消失或死亡的特效，可以放在这里
        Destroy(gameObject);
    }

    /// <summary>
    /// 在 GameManager 确认“治愈”之后，敌人复活
    /// </summary>
    public void ConfirmHeal()
    {
        // 血量重置
        hasTriggeredZero = false;
        currentHP = maxHP;

        // 如果敌人身上有 NavMeshAgent 或其他移动逻辑，需要恢复它们
        var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null)
        {
            nav.isStopped = false;
        }

        // 如果有 Animator，可以播放一个“复活”动画
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Revive");
        }
    }
    void Update()
    {
        // 为了避免日志太刷屏，你可以仅在值发生了变化时才打印，或者每隔一段时间打印一次
        // 这里示例是每帧都打印（如果太频繁，可自行删减）
        Debug.Log($"[CharacterStats.Update] 来自Charactor：当前红白条：RedDisplay={RedDisplay}, WhiteDisplay={WhiteDisplay}");
    }

}
