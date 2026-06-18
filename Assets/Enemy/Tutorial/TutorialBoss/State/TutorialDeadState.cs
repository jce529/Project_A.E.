using System.Collections;
using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TutorialDeadState - 사망 상태
    //
    // 콜라이더 비활성화 후 사망 연출(대기) → 클리어 UI 표시
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialDeadState : IBossState
    {
        public void Enter(BossController boss)
        {
            Debug.Log("[TutorialBoss] ──→ Dead 상태! 보스 사망");
            boss.StopMove();
            boss.Anim?.SetTrigger("GroggyTrigger");

            // 코어 콜라이더 비활성화 (더 이상 피격되지 않도록)
            var col = boss.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            boss.StartCoroutine(DeathSequence(boss));
        }

        public void Execute(BossController boss) { }
        public void Exit(BossController boss)    { }

        private IEnumerator DeathSequence(BossController boss)
        {
            // 사망 애니메이션 재생 구간 (필요 시 주석 해제)
            // boss.Anim?.SetTrigger("Die");
            yield return new WaitForSeconds(2.5f);

            // 벽 열기
            var tutorialBoss = boss as TutorialBossController;
            if (tutorialBoss != null && tutorialBoss.WallToUnlock != null)
                tutorialBoss.WallToUnlock.UnlockWall();

            // 클리어 UI 표시
            var tb = boss as TutorialBossController;
            if (tb != null && tb.ClearPanel != null)
            {
                tb.ClearPanel.SetActive(true);
                GameStateManager.Instance?.SetState(GameStateManager.GameState.Paused);
            }
        }
    }
}
