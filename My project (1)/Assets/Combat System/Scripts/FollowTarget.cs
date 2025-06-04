using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [Header("Ŀ������")]
    [SerializeField] private string targetTag = "Player"; // Ŀ���ǩ
    [SerializeField] private Transform targetOverride; // ��ѡ��ֱ��ָ��Ŀ�����

    [Header("�������")]
    [SerializeField] private float followSpeed = 5f; // �����ٶ�
    [SerializeField] private bool followRotation = false; // �Ƿ����Ŀ����ת

    [Header("λ�ò���")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 5, -10); // λ��ƫ��
    [SerializeField] private bool useLocalOffset = true; // �Ƿ�ʹ�þֲ�����ϵƫ��

    [Header("��ת����")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(30, 0, 0); // �̶���ת�Ƕ�
    [SerializeField] private bool lookAtTarget = false; // �Ƿ�ʼ�տ���Ŀ��

    private Transform target;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 initialOffset;

    void Start()
    {
        // ����Ŀ�����
        FindTarget();

        // ��¼��ʼƫ����
        if (target != null)
            initialOffset = transform.position - target.position;
    }

    void FindTarget()
    {
        // ����ʹ��ֱ��ָ����Ŀ��
        if (targetOverride != null)
        {
            target = targetOverride;
            return;
        }

        // ����ͨ����ǩ����
        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
        {
            target = targetObj.transform;
        }
        else
        {
            Debug.LogError($"�Ҳ�����ǩΪ '{targetTag}' ����Ϸ����!");
        }
    }

    void LateUpdate()
    {
        // ���û��Ŀ�꣬�������²���
        if (target == null)
        {
            FindTarget();
            return;
        }

        // ����Ŀ��λ��
        Vector3 targetPosition;

        if (useLocalOffset)
        {
            // ʹ�þֲ�����ϵƫ�ƣ�ԭʼ����ĺ����߼���
            targetPosition = target.position + target.TransformDirection(positionOffset);
        }
        else
        {
            // ʹ����������ϵƫ��
            targetPosition = target.position + positionOffset;
        }

        // ƽ���ƶ������Ŀ��λ��
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / followSpeed
        );

        // ���������ת
        if (lookAtTarget)
        {
            // ����Ŀ��
            transform.LookAt(target);
        }
        else if (!followRotation)
        {
            // ʹ�ù̶���ת�Ƕ�
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
    }
}
