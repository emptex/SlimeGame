using System;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CombatSystem : MonoBehaviour
{
    [Header("角色数据引用")]
    public CharacterStats stats;

    [Header("攻击判定体设置")]
    public GameObject attackHitboxPrefab;
    public Transform hitboxSpawnPoint;

    // —— 新增事件 —— //
    /// <summary>外部调用 Attack() 时触发</summary>
    public event Action OnAttack;
    /// <summary>当 TakeDamage 被调用时触发，参数是实际扣的血</summary>
    public event Action<int> OnTakeDamage;

    void Awake()
    {
        if (stats == null)
            stats = GetComponent<CharacterStats>();
    }

    /// <summary>
    /// 外部判定体碰撞时会调用此方法
    /// </summary>
    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(0, damage - stats.defense);
        // 把血量变化交给 CharacterStats
        stats.ApplyHPChange(finalDamage);
        // —— 抛出“受伤”事件 —— //
        OnTakeDamage?.Invoke(finalDamage);

       
    }

    /// <summary>
    /// 本角色主动发起一次攻击
    /// </summary>
    public void Attack()
    {
        if (attackHitboxPrefab == null || hitboxSpawnPoint == null)
        {
            Debug.LogWarning($"[{nameof(CombatSystem)}] AttackHitboxPrefab 或 HitboxSpawnPoint 未设置");
            return;
        }

        var obj = Instantiate(attackHitboxPrefab,
                              hitboxSpawnPoint.position,
                              hitboxSpawnPoint.rotation);
        var ah = obj.GetComponent<AttackHitbox>();
        if (ah != null)
        {
            ah.damage = stats.attackPower;
            ah.owner = this;
        }
        else
        {
            Debug.LogWarning($"[{nameof(CombatSystem)}] 未找到 AttackHitbox 脚本");
        }

        // —— 抛出“攻击”事件 —— //
        OnAttack?.Invoke();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 死亡");
        Destroy(gameObject);
    }
}