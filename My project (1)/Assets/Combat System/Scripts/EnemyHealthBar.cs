using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("血条相关引用")]
    [Tooltip("拖入红色血条的 Image 组件（RectTransform 锚点需设置在左侧）")]
    public Image redBar;

    [Tooltip("拖入怪物本体上的 CharacterStats 脚本，用来读取 currentHP / maxHP")]
    public CharacterStats targetStats;

    // 记录 redBar 的完整宽度（即满血时的 sizeDelta.x）
    private float fullWidth;

    // redBar 的 RectTransform 缓存
    private RectTransform redBarRect;

    void Start()
    {
        // 检查必填引用
        if (redBar == null)
        {
            Debug.LogError($"[{nameof(EnemyHealthBar)}] 未关联 redBar，请在 Inspector 里拖入血条的 Image。");
            enabled = false;
            return;
        }
        if (targetStats == null)
        {
            Debug.LogError($"[{nameof(EnemyHealthBar)}] 未关联 targetStats，请在 Inspector 里拖入怪物的 CharacterStats。");
            enabled = false;
            return;
        }

        // 缓存 RectTransform，并记录初始宽度
        redBarRect = redBar.GetComponent<RectTransform>();
        fullWidth = redBarRect.sizeDelta.x;

        // 一开始就更新一次，保证 UI 上显示正确的初始血量
        UpdateHealthBar();
    }

    void Update()
    {
        // 每帧都检查一次，如果怪物血量变化就刷新血条
        UpdateHealthBar();
    }

    /// <summary>
    /// 根据 targetStats.currentHP / maxHP 来调整 redBar 的宽度
    /// </summary>
    private void UpdateHealthBar()
    {
        // 防止除以 0
        if (targetStats.maxHP <= 0)
            return;

        // 计算血量百分比，范围 [0,1]
        float hpPercent = Mathf.Clamp01((float)targetStats.currentHP / targetStats.maxHP);

        // 计算新的宽度
        float newWidth = fullWidth * hpPercent;

        // 直接修改 redBarRect 的 sizeDelta.x，让它在左锚点基础上左右缩放
        Vector2 size = redBarRect.sizeDelta;
        size.x = newWidth;
        redBarRect.sizeDelta = size;
    }
}
