using UnityEngine;


namespace WoodBoss 
{ 
    public class IdleState : IBossState
    {
        public void Enter(BossController boss) { boss.StopMove(); }

        public void Execute(BossController boss)
        {
            if (boss.TargetFound)
            {
                Debug.Log("[Boss] 플레이어 발견! 추적 시작.");

                // [변경] 바로 공격(AttackState)하지 않고, 추적(ChaseState)으로 전환
                boss.ChangeState(new WoodBossAttackState());
            }
        }

        public void Exit(BossController boss) { }
    }
}
