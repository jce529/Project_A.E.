using System.Collections;
using UnityEngine;

namespace TutorialBoss
{
    // 플레이어 위치에 스파이크를 소환하는 독립 공격 패턴.
    // TutorialAttackState의 별도 타이머로 5초마다 호출됨.
    public class TutorialRootSpikeStrategy
    {
        private readonly float _warningTime = 0.6f;
        private readonly GameObject _warningPrefab;
        private readonly GameObject _rootSpikePrefab;

        public TutorialRootSpikeStrategy(GameObject warningPrefab, GameObject rootSpikePrefab)
        {
            _warningPrefab   = warningPrefab;
            _rootSpikePrefab = rootSpikePrefab;
        }

        public void Execute(BossController boss)
        {
            if (_rootSpikePrefab == null)
            {
                Debug.LogWarning("[TutorialRootSpike] RootSpikePrefab이 없습니다!");
                return;
            }
            boss.StartCoroutine(AttackRoutine(boss));
        }

        private IEnumerator AttackRoutine(BossController boss)
        {
            if (boss.Target == null) yield break;

            // 플레이어 직하방 Raycast로 가장 가까운 바닥 위치를 구함
            Vector2 playerPos = boss.Target.position;
            Vector2 spawnPos  = playerPos;

            RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 30f, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
                spawnPos = new Vector2(playerPos.x, hit.point.y);
            else
                Debug.LogWarning("[TutorialRootSpike] 바닥 감지 실패 - Ground 레이어 확인");

            GameObject warning = null;
            if (_warningPrefab != null)
            {
                warning = Object.Instantiate(_warningPrefab, spawnPos, Quaternion.identity);
                warning.transform.localScale = new Vector3(1.5f, 2.0f, 1f);
            }

            yield return new WaitForSeconds(_warningTime);

            if (warning != null)
                Object.Destroy(warning);

            Object.Instantiate(_rootSpikePrefab, spawnPos, Quaternion.identity);
        }
    }
}
