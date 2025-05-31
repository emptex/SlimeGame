using UnityEngine;

/// <summary>
/// EnemyAI：
/// 1. 如果在指定的“索敌半径”(detectionRadius)内检测到 Tag="Player" 的物体，就向玩家移动；
/// 2. 如果没有检测到玩家，就在场景里随机漫游；
/// 3. 整个移动操作通过 Move() 方法来完成，方便以后换成 NavMeshAgent 或其它系统；
/// 4. 可在 Inspector 里面调整索敌半径、追击速度、漫游速度、漫游切换间隔等参数。
/// </summary>
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(CombatSystem))]
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

    // --------------- 组件引用 ---------------
    // 方便以后扩展到 NavMeshAgent：可以在子类里覆写 Move() 方法
    protected CharacterStats stats;      // 如果后续需要血量判断，也可以直接用
    protected CombatSystem combat;       // 预留，攻击逻辑可以在这里接入
    protected Transform playerTransform; // 缓存玩家 Transform，Tag="Player"

    // 漫游相关
    private float roamTimer = 0f;
    private Vector3 roamDirection = Vector3.zero;

    void Awake()
    {
        // 缓存组件引用
        stats = GetComponent<CharacterStats>();
        combat = GetComponent<CombatSystem>();

        // 找到场景里 Tag="Player" 的物体
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerTransform = playerGO.transform;
        else
            Debug.LogWarning($"[{nameof(EnemyAI)}] 在 Start 阶段未能找到 Tag=\"Player\" 的物体，请确认场景中有正确标记。");

        // 初始化漫游计时器
        roamTimer = roamChangeInterval;
        ChooseNewRoamDirection();
    }

    void Update()
    {
        // 如果自身已经死亡（假设 CombatSystem.Die() 会销毁 GameObject），这里就不做任何处理
        // （你可以在 CombatSystem.Die() 里先播放死亡动画，再 Destroy(gameObject)）
        if (stats.currentHP <= 0)
        {
            return;
        }

        // 如果场景里找不到玩家，或者玩家空了，就直接漫游
        if (playerTransform == null)
        {
            HandleRoaming();
            return;
        }

        // 计算与玩家的距离
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 玩家在检测范围内 —— 进入追击逻辑
        if (distToPlayer <= detectionRadius)
        {
            HandleChase(distToPlayer);
        }
        // 玩家不在检测范围内 —— 进入漫游逻辑
        else
        {
            HandleRoaming();
        }
    }

    #region —— 追击逻辑（Chase） —— 

    /// <summary>
    /// 当播放器处于敌人detectionRadius范围内时调用
    /// </summary>
    /// <param name="distToPlayer">当前距离</param>
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

    #region —— Gizmos 绘制（可选） —— 

    void OnDrawGizmosSelected()
    {
        // 在 Scene 视图里画出 detectionRadius 方便调试
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 画出 attackRange（如果你想后面加 attackRange）
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #endregion
}