using UnityEngine;

/// <summary>
/// PlayerSkillController: 
/// 用于在不修改现有 CombatSystem/CharacterStats 的前提下，为玩家添加多种按键触发的技能。
/// 每个技能由一个“判定体 Prefab”+“生成点”+“伤害倍率”+“触发按键”组成。按键触发时直接实例化该判定体，
/// 并给它赋值 damage = stats.attackPower * multiplier，owner = combat，从而沿用现有 CombatSystem 的伤害结算逻辑。
/// </summary>
[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(CharacterStats))]
public class PlayerSkillController : MonoBehaviour
{
    [System.Serializable]
    public class SkillEntry
    {
        [Tooltip("按下此键触发对应技能")]
        public KeyCode activationKey;

        [Tooltip("技能判定体 Prefab，须包含 AttackHitbox 脚本")]
        public GameObject skillPrefab;

        [Tooltip("判定体生成位置的 Transform（可与角色挂相同位置或子物体）")]
        public Transform spawnPoint;

        [Tooltip("伤害倍率 = stats.attackPower * multiplier")]
        public float damageMultiplier = 1f;
    }

    [Header("技能列表")]
    [Tooltip("在这里添加多个技能，每个技能对应一个按键、Prefab、SpawnPoint 及伤害倍率")]
    public SkillEntry[] skills;

    private CombatSystem combat;
    private CharacterStats stats;

    void Awake()
    {
        combat = GetComponent<CombatSystem>();
        stats = GetComponent<CharacterStats>();
    }

    void Update()
    {
        if (skills == null || skills.Length == 0) return;

        // 遍历每个技能条目，检测按键
        foreach (var entry in skills)
        {
            if (entry.activationKey == KeyCode.None) continue;
            if (Input.GetKeyDown(entry.activationKey))
            {
                TryUseSkill(entry);
            }
        }
    }

    private void TryUseSkill(SkillEntry entry)
    {
        if (entry.skillPrefab == null)
        {
            Debug.LogWarning($"[PlayerSkillController] 技能 Prefab 未设置 (Key = {entry.activationKey})，跳过。");
            return;
        }

        if (entry.spawnPoint == null)
        {
            Debug.LogWarning($"[PlayerSkillController] SpawnPoint 未设置 (Key = {entry.activationKey})，跳过。");
            return;
        }

        // 实例化判定体 Prefab
        GameObject go = Instantiate(
            entry.skillPrefab,
            entry.spawnPoint.position,
            entry.spawnPoint.rotation
        );

        // 给判定体上的 AttackHitbox 赋值
        var hitbox = go.GetComponent<AttackHitbox>();
        if (hitbox != null)
        {
            int dmg = Mathf.RoundToInt(stats.attackPower * entry.damageMultiplier);
            hitbox.damage = dmg;
            hitbox.owner = combat;
        }
        else
        {
            Debug.LogWarning($"[PlayerSkillController] Prefab 上找不到 AttackHitbox 脚本 (Key = {entry.activationKey})");
        }
    }
}