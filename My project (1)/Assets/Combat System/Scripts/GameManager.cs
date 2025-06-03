using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Execution UI Ԥ�Ƽ�")]
    public GameObject executionUIPrefab; // �ϣ�Assets/Combat System/Prefabs/ExecutionUI.prefab

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // �������������ʱ�����ɵ��ˣ������ڵ������ɵĵط��� OnZeroHP �󶨹�ȥ
        // �����������ǣ��� EnemyHealth.Start() ��ͻ���� RegisterEnemy(this)
        // ����������
    }

    /// <summary>
    /// �� GameManager ����һ�������ɻ򳡾������е� EnemyHealth
    /// </summary>
    public void RegisterEnemy(CharacterStats stats)
    {
        stats.OnZeroHP += HandleEnemyZeroHP;
    }

    /// <summary>
    /// EnemyHealth ���� OnZeroHP ʱ������õ����
    /// ���𵯳��Ǹ����� UI ���
    /// </summary>
    private void HandleEnemyZeroHP(CharacterStats stats)
    {
        // 1. ʵ���� ExecutionUI
        GameObject uiGO = Instantiate(executionUIPrefab);

        // 2. ������ ExecutionUIController �õ������� Init(enemy)
        ExecutionUIController uiCtrl = uiGO.GetComponent<ExecutionUIController>();
        uiCtrl.Init(stats);
    }

    /// <summary>
    /// ������� UI ���㡰ɱ����ʱ��ExecutionUIController ����õ�����
    /// </summary>
    public void OnEnemyKillConfirmed(CharacterStats enemyStats)
    {
        // ������Լӡ�������������߼�
        // ���磺RedWhiteManager.Instance.AddRed(1); RedWhiteManager.Instance.AddWhite(-1);

        // �����õ�������
        enemyStats.ConfirmKill();
    }

    /// <summary>
    /// ������� UI ���㡰������ʱ��ExecutionUIController ����õ�����
    /// </summary>
    public void OnEnemyHealConfirmed(CharacterStats enemyStats)
    {
        // ������Լӡ�������������߼�
        // ���磺RedWhiteManager.Instance.AddWhite(1); RedWhiteManager.Instance.AddRed(-1);

        // �õ��˸���
        enemyStats.ConfirmHeal();
    }
}

