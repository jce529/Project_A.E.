using System.Collections;
using UnityEngine;

namespace TutorialBoss
{
    // 판단 로직(SelectPattern)이 다음에 실행할 패턴을 지칭하는 데 사용하는 식별자.
    // §7 확장 훅: 향후 '휘두르기'(매우 근접) 패턴이 추가되면 여기에 값만 추가.
    public enum PatternType
    {
        TentacleStab,   // 촉수 찌르기 (근접)
        GroundTentacle, // 바닥 촉수 (원거리/저지)
    }

    // ════════════════════════════════════════════════════════════════════════
    // TutorialBossController
    //
    // [외형 구성]
    //   - 중앙 코어(Core) 1개 + 촉수(Tentacle) 6개
    //   - 피격 판정(Hitbox)은 코어에만 존재
    //   - 평소 코어는 높은 위치 → 플레이어 근접 공격 불가
    //
    // [그로기 시스템]
    //   - HP가 70%/30% 임계치를 처음 통과하면, 실행 중이던 패턴이 끝난 뒤 그로기 예약
    //   - 그로기 진입 시 코어가 바닥으로 서서히 이동 → 플레이어 타격 기회
    //   - 일정 시간 후 코어 복귀 → Idle 상태 반환
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialBossController : BossController
    {
        // ─── 코어(Core) 설정 ─────────────────────────────────────────────
        [Header("코어(Core) Transform 설정")]
        [Tooltip("중앙 코어 오브젝트의 Transform. 피격 콜라이더(Hitbox)가 이 오브젝트에 있어야 합니다.")]
        public Transform CoreTransform;

        [Tooltip("평상시 코어 Y 오프셋 (초기 코어 위치 기준. 양수=위쪽, 음수=아래쪽)")]
        public float CoreNormalY = 3f;

        [Tooltip("그로기 시 코어 Y 오프셋 (초기 코어 위치 기준. 음수=아래쪽)")]
        public float CoreGroggyY = -3.5f;

        [Tooltip("코어가 위/아래로 이동하는 데 걸리는 시간 (초). 작을수록 빠르게 낙하/상승합니다.")]
        public float CoreMoveDuration = 1.2f;

        // ─── 그로기(Groggy) 설정 ─────────────────────────────────────────
        [Header("그로기(Groggy) 설정")]
        [Tooltip("코어가 바닥에 머무르는 시간 (초). 이 시간 동안 플레이어가 코어를 공격할 수 있습니다.")]
        [Range(3f, 8f)]
        public float GroggyDuration = 5f;

        [Tooltip("그로기 종료 후 일어나는 애니메이션 재생 시간 (초). GroggyExit 클립 길이에 맞춰 설정하세요.")]
        public float GroggyExitDuration = 1f;

        // ─── 벽 오픈 연동 ─────────────────────────────────────────────────
        [Header("사망 시 열릴 벽")]
        [Tooltip("보스 사망 시 UnlockWall()을 호출할 InteractableWall 오브젝트")]
        public InteractableWall WallToUnlock;

        [Header("클리어 UI")]
        public GameObject ClearPanel;

        // ─── 공격 프리팹 ──────────────────────────────────────────────────
        // ─── 패턴 판단 - 거리/시간 기준 (프로그래머 자유 조정 가능) ──────
        [Header("패턴 판단 - 거리/시간 기준")]
        [Tooltip("플레이어 가로 길이 기준값. 근접/원거리 판정의 기준 단위로 쓰입니다 (플레이어 콜라이더 실측값으로 조정하세요).")]
        public float PlayerWidth = 1f;

        [Tooltip("촉수 찌르기 사용 거리 = PlayerWidth × 이 배수 이내")]
        public float TentacleStabRangeMultiplier = 3f;

        [Tooltip("바닥 촉수 사용 거리 = PlayerWidth × 이 배수 이상 (또는 아래 정지 시간 조건)")]
        public float GroundTentacleRangeMultiplier = 3f;

        [Tooltip("플레이어가 이 시간(초) 이상 같은 자리에 머무르면 바닥 촉수 후보 조건 충족")]
        public float StationaryTimeThreshold = 2f;

        [Tooltip("플레이어 위치 변화가 이 값(월드 유닛) 이하이면 '정지'로 간주")]
        public float StationaryPositionTolerance = 0.1f;

        [Tooltip("공격 종료 후 다음 패턴 탐색을 시작하기까지 Idle로 대기하는 시간(초)")]
        public float PostAttackIdleTime = 2f;

        [Header("패턴 쿨타임 (프로그래머 자유 조정 가능)")]
        [Tooltip("촉수 찌르기 재사용 대기시간(초)")]
        public float TentacleStabCooldown = 8f;

        [Tooltip("바닥 촉수 재사용 대기시간(초)")]
        public float GroundTentacleCooldown = 5f;

        [Tooltip("바닥 촉수 패턴이 Attack 상태를 점유하는 시간(초). 전조(0.6s) + 스파이크 연출 여유를 포함해 조정하세요.")]
        public float GroundTentacleActiveDuration = 1.5f;

        [Tooltip("촉수 찌르기 패턴이 Attack 상태를 점유하는 시간(초). 전조(0.8s)+즉발+후딜(1.4s) 기준.")]
        public float TentacleStabActiveDuration = 2.8f;

        // ─── 그로기 HP 임계치 (구조 변경 시 기획 협의 필요) ─────────────
        [Header("그로기 HP 임계치 (%) - 값 변경은 자유, 단계/구조 변경은 기획 협의 필요")]
        [Range(0f, 1f)] public float GroggyThresholdHigh = 0.7f;
        [Range(0f, 1f)] public float GroggyThresholdLow  = 0.3f;

        [Header("공격에 사용할 프리팹")]
        [Tooltip("공격 전조(경고 표시)에 사용할 프리팹. SpriteRenderer가 있는 단순한 이미지 오브젝트 권장.")]
        public GameObject WarningPrefab;

        [Tooltip("촉수 휘두르기 시각 효과 프리팹. BoxCollider2D(IsTrigger)를 포함하면 충돌 감지에 활용됩니다.")]
        public GameObject SwipePrefab;

        [Tooltip("땅에서 솟아오르는 가시 프리팹. RootSpike 컴포넌트와 BoxCollider2D(IsTrigger)가 있어야 합니다.")]
        public GameObject RootSpikePrefab;

        // ─── 내부 상태 변수 ──────────────────────────────────────────────
        private HP _hp;
        private float _coreStartY;
        private HP.OnHealthChangedDelegate _onDamageLog;

        // 패턴 판단용 내부 타이머/기록 (Idle↔Attack 상태 전환에도 유지되어야 하므로 컨트롤러가 소유)
        private float _tentacleStabCooldownTimer;
        private float _groundTentacleCooldownTimer;
        private float _stationaryTimer;
        private Vector3 _lastPlayerPos;
        private bool _groggyHighConsumed;
        private bool _groggyLowConsumed;

        // ─── 공개 프로퍼티 (State 클래스에서 접근) ───────────────────────

        /// <summary>현재 그로기 진행 중 여부. 중복 예약을 방지.</summary>
        public bool IsGroggy { get; private set; } = false;

        /// <summary>직전에 실행된 패턴. 연속 사용 금지 판정에 사용.</summary>
        public PatternType? LastUsedPattern { get; set; } = null;

        /// <summary>공격 시작 직전 1회 저장되는 플레이어 위치. 공격 중에는 재조회하지 않고 이 값만 참조.</summary>
        public Vector3 PlayerPositionSnapshot { get; private set; }

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
                _onDamageLog = () =>
                {
                    Anim?.SetTrigger("HitTrigger");
                };
                _hp.onHealthChangedCallback += _onDamageLog;
            }

            // 코어 초기 Y 위치 캐싱 (이후 오프셋 계산의 기준점)
            if (CoreTransform != null)
                _coreStartY = CoreTransform.position.y;

            if (Target != null)
                _lastPlayerPos = Target.position;

            // 초기 상태: 대기(Idle)
            ChangeState(new TutorialIdleState());
        }

        // 쿨타임/정지 타이머는 Idle↔Attack 전환과 무관하게 항상 흘러야 하므로
        // 상태(State)가 아닌 컨트롤러의 Update에서 매 프레임 갱신한다.
        protected override void Update()
        {
            base.Update();

            if (_tentacleStabCooldownTimer   > 0f) _tentacleStabCooldownTimer   -= Time.deltaTime;
            if (_groundTentacleCooldownTimer > 0f) _groundTentacleCooldownTimer -= Time.deltaTime;

            if (Target != null)
            {
                float moved = Vector3.Distance(Target.position, _lastPlayerPos);
                _stationaryTimer = moved <= StationaryPositionTolerance ? _stationaryTimer + Time.deltaTime : 0f;
                _lastPlayerPos = Target.position;
            }
        }

        // ─── 패턴 판단 로직 (Idle 상태에서만 호출) ───────────────────────
        // §7 확장 훅: 거리 구간이 명확히 분리되어 있어, 향후 '휘두르기'(매우 근접,
        // ×1.5 이내) 패턴을 추가할 때는 이 리스트 맨 앞에 CanUseSwing() 분기만
        // 끼워 넣으면 되고, 아래 두 패턴의 조건/우선순위는 그대로 유지된다.
        public PatternType? SelectPattern()
        {
            if (CanUseTentacleStab()) return PatternType.TentacleStab;
            if (CanUseGroundTentacle()) return PatternType.GroundTentacle;
            return null; // 사용 가능한 패턴 없음 → Idle 유지
        }

        private bool CanUseTentacleStab()
        {
            float distance  = Vector2.Distance(transform.position, Target.position);
            float nearRange = PlayerWidth * TentacleStabRangeMultiplier;

            return distance <= nearRange
                && _tentacleStabCooldownTimer <= 0f
                && LastUsedPattern != PatternType.TentacleStab;
        }

        private bool CanUseGroundTentacle()
        {
            float distance  = Vector2.Distance(transform.position, Target.position);
            float farRange  = PlayerWidth * GroundTentacleRangeMultiplier;
            bool  triggerCondition = _stationaryTimer >= StationaryTimeThreshold || distance >= farRange;

            return triggerCondition
                && _groundTentacleCooldownTimer <= 0f
                && LastUsedPattern != PatternType.GroundTentacle;
        }

        /// <summary>선택된 패턴의 쿨타임을 시작한다 (TutorialAttackState.Enter에서 호출).</summary>
        public void StartPatternCooldown(PatternType pattern)
        {
            switch (pattern)
            {
                case PatternType.TentacleStab:   _tentacleStabCooldownTimer   = TentacleStabCooldown;   break;
                case PatternType.GroundTentacle: _groundTentacleCooldownTimer = GroundTentacleCooldown; break;
            }
        }

        /// <summary>공격 실행 직전 1회만 플레이어 위치를 저장한다 (§6).</summary>
        public void SavePlayerPositionSnapshot()
        {
            if (Target != null)
                PlayerPositionSnapshot = Target.position;
        }

        // ─── 그로기 진입 판단 (§2) ────────────────────────────────────────
        // HP가 임계치(70%/30%)를 처음 통과할 때 한 번씩만 true를 반환한다.
        // 그로기 상태머신 자체(낙하→대기→복귀)는 TutorialGroggyState가 담당하며,
        // 여기서는 "지금 그로기로 들어가야 하는가"만 판단한다 (기획 협의 필요 항목).
        public bool ShouldEnterGroggy()
        {
            if (_hp == null) return false;
            float ratio = _hp.Health / _hp.MaxHealth;

            if (!_groggyHighConsumed && ratio <= GroggyThresholdHigh)
            {
                _groggyHighConsumed = true;
                return true;
            }

            if (!_groggyLowConsumed && ratio <= GroggyThresholdLow)
            {
                _groggyLowConsumed = true;
                return true;
            }

            return false;
        }

        // ─── 그로기 플래그 세터 (GroggyState에서 사용) ───────────────────
        /// <summary>GroggyState의 Enter/Exit에서 IsGroggy 플래그를 설정합니다.</summary>
        public void SetGroggyFlag(bool value) => IsGroggy = value;

        // ─── 코어 이동 코루틴 (GroggyState에서 StartCoroutine으로 호출) ──
        /// <summary>
        /// CoreTransform을 targetY(절대 Y)까지 duration 시간 동안 부드럽게 이동시킨다.
        /// SmoothStep을 사용해 시작/끝 지점에서 자연스럽게 가속·감속한다.
        /// </summary>
        /// <param name="targetY">이동할 절대 Y 좌표</param>
        /// <param name="duration">이동에 걸리는 시간 (초)</param>
        public IEnumerator MoveCoreToY(float targetY, float duration)
        {
            if (CoreTransform == null)
            {
                Debug.LogWarning("[TutorialBoss] CoreTransform이 Inspector에 연결되지 않았습니다!");
                yield break;
            }

            Vector3 startPos = CoreTransform.position;
            Vector3 endPos   = new Vector3(startPos.x, targetY, startPos.z);
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

            // Phase 11 (D-01): boss defeat auto-save. Group A - reached via HP.OnDeath.
            if (SaveLoadManager.Instance != null)
                SaveLoadManager.Instance.SaveOnBossDefeated("TutorialBoss");

            ChangeState(new TutorialDeadState());
        }

        // 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        private void OnDestroy()
        {
            if (_hp != null)
            {
                _hp.OnDeath -= HandleDeath;
                _hp.onHealthChangedCallback -= _onDamageLog;
            }
        }

        // ─── 에디터 Gizmo (개발 중 코어 위치 시각화) ────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (CoreTransform == null) return;
            float cx   = CoreTransform.position.x;
            // 에디터 모드: 코어의 현재 씬 Y를 기준점으로, 플레이 중: 캐싱된 초기 Y 사용
            float baseY = Application.isPlaying ? _coreStartY : CoreTransform.position.y;

            // 청록 구체: 평상시 코어가 위치하는 높이
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(cx, baseY + CoreNormalY, 0f), 0.6f);
            UnityEditor.Handles.Label(
                new Vector3(cx + 0.8f, baseY + CoreNormalY, 0f), $"Normal Y (평상시) [{baseY + CoreNormalY:F1}]");

            // 노랑 구체: 그로기 시 코어가 내려오는 높이
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(cx, baseY + CoreGroggyY, 0f), 0.6f);
            UnityEditor.Handles.Label(
                new Vector3(cx + 0.8f, baseY + CoreGroggyY, 0f), $"Groggy Y (공격 기회) [{baseY + CoreGroggyY:F1}]");
        }
#endif
    }
}
