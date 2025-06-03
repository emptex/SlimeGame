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

    private CharacterStats targetEnemy;   // ��ǰ�ȴ����ѡ��� Enemy

    void Awake()
    {
        // �Ȱ� UI ���أ�ֻ�� Init ʱ����ʾ
        gameObject.SetActive(false);

        // �󶨰�ť�ص�
        killButton.onClick.AddListener(OnKillClicked);
        healButton.onClick.AddListener(OnHealClicked);
    }

    /// <summary>
    /// ��ʼ����� UI��������������Ҫ����һ������������ѡ�񡱡�
    /// </summary>
    /// <param name="enemy">��ǰ���ڴ�����״̬�� EnemyHealth</param>
    public void Init(CharacterStats enemyStats)
    {
        targetEnemy = enemyStats;
        gameObject.SetActive(true);

        // �� UI ��λ������ͷ����������˵ĸ��ڵ��ڵ���߶ȣ��� 0��������Ӹ�ƫ��
        Vector3 headPos = targetEnemy.transform.position + Vector3.up * 2.0f; // ����Ը��ݵ���ģ�͸߶ȵ���ƫ����
        transform.position = headPos;

        // �� UI �泯������������ֿ���
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        }
    }

    private void LateUpdate()
    {
        // �����Һ������λ�ñ仯�����˻�û�����٣����� UI ����ͷ��
        if (targetEnemy != null && gameObject.activeSelf)
        {
            Vector3 headPos = targetEnemy.transform.position + Vector3.up * 2.0f;
            transform.position = headPos;
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
            }
        }
    }

    private void OnKillClicked()
    {
        if (targetEnemy != null)
        {
            // ֪ͨ GameManager�����ѡ��ɱ�����������
            GameManager.Instance.OnEnemyKillConfirmed(targetEnemy);
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
