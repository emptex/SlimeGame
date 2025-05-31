using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AttackHitbox : MonoBehaviour
{
    [Header("判定参数")]
    public int damage;                    // 来自攻击者的攻击力
    public float lifeTime = 0.1f;         // 判定体存活时长

    [HideInInspector] public CombatSystem owner; // 攻击者的 CombatSystem

    void Start()
    {
        // 确保是触发器
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner == null) return;
        // 避免自伤
        if (other.GetComponent<CombatSystem>() == owner) return;

        var target = other.GetComponent<CombatSystem>();
        if (target != null)
        {
            target.TakeDamage(damage);
            // TODO: 可以通知 owner 播放击中反馈
        }
    }
}