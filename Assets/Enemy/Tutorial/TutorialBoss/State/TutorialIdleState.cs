using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TutorialIdleState - 대기 상태
    //
    // 플레이어가 VisionRange 내에 들어오면 Attack 상태로 전환.
    // 보스는 이 상태에서 이동하지 않으며 공격하지 않는다.
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialIdleState : IBossState
    {
        public void Enter(BossController boss)
        {
            boss.StopMove();
            boss.Anim?.SetTrigger("IdleTrigger");
            Debug.Log("[TutorialBoss] ──→ Idle 상태");
        }

        public void Execute(BossController boss)
        {
            // 플레이어가 시야 범위(VisionRange) 내에 들어오면 공격 상태로 전환
            if (boss.TargetFound)
            {
                boss.ChangeState(new TutorialAttackState());
            }
        }

        public void Exit(BossController boss) { }
    }
}
