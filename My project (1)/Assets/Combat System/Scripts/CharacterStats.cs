using UnityEngine;

[DisallowMultipleComponent]
public class CharacterStats : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHP = 100;
    public int currentHP;      // 这里去掉 [ReadOnly]，直接公开就行

    [Header("战斗数值")]
    public int attackPower = 10;
    public int defense = 2;
    public float critChance = 0.1f; // 暴击概率（0–1）

    void Awake()
    {
        currentHP = maxHP;
    }
}