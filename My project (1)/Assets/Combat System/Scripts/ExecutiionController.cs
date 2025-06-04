using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���ڿ��ƴ��� UI ����ʾ�밴ť�ص���
/// ����ű���Ҫ���� ExecutionUI.prefab �ĸ��ڵ㣨�����Ǹ� World Space Canvas �ϣ���
/// </summary>
public class ExecutionUIController : MonoBehaviour
{
    [Header("UI ����")]
    public Button killButton;    // �ϣ�ExecutionUI.prefab �ɱ������ť
    public Button healButton;    // �ϣ�ExecutionUI.prefab ���������ť

    [Header("������ƫ������")]
    [Tooltip("X ��ʾ�ع��ﱾ���Ҳ�ƫ�ƶ��٣�Y ��ʾ̧�߶��٣�Z ��ʾ�ع��ﱾ��ǰ����ƫ�ƶ���")]
    public Vector3 localOffset = new Vector3(1.0f, 1.5f, 0.0f);

    private CharacterStats targetEnemy;   // ��ǰ�ȴ����ѡ��� Enemy

    void Awake()
    {
        Debug.Log("[ExecutionUIController] Awake, killButton=" + (killButton ? killButton.name : "null") + ", healButton=" + (healButton ? healButton.name : "null"));
        // �Ȱ� UI ���أ�ֻ�� Init ʱ����ʾ
        gameObject.SetActive(false);

        // �󶨰�ť�ص�
        killButton.onClick.AddListener(OnKillClicked);
        healButton.onClick.AddListener(OnHealClicked);
    }

    /// <summary>
    /// ��ʼ����� UI��������������Ҫ����һ������������ѡ�񡱡�
    /// ���������ڹ������ + һ��̧�ߣ�ʼ���泯�������
    /// </summary>
    /// <param name="enemy">��ǰ���ڴ�����״̬�ĵ��� CharacterStats</param>
    public void Init(CharacterStats enemyStats)
    {
        targetEnemy = enemyStats;
        Debug.Log("[ExecutionUIController] Init(): �л��ɿɼ� (SetActive(true))��Init ֮ǰ activeSelf = " + gameObject.activeSelf);
        gameObject.SetActive(true);
        Debug.Log("[ExecutionUIController] Init(): �л��� activeSelf = " + gameObject.activeSelf);

        // ���㡰������� + ̧�ߡ����������꣺
        //    ���ȣ��õ����������λ��
        Vector3 basePos = targetEnemy.transform.position;
        //    Ȼ�󣬽� localOffset �ӹ���ı��ؿռ�ת��������ռ䣺
        Vector3 worldOffset =
            targetEnemy.transform.right * localOffset.x   // �򱾵��Ҳ�ƫ��
          + Vector3.up * localOffset.y   // ������ Y �᷽��̧��
          + targetEnemy.transform.forward * localOffset.z;  // �򱾵ء�ǰ����ƫ�ƣ������Ҫ��

        transform.position = basePos + worldOffset;

        // �� UI �泯������������ֿ���
        if (Camera.main != null)
        {
            Vector3 toCamera = Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-toCamera, Vector3.up);
        }
    }

    void LateUpdate()
    {
        if (targetEnemy != null && gameObject.activeSelf)
        {
            // ÿһ֡�����������¼�����������
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

    private void OnKillClicked()
    {
        Debug.Log("[ExecutionUIController] OnKillClicked ������");
        if (targetEnemy != null)
        {
            Debug.Log("[ExecutionUIController] targetEnemy ��Ϊ null������ GameManager.Instance.OnEnemyKillConfirmed");
            // ֪ͨ GameManager�����ѡ��ɱ�����������
            GameManager.Instance.OnEnemyKillConfirmed(targetEnemy);
        }
        else
        {
            Debug.LogWarning("[ExecutionUIController] targetEnemy Ϊ null");
        }
        Destroy(gameObject);
    }

    private void OnHealClicked()
    {

        if (targetEnemy != null)
        {
            // ֪ͨ GameManager�����ѡ���������������
            GameManager.Instance.OnEnemyHealConfirmed(targetEnemy);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // ע�⣺����¼�����������й¶
        killButton.onClick.RemoveListener(OnKillClicked);
        healButton.onClick.RemoveListener(OnHealClicked);
    }
}
