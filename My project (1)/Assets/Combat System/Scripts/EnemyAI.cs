using UnityEngine;

/// <summary>
/// EnemyAI：
/// 1. 如果在指定的“索敌半径”(detectionRadius)内检测到 Tag="Player" 的物体，就向玩家移动；
/// 2. 当玩家进入“攻击范围 Collider”时，如果冷却到位，就调用 CombatSystem.Attack()；
/// 3. 如果没有检测到玩家，就在场景里随机漫游；
/// 4. 可在 Inspector 里面调整索敌半径、追击速度、漫游速度、漫游切换间隔、攻击间隔等参数；
/// 5. 通过 Move() 方法执行实际移动，方便以后切换成 NavMeshAgent 或其它移动方式；
/// 6. 通过 OnTriggerEnter/Exit 监测“玩家是否在攻击范围 Collider 内”，并配合攻击冷却决定何时发起攻击；
/// 7. **新增：活动范围 Trigger (roamAreaTrigger)。当敌人跑出此范围时，马上取反漫游方向，避免乱跑。**
/// </summary>
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(Rigidbody))] // 需要 Rigidbody 去处理触发器检测
public class EnemyAI : MonoBehaviour
{
    [Header("索敌与移动参数")]
    [Tooltip("玩家进入此半径内后，敌人开始追击")]
    public float detectionRadius = 6f;

    [Tooltip("当玩家在索敌范围内时，追击速度")]
    public float chaseSpeed = 3f;

    [Tooltip("当玩家不在索敌范围时，漫游速度")]
    public float roamSpeed = 1.5f;

    [Tooltip("漫游方向每隔多少秒随机切换一次")]
    public float roamChangeInterval = 4f;

    [Header("攻击参数")]
    [Tooltip("攻击范围使用子物体挂载的 Collider（Trigger）检测")]
    public Collider attackRangeCollider;
    [Tooltip("两次攻击之间的最短时间间隔（秒）")]
    public float attackInterval = 1f;

    [Header("活动范围 Trigger")]
    [Tooltip("挂载在敌人子物体上的 Trigger Collider，控制漫游最大活动范围")]
    public Collider roamAreaTrigger;

    // --------------- 组件引用 ---------------
    protected CharacterStats stats;      // 存血量/数值，用于判断是否死亡
    protected CombatSystem combat;       // 负责实际发起攻击（Instantiate Hitbox 等）
    protected Transform playerTransform; // 缓存玩家 Transform，Tag="Player"

    // 漫游相关
    private float roamTimer = 0f;
    private Vector3 roamDirection = Vector3.zero;

    // 攻击相关
    private bool isPlayerInAttackRange = false;       // 标记玩家是否在攻击范围 Collider 内
    private float lastAttackTime = -Mathf.Infinity;   // 上一次攻击的时间戳

    void Awake()
    {
        // 缓存组件引用
        stats = GetComponent<CharacterStats>();
        combat = GetComponent<CombatSystem>();

        // 确保 attackRangeCollider 已经被赋值
        if (attackRangeCollider == null)
        {
            Debug.LogError($"[{nameof(EnemyAI)}] 需要在 Inspector 里将 'attackRangeCollider' 赋值为挂在 Enemy 身上的子物体 Collider (Is Trigger)。");
        }
        else
        {
            attackRangeCollider.isTrigger = true;
        }

        // 确保 roamAreaTrigger 已经被赋值
        if (roamAreaTrigger == null)
        {
            Debug.LogError($"[{nameof(EnemyAI)}] 需要在 Inspector 里将 'roamAreaTrigger' 赋值为挂在 Enemy 身上的子物体 Collider (Is Trigger)。");
        }
        else
        {
            // 为了确保触发正常，这里也把它设为 Trigger
            roamAreaTrigger.isTrigger = true;
        }

        // 强制要求有 Rigidbody（最好在 Inspector 里已经设置为 Is Kinematic = true）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;  // 物理不控制运动，只做 Trigger 检测

        // 找到场景里 Tag="Player" 的物体
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
        }
        else
        {
            Debug.LogWarning($"[{nameof(EnemyAI)}] 找不到 Tag=\"Player\" 的对象，请检查场景中是否有正确标记的玩家。");
        }

        // 初始化漫游计时器 & 随机漫游方向
        roamTimer = roamChangeInterval;
        ChooseNewRoamDirection();
    }

    void Update()
    {
        // 如果自身已经死亡，就不做任何后续逻辑
        if (stats.currentHP <= 0)
        {
            return;
        }

        // 如果场景里找不到玩家，就直接漫游
        if (playerTransform == null)
        {
            HandleRoaming();
            return;
        }

        // 计算与玩家的距离
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 玩家在检测范围内：先尝试攻击，再追击
        if (distToPlayer <= detectionRadius)
        {
            // 如果玩家同时也在“攻击范围 Trigger”里，那么就尝试攻击
            if (isPlayerInAttackRange)
            {
                HandleAttack();
            }
            else
            {
                // 否则就继续 Chase
                HandleChase(distToPlayer);
            }
        }
        else
        {
            // 玩家不在检测范围内，进入漫游
            HandleRoaming();
        }
    }

    #region —— 攻击逻辑（Attack） —— 

    /// <summary>
    /// 当玩家位于 attackRangeCollider 的 Trigger 范围内，并且攻击冷却到位时调用。
    /// </summary>
    protected virtual void HandleAttack()
    {
        // 检查冷却
        if (Time.time - lastAttackTime >= attackInterval)
        {
            // 朝向玩家
            Vector3 dir = (playerTransform.position - transform.position);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 10f);
            }

            // 真正调用 CombatSystem 执行攻击（生成判定体等）
            combat.Attack();
            lastAttackTime = Time.time;
        }
        else
        {
            // 冷却未到，则保持 Idle 或小幅度转向玩家
            Vector3 dir = (playerTransform.position - transform.position);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 10f);
            }
        }
    }

    #endregion

    #region —— 追击逻辑（Chase） —— 

    /// <summary>
    /// 当玩家在检测范围内，但不在攻击范围 Trigger 内时，敌人向玩家直线移动（XZ 平面）。
    /// </summary>
    /// <param name="distToPlayer">与玩家的距离</param>
    protected virtual void HandleChase(float distToPlayer)
    {
        // 计算玩家方向（只在XZ平面移动）
        Vector3 direction = (playerTransform.position - transform.position);
        direction.y = 0f; // 忽略高度差

        // 让敌人向玩家移动
        Move(direction, chaseSpeed);
    }

    #endregion

    #region —— 漫游逻辑（Roaming） —— 

    /// <summary>
    /// 当玩家不在检测范围、或场景里没有玩家时，每隔 roamChangeInterval 秒更换一个随机方向，并匀速移动
    /// </summary>
    protected void HandleRoaming()
    {
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f)
        {
            roamTimer = roamChangeInterval;
            ChooseNewRoamDirection();
        }

        // 沿当前 roamDirection 以 roamSpeed 移动
        Move(roamDirection, roamSpeed);
    }

    /// <summary>
    /// 随机生成一个单位向量，用于漫游时的移动方向（XZ 平面）
    /// </summary>
    protected void ChooseNewRoamDirection()
    {
        // 随机一个 0~360 度角
        float angle = Random.Range(0f, 360f);
        roamDirection = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;
    }

    #endregion

    #region —— 核心移动接口（可扩展／预留） —— 

    /// <summary>
    /// 敌人真正执行移动的接口：当前实现直接操纵 transform。
    /// 未来如果想用 NavMeshAgent，只要在子类里 override 这个方法，改成 agent.SetDestination(...) 即可。
    /// </summary>
    /// <param name="direction">
    ///     想要移动的方向向量（可不归一化）。
    ///     XZ 平面移动时，最好把 .y 置为 0，再传进来。</param>
    /// <param name="speed">移动速度</param>
    protected virtual void Move(Vector3 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        // 先让角色面向移动方向
        Vector3 flatDir = direction;
        flatDir.y = 0;
        Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 10f
        );

        // 再沿该方向平移
        Vector3 moveDelta = flatDir.normalized * speed * Time.deltaTime;
        transform.position += moveDelta;
    }

    #endregion

    #region —— 攻击范围 Trigger 感知（OnTriggerEnter/Exit） —— 

    void OnTriggerEnter(Collider other)
    {
        // 当玩家的 Collider 进入“攻击范围 Trigger”时，把标志置 true
        if (attackRangeCollider != null && other.CompareTag("Player"))
        {
            isPlayerInAttackRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 当玩家的 Collider 离开“攻击范围 Trigger”时，把标志置 false
        if (attackRangeCollider != null && other.CompareTag("Player"))
        {
            isPlayerInAttackRange = false;
        }
    }

    #endregion

    #region —— Gizmos 绘制（可选） —— 

    void OnDrawGizmosSelected()
    {
        // 在 Scene 视图里画出 detectionRadius 方便调试
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 画出攻击范围
        if (attackRangeCollider is SphereCollider sphere)
        {
            Gizmos.color = Color.red;
            Vector3 worldCenter = transform.TransformPoint(sphere.center);
            Gizmos.DrawWireSphere(worldCenter, sphere.radius * transform.localScale.x);
        }
        else if (attackRangeCollider is BoxCollider box)
        {
            Gizmos.color = Color.red;
            Vector3 worldCenter = transform.TransformPoint(box.center);
            Vector3 worldSize = Vector3.Scale(box.size, transform.localScale);
            Gizmos.DrawWireCube(worldCenter, worldSize);
        }

        // 画出漫游活动范围
        if (roamAreaTrigger is SphereCollider roamSphere)
        {
            Gizmos.color = Color.cyan;
            Vector3 roamCenter = transform.TransformPoint(roamSphere.center);
            Gizmos.DrawWireSphere(roamCenter, roamSphere.radius * transform.localScale.x);
        }
        else if (roamAreaTrigger is BoxCollider roamBox)
        {
            Gizmos.color = Color.cyan;
            Vector3 roamCenter = transform.TransformPoint(roamBox.center);
            Vector3 roamSize = Vector3.Scale(roamBox.size, transform.localScale);
            Gizmos.DrawWireCube(roamCenter, roamSize);
        }
    }

    #endregion
}