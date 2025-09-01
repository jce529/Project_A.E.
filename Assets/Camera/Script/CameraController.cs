using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; //타겟 위치
    public float smoothing = 5f; //카메라 이동 속도
    Vector3 offset; //카메라와 타겟 사이의 거리


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - target.position; //카메라와 타겟 사이의 거리 계산
    }

    void LateUpdate() //LateUpdate는 Update 후에 호출되어 카메라 위치를 업데이트
    {
        Vector3 targetCamPos = target.position + offset; //타겟 위치에 오프셋을 더하여 카메라의 목표 위치 계산
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime); //카메라를 부드럽게 이동
    }
}
