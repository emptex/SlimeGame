using UnityEngine;

public class Followtarget : MonoBehaviour
{
    // ����������
    public Vector3 offset = new Vector3(0, 2f, -5f); // ���Ŀ���ƫ����
    public float rotationDamping = 3f; // ��ת�����ٶ�
    public float positionDamping = 3f; // λ�ø����ٶ�

    // ����Inspector��ָ���ĸ���Ŀ��
    [SerializeField] private Transform player;
    [SerializeField] private Transform currentTarget;
    private Vector3 currentOffset;

    void Start()
    {
        // �����Inspector��δָ��Ŀ�꣬���Բ��ұ�ǩΪ"Player"�Ķ���
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        currentTarget = player;
        currentOffset = offset;
    }

    // ���ű���Inspector�б��޸�ʱ���ã�ȷ��offset�仯ʱ����currentOffset
    void OnValidate()
    {
        currentOffset = offset;
    }

    void LateUpdate()
    {
        if (currentTarget == null)
        {
            ResetToPlayer();
            return;
        }

        // ����Ŀ��λ�ã�����Ŀ�����ת��
        Vector3 targetPosition = currentTarget.position +
            currentTarget.rotation * currentOffset;

        // ƽ���ƶ������Ŀ��λ��
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * positionDamping
        );

        // ƽ����ת�������Ŀ��
        Quaternion targetRotation = Quaternion.LookRotation(
            currentTarget.position - transform.position,
            currentTarget.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationDamping
        );
    }

    // ������Ŀ�겢ʹ��Ĭ��ƫ����
    public void SetTarget(Transform newTarget)
    {
        SetTarget(newTarget, offset);
    }

    // ������Ŀ�겢ʹ���Զ���ƫ����
    public void SetTarget(Transform newTarget, Vector3 newOffset)
    {
        if (newTarget != null)
        {
            currentTarget = newTarget;
            currentOffset = newOffset;
        }
    }

    // ���õ���Ҳ�ʹ��Ĭ��ƫ����
    public void ResetToPlayer()
    {
        ResetToPlayer(offset);
    }

    // ���õ���Ҳ�ʹ���Զ���ƫ����
    public void ResetToPlayer(Vector3 newOffset)
    {
        if (player != null)
        {
            currentTarget = player;
            currentOffset = newOffset;
        }
    }
}
