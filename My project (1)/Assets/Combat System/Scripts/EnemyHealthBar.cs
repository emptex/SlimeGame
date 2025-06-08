using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Ѫ���������")]
    [Tooltip("�����ɫѪ���� Image �����RectTransform ê������������ࣩ")]
    public Image redBar;

    [Tooltip("������ﱾ���ϵ� CharacterStats �ű���������ȡ currentHP / maxHP")]
    public CharacterStats targetStats;

    // ��¼ redBar ��������ȣ�����Ѫʱ�� sizeDelta.x��
    private float fullWidth;

    // redBar �� RectTransform ����
    private RectTransform redBarRect;

    void Start()
    {
        // ����������
        if (redBar == null)
        {
            Debug.LogError($"[{nameof(EnemyHealthBar)}] δ���� redBar������ Inspector ������Ѫ���� Image��");
            enabled = false;
            return;
        }
        if (targetStats == null)
        {
            Debug.LogError($"[{nameof(EnemyHealthBar)}] δ���� targetStats������ Inspector ���������� CharacterStats��");
            enabled = false;
            return;
        }

        // ���� RectTransform������¼��ʼ���
        redBarRect = redBar.GetComponent<RectTransform>();
        fullWidth = redBarRect.sizeDelta.x;

        // һ��ʼ�͸���һ�Σ���֤ UI ����ʾ��ȷ�ĳ�ʼѪ��
        UpdateHealthBar();
    }

    void Update()
    {
        // ÿ֡�����һ�Σ��������Ѫ���仯��ˢ��Ѫ��
        UpdateHealthBar();
    }

    /// <summary>
    /// ���� targetStats.currentHP / maxHP ������ redBar �Ŀ��
    /// </summary>
    private void UpdateHealthBar()
    {
        // ��ֹ���� 0
        if (targetStats.maxHP <= 0)
            return;

        // ����Ѫ���ٷֱȣ���Χ [0,1]
        float hpPercent = Mathf.Clamp01((float)targetStats.currentHP / targetStats.maxHP);

        // �����µĿ��
        float newWidth = fullWidth * hpPercent;

        // ֱ���޸� redBarRect �� sizeDelta.x����������ê���������������
        Vector2 size = redBarRect.sizeDelta;
        size.x = newWidth;
        redBarRect.sizeDelta = size;
    }
}
