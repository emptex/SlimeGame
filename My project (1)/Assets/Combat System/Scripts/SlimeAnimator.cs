using System;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyAIStateAnimator : MonoBehaviour
{
    [Header("目标 Animator（播放动画用）")]
    public Animator targetAnimator;

    [Header("AI 状态触发器名称")]
    public string senseTrigger = "sense";     // 索敌开始
    public string roamTrigger = "WalkFWD";   // 漫游开始

    [Header("战斗触发器名称")]
    public string attackTrigger = "attack";    // 每次 Attack() 
    public string getHitTrigger = "gethit";    // 每次 TakeDamage() 
    public string dieTrigger = "kill";      // 血量归零

    // 引用
    private EnemyAI ai;
    private CombatSystem combat;
    private CharacterStats stats;

    // 记录上次 chase 状态，避免重复触发
    private bool wasChasing = false;

    void Awake()
    {
        ai = GetComponent<EnemyAI>();
        combat = GetComponent<CombatSystem>();
        stats = GetComponent<CharacterStats>();

        if (targetAnimator == null)
            Debug.LogWarning("[EnemyAIStateAnimator] targetAnimator 未指定，动画不会播放。");
    }

    void OnEnable()
    {
        // 订阅 AI 状态切换
        ai.OnStartChase += OnChaseStart;
        ai.OnStartRoam += OnRoamStart;

        // 订阅战斗事件
        combat.OnAttack += OnAttack;
        combat.OnTakeDamage += OnTakeDamage;

        // 订阅死亡事件
        stats.OnZeroHP += OnDeath;
    }

    void OnDisable()
    {
        // 取消订阅
        ai.OnStartChase -= OnChaseStart;
        ai.OnStartRoam -= OnRoamStart;

        combat.OnAttack -= OnAttack;
        combat.OnTakeDamage -= OnTakeDamage;

        stats.OnZeroHP -= OnDeath;
    }

    // —— AI 事件回调 —— //
    private void OnChaseStart()
    {
        if (targetAnimator != null)
            targetAnimator.SetTrigger(senseTrigger);
    }

    private void OnRoamStart()
    {
        if (targetAnimator != null)
            targetAnimator.SetTrigger(roamTrigger);
    }

    // —— 战斗事件回调 —— //
    private void OnAttack()
    {
        if (targetAnimator != null)
            targetAnimator.SetTrigger(attackTrigger);
    }

    private void OnTakeDamage(int damage)
    {
        if (targetAnimator != null)
            targetAnimator.SetTrigger(getHitTrigger);
    }

    // —— 死亡回调 —— //
    private void OnDeath(CharacterStats cs)
    {
        if (targetAnimator != null)
            targetAnimator.SetTrigger(dieTrigger);
    }
}