using UnityEngine;

public class ChaseState : IBossState
{
    public void Enter(BossController boss)
    {
        // ������ ���� ���¿� �� �� �ʿ��� �ʱ�ȭ �۾� ����
    }
    public void Execute(BossController boss)
    {
        if (!boss.TargetFound)
        {
            boss.StopMove();
            boss.ChangeState(new IdleState());
            return;
        }

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
        // ������ ���� ���¿��� ���� �� �ʿ��� ���� �۾� ����
    }
}
