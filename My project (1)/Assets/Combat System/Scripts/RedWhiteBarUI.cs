using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RedWhiteBarUI��
///  - ����ҵ� CharacterStats.RedDisplay / CharacterStats.WhiteDisplay ӳ�䵽���� Slider �ϡ�
///  - RedDisplay / WhiteDisplay ���� 0 �� 200 ֮�����ֵ����ʾ���� 200����
///  - ʹ�ò��裺
///    1. �ڳ����ﴴ������������ Slider��һ����������һ�����������������������ɫ��
///    2. �Ѵ˽ű�����ͬһ���������ϣ������ Canvas �϶����ԣ���
///    3. �� Inspector ��� ��redSlider�� �ֶ�������� Slider���� ��whiteSlider�� ������� Slider��
///       �� ��targetStats�� ������ҵ� CharacterStats �����Tag=��Player�� �������ϵ��Ǹ�����
///  ����ʱ������� Slider ��ֵͬ����Ŀ���ɫ�� RedDisplay / WhiteDisplay��
/// </summary>
public class RedWhiteBarUI : MonoBehaviour
{
    [Header("UI ����")]
    [Tooltip("��������� Slider ��������ͼ�ɸ�Ϊ��ɫ��")]
    public Slider redSlider;

    [Tooltip("��������� Slider ��������ͼ�ɸ�Ϊ��ɫ��")]
    public Slider whiteSlider;

    [Header("Ŀ���ɫ CharacterStats��Tag=Player��")]
    [Tooltip("������ҽ�ɫ���Ϲҵ� CharacterStats ���")]
    public CharacterStats targetStats;

    void Start()
    {
        // ���ü��
        if (redSlider == null)
        {
            Debug.LogError($"[{nameof(RedWhiteBarUI)}] û��ָ�� redSlider������ Inspector ������һ������ Slider��");
            enabled = false;
            return;
        }
        if (whiteSlider == null)
        {
            Debug.LogError($"[{nameof(RedWhiteBarUI)}] û��ָ�� whiteSlider������ Inspector ������һ������ Slider��");
            enabled = false;
            return;
        }
        if (targetStats == null)
        {
            Debug.LogError($"[{nameof(RedWhiteBarUI)}] û��ָ�� targetStats������ Inspector ������һ���� CharacterStats ����Ҷ���");
            enabled = false;
            return;
        }

        // ���� Slider ����ֵ��Χ��0 �� 200���߻��ĵ������ / ������ʾֵ���޶��� 200��
        redSlider.maxValue = 200;
        redSlider.minValue = 0;
        whiteSlider.maxValue = 200;
        whiteSlider.minValue = 0;

  
    }

    void Update()
    {
        // ����ʲôʱ�򣬶������µ���ֵд��ȥ
        int r = targetStats.RedDisplay;
        int w = targetStats.WhiteDisplay;
        redSlider.value = r;
        whiteSlider.value = w;
        Debug.Log($"[RedWhiteBarUI.Update] targetStats.host = {targetStats.gameObject.name} (InstanceID={targetStats.GetInstanceID()})");
        Debug.Log($"[RedWhiteBarUI.Update] l����BarUI update��RedDisplay={targetStats.RedDisplay}, WhiteDisplay={targetStats.WhiteDisplay}");
        // ÿ֡�� CharacterStats ��ȡ���µ� RedDisplay / WhiteDisplay
        // �����µ����� Slider ��
    
    }
}
