using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // ���ΰ�
    public float smoothing = 5f;

    // [����] �������� ���ο��� ������� �ʰ�, �⺻���� ���⼭ ���ع����ϴ�.
    // (0, 0, -10)�� 2D ������ ���� ��ġ�Դϴ�. (X,Y�� ���߾�, Z�� �ڷ� 10��ŭ)
    public Vector3 offset = new Vector3(0f, 0f, 10f);

    void Start()
    {
        // [�߿�] ���� ī�޶� ��� �ֵ� �������,
        // ���� �������ڸ��� ������ Ÿ���� '�̻����� ��ġ(������ ����)'�� �����̵���ŵ�ϴ�.
        transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }
}