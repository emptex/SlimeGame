using UnityEngine;

[DisallowMultipleComponent]
public class CombatSystem : MonoBehaviour
{
    [Header("引用：自身属性组件")]
    public CharacterStats stats;

    [Header("攻击判定体")]
    public GameObject attackHitboxPrefab;      // 击中判定体的 Prefab
    public Transform hitboxSpawnPoint;         // 判定体生成位置

    /// <summary>
    /// 发起一次攻击：生成一个短命的 Hitbox，并把攻击力传给它
    /// </summary>
    public void Attack()
    {
        if (attackHitboxPrefab == null || hitboxSpawnPoint == null) return;

        var hitbox = Instantiate(
            attackHitboxPrefab,
            hitboxSpawnPoint.position,
            hitboxSpawnPoint.rotation
        );
        var ah = hitbox.GetComponent<AttackHitbox>();
        if (ah != null)
        {
            ah.damage = stats.attackPower;
            ah.owner = this;
        }
    }

    /// <summary>
    /// 受到伤害调用：根据防御计算最终伤害，更新血量，处理死亡
    /// </summary>
    public void TakeDamage(int incomingAttack)
    {
        int raw = incomingAttack - stats.defense;
        int dmg = Mathf.Max(raw, 1);
        stats.currentHP -= dmg;
        Debug.Log($"{gameObject.name} took {dmg} damage. HP left: {stats.currentHP}");

        // TODO: 播放受击动画、特效等

        if (stats.currentHP <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        // TODO: 播放死亡动画、禁用控制器、掉落物品等
        Destroy(gameObject);
    }
}