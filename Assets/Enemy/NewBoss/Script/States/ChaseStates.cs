using UnityEngine;

public class ChaseState : IBossState
{
    public void Enter(BossController boss)
    {
        Debug.Log("Entering Chase State");
        // 보스가 추적 상태에 들어갈 때 필요한 초기화 작업 수행
    }
    public void Execute(BossController boss)
    {
        float distnace = Vector2.Distance(boss.transform.position, boss.Target.transform.position);

        if (distnace <= boss.AttackRange)
        {
            boss.ChangeState(new CombatState());
        }
        else
        {
            boss.MoveTo(boss.Target.position);
        }
    }
    public void Exit(BossController boss)
    {
        Debug.Log("Exiting Chase State");
        // 보스가 추적 상태에서 나올 때 필요한 정리 작업 수행
    }
}
