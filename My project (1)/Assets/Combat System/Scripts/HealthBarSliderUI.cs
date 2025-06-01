using UnityEngine;
using UnityEngine.UI;  // 引用 UGUI

/// <summary>
/// HealthBarSliderUI：
///   - 把目标角色的 currentHP / maxHP 映射到一个 UI Slider.value 上。
///   - 适合把 Slider 当成血条，脚本会在 Update() 里不断更新 value。
/// 
/// 使用步骤：
///   1. 在场景里创建一个 Slider（如上文所述，去掉 Handle，把背景和填充图片替换成你的血条素材）。
///   2. 把此脚本挂到 Slider 本身或它的父 GameObject 上都可以（脚本会寻找引用的 Slider 组件）。
///   3. 在 Inspector 里，将 "healthSlider" 字段拖入场景里的那个 Slider 组件，将 "targetStats" 字段拖入你想要显示血量的角色的 CharacterStats 组件。
///   4. 运行时它会自动把 Slider.maxValue 设为 targetStats.maxHP，并把 Slider.value 设为 targetStats.currentHP。
/// 
/// 注意：如果有多个敌人要显示多条血条，只需再复制一个 Slider + 复制挂载此脚本并改引用 targetStats 即可。
/// </summary>
public class HealthBarSliderUI : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("拖入场景里的那个 Slider 组件")]
    public Slider healthSlider;

    [Header("要显示血量的角色")]
    [Tooltip("拖入一个含 CharacterStats 的角色（例如你的 Player 或 Enemy）")]
    public CharacterStats targetStats;

    void Start()
    {
        // 引用检查
        if (healthSlider == null)
        {
            Debug.LogError($"[{nameof(HealthBarSliderUI)}] 没有指定 healthSlider，请先在 Inspector 里拖入一个 Slider。");
            enabled = false;
            return;
        }
        if (targetStats == null)
        {
            Debug.LogError($"[{nameof(HealthBarSliderUI)}] 没有指定 targetStats，请先在 Inspector 里拖入一个 CharacterStats 组件。");
            enabled = false;
            return;
        }

        // 初次把 Slider 的范围（maxValue）设为角色的 maxHP
        healthSlider.maxValue = targetStats.maxHP;
        healthSlider.minValue = 0;

        // 同时把当前值设为 currentHP（防止刚开始差异）
        healthSlider.value = targetStats.currentHP;
    }

    void Update()
    {
        if (targetStats == null || healthSlider == null) return;

        // 如果 maxHP 改了，及时更新 Slider.maxValue
        if (healthSlider.maxValue != targetStats.maxHP)
            healthSlider.maxValue = targetStats.maxHP;

        // 把当前血量映射到 Slider.value
        healthSlider.value = targetStats.currentHP;
    }
}