using System.Collections;
using UnityEngine;

namespace WoodBoss
{
    public class RootSpikeStrategy : IAttackStrategy
    {
        public float Cooldown => 5.0f; // 5초 쿨타임
        public string AnimationName => "Attack_RootSpike";

        // 공격 전조(캐스팅) 시간
        private float _castTime = 0.5f;

        // [추가] 뿌리 공격의 경고 범위 크기 (가시의 크기에 맞춰 조절하세요)
        private Vector2 _warningSize = new Vector2(1.5f, 2.0f);

        public void ExecuteAttack(BossController boss)
        {
            boss.StartCoroutine(AttackRoutine(boss));
        }

        private IEnumerator AttackRoutine(BossController boss)
        {
            var woodBoss = boss as WoodBossController;
            if (woodBoss == null || woodBoss.RootSpikePrefab == null)
            {
                Debug.LogWarning("RootSpikePrefab is missing!");
                yield break;
            }

            // [Phase 1] Casting Animation 및 경고
            boss.StopMove();
            Debug.Log($"[Pattern 2] Casting Root Spike... ({_castTime}s)");

            // 플레이어의 현재 위치를 가져와서 고정 (이 위치에 가시가 솟아남)
            Vector2 targetPos = boss.Target.position;

            // [추가] 경고 박스 생성 로직
            GameObject warningBox = null;
            if (woodBoss.WarningPrefab != null)
            {
                // 타겟의 위치에 경고 박스 생성
                warningBox = Object.Instantiate(woodBoss.WarningPrefab, targetPos, Quaternion.identity);

                // 경고 박스 크기를 설정한 범위(_warningSize)로 늘려줌
                warningBox.transform.localScale = new Vector3(_warningSize.x, _warningSize.y, 1f);
            }

            // 캐스팅 시간(전조 시간) 동안 대기하며 플레이어에게 피할 기회를 줌
            yield return new WaitForSeconds(_castTime);

            // [추가] 실제 공격이 나오기 직전에 경고 박스 삭제
            if (warningBox != null)
            {
                Object.Destroy(warningBox);
            }

            // [Phase 2] Spawn Spike
            Debug.Log("[Pattern 2] >> Spike Erupts!");

            Vector2 spawnLocation = targetPos;

            // 실제 뿌리 가시 프리팹을 해당 위치에 생성
            Object.Instantiate(woodBoss.RootSpikePrefab, spawnLocation, Quaternion.identity);

            // 공격 후 후딜레이
            yield return new WaitForSeconds(0.5f);
        }
    }
}