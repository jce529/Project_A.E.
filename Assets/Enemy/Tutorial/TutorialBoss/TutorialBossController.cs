using System.Collections;
using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TutorialBossController
    //
    // [외형 구성]
    //   - 중앙 코어(Core) 1개 + 촉수(Tentacle) 6개
    //   - 피격 판정(Hitbox)은 코어에만 존재
    //   - 평소 코어는 높은 위치 → 플레이어 근접 공격 불가
    //
    // [그로기 시스템]
    //   - TentaclePierceStrategy 완료 시 → 그로기 확정 예약
    //   - 그로기 진입 시 코어가 바닥으로 서서히 이동 → 플레이어 타격 기회
    //   - 일정 시간 후 코어 복귀 → Idle 상태 반환
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialBossController : BossController
    {
        // ─── 코어(Core) 설정 ─────────────────────────────────────────────
        [Header("코어(Core) Transform 설정")]
        [Tooltip("중앙 코어 오브젝트의 Transform. 피격 콜라이더(Hitbox)가 이 오브젝트에 있어야 합니다.")]
        public Transform CoreTransform;

        [Tooltip("평상시 코어의 Y 위치 (플레이어 근접 공격이 닿지 않는 높은 위치)")]
        public float CoreNormalY = 8f;

        [Tooltip("그로기 시 코어가 내려오는 Y 위치 (플레이어가 타격 가능한 바닥 근처 높이)")]
        public float CoreGroggyY = 1.5f;

        [Tooltip("코어가 위/아래로 이동하는 데 걸리는 시간 (초). 작을수록 빠르게 낙하/상승합니다.")]
        public float CoreMoveDuration = 1.2f;

        // ─── 그로기(Groggy) 설정 ─────────────────────────────────────────
        [Header("그로기(Groggy) 설정")]
        [Tooltip("코어가 바닥에 머무르는 시간 (초). 이 시간 동안 플레이어가 코어를 공격할 수 있습니다.")]
        [Range(3f, 8f)]
        public float GroggyDuration = 5f;

        // ─── 공격 프리팹 ──────────────────────────────────────────────────
        [Header("공격에 사용할 프리팹")]
        [Tooltip("공격 전조(경고 표시)에 사용할 프리팹. SpriteRenderer가 있는 단순한 이미지 오브젝트 권장.")]
        public GameObject WarningPrefab;

        [Tooltip("촉수 휘두르기 시각 효과 프리팹. BoxCollider2D(IsTrigger)를 포함하면 충돌 감지에 활용됩니다.")]
        public GameObject SwipePrefab;

        // ─── 내부 상태 변수 ──────────────────────────────────────────────
        private HP _hp;

        // ─── 공개 프로퍼티 (State 클래스에서 접근) ───────────────────────

        /// <summary>
        /// 공격 패턴 완료 후 그로기 진입을 예약하는 플래그.
        /// TentaclePierceStrategy 종료 시 true로 설정 →
        /// TutorialAttackState.Execute()에서 감지하여 GroggyState로 전환.
        /// </summary>
        public bool PendingGroggy { get; set; } = false;

        /// <summary>현재 그로기 진행 중 여부. 중복 예약을 방지.</summary>
        public bool IsGroggy { get; private set; } = false;

        // ─── Unity 생명주기 ───────────────────────────────────────────────
        protected override void Awake()
        {
            _hp = GetComponent<HP>();
            base.Awake();

            // 보스는 물리 힘/중력의 영향을 받지 않아야 함 → Kinematic 설정
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Start()
        {
            if (_hp != null)
            {
                _hp.ManualDeath = true;     // HP 0이 되어도 자동 Destroy 방지
                _hp.OnDeath += HandleDeath; // 사망 이벤트 구독
            }

            // 초기 상태: 대기(Idle)
            ChangeState(new TutorialIdleState());
        }

        // ─── 그로기 플래그 세터 (GroggyState에서 사용) ───────────────────
        /// <summary>GroggyState의 Enter/Exit에서 IsGroggy 플래그를 설정합니다.</summary>
        public void SetGroggyFlag(bool value) => IsGroggy = value;

        // ─── 코어 이동 코루틴 (GroggyState에서 StartCoroutine으로 호출) ──
        /// <summary>
        /// CoreTransform을 targetY까지 duration 시간 동안 부드럽게 이동시킨다.
        /// SmoothStep을 사용해 시작/끝 지점에서 자연스럽게 가속·감속한다.
        /// </summary>
        /// <param name="targetY">이동할 목표 Y 값 (CoreGroggyY 또는 CoreNormalY)</param>
        /// <param name="duration">이동에 걸리는 시간 (초)</param>
        public IEnumerator MoveCoreToY(float targetY, float duration)
        {
            if (CoreTransform == null)
            {
                Debug.LogWarning("[TutorialBoss] CoreTransform이 Inspector에 연결되지 않았습니다!");
                yield break;
            }

            Vector3 startPos = CoreTransform.position;
            // X, Z 좌표는 고정 / Y 좌표만 목표로 변경
            Vector3 endPos  = new Vector3(startPos.x, targetY, startPos.z);
            float   elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // SmoothStep(0, 1, t): t=0에서 천천히 시작, 중간에 빠르게, t=1에서 천천히 감속
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                CoreTransform.position = Vector3.Lerp(startPos, endPos, t);

                yield return null;
            }

            // 부동소수점 오차 제거 - 정확한 목표 위치로 스냅
            CoreTransform.position = endPos;
        }

        // ─── 사망 처리 ────────────────────────────────────────────────────
        private void HandleDeath()
        {
            StopAllCoroutines();
            ChangeState(new TutorialDeadState());
        }

        // 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        private void OnDestroy()
        {
            if (_hp != null)
                _hp.OnDeath -= HandleDeath;
        }

        // ─── 에디터 Gizmo (개발 중 코어 위치 시각화) ────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (CoreTransform == null) return;
            float cx = CoreTransform.position.x;

            // 청록 구체: 평상시 코어가 위치하는 높이
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(cx, CoreNormalY, 0f), 0.6f);
            UnityEditor.Handles.Label(
                new Vector3(cx + 0.8f, CoreNormalY, 0f), "Normal Y (평상시)");

            // 노랑 구체: 그로기 시 코어가 내려오는 높이
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(cx, CoreGroggyY, 0f), 0.6f);
            UnityEditor.Handles.Label(
                new Vector3(cx + 0.8f, CoreGroggyY, 0f), "Groggy Y (공격 기회)");
        }
#endif
    }
}
