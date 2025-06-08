using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExecutionUIController : MonoBehaviour
{
    [Header("提示组件（Image 或 TMP）")]
    public Graphic promptGraphic;
    public TextMeshProUGUI promptText;

    [Header("玩家靠近显示距离")]
    public float showDistance = 3f;

    private CharacterStats targetEnemy;
    private Transform playerTransform;

    public void Init(CharacterStats enemyStats)
    {
        targetEnemy = enemyStats;
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerGO ? playerGO.transform : null;

        if (promptText != null)
            promptText.text = "按 H 治愈\n按 K 杀死";

        if (promptGraphic != null)
            promptGraphic.enabled = false;

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (targetEnemy == null || playerTransform == null) return;

        // 如果敌人已复活/被移除，销毁 UI
        if (targetEnemy.currentHP != 0)
        {
            Destroy(gameObject);
            return;
        }

        float dist = Vector3.Distance(playerTransform.position, targetEnemy.transform.position);
        if (dist <= showDistance)
        {
            // 进入范围：显示提示
            if (promptGraphic != null && !promptGraphic.enabled)
                promptGraphic.enabled = true;

            // 按键检测
            if (Input.GetKeyDown(KeyCode.H))
            {
                GameManager.Instance.OnEnemyHealConfirmed(targetEnemy);
                Destroy(gameObject);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.Instance.OnEnemyKillConfirmed(targetEnemy);
                Destroy(gameObject);
            }
        }
        else
        {
            // 离开范围：只隐藏，不销毁
            if (promptGraphic != null && promptGraphic.enabled)
                promptGraphic.enabled = false;
        }
    }
}