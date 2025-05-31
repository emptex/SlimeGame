using UnityEngine;

[RequireComponent(typeof(CombatSystem))]
public class PlayerCombatController : MonoBehaviour
{
    private CombatSystem combat;

    void Awake()
    {
        combat = GetComponent<CombatSystem>();
    }

    void Update()
    {
        // 按下左 Ctrl 键触发攻击（可换成其他按键或输入系统）
        if (Input.GetMouseButtonDown(0))
        {
            combat.Attack();
        }
    }
}