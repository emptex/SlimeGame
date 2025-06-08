using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private List<CharacterStats> enemies = new List<CharacterStats>();


    // �洢��ǰ�ؿ��У���ҽ�ɫ�ġ��������͡�����ʵ��ֵ���������糡���̳�
    [HideInInspector] public int playerRed;
    [HideInInspector] public int playerWhiteReal;

    // ����Ƿ�ոմ���ʵ�����������֡���һ�ء�������ؿ���
    private bool justCreated = false;

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

        // ��ʼ����һ�غ����
        justCreated = true;
        playerRed = 100;
        playerWhiteReal = 100;
        SceneManager.sceneLoaded += OnLevelLoaded; // ����������������¼����Ա���¹ؿ�����Ҵ�ֵ
    }
    /// ÿ����һ���³���������ɣ��ͻ��������ص������ϴιؿ�����ĺ�����������³������������ϡ�
    private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] ���� {scene.name} ������ɣ�׼������ҳ�ʼ���������({playerRed},{playerWhiteReal})");
        // �����ҳ����� Tag="Player" �����壬�Ѻ�������ݸ���
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");   
        if (playerGO != null)
        {
            CharacterStats playerStats = playerGO.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                // ����ոմ��������ǵ�һ�أ���justCreated=true����ʱ playerRed/WhiteReal �Ѿ��� (100,100)
                playerStats.InitializeRedWhite(playerRed, playerWhiteReal);
                Debug.Log($"[GameManager] ���� ��{scene.name}�� ������ɣ����ݸ���� ��={playerStats.redValue}, ��ʵ��={playerStats.redValue} (whiteDisplay={playerStats.WhiteDisplay})");
            }
            else
            {
                Debug.LogWarning("[GameManager] ����������ɣ����Ҳ��� CharacterStats �����");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] ����������ɣ����Ҳ��� Tag=\"Player\" �����壡");
        }

        // ������һ���ؿ���justCreated Ӧ����Ϊ false�����涼���ü̳�ֵ��
        justCreated = false;
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

    /// Execution:ȷ�ϻ�ɱ
    public void OnEnemyKillConfirmed(CharacterStats enemyStats)
    {

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");//���¡���ҵĺ������
        if (playerGO != null)
        {
            CharacterStats playerStats = playerGO.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                playerStats.OnEnemyKilled(); // ����+4������-5

                playerRed = playerStats.redValue;// ���µ� GameManager ��Ա�ؿ�������̳�
                playerWhiteReal = playerStats.whiteRealValue;
            }
            enemyStats.ConfirmKill();// �����õ�������
        }
    }
    /// Execution:ȷ������
    public void OnEnemyHealConfirmed(CharacterStats enemyStats)
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            CharacterStats playerStats = playerGO.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                playerStats.OnEnemyHealed(); // ����-5������+12���������

                // ���µ� GameManager ����ڼ̳�
                playerRed = playerStats.redValue;
                playerWhiteReal = playerStats.whiteRealValue;
            }
        }
        enemyStats.ConfirmHeal();
    }
}

