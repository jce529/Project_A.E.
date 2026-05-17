using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // 주인공
    public float smoothing = 5f;

    // [수정] 오프셋을 내부에서 계산하지 않고, 기본값을 여기서 정해버립니다.
    // (0, 0, -10)은 2D 게임의 국룰 위치입니다. (X,Y는 정중앙, Z는 뒤로 10만큼)
    public Vector3 offset = new Vector3(0f, 0f, 10f);

    void Start()
    {
        // [중요] 현재 카메라가 어디 있든 상관없이,
        // 게임 시작하자마자 강제로 타겟의 '이상적인 위치(오프셋 적용)'로 순간이동시킵니다.
        transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }
}