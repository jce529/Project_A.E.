using UnityEngine;

public class CombatState : IBossState
{
    protected float _decisionTimer;

    // 현재 공격 중인 전략을 관리하기 위한 필드
    private IAttackStrategy _currentAttack;
    private bool _isAttacking; // 공격 중인지 여부

    private float _attackWaitTimer; // 애니메이션 타임아웃용 타이머
    private const float MaxAttackDuration = 0.5f; // 최대 대기 시간 (애니메이션 없을 때 대비)

    public virtual void Enter(BossController boss)
    {
        boss.StopMove();
        _decisionTimer = 0;
        _isAttacking = false;
        _currentAttack = null;
        _attackWaitTimer = 0;
    }

    public virtual void Execute(BossController boss)
    {
        // 1. [공격중] 공격 중이라면, 애니메이션이 끝났는지 확인
        if (_isAttacking && _currentAttack != null)
        {
            _attackWaitTimer += Time.deltaTime;

            // 애니메이션이 끝났거나, 너무 오래(0.5초) 기다렸다면 공격 종료
            if (boss.CheckAnimationState(_currentAttack.AnimationName) || _attackWaitTimer >= MaxAttackDuration)
            {
                if (_attackWaitTimer >= MaxAttackDuration)
                {
                    Debug.LogWarning($"[CombatState] 애니메이션 타임아웃으로 공격 강제 종료: {_currentAttack.AnimationName}");
                }
                else
                {
                    Debug.Log($"[CombatState] 공격 종료: {_currentAttack.AnimationName}");
                }

                _isAttacking = false;
                _currentAttack = null;
                _attackWaitTimer = 0;
            }
            else
            {
                return;
            }
        }

        // 2. 쿨타임 체크 (공격 종료 후 쿨타임 동안 대기)
        _decisionTimer -= Time.deltaTime;
        if (_decisionTimer > 0) return;

        // 3. 상태 체크 (베리어가 없으면 그로기)
        if (ShouldTransitionToGroggy(boss))
        {
            boss.ChangeState(new GroggyState());
            return;
        }

        // 4. 거리 체크
        float dist = Vector2.Distance(boss.transform.position, boss.Target.position);
        if (dist > boss.AttackRange + 1.0f)
        {
            if (!boss.TargetFound)
            {
                Debug.Log($"[CombatState] 타겟이 인식 범위 밖(거리: {dist:F1}). IdleState로 전환.");
                boss.ChangeState(new IdleState());
            }
            else
            {
                Debug.Log($"[CombatState] 타겟이 사거리 밖임(거리: {dist:F1}). ChaseState로 전환.");
                boss.ChangeState(new ChaseState());
            }
            return;
        }

        // 5. 새로운 공격 전략 선택
        IAttackStrategy attack = SelectAttackStrategy(boss, dist);

        if (attack != null)
        {
            Debug.Log($"[CombatState] 새로운 공격 시작: {attack.GetType().Name}");
            _currentAttack = attack;
            _isAttacking = true;
            _attackWaitTimer = 0; // 타이머 초기화

            attack.ExecuteAttack(boss);
            _decisionTimer = attack.Cooldown;
        }
    }

    protected virtual bool ShouldTransitionToGroggy(BossController boss)
    {
        return !boss.Stats.IsBarrierActive;
    }

    protected virtual IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (dist > 8f) return new RangedPokeAttack();
        if (boss.CanUseHeavyAttack) return new HeavyAttack();
        return new LightAttack();
    }

    public virtual void Exit(BossController boss)
    {
        _isAttacking = false;
        _currentAttack = null;
    }
}
