using System.Collections;
using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TentaclePierceStrategy - 촉수 찌르기 공격 (거대 AoE)
    //
    // [기반] VineSwingStrategy 구조 참고
    //
    // [판정 특징]
    //   - 보스 중심으로 상·중·하단 대부분의 공간을 동시에 타격하는 거대 박스 판정
    //   - X축을 _hitWidth로 제한 → 좌우 가장자리(Far Left/Right)는 안전지대
    //   - Y축을 _hitHeight로 지정 → 대부분의 층 높이를 커버
    //
    // [회피 방법] 맵의 좌우 끝(Far Edge)으로 이동
    //
    // [핵심 역할]
    //   - 이 공격이 완료되면 PendingGroggy = true 설정
    //   - TutorialAttackState에서 이를 감지하여 GroggyState로 전환
    //   즉, TentaclePierce는 그로기를 유발하는 '확정 그로기 트리거 패턴'
    // ════════════════════════════════════════════════════════════════════════
    public class TentaclePierceStrategy : IAttackStrategy
    {
        // ─── IAttackStrategy 구현 ─────────────────────────────────────────
        public float  Cooldown      => 8.0f;
        public string AnimationName => "Attack_TentaclePierce";

        // ─── 공격 파라미터 ────────────────────────────────────────────────

        // 전조 표시 유지 시간 (초) - 플레이어가 좌우 끝으로 이동할 준비 시간
        private readonly float _telegraphTime = 0.8f;

        // ──────────────────────────────────────────────────────────────────
        // 핵심: AoE 히트박스 가로 크기 (_hitWidth)
        //   - 보스 중심에서 ±(_hitWidth/2) 범위 내의 플레이어를 타격
        //   - 이 범위 바깥(맵의 Far Left/Right)은 공격이 닿지 않는 안전지대
        //   - 맵 전체 가로 크기보다 작게 설정하여 좌우 끝에 안전 구역 확보
        //   예) 맵 가로 = 30유닛, _hitWidth = 14 → 각 끝에 약 8유닛의 안전지대
        // ──────────────────────────────────────────────────────────────────
        private readonly float _hitWidth = 14.0f;

        // AoE 히트박스 세로 크기 (_hitHeight)
        //   - 보스 중심에서 ±(_hitHeight/2) 높이 범위를 타격
        //   - 맵의 대부분 층 높이를 커버할 수 있도록 충분히 크게 설정
        private readonly float _hitHeight = 12.0f;

        // 타격 후 후딜레이 (초) - 이 시간 이후 PendingGroggy가 설정됨
        private readonly float _afterAttackDelay = 1.4f;

        // 데미지
        private readonly float _damage = 30f;

        // 경고 프리팹 참조 (생성자 주입)
        private readonly GameObject _warningPrefab;

        // ─── 생성자 ───────────────────────────────────────────────────────
        public TentaclePierceStrategy(GameObject warningPrefab)
        {
            _warningPrefab = warningPrefab;
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

            // AoE 판정의 중심점: 보스 Transform 위치 사용
            // _hitHeight가 충분히 크므로 보스 위치 기준으로도 상·중·하단을 커버함
            Vector2 attackCenter = (Vector2)boss.transform.position;

            // ── [Phase 1] 전조 경고 표시 ─────────────────────────────────
            // 플레이어에게 대피(좌우 가장자리로 이동)할 시간을 주는 경고 연출
            Debug.Log($"[TentaclePierce] 전조 표시 ({_telegraphTime}s) - 좌우 끝으로 피하세요!");
            GameObject warningObj = null;

            if (_warningPrefab != null)
            {
                // 타격 범위와 동일한 크기로 경고 박스 생성
                // → 플레이어가 어디까지 피해야 하는지 직관적으로 표시
                warningObj = Object.Instantiate(
                    _warningPrefab,
                    attackCenter,
                    Quaternion.identity
                );
                warningObj.transform.localScale = new Vector3(_hitWidth, _hitHeight, 1f);
            }

            yield return new WaitForSeconds(_telegraphTime);

            if (warningObj != null)
                Object.Destroy(warningObj);

            // ── [Phase 2] 즉발 AoE 타격 ─────────────────────────────────
            Debug.Log($"[TentaclePierce] >> 거대 범위 찌르기! (W:{_hitWidth} H:{_hitHeight})");

            // OverlapBoxAll: 지정한 박스 범위 내 모든 Collider2D를 반환
            // ──────────────────────────────────────────────────────────────
            //   attackCenter : 판정 중심 (보스 위치)
            //   _hitWidth    : 박스 가로 크기 → 이 값보다 멀리 있으면 안전
            //   _hitHeight   : 박스 세로 크기 → 상·중·하 대부분 커버
            //   0f           : 회전각 없음 (축 정렬 박스)
            // ──────────────────────────────────────────────────────────────
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackCenter,
                new Vector2(_hitWidth, _hitHeight),
                0f
            );

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var hp = hit.GetComponent<HP>();
                    if (hp != null)
                    {
                        Debug.Log($"[TentaclePierce] >> Player Hit! Damage: {_damage}");
                        hp.TakeDamage(_damage);
                    }
                }
            }

            // ── [Phase 3] 후딜레이 ───────────────────────────────────────
            // 공격 연출(애니메이션)이 완전히 끝날 때까지 대기
            yield return new WaitForSeconds(_afterAttackDelay);

            // ── [Phase 4] 그로기 예약 ────────────────────────────────────
            // 이 패턴이 완료되면 반드시 그로기 상태로 진입하도록 플래그를 설정
            // TutorialAttackState.Execute()가 다음 틱에 PendingGroggy를 감지하여
            // GroggyState로 상태 전환을 수행함 (직접 ChangeState 하지 않고 위임)
            var tb = boss as TutorialBossController;
            if (tb != null)
            {
                tb.PendingGroggy = true;
                Debug.Log("[TentaclePierce] 그로기 예약 완료! (다음 프레임에 GroggyState 진입)");
            }
        }

        // ─── 에디터 Gizmo (AoE 범위 시각화용) ───────────────────────────
#if UNITY_EDITOR
        /// <summary>
        /// 타격 범위(빨간색)와 좌우 안전지대(초록색)를 에디터에서 시각화합니다.
        /// 안전지대 너비는 실제 맵 크기에 맞게 safeZoneWidth를 조절하세요.
        /// </summary>
        public void DrawGizmos(Vector3 bossPos)
        {
            float halfW = _hitWidth  * 0.5f;
            float halfH = _hitHeight * 0.5f;

            // ── 타격 범위 (빨간색) ─────────────────────────────────────
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.15f);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(
                new Rect(bossPos.x - halfW, bossPos.y - halfH, _hitWidth, _hitHeight),
                new Color(1f, 0f, 0f, 0.12f),
                new Color(1f, 0f, 0f, 0.9f)
            );

            // ── 좌우 안전지대 (초록색) ────────────────────────────────
            // 맵 가장자리의 안전 구역 시각화 (safeZoneWidth는 레벨 디자인에 맞게 조정)
            float safeZoneWidth = 6f;
            UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.2f);

            // 왼쪽 안전지대
            UnityEditor.Handles.DrawSolidRectangleWithOutline(
                new Rect(bossPos.x - halfW - safeZoneWidth, bossPos.y - halfH, safeZoneWidth, _hitHeight),
                new Color(0f, 1f, 0f, 0.15f),
                Color.green
            );

            // 오른쪽 안전지대
            UnityEditor.Handles.DrawSolidRectangleWithOutline(
                new Rect(bossPos.x + halfW, bossPos.y - halfH, safeZoneWidth, _hitHeight),
                new Color(0f, 1f, 0f, 0.15f),
                Color.green
            );

            // 안전지대 레이블
            UnityEditor.Handles.Label(
                new Vector3(bossPos.x - halfW - safeZoneWidth * 0.5f, bossPos.y + halfH + 0.3f),
                "안전지대");
            UnityEditor.Handles.Label(
                new Vector3(bossPos.x + halfW + safeZoneWidth * 0.5f - 1.5f, bossPos.y + halfH + 0.3f),
                "안전지대");
        }
#endif
    }
}
