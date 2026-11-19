using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour
{
    // 현재 밟고 있는 플랫폼을 저장할 변수
    private GameObject currentOneWayPlatform;

    [Tooltip("아래로 내려갈 때 사용할 점프 버튼 이름 (Input Manager 기준)")]
    [SerializeField] private string jumpButtonName = "Jump";

    void Update()
    {
        // '아래 방향키'와 '점프 키'를 동시에 눌렀는지 확인
        if (Input.GetAxisRaw("Vertical") < 0 && Input.GetButtonDown(jumpButtonName))
        {
            // 밟고 있는 플랫폼이 있다면 내려가기 코루틴 실행
            if (currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }
        }
    }

    // 오브젝트가 다른 콜라이더와 충돌을 시작했을 때 호출
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // =================================================================
        // ▼▼▼ 여기를 "Platform"으로 수정했습니다 ▼▼▼
        // =================================================================
        // 충돌한 오브젝트의 레이어가 'Platform'이라면
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            // 현재 밟고 있는 플랫폼으로 지정
            currentOneWayPlatform = collision.gameObject;
        }
    }

    // 오브젝트가 다른 콜라이더와 충돌에서 벗어났을 때 호출
    private void OnCollisionExit2D(Collision2D collision)
    {
        // 떨어진 오브젝트가 현재 밟고 있던 플랫폼이라면
        if (collision.gameObject == currentOneWayPlatform)
        {
            // 참조 초기화
            currentOneWayPlatform = null;
        }
    }

    // 플랫폼과의 충돌을 잠시 비활성화하는 코루틴
    private IEnumerator DisableCollision()
    {
        // 플랫폼의 콜라이더를 가져옴
        Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();

        // 플레이어의 콜라이더와 플랫폼의 콜라이더 간의 충돌을 무시
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), platformCollider, true);

        // 아주 짧은 시간(0.25초) 동안 기다림
        yield return new WaitForSeconds(0.25f);

        // 비활성화 했던 충돌을 다시 활성화
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), platformCollider, false);
    }
}