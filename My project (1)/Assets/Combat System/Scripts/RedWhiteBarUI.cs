using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RedWhiteBarUI：
///  - 把玩家的 CharacterStats.RedDisplay / CharacterStats.WhiteDisplay 映射到两个 Slider 上。
///  - RedDisplay / WhiteDisplay 均是 0 ～ 200 之间的数值（显示上限 200）。
///  - 使用步骤：
///    1. 在场景里创建或拖入两个 Slider（一个当红条，一个当白条），并调整外观颜色。
///    2. 把此脚本挂在同一个空物体上（或挂在 Canvas 上都可以）。
///    3. 在 Inspector 里，将 “redSlider” 字段拖入红条 Slider，将 “whiteSlider” 拖入白条 Slider，
///       将 “targetStats” 拖入玩家的 CharacterStats 组件（Tag=“Player” 的物体上的那个）。
///  运行时会把两个 Slider 的值同步到目标角色的 RedDisplay / WhiteDisplay。
/// </summary>
public class RedWhiteBarUI : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("拖入红条的 Slider 组件（填充图可改为红色）")]
    public Slider redSlider;

    [Tooltip("拖入白条的 Slider 组件（填充图可改为白色）")]
    public Slider whiteSlider;

    [Header("目标角色 CharacterStats（Tag=Player）")]
    [Tooltip("拖入玩家角色身上挂的 CharacterStats 组件")]
    public CharacterStats targetStats;

    void Start()
    {
        // 引用检查
        if (redSlider == null)
        {
            Debug.LogError($"[{nameof(RedWhiteBarUI)}] 没有指定 redSlider，请在 Inspector 里拖入一个红条 Slider。");
            enabled = false;
            return;
        }
        if (whiteSlider == null)
        {
            Debug.LogError($"[{nameof(RedWhiteBarUI)}] 没有指定 whiteSlider，请在 Inspector 里拖入一个白条 Slider。");
            enabled = false;
            return;
        }
        if (targetStats == null)
        {
            Debug.LogError($"[{nameof(RedWhiteBarUI)}] 没有指定 targetStats，请在 Inspector 里拖入一个含 CharacterStats 的玩家对象。");
            enabled = false;
            return;
        }

        // 两个 Slider 的数值范围：0 ～ 200（策划文档里，红条 / 白条显示值上限都是 200）
        redSlider.maxValue = 200;
        redSlider.minValue = 0;
        whiteSlider.maxValue = 200;
        whiteSlider.minValue = 0;

  
    }

    void Update()
    {
        // 不管什么时候，都把最新的数值写进去
        int r = targetStats.RedDisplay;
        int w = targetStats.WhiteDisplay;
        redSlider.value = r;
        whiteSlider.value = w;
        Debug.Log($"[RedWhiteBarUI.Update] targetStats.host = {targetStats.gameObject.name} (InstanceID={targetStats.GetInstanceID()})");
        Debug.Log($"[RedWhiteBarUI.Update] l来自BarUI update：RedDisplay={targetStats.RedDisplay}, WhiteDisplay={targetStats.WhiteDisplay}");
        // 每帧从 CharacterStats 里取最新的 RedDisplay / WhiteDisplay
        // 并更新到两个 Slider 上
    
    }
}
