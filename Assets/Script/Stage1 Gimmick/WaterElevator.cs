//물 장판에 들어가는 코드
using System.Collections;
using UnityEngine;

public class WaterElevatorGimmick : MonoBehaviour
{
    [Header("오브젝트 할당")]
    public Transform platform;           // 이동할 장판 (Rigidbody2D Kinematic 필수)
    [Tooltip("Pivot이 'Bottom'으로 설정된 물 스프라이트 오브젝트")]
    public Transform waterVisual;        // 위로 길어질 물 오브젝트
    public Collider2D upperAreaBlocker;  // 상승 완료 전 위쪽 진입을 막는 투명 가벽

    [Header("기믹 설정")]
    [Tooltip("상승할 목표 높이 (장판의 이동량 & 물의 최종 Y 스케일)")]
    public float targetHeight = 5f;
    [Tooltip("상승 및 하강 속도")]
    public float riseSpeed = 2f;
    [Tooltip("버튼 작동 후 물이 차오르기까지의 대기 시간")]
    public float delayBeforeRise = 2f;
    [Tooltip("최고점 도달 후 대기하는 시간")]
    public float holdTimeAtTop = 3f;

    private Vector2 platformStartPos;
    private Vector3 waterStartScale;
    private bool isActivated = false;
    private Rigidbody2D platformRb;

    void Start()
    {
        // 초기 위치 및 스케일 저장
        platformStartPos = platform.position;
        waterStartScale = waterVisual.localScale; // 물은 위치가 아닌 초기 크기를 저장

        platformRb = platform.GetComponent<Rigidbody2D>();

        if (upperAreaBlocker != null)
            upperAreaBlocker.enabled = true;
    }

    public void ActivateGimmick()
    {
        if (!isActivated)
        {
            StartCoroutine(ElevatorRoutine());
        }
    }

    private IEnumerator ElevatorRoutine()
    {
        isActivated = true;

        yield return new WaitForSeconds(delayBeforeRise);

        // 장판의 목표 위치
        Vector2 platformTargetPos = platformStartPos + Vector2.up * targetHeight;

        // 물의 목표 스케일 (X, Z는 유지, Y만 targetHeight만큼 증가)
        // ※ 물의 초기 Y 스케일이 1이라고 가정할 때, 최종 스케일은 1 + targetHeight 정도가 되도록 기획에 맞춰 세팅
        Vector3 waterTargetScale = new Vector3(waterStartScale.x, waterStartScale.y + targetHeight, waterStartScale.z);

        // 2. 장판은 상승, 물은 위로 길어짐
        while (Vector2.Distance(platform.position, platformTargetPos) > 0.01f)
        {
            // 장판 물리 이동
            if (platformRb != null)
            {
                Vector2 nextPos = Vector2.MoveTowards(platformRb.position, platformTargetPos, riseSpeed * Time.fixedDeltaTime);
                platformRb.MovePosition(nextPos);
            }
            else
            {
                platform.position = Vector2.MoveTowards(platform.position, platformTargetPos, riseSpeed * Time.fixedDeltaTime);
            }

            // 물 비주얼 스케일(크기) 증가
            waterVisual.localScale = Vector3.MoveTowards(waterVisual.localScale, waterTargetScale, riseSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
        }

        // 보정
        if (platformRb != null) platformRb.position = platformTargetPos;
        else platform.position = platformTargetPos;
        waterVisual.localScale = waterTargetScale;

        // 3. 상승 완료: 가벽 해제
        if (upperAreaBlocker != null)
            upperAreaBlocker.enabled = false;

        // 4. 대기
        yield return new WaitForSeconds(holdTimeAtTop);

        // 하강 시작: 가벽 다시 활성화
        if (upperAreaBlocker != null)
            upperAreaBlocker.enabled = true;

        // 5. 원상복구 (장판 하강, 물 수축)
        while (Vector2.Distance(platform.position, platformStartPos) > 0.01f)
        {
            if (platformRb != null)
            {
                Vector2 nextPos = Vector2.MoveTowards(platformRb.position, platformStartPos, riseSpeed * Time.fixedDeltaTime);
                platformRb.MovePosition(nextPos);
            }
            else
            {
                platform.position = Vector2.MoveTowards(platform.position, platformStartPos, riseSpeed * Time.fixedDeltaTime);
            }

            waterVisual.localScale = Vector3.MoveTowards(waterVisual.localScale, waterStartScale, riseSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
        }

        // 초기화 보정
        if (platformRb != null) platformRb.position = platformStartPos;
        else platform.position = platformStartPos;
        waterVisual.localScale = waterStartScale;

        isActivated = false;
    }
}