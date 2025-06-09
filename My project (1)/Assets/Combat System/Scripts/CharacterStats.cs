using UnityEngine;
using System;
using UnityEngine.Events;

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

    void Awake()
    {
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

        ///Debug.Log($"{gameObject.name}.ApplyHPChange({value}) 实施后，currentHP = {currentHP}/{maxHP}");

        ///Debug.Log($"[DEBUG] 判断前：currentHP = {currentHP}, hasTriggeredZero = {hasTriggeredZero}");
        // 如果血量为 0，且还没触发过“从正数变到 0”这一事件，就触发
        if (currentHP == 0 && hasTriggeredZero == false)
        {
            hasTriggeredZero = true;  // 标记已经触发过，避免重复
            ///Debug.Log($"[CharacterStats] {gameObject.name} 血量归零，准备触发 OnZeroHP");
            // 触发事件：只要别人（GameManager）订阅了，就会被调用
            OnZeroHP?.Invoke(this);
        }

    }

    /// <summary>
    /// 在 GameManager 确认“杀死”之后，才真正销毁敌人
    /// </summary>
    [Header("确认处决／复活时的扩展事件（可在 Inspector 里配置）")]
    public UnityEvent onConfirmedKill;
    public UnityEvent onConfirmedHeal;

    // … 其他字段、ApplyHPChange …

    public void ConfirmKill()
    {
        // 先触发外部钩子
        onConfirmedKill?.Invoke();

        // 再真正销毁
        //Destroy(gameObject);
    }

    public void ConfirmHeal()
    {
        // 先触发复活回调
        onConfirmedHeal?.Invoke();

        // 血量重置
        hasTriggeredZero = false;
        currentHP = maxHP;

        // 恢复 NavMeshAgent
        var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null) nav.isStopped = false;

        // 播放复活动画
        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Revive");
    }
}
