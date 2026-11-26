using UnityEngine;

public class Mimic : MonoBehaviour
{
    [Header("미믹 설정")]
    [Tooltip("플레이어 상호작용 키")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("깨어났을 때 바뀔 몬스터 스프라이트 (애니메이터가 있다면 비워둬도 됨)")]
    public Sprite monsterSprite;

    [Header("제어할 AI 컴포넌트들")]
    [Tooltip("깨어날 때 켜질 스크립트들 (예: Chase, EnemyBrain, HP 등)")]
    public Behaviour[] componentsToEnable;

    private SpriteRenderer sr;
    private Animator anim;
    private bool isAwake = false;
    private bool isPlayerInRange = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // 처음 상자 상태일 때는 몬스터 AI와 체력 관련 스크립트들을 전부 꺼둡니다.
        foreach (Behaviour comp in componentsToEnable)
        {
            if (comp != null) comp.enabled = false;
        }
    }

    void Update()
    {
        // 이미 깨어난 상태면 Update 무시
        if (isAwake) return;

        // 플레이어가 근처에 있고 상호작용 키를 누르면 기상
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            WakeUp();
        }
    }

    // 1. 플레이어 접근 체크 및 공격 피격 체크
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAwake) return;

        // 상호작용 범위에 플레이어가 들어옴
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
        else if (other.CompareTag("PlayerAttack"))
        {
            WakeUp();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    // 2. 미믹 기상 로직 (스위치 ON)
    public void WakeUp()
    {
        if (isAwake) return;
        isAwake = true;

        // 외형을 몬스터로 변경 (애니메이터가 있다면 Trigger 방식 추천)
        if (anim != null)
        {
            anim.SetTrigger("Awake"); // 미믹 기상 애니메이션 재생
        }
        else if (sr != null && monsterSprite != null)
        {
            sr.sprite = monsterSprite; // 애니메이션이 없다면 이미지만 틱 교체
        }

        // 꺼두었던 AI 스크립트(추적, 공격, 체력 관리 등)를 일제히 켬
        foreach (Behaviour comp in componentsToEnable)
        {
            if (comp != null) comp.enabled = true;
        }

    }
}