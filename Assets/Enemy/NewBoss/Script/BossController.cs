using UnityEngine;
using System.Collections;

// 필수 컴포넌트 자동 추가 (실수로 컴포넌트 삭제 방지)
[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어(타겟)를 여기에 연결하세요")]
    public Transform Target;
    public BossStatsSystem Stats;
    public Animator Anim;

    [Header("Movement Settings")]
    public float MoveSpeed = 3.0f;//이동 속도
    public float AttackRange = 2.5f; //공격 사거리
    public float VisionRange = 10.0f; // 추적 시작 거리

    [Header("Debug Info")]
    [SerializeField] private string _currentStateName; // 현재 상태 확인용

    // 내부 변수
    private Rigidbody2D _rb;
    private IBossState _currentState;

    // 프로퍼티 (상태 클래스에서 접근용)
    public bool CanUseHeavyAttack { get; private set; } = true;
    public bool TargetFound { get; private set; }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // 레퍼런스가 비어있으면 자동으로 찾아서 연결
        if (Stats == null) Stats = GetComponent<BossStatsSystem>();
       // if (Anim == null) Anim = GetComponent<Animator>();
    }

    private void Start()
    {
        // 시작 시 대기(Idle) 상태로 진입
        ChangeState(new IdleState());

        // 기믹 처리를 위한 이벤트 연결
        if (Stats != null)
        {
            Stats.OnWaterDepleted += HandleWaterDepleted;
            Stats.OnDamageTaken += HandleDamageTaken;
        }
    }

    private void Update()
    {
        if (Target != null)
        {
            // Unity 6에서도 2D 거리는 Vector2로 계산하는 것이 정확합니다.
            float dist = Vector2.Distance(transform.position, Target.position);
            TargetFound = dist <= VisionRange;
        }

        // 현재 상태의 로직 실행 (매 프레임)
        _currentState?.Execute(this);
    }

    public void ChangeState(IBossState newState)
    {
        if (_currentState != null)
            _currentState.Exit(this);

        _currentState = newState;
        _currentStateName = newState.GetType().Name; // 인스펙터에 상태 이름 표시
        _currentState.Enter(this);
    }

    // --- 이동 로직 (Unity 6 Physics 2D) ---
    public void MoveTo(Vector2 targetPos)
    {
        Vector2 currentPos = transform.position;
        Vector2 direction = (targetPos - currentPos).normalized;

        // [Unity 6 변경점] velocity 대신 linearVelocity를 사용합니다.
        _rb.linearVelocity = direction * MoveSpeed;

        LookAtTarget(targetPos);

        //if (Anim != null) Anim.SetBool("IsMoving", true);
    }

    public void StopMove()
    {
        // 이동 정지
        _rb.linearVelocity = Vector2.zero;
        //if (Anim != null) Anim.SetBool("IsMoving", false);
    }

    private void LookAtTarget(Vector2 targetPos)
    {
        // 크기(Scale)는 Z축이 1이어야 하므로 Vector3를 사용합니다.
        if (targetPos.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);       // 오른쪽 보기
        else if (targetPos.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);      // 왼쪽 보기
    }

    // --- 공격 쿨타임 및 루틴 ---
    public void StartHeavyAttackCooldown(float cooldown)
    {
        StartCoroutine(HeavyAttackRoutine(cooldown));
    }

    private IEnumerator HeavyAttackRoutine(float cooldown)
    {
        CanUseHeavyAttack = false;
        yield return new WaitForSeconds(cooldown);
        CanUseHeavyAttack = true;
    }

    // 특정 애니메이션이 끝났는지 확인하는 함수 (이름으로 체크)
    public bool CheckAnimationState(string stateName)
    {
        // 0번 레이어의 현재 상태 정보를 가져옴
        AnimatorStateInfo stateInfo = Anim.GetCurrentAnimatorStateInfo(0);

        // 1. 현재 재생 중인 애니메이션이 stateName과 일치하는가?
        // 2. 진행도(normalizedTime)가 1.0(100%)을 넘었는가?
        // 3. 현재 전환(Transition) 중이 아닌가?
        if (stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1.0f && !Anim.IsInTransition(0))
        {
            return true;
        }
        return false;
    }

    // --- 이벤트 핸들러 (기믹 구현) ---

    // 피격 시 반격 로직
    private void HandleDamageTaken()
    {
        // 조건: 베리어 있음 + 현재 반격 중 아님 + 그로기 아님
        if (Stats.IsBarrierActive && !(_currentState is CounterState) && !(_currentState is GroggyState))
        {
            ChangeState(new CounterState());
        }
    }

    // 물 고갈 시 그로기 로직
    private void HandleWaterDepleted()
    {
        // 이미 그로기가 아니라면 전환
        if (!(_currentState is GroggyState))
        {
            StopMove();
            ChangeState(new GroggyState());
        }
    }

    // 메모리 누수 방지 (이벤트 연결 해제)
    private void OnDestroy()
    {
        if (Stats != null)
        {
            Stats.OnWaterDepleted -= HandleWaterDepleted;
            Stats.OnDamageTaken -= HandleDamageTaken;
        }
    }

    // 에디터 상에서 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, VisionRange);
    }
}