# Phase 5: 보스 기반 엔티티 및 스테이지 1 공격 패턴 - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning

<domain>
## Phase Boundary

`SpiritController : BossController` 독립 엔티티를 씬에 배치하고, 플레이어 감지 시 전투 상태로 진입한다.
3종 공격 패턴(S1-01 돌진 / S1-02 투사체 / S1-03 튕겨내기)이 쿨다운 기반으로 동작하며, HP가 0이 되면 사망 처리된다.

**범위 외:**
- 스테이지 2 전환·은신·분신 시스템 (Phase 6)
- 애니메이션·시각 이펙트 연동 (v3.0+)
- 물 배리어·그로기 메커니즘 (WaterMonster 전용)

</domain>

<decisions>
## Implementation Decisions

### D-01: HP·데미지 파이프라인
- **D-01a:** `SpiritStats : BossStatsSystem` 을 신규 작성한다.
- **D-01b:** `protected override void TakeDamage(DamageInfo info)` 에서 배리어/물 로직을 완전히 제거하고, `_currentHealth -= info.amount` 로 직접 HP를 차감한다.
- **D-01c:** `IsBarrierActive` 를 사용하지 않는다. Spirit Boss는 배리어 없음.
- **D-01d:** `SpiritCombatState.ShouldTransitionToGroggy()` 는 `return false` 로 고정한다 (그로기 없음).

### D-02: 3종 패턴 거리 임계치
- **D-02a:** `RepelRange`, `ChargeRange`, `ProjectileRange` 3개를 `[SerializeField]` 로 Inspector에 노출한다.
- **D-02b:** 기본값은 플래너(Claude) 재량으로 설정한다.

### D-03: 패턴 선택 우선순위 및 쿨다운
- **D-03a:** 우선순위: 근접(RepelRange 이내) → 튕겨내기. 중거리(ChargeRange 이내) → 돌진. 그 외 → 투사체.
- **D-03b:** 공통 쿨다운 방식: 한 패턴 시전 후 `_decisionTimer` 에 해당 패턴의 Cooldown 값을 적용하며, 타이머가 0 이하일 때 다음 패턴 실행.
- **D-03c:** 각 패턴의 Cooldown 값은 Inspector 노출 또는 플래너 재량.

### D-04: 돌진 패턴 (S1-01)
- **D-04a:** 2단계 구조 — ①준비(Windup): 보스 정지 후 ChargeWindup(초) 대기, ②돌진: 목표점으로 직선 이동.
- **D-04b:** 목표점 고정 시점: 준비 종료 시점의 플레이어 위치 + 진행 방향으로 OvershotDistance(유닛) 만큼 더 나간 좌표.
- **D-04c:** 플레이어가 피해도 고정된 목표점까지 이동 후 종료.
- **D-04d:** 돌진 중 Player 레이어와 충돌(OnTriggerEnter2D) 시 데미지 1회 적용.
- **D-04e:** ChargeWindup / ChargeSpeed / OvershotDistance 모두 `[SerializeField]` 로 Inspector 노출.

### Claude's Discretion
- 파일 위치: `Assets/Enemy/WaterSpirit/Script/` (Spirit 전용 폴더 신규 생성)
- `SpiritController` 의 Update() 에서 `CombatState` → `SpiritCombatState` 교체 인터셉트 방식 (WaterMonsterController 동일 패턴 참고)
- 사망 처리: `SpiritStats.Die()` override → `gameObject.SetActive(false)` 또는 DeadState 전환 (플래너 재량)
- S1-02 투사체 세부 동작 (속도·수명·데미지): Inspector 노출 + WaterSpitProjectile 패턴 참고
- S1-03 튕겨내기 세부 동작 (knockback force·데미지): Inspector 노출
- 수치 기본값 전부: 플래너 재량 (Inspector 조정 가능하게)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기반 클래스 (상속 대상)
- `Assets/Enemy/NewBoss/Script/BossController.cs` — protected virtual Awake/Start/Update/OnDestroy, ChangeState(), MoveTo(), StopMove(), LookAtTarget(), StartHeavyAttackCooldown()
- `Assets/Enemy/NewBoss/Script/BossStatesSystem.cs` — HP/_currentHealth, TakeDamage(DamageInfo) protected virtual, Die() protected virtual, OnDamageTaken 이벤트
- `Assets/Enemy/NewBoss/Script/States/IBossState.cs` — 상태 인터페이스 (Enter/Execute/Exit)
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — _decisionTimer, _isAttacking, SelectAttackStrategy() virtual, ShouldTransitionToGroggy() virtual
- `Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs` — AnimationName, Cooldown, ExecuteAttack(boss)

### 레퍼런스 구현 (패턴 참고)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — Update() 인터셉트로 CombatState 교체 패턴, CheckEnrageTrigger, Inspector SerializeField 패턴
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — TakeDamage override 예시
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — ShouldTransitionToGroggy override, SelectAttackStrategy override 예시
- `Assets/Enemy/WaterMonster/Script/States/Attacks/WaterRangedSpit.cs` — IAttackStrategy 구현 예시 (투사체)
- `Assets/Enemy/WaterMonster/Script/States/Attacks/WaterSpitProjectile.cs` — 투사체 비행 구현 예시

### 프로젝트 요구사항
- `.planning/REQUIREMENTS.md` — CORE-01, CORE-02, CORE-04, S1-01, S1-02, S1-03 상세 조건
- `.planning/ROADMAP.md` §Phase 5 — 성공 기준 5개 항목

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `BossController.MoveTo(Vector2)` / `StopMove()` — 이동 로직 재사용 (linearVelocity, LookAtTarget 포함)
- `BossController.ChangeState(IBossState)` — 상태 전환 그대로 사용
- `CombatState` — `SpiritCombatState : CombatState` 로 상속해 SelectAttackStrategy, ShouldTransitionToGroggy override
- `IAttackStrategy` — 돌진/투사체/튕겨내기 3종 모두 이 인터페이스로 구현
- `IdleState`, `ChaseState` — 재사용 (Spirit에 그로기·반격 불필요하므로 이벤트 핸들러 오버라이드 필요)

### Established Patterns
- 상태 교체 인터셉트: `WaterMonsterController.Update()` 에서 `typeof(CombatState)` 체크 → 서브클래스 교체. 동일 패턴 사용.
- HP 이벤트 구독: `Start()` 에서 `Stats.OnDamageTaken += Handler`, `OnDestroy()` 에서 해제
- Unity 6 Physics 2D: `_rb.linearVelocity = direction * speed` (velocity 아님)
- Inspector 노출: `[SerializeField] [Range]` 적극 활용

### Integration Points
- 보스 피격: `TakeDmg.cs` (Assets/Script/) 가 `BossStatsSystem.TakeDamageInfo(DamageInfo)` 호출 — SpiritStats가 BossStatsSystem 상속이면 자동 연동
- Player 레이어 타겟팅: Physics Layer Matrix에서 Enemy ↔ Player 충돌 설정 (기존 설정 재사용)
- 씬 배치: InGame.unity에 WaterMonster와 별도로 배치

</code_context>

<specifics>
## Specific Ideas

- 돌진 목표점 = `playerPos + (bossToPlayer.normalized * (distToPlayer + OvershotDistance))` — 플레이어를 관통해 뒤쪽 좌표로 설정
- `SpiritController` 는 `WaterMonster` 의 Phase2/3/4 이벤트 연결 없음 — 클린한 상속만 사용
- Phase 6을 위해 `isDummy` 플래그를 `SpiritController` 에 미리 선언해두면 좋음 (플래너 재량)

</specifics>

<deferred>
## Deferred Ideas

- 사망 처리 세부 방식 — Phase 5 범위이나 구현 방식(비활성화 vs DeadState)은 플래너 재량
- 스테이지 2 전환 트리거 — Phase 6에서 구현
- 애니메이션·이펙트 — v3.0+

</deferred>

---

*Phase: 05-spirit-boss-stage1-patterns*
*Context gathered: 2026-04-30*
