using UnityEngine;

/// <summary>
/// ��һ�� World Space Canvas �µ� UI������Ѫ����ʼ������ָ���������
/// ���û���� Inspector ���ֶ�ָ�� targetCamera���᳢��ʹ�� Camera.main�������� Tag Ϊ MainCamera �����������
/// ͬʱ����� Debug.Log ���������������ú���ת�߼��Ƿ���ȷ��
/// </summary>
public class FaceUICamera : MonoBehaviour
{
    [Header("���� ָ����Ҫ�泯������� ����")]
    [Tooltip("�����ĳ��������������Ĭ�ϵ� MainCamera�������� Inspector �����Ҫ�õ�������ϵ����")]
    public Camera targetCamera;

    // �����Ա���һ֡Ŀ����ת��ֻ�е��Ƕȱ仯ʱ�Ŵ�ӡ��־
    private Quaternion lastTargetRotation;

    void Start()
    {
        // ���û���� Inspector �︳ֵ���ͳ����� Camera.main
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera != null)
            {
                Debug.Log($"[{nameof(FaceUICamera)}] Start(): δָ�� targetCamera���Զ�ʹ�� Camera.main ({targetCamera.name})��");
            }
            else
            {
                Debug.LogError($"[{nameof(FaceUICamera)}] Start(): targetCamera Ҳû�У�Camera.main Ϊ null�������� Inspector ��ָ��һ����������á�");
                enabled = false;
                return;
            }
        }
        else
        {
            Debug.Log($"[{nameof(FaceUICamera)}] Start(): ʹ�� Inspector ��ָ�����������{targetCamera.name}��");
        }

        // ��ʼ�� lastTargetRotation���������Ƚ��Ƿ��нǶȱ仯
        lastTargetRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            return;

        // ����ӵ�ǰ����ָ�������������
        Vector3 dirToCamera = targetCamera.transform.position - transform.position;

        // �����ֻ���� UI ��ˮƽ�棨XZ ƽ�棩���������ת��������������ĸ�������б���ʹ���һ�У�
        // dirToCamera.y = 0f;

        // �������̫С��������Ϊ�㣬��������֡��ת
        if (dirToCamera.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning($"[{nameof(FaceUICamera)}] LateUpdate(): ����������������غϣ�������֡��ת��");
            return;
        }

        // ����Ŀ����ת���ñ�����ı��ء�ǰ������Z+ ���򣩶�׼�����
        Quaternion targetRot = Quaternion.LookRotation(dirToCamera);

        // ֻ������ת�Ƕ������Ա仯��ʱ��Ŵ�ӡ������Ϣ
        float angleDelta = Quaternion.Angle(lastTargetRotation, targetRot);
        if (angleDelta > 0.01f)
        {
            //Debug.Log($"[{nameof(FaceUICamera)}] LateUpdate(): dirToCamera.normalize = {dirToCamera.normalized}, Ŀ��ŷ���� = {targetRot.eulerAngles}");
            lastTargetRotation = targetRot;
        }

        // ���ո�ֵ���������泯�����
        transform.rotation = targetRot;
    }
}
