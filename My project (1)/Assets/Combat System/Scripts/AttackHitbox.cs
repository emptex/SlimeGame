using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AttackHitbox : MonoBehaviour
{
    [Header("判定设置")]
    public int damage;               // 由 CombatSystem 在实例化时赋值
    public float lifeTime = 0.1f;    // 判定体存活时长

    [HideInInspector] public CombatSystem owner;  // 发起攻击的 CombatSystem

    void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        Debug.Log($"[AttackHitbox] Start: owner = {owner} （应当是挂这个 Hitbox 的那把武器或玩家的 CombatSystem）");
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner == null)
        {
            Debug.LogWarning("AttackHitbox: owner 为空，跳过");
            return;
        }

        // —— 新增：如果撞到的是发起者自己（或自己身上的任何子物体），就跳过 —— //
        // 这里用 IsChildOf：只要 other 在 owner.transform 的层级里，就一定是自己人
        if (other.transform.IsChildOf(owner.transform))
        {
            // Debug.Log("AttackHitbox: 撞到自己，跳过");
            return;
        }

        // 碰到的是别人，才继续下面的伤害逻辑
        CombatSystem otherCombat = other.GetComponent<CombatSystem>();
        if (otherCombat == null)
        {
            // Debug.Log($"AttackHitbox: 碰到 {other.name}，但它没有 CombatSystem");
            return;
        }

        // 额外再保留一次 owner 判断，防止两边都是某种挂载了 CombatSystem 的同一角色
        if (otherCombat == owner)
        {
            // Debug.Log("AttackHitbox: 撞到 owner 自身的 CombatSystem，跳过");
            return;
        }

        // 真正调用对方的 TakeDamage
        // Debug.Log($"AttackHitbox: 命中 {other.name}，调用 TakeDamage({damage})");
        otherCombat.TakeDamage(damage);
    }
}