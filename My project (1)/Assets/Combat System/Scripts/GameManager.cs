using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private List<CharacterStats> enemies = new List<CharacterStats>();

    [Header("Execution UI Ԥ�Ƽ�")]
    public GameObject executionUIPrefab; // �ϣ�Assets/Combat System/Prefabs/ExecutionUI.prefab

    // ��֤����������ֻ��һ�����泡�����ٵ�GameManagerʵ��
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[GameManager] �����ظ��� GameManager ʵ�������٣�{name}");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        Debug.Log($"[GameManager] ������ʼ����ɣ�ʵ������{name}");
    }


    /// ����һ�����ú���RegisterEnemy���� GameManager ����һ�������ɻ򳡾������е� CharacterStats�������� stats �� ApplyHPChange(...) �з���Ѫ������ʱ���ᴥ�� OnZeroHP �¼�����ʱ�Զ����� HandleEnemyZeroHP(stats)
    public void RegisterEnemy(CharacterStats stats)
    {
        if (!enemies.Contains(stats))
        {
            enemies.Add(stats);
            // ����������Ѫ�¼�
            stats.OnZeroHP += HandleEnemyZeroHP;
            Debug.Log($"[GameManager] �Ѷ��� {stats.gameObject.name} �� OnZeroHP");
        }
    }

    /// <summary>
    /// EnemyHealth ���� OnZeroHP ʱ������õ����
    /// ���𵯳��Ǹ����� UI ���
    /// </summary>
    private void HandleEnemyZeroHP(CharacterStats stats)
    {
        Debug.Log($"[GameManager] �յ� {stats.gameObject.name} Ѫ�������¼����������� UI");
        if (executionUIPrefab == null)
        {
            Debug.LogError("[GameManager] executionUIPrefab Ϊ null������ Inspector ������ Prefab");
            return;

        }
        // 1. ʵ���� ExecutionUI
        GameObject uiGO = Instantiate(executionUIPrefab);
        uiGO.SetActive(true); // ǿ�Ƽ���
        Debug.Log("[GameManager] Instantiate ����ɣ�uiGO = " + uiGO.name
              + "��activeSelf = " + uiGO.activeSelf);

        // 2. ������ ExecutionUIController �õ������� Init(stats)
        ExecutionUIController uiCtrl = uiGO.GetComponent<ExecutionUIController>();
        if (uiCtrl == null)
        {
            Debug.LogError("[GameManager] ִ�� Prefab ��û�õ� ExecutionUIController �����");
            return;
        }
        Debug.Log("[GameManager] �õ� uiCtrl��׼������ Init()������ǰ uiGO.activeSelf = " + uiGO.activeSelf);
        uiCtrl.Init(stats);
        Debug.Log("[GameManager] ������ Init(), uiGO.activeSelf = " + uiGO.activeSelf);
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

