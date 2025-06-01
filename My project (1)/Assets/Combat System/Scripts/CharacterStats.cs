using UnityEngine;

/// <summary>
/// CharacterStats：
/// 只负责存储和维护一个角色的基础数值（最大血量、当前血量、攻击力、防御等）
/// 提供一个 ApplyHPChange(int) 方法，供外部 CombatSystem 调用来扣血或回血。
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("角色基础属性")]
    public int maxHP = 100;            // 最大生命值，可在 Inspector 里调整
    public int currentHP; // 当前生命值，不在 Inspector 里显示，但会根据 maxHP 初始化

    public int attackPower = 20;       // 攻击力
    public int defense = 5;            // 防御

    void Awake()
    {
        // 游戏开始时，让 currentHP 等于 maxHP
        currentHP = maxHP;
    }

    /// <summary>
    /// 外部调用：对当前血量进行一次增减，并将结果限制在 [0, maxHP] 区间。
    /// value 为正时表示扣血，value 为负时表示回血。
    /// </summary>
    public void ApplyHPChange(int value)
    {
        currentHP -= value;
        if (currentHP < 0) currentHP = 0;
        if (currentHP > maxHP) currentHP = maxHP;

        Debug.Log($"{gameObject.name}.ApplyHPChange({value}) 实施后，currentHP = {currentHP}/{maxHP}");
    }
}