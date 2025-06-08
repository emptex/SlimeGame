using UnityEngine;

public class DistanceVisibility : MonoBehaviour
{
    public float visibilityDistance = 2f; // �ɼ����л�����
    public Transform player; // ��ҵ�Transform����
    public bool useRenderer = true; // �Ƿ�ʹ��Renderer���ƿɼ���
    public bool useCollider = false; // �Ƿ�ʹ��Collider������ײ

    private Renderer objectRenderer;
    private Collider objectCollider;
    private bool isVisible = true;

    void Start()
    {
        // ���δָ����ң����Բ��ұ�ǩΪ"Player"�Ķ���
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // ��ȡ�����Renderer���
        objectRenderer = GetComponent<Renderer>();

        // ��ȡ�����Collider���
        objectCollider = GetComponent<Collider>();
    }

    void Update()
    {
        // ���û��������ã���ִ�к����߼�
        if (player == null)
            return;

        // ��������ҵľ���
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ���ݾ����������Ŀɼ���
        if (distanceToPlayer <= visibilityDistance && isVisible)
        {
            // ����С�ڵ�����ֵ�ҵ�ǰ�ɼ������������
            HideObject();
        }
        else if (distanceToPlayer > visibilityDistance && !isVisible)
        {
            // ���������ֵ�ҵ�ǰ���ɼ�������ʾ���
            ShowObject();
        }
    }

    // �������
    void HideObject()
    {
        isVisible = false;

        // ���������Renderer���ƣ�������Ϊ���ɼ�
        if (useRenderer && objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }

        // ���������Collider���ƣ��������ײ
        if (useCollider && objectCollider != null)
        {
            objectCollider.enabled = false;
        }
    }

    // ��ʾ���
    void ShowObject()
    {
        isVisible = true;

        // ���������Renderer���ƣ�������Ϊ�ɼ�
        if (useRenderer && objectRenderer != null)
        {
            objectRenderer.enabled = true;
        }

        // ���������Collider���ƣ���������ײ
        if (useCollider && objectCollider != null)
        {
            objectCollider.enabled = true;
        }
    }

    // ���ű���Inspector�б��޸�ʱ����
    void OnValidate()
    {
        // ȷ������ֵΪ����
        visibilityDistance = Mathf.Max(0, visibilityDistance);
    }
}
