using System.Collections;
using UnityEngine;

namespace WoodBoss
{
    public class FloorSweepStrategy : IAttackStrategy
    {
        public float Cooldown => 7.0f;
        public string AnimationName => "Attack_FloorSweep";

        // 경고 표시 유지 시간
        private float _telegraphTime = 1.2f;

        // 쓸고 지나가는 총 시간 (Lerp 지속 시간 - 값이 클수록 느려짐)
        private float _sweepDuration = 1.5f;

        // 바닥 너비 (경고 박스 가로 크기 & 이동 거리로 사용)
        private float _floorWidth = 20.0f;

        // 경고 박스 세로 두께
        private float _warningHeight = 1.0f;

        // 데미지
        private float _damage = 25.0f;

        public void ExecuteAttack(BossController boss)
        {
            boss.StartCoroutine(AttackRoutine(boss));
        }

        private IEnumerator AttackRoutine(BossController boss)
        {
            var woodBoss = boss as WoodBossController;
            if (woodBoss == null || woodBoss.SweepPrefab == null)
            {
                Debug.LogWarning("[FloorSweep] SweepPrefab이 없습니다!");
                yield break;
            }

            boss.StopMove();

            // 플레이어 발밑에서 아래로 Raycast → 실제 바닥 Y 감지
            // Ground 레이어가 설정되어 있어야 합니다
            float floorY;
            RaycastHit2D groundHit = Physics2D.Raycast(
                boss.Target.position,
                Vector2.down,
                5f,
                LayerMask.GetMask("Ground")
            );

            if (groundHit.collider != null)
            {
                // 바닥 표면 Y + 프리팹 높이 절반 → 바닥에 딱 붙음
                floorY = groundHit.point.y + _warningHeight / 2f;
            }
            else
            {
                // Raycast 실패 시 (Ground 레이어 미설정 등) 플레이어 발밑 근사값으로 대체
                Debug.LogWarning("[FloorSweep] 바닥 감지 실패 - Ground 레이어 확인 필요");
                floorY = boss.Target.position.y - 0.5f + _warningHeight / 2f;
            }

            // 바닥 중앙 X는 보스 위치 기준
            float floorCenterX = boss.transform.position.x;

            // Lerp 시작점(오른쪽 끝) / 종료점(왼쪽 끝)
            Vector2 sweepStart = new Vector2(floorCenterX + _floorWidth / 2f, floorY);
            Vector2 sweepEnd = new Vector2(floorCenterX - _floorWidth / 2f, floorY);

            // ── [Phase 1] 경고 박스 표시 ──────────────────────────────
            Debug.Log($"[FloorSweep] 경고 표시 중... ({_telegraphTime}s)");

            GameObject warningBox = null;
            if (woodBoss.WarningPrefab != null)
            {
                // 바닥 중앙에 경고 박스 생성 후 바닥 전체 너비로 크기 설정
                warningBox = Object.Instantiate(
                    woodBoss.WarningPrefab,
                    new Vector2(floorCenterX, floorY),
                    Quaternion.identity
                );
                warningBox.transform.localScale = new Vector3(_floorWidth, _warningHeight, 1f);
            }

            yield return new WaitForSeconds(_telegraphTime);

            // 경고 박스 제거
            if (warningBox != null)
                Object.Destroy(warningBox);

            // ── [Phase 2] 스윕 프리팹 생성 (오른쪽 끝에서 등장) ────────
            Debug.Log("[FloorSweep] >> 바닥 쓸어내기 시작!");

            GameObject sweepObj = Object.Instantiate(
                woodBoss.SweepPrefab,
                sweepStart,
                Quaternion.identity
            );

            // ── [Phase 3] Lerp로 왼쪽까지 이동 ─────────────────────────
            float elapsed = 0f;
            bool playerHit = false; // 한 번만 데미지 적용하려면 이 플래그 사용

            while (elapsed < _sweepDuration)
            {
                elapsed += Time.deltaTime;

                // t: 0 → 1 (처음엔 빠르게, 끝에 가까워질수록 느려짐)
                float t = elapsed / _sweepDuration;

                sweepObj.transform.position = Vector2.Lerp(sweepStart, sweepEnd, t);

                // 히트 체크 (매 프레임)
                if (!playerHit)
                    playerHit = CheckHit(sweepObj, boss);

                yield return null;
            }

            // 화면 밖으로 완전히 나가면 제거
            Object.Destroy(sweepObj);
            Debug.Log("[FloorSweep] 스윕 종료");
        }

        private bool CheckHit(GameObject sweepObj, BossController boss)
        {
            // 스윕 오브젝트의 Collider2D 기반으로 플레이어 감지
            var col = sweepObj.GetComponent<Collider2D>();
            if (col == null) return false;

            var filter = new ContactFilter2D();
            filter.useTriggers = true;

            var results = new Collider2D[5];
            int count = col.Overlap(filter, results);

            for (int i = 0; i < count; i++)
            {
                if (results[i].CompareTag("Player"))
                {
                    Debug.Log($"[FloorSweep] >> Player Hit! Damage: {_damage}");
                    // IDamageable 등 데미지 인터페이스가 있다면 여기서 호출
                    // results[i].GetComponent<IDamageable>()?.TakeDamage(_damage);
                    return true;
                }
            }
            return false;
        }
    }
}