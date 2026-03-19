using UnityEngine;

public class CounterState : IBossState
{
    public void Enter(BossController boss)
    {
        //애니메이션 : 반격 모션
        //물 소모 (기획 : 10%)
        boss.Stats.ConsumeWater(boss.Stats.MaxWater * 0.1f);
        //즉발성 회피 불가 공격 로직 수행
    }

    public void Execute(BossController boss)
    {
        //반격 모션이 끝나면 전투 상태로 복귀
        if (boss.CheckAnimationState("CounterAttack"))
        {
            boss.ChangeState(new CombatState());
        }
    }

    public void Exit(BossController boss)
    {
        //반격 상태에서 나올 때 필요한 정리 작업 수행
    }
}
