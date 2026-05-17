using UnityEngine;

public class LadderMove : MonoBehaviour
{
    private Rigidbody Rigidbody;
    private CapsuleCollider2D capsuleCollider;
    GameObject groundCheck; // 바닥 체크용 오브젝트

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    void LadderMod() //사다리 타기 활성화 (중력 0, IsTrigger를 통해서 타일 충돌 방지, 위 아래로만 이동 가능, 스페이스바를 통해서 탈출, 전부 오르거나 내려와도 탈출, 공격받을 시 튕겨짐)
    {
        Rigidbody.useGravity = false; // 중력 비활성화
        capsuleCollider.isTrigger = true; // 충돌을 트리거로 설정
        Rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation; // X축 이동과 회전 고정

        float virticalinput = Input.GetAxisRaw("Vertical"); // 수직 입력 받기
        Rigidbody.linearVelocity = new Vector2(Rigidbody.linearVelocity.x, virticalinput * 5f); // 수직 이동 속도 설정
        
        if (Input.GetButtonDown("Jump")) // 스페이스바로 탈출
        {
            Rigidbody.useGravity = true; // 중력 활성화
            capsuleCollider.isTrigger = false; // 충돌 트리거 비활성화
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation; // 회전만 고정
        }





    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
