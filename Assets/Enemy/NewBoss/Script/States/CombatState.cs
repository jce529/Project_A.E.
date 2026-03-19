using UnityEngine;

public class CombatState : IBossState
{
    private float _decisionTimer;

    // 현재 실행 중인 공격 전략을 기억하기 위한 변수
    private IAttackStrategy _currentAttack;
    private bool _isAttacking; // 공격 중인지 여부

    public void Enter(BossController boss)
    {
        _decisionTimer = 0;
        _isAttacking = false;
        _currentAttack = null;
    }

    public void Execute(BossController boss)
    {
        // 1. [수정됨] 공격 중이라면, 애니메이션이 끝났는지 확인
        if (_isAttacking && _currentAttack != null)
        {
            // BossController에 있는 'CheckAnimationState' 함수 활용
            if (boss.CheckAnimationState(_currentAttack.AnimationName))
            {
                // 애니메이션이 끝났으면 공격 종료 처리
                _isAttacking = false;
                _currentAttack = null;
            }
            else
            {
                // 아직 애니메이션 재생 중이므로 대기 (아무것도 안 함)
                return;
            }
        }

        // 2. 쿨타임 체크
        _decisionTimer -= Time.deltaTime;
        if (_decisionTimer > 0) return;

        // 3. 기믹 체크 (베리어 꺼지면 그로기)
        if (!boss.Stats.IsBarrierActive)
        {
            boss.ChangeState(new GroggyState());
            return;
        }

        // 4. 거리 체크
        float dist = Vector2.Distance(boss.transform.position, boss.Target.position);
        if (dist > boss.AttackRange + 1.0f)
        {
            boss.ChangeState(new ChaseState());
            return;
        }

        // 5. 새로운 공격 선택 및 실행
        IAttackStrategy attack = SelectAttackStrategy(boss, dist);

        if (attack != null)
        {
            _currentAttack = attack; // 현재 공격 기억
            _isAttacking = true;     // 공격 시작 플래그

            attack.ExecuteAttack(boss);

            // 다음 행동까지의 대기 시간 (후딜레이)
            _decisionTimer = attack.Cooldown;
        }
    }

    private IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (dist > 8f) return new RangedPokeAttack();
        if (boss.CanUseHeavyAttack) return new HeavyAttack();
        return new LightAttack();
    }

    public void Exit(BossController boss)
    {
        _isAttacking = false;
        _currentAttack = null;
    }
}