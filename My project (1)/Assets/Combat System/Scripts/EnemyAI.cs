using UnityEngine;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("目标检测配置")]
    [Tooltip("如果不想按 Tag 自动查找，可开启并手动在下方填入一组 Transform")]
    public bool useManualTargets = false;
    [Tooltip("自动查找时，对应要索敌的 Tag，比如 \"Player\"")]
    public string detectionTag = "Player";
    [Tooltip("手动指定一组目标 Transform（优先使用）")]
    public Transform[] manualTargets;

    [Header("追击与漫游参数")]
    public float detectionRadius = 6f;
    public float chaseSpeed = 3f;
    public float roamSpeed = 1.5f;
    public float roamChangeInterval = 4f;

    [Header("攻击范围 & 冷却")]
    [Tooltip("挂载一个 Trigger Collider，用于检测玩家进入近战范围")]
    public Collider attackRangeCollider;
    public float attackInterval = 1f;

    [Header("漫游区域 Trigger")]
    [Tooltip("挂载一个 Trigger Collider，离开时反向漫游")]
    public Collider roamAreaTrigger;

    // 组件引用
    private CharacterStats stats;
    private CombatSystem combat;
    private Rigidbody rb;

    // 漫游状态
    private Vector3 roamDirection;
    private float roamTimer;

    // 攻击状态
    private bool isPlayerInAttackRange;
    private float lastAttackTime = -Mathf.Infinity;

    // chase<>roam 事件（可订阅）
    public event Action OnStartChase;
    public event Action OnStartRoam;
    private bool wasChasing = false;

    // 运行时目标列表
    private Transform[] targets;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
        combat = GetComponent<CombatSystem>();
        rb = GetComponent<Rigidbody>();

        // 物理设置：开启重力，连续检测，冻结旋转
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 确保 Trigger Collider 标记正确
        if (attackRangeCollider) attackRangeCollider.isTrigger = true;
        if (roamAreaTrigger) roamAreaTrigger.isTrigger = true;

        // 漫游初始化
        roamTimer = roamChangeInterval;
        ChooseNewRoamDirection();

        // 预先填充 targets
        RefreshTargets();
    }

    void RefreshTargets()
    {
        if (useManualTargets && manualTargets != null && manualTargets.Length > 0)
        {
            targets = manualTargets;
        }
        else if (!string.IsNullOrEmpty(detectionTag))
        {
            var gos = GameObject.FindGameObjectsWithTag(detectionTag);
            targets = new Transform[gos.Length];
            for (int i = 0; i < gos.Length; i++)
                targets[i] = gos[i].transform;
        }
        else
        {
            targets = new Transform[0];
        }
    }

    void Update()
    {
        if (stats.currentHP <= 0) return;

        // 如果没有手动目标，则根据 Tag 动态刷新
        if (!useManualTargets && (targets == null || targets.Length == 0))
            RefreshTargets();

        // 找最近目标
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var t in targets)
        {
            if (t == null) continue;
            float d = Vector3.Distance(transform.position, t.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = t;
            }
        }

        bool isChasingNow = (nearest != null && minDist <= detectionRadius);

        // 触发 chase/roam 事件一次
        if (isChasingNow && !wasChasing) OnStartChase?.Invoke();
        if (!isChasingNow && wasChasing) OnStartRoam?.Invoke();
        wasChasing = isChasingNow;

        // 执行对应逻辑
        if (nearest != null && isChasingNow)
        {
            if (isPlayerInAttackRange)
                HandleAttack(nearest);
            else
                HandleChase(nearest);
        }
        else
        {
            HandleRoaming();
        }
    }

    #region —— Chase / Roam / Attack —— 

    private void HandleChase(Transform target)
    {
        Vector3 dir = (target.position - transform.position).WithY(0).normalized;
        Move(dir, chaseSpeed);
    }

    private void HandleRoaming()
    {
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f)
        {
            roamTimer = roamChangeInterval;
            ChooseNewRoamDirection();
        }
        Move(roamDirection, roamSpeed);
    }

    private void HandleAttack(Transform target)
    {
        if (Time.time - lastAttackTime < attackInterval)
        {
            FaceTarget(target);
            return;
        }

        FaceTarget(target);
        combat.Attack();
        lastAttackTime = Time.time;
    }

    #endregion

    #region —— Movement —— 

    private void Move(Vector3 direction, float speed)
    {
        // 设置水平速度，保留垂直分量由物理处理
        Vector3 horizontal = direction * speed;
        rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);

        // 平滑转向
        if (horizontal.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(horizontal.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void FaceTarget(Transform t)
    {
        Vector3 dir = (t.position - transform.position).WithY(0);
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
        }
    }

    private void ChooseNewRoamDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        roamDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    #endregion

    #region —— Trigger 感知 —— 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(detectionTag))
            isPlayerInAttackRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(detectionTag))
            isPlayerInAttackRange = false;

        if (other == roamAreaTrigger)
        {
            roamDirection = -roamDirection;
            roamTimer = roamChangeInterval;
        }
    }

    #endregion
}

// 小扩展方法：清除 Y 分量
public static class Vector3Extensions
{
    public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
}