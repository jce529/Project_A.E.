using System.Collections;
using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TentacleSwipeStrategy - 촉수 휘두르기 공격
    //
    // [기반] FloorSweepStrategy 구조 참고
    //
    // [판정 특징]
    //   - 보스 하단(바닥) 영역만 가로로 훑는 낮은 히트박스
    //   - 플레이어가 _swipeHeight 이상 높이로 점프하면 피격되지 않음
    //   - 스윕 오브젝트가 오른쪽 끝 → 왼쪽 끝으로 Lerp 이동
    //
    // [회피 방법] 점프 (Y축으로 일정 높이 이상 올라가기)
    // ════════════════════════════════════════════════════════════════════════
    public class TentacleSwipeStrategy : IAttackStrategy
    {
        // ─── IAttackStrategy 구현 ─────────────────────────────────────────
        public float  Cooldown      => 4.0f;
        public string AnimationName => "Attack_TentacleSwipe";

        // ─── 공격 파라미터 ────────────────────────────────────────────────

        // 경고 표시 유지 시간 (초)
        private readonly float _telegraphTime = 1.0f;

        // 스윕 오브젝트가 오른쪽 → 왼쪽으로 이동하는 데 걸리는 시간 (초)
        // 값이 클수록 느리게 이동 → 플레이어가 피하기 쉬워짐
        private readonly float _sweepDuration = 2.0f;

        // 스윕 이동 범위 (보스 X 위치 기준 좌우 총 길이)
        private readonly float _sweepWidth = 18.0f;

        // ──────────────────────────────────────────────────────────────────
        // 핵심: 히트박스 높이 (swipeHeight)
        //   - 스윕 오브젝트의 BoxCollider2D / OverlapBox의 세로 크기
        //   - 플레이어의 Y 위치가 (바닥Y + _swipeHeight) 이상이면 판정 범위 밖
        //   - 즉, 이 높이만큼 점프하면 자연스럽게 회피됨
        // ──────────────────────────────────────────────────────────────────
        private readonly float _swipeHeight = 2.0f;

        // 경고 박스의 시각적 두께 (실제 판정과 무관, 연출용)
        private readonly float _warningHeight = 0.4f;

        // 데미지
        private readonly float _damage = 20f;

        // 프리팹 참조 (TutorialAttackState에서 생성자로 주입)
        private readonly GameObject _warningPrefab;
        private readonly GameObject _swipePrefab;

        // ─── 생성자: 필요한 프리팹을 외부에서 주입받음 ───────────────────
        public TentacleSwipeStrategy(GameObject warningPrefab, GameObject swipePrefab)
        {
            _warningPrefab = warningPrefab;
            _swipePrefab   = swipePrefab;
        }

        // ─── IAttackStrategy.ExecuteAttack ────────────────────────────────
        public void ExecuteAttack(BossController boss)
        {
            boss.StartCoroutine(AttackRoutine(boss));
        }

        // ─── 공격 시퀀스 코루틴 ──────────────────────────────────────────
        private IEnumerator AttackRoutine(BossController boss)
        {
            boss.StopMove();

            // ── 바닥 Y 위치 감지 ────────────────────────────────────────
            // 플레이어 발밑에서 아래로 Raycast를 쏘아 실제 바닥 표면 Y를 구함
            // → 맵의 실제 지형 높이에 맞게 히트박스 위치를 자동 보정
            float floorY;
            RaycastHit2D groundHit = Physics2D.Raycast(
                boss.Target.position,
                Vector2.down,
                10f,
                LayerMask.GetMask("Ground")
            );

            if (groundHit.collider != null)
            {
                // 바닥 표면 Y + 히트박스 높이의 절반 = 히트박스 중심이 바닥에 딱 붙는 Y
                floorY = groundHit.point.y + _swipeHeight * 0.5f;
            }
            else
            {
                // Ground 레이어가 설정되지 않은 경우 근사값으로 대체
                Debug.LogWarning("[TentacleSwipe] 바닥 감지 실패 (Inspector에서 Ground 레이어 확인)");
                floorY = boss.Target.position.y - 0.5f + _swipeHeight * 0.5f;
            }

            // 스윕 이동 시작점(우측)과 끝점(좌측) 계산
            // 보스 X 위치를 중심으로 좌우로 sweepWidth/2 씩 뻗음
            float   bossCenterX = boss.transform.position.x;
            Vector2 sweepStart  = new Vector2(bossCenterX + _sweepWidth * 0.5f, floorY);
            Vector2 sweepEnd    = new Vector2(bossCenterX - _sweepWidth * 0.5f, floorY);

            // ── [Phase 1] 경고 표시 ─────────────────────────────────────
            Debug.Log($"[TentacleSwipe] 경고 표시 ({_telegraphTime}s) / 바닥Y={floorY:F2}");
            GameObject warningObj = null;

            if (_warningPrefab != null)
            {
                // 경고 오브젝트를 바닥 전체 범위에 맞춰 생성 및 크기 설정
                warningObj = Object.Instantiate(
                    _warningPrefab,
                    new Vector2(bossCenterX, floorY - _swipeHeight * 0.5f + _warningHeight * 0.5f),
                    Quaternion.identity
                );
                warningObj.transform.localScale = new Vector3(_sweepWidth, _warningHeight, 1f);
            }

            yield return new WaitForSeconds(_telegraphTime);

            if (warningObj != null)
                Object.Destroy(warningObj);

            // ── [Phase 2] 스윕 오브젝트 생성 (오른쪽 끝에서 등장) ───────
            Debug.Log("[TentacleSwipe] >> 바닥 휘두르기 시작!");
            GameObject swipeObj = null;

            if (_swipePrefab != null)
            {
                swipeObj = Object.Instantiate(_swipePrefab, sweepStart, Quaternion.identity);

                // 프리팹에 BoxCollider2D가 있으면 판정 크기를 swipeHeight 기준으로 설정
                // 이 높이가 핵심: 이보다 위에 있으면 피격되지 않음
                var box = swipeObj.GetComponent<BoxCollider2D>();
                if (box != null)
                    box.size = new Vector2(1.5f, _swipeHeight);
            }

            // ── [Phase 3] Lerp로 오른쪽 → 왼쪽으로 이동 ────────────────
            float elapsed   = 0f;
            bool  playerHit = false; // 중복 데미지 방지 플래그

            while (elapsed < _sweepDuration)
            {
                elapsed += Time.deltaTime;

                // t: 0 → 1, Lerp로 선형 이동
                float t = elapsed / _sweepDuration;

                if (swipeObj != null)
                    swipeObj.transform.position = Vector2.Lerp(sweepStart, sweepEnd, t);

                // ── 매 프레임 히트 체크 (단 한 번만 데미지 적용) ─────────
                // playerHit이 false인 동안만 체크 → 이미 피격됐으면 건너뜀
                if (!playerHit && swipeObj != null)
                    playerHit = CheckHit(swipeObj, boss);

                yield return null;
            }

            if (swipeObj != null)
                Object.Destroy(swipeObj);

            Debug.Log("[TentacleSwipe] 스윕 종료");
        }

        // ─── 피격 체크 ────────────────────────────────────────────────────
        /// <summary>
        /// 스윕 오브젝트 위치의 BoxCollider2D 또는 OverlapBox로 플레이어를 감지.
        /// 히트박스 높이(_swipeHeight)가 낮으므로 점프한 플레이어는 범위 밖에 위치하여 피격되지 않음.
        /// </summary>
        private bool CheckHit(GameObject swipeObj, BossController boss)
        {
            Collider2D col = swipeObj.GetComponent<Collider2D>();

            if (col != null)
            {
                // 프리팹에 콜라이더가 있으면 Overlap으로 감지
                var filter  = new ContactFilter2D { useTriggers = true };
                var results = new Collider2D[8];
                int count   = col.Overlap(filter, results);

                for (int i = 0; i < count; i++)
                {
                    if (results[i] != null && results[i].CompareTag("Player"))
                    {
                        ApplyDamage(results[i]);
                        return true;
                    }
                }
            }
            else
            {
                // 콜라이더가 없으면 직접 OverlapBox로 판정
                // 크기는 swipeHeight 기준 → 낮은 박스이므로 점프로 회피 가능
                Collider2D[] hits = Physics2D.OverlapBoxAll(
                    swipeObj.transform.position,
                    new Vector2(1.5f, _swipeHeight),
                    0f
                );

                foreach (var h in hits)
                {
                    if (h.CompareTag("Player"))
                    {
                        ApplyDamage(h);
                        return true;
                    }
                }
            }

            return false;
        }

        private void ApplyDamage(Collider2D target)
        {
            var hp = target.GetComponentInParent<HP>();
            if (hp != null)
            {
                Debug.Log($"[TentacleSwipe] >> Player Hit! Damage: {_damage}");
                hp.TakeDamage(_damage);
            }
        }

        // ─── 에디터 Gizmo (Attack 범위 시각화용) ─────────────────────────
#if UNITY_EDITOR
        /// <summary>
        /// BossController.OnDrawGizmosSelected()에서 호출하면 스윕 범위를 시각화합니다.
        /// 주황색 영역: 타격 범위 / 가로로 넓고 세로로 낮은 것이 특징
        /// </summary>
        public void DrawGizmos(Vector3 bossPos)
        {
            float yCenter = bossPos.y - 2f; // 대략적인 바닥 위치 (실제 바닥은 Raycast로 동적 계산)

            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.15f);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(
                new Rect(
                    bossPos.x - _sweepWidth * 0.5f,
                    yCenter    - _swipeHeight * 0.5f,
                    _sweepWidth,
                    _swipeHeight
                ),
                new Color(1f, 0.5f, 0f, 0.15f),
                new Color(1f, 0.5f, 0f, 0.8f)
            );
        }
#endif
    }
}
