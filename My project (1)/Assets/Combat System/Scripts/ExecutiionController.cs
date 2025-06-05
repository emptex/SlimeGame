using UnityEngine;
using UnityEngine.UI;

public class ExecutionUIController : MonoBehaviour
{
    [Header("UI 提示文字")]
    public Text promptText; // UI Text：显示“Press R to Kill / Y to Heal”

    [Header("偏移设置")]
    public Vector3 localOffset = new Vector3(1.0f, 1.5f, 0.0f);

    private CharacterStats targetEnemy;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Init(CharacterStats enemyStats)
    {
        targetEnemy = enemyStats;
        gameObject.SetActive(true);

        if (promptText != null)
        {
            promptText.text = "Press R to Kill\nPress Y to Heal";
        }

        UpdatePositionAndRotation();
    }

    void LateUpdate()
    {
        if (targetEnemy != null && gameObject.activeSelf)
        {
            UpdatePositionAndRotation();

            // 🔁 监听按键
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameManager.Instance.OnEnemyKillConfirmed(targetEnemy);
                Destroy(gameObject);
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                GameManager.Instance.OnEnemyHealConfirmed(targetEnemy);
                Destroy(gameObject);
            }
        }
    }

    void UpdatePositionAndRotation()
    {
        Vector3 basePos = targetEnemy.transform.position;
        Vector3 worldOffset =
              targetEnemy.transform.right * localOffset.x
            + Vector3.up * localOffset.y
            + targetEnemy.transform.forward * localOffset.z;

        transform.position = basePos + worldOffset;

        if (Camera.main != null)
        {
            Vector3 toCamera = Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-toCamera, Vector3.up);
        }
    }
}