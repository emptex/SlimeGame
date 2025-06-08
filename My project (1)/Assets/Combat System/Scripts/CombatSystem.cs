using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CombatSystem : MonoBehaviour
{
    [Header("角色数据引用")]
    public CharacterStats stats;      // 在 Inspector 里拖入与本对象同级的 CharacterStats

    [Header("攻击判定体设置")]
    public GameObject attackHitboxPrefab;  // 预制：带有 AttackHitbox 脚本的判定体
    public Transform hitboxSpawnPoint;     // 判定体生成位置

    void Awake()
    {
        // 如果在 Inspector 没拖入，则自动获取同一 GameObject 上的 CharacterStats
        if (stats == null)
        {
            stats = GetComponent<CharacterStats>();
        }
    }


    /// 被外部调用：对本角色造成一次原始伤害（其他 CombatSystem 生成的 Hitbox 会调用到这里）。<param name="damage">外部判定体传过来的原始伤害值</param>
    /// . 先计算扣血：finalDamage = damage - 防御；1. 真正的扣血逻辑由CharacterStats.ApplyHPChange完成，这是为了确保执行到Charactor里的血量归零事件
    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(0, damage - stats.defense);
        stats.ApplyHPChange(finalDamage);
    }

    /// <summary>
    /// 对外调用：本角色主动发起一次攻击，实例化一个短暂的判定体（Hitbox）。
    /// </summary>
    public void Attack()
    {
        if (attackHitboxPrefab == null || hitboxSpawnPoint == null)
        {
            Debug.LogWarning($"[{nameof(CombatSystem)}] AttackHitboxPrefab 或 HitboxSpawnPoint 没设置");
            return;
        }

        // 在指定位置生成一个 Hitbox 预制体
        GameObject obj = Instantiate(attackHitboxPrefab,
                                     hitboxSpawnPoint.position,
                                     hitboxSpawnPoint.rotation);

        // 将攻击数值和 owner 赋给判定体，让它在碰撞时调用 TakeDamage()
        AttackHitbox ah = obj.GetComponent<AttackHitbox>();
        if (ah != null)
        {
            ah.damage = stats.attackPower;
            ah.owner = this;
        }
        else
        {
            Debug.LogWarning($"[{nameof(CombatSystem)}] 生成的 Hitbox Prefab 上未找到 AttackHitbox 脚本");
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 死亡");
        Destroy(gameObject);
    }
}