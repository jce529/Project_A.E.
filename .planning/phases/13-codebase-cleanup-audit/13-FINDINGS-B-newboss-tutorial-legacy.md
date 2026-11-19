# Phase 13 Audit — Findings B: NewBoss / Tutorial / Legacy Enemies

**Scope:** `Assets/Enemy/NewBoss/**`, `Assets/Enemy/Tutorial/**`, `Assets/Enemy/Boss/**`, `Assets/Enemy/Monster_Alpha/**`, `Assets/Enemy/Script/**`
**Files scanned:** 42
**Risk tier:** 혼합 — NewBoss/Tutorial = 고위험, Boss/Monster_Alpha/Script = 일반
**Generated:** 2026-08-19

## D-07 — 죽은 코드

### D-07 일반 항목

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| B-D07-01 | Assets/Enemy/Monster_Alpha/Script/EnemyBrain.cs | 12-162 | `EnemyBrain` (클래스 전체) | 프로젝트 전체 코드 참조 0건(선언부만), `.cs.meta` GUID로 전체 `.unity`/`.prefab` 검색해도 어디에도 컴포넌트로 부착되지 않음 — 씬/코드 양쪽에서 완전히 고립된 클래스 |
| B-D07-02 | Assets/Enemy/Monster_Alpha/Script/patorl.cs | 3-63 | `PatrolMovement` (클래스 전체) | 프로젝트 전체 코드 참조 0건, GUID 검색으로도 씬/프리팹 부착 0건 |

### D-07 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| B-D07-03 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 5-45 | `SeedProjectile` (클래스 전체) | 프로젝트 전체 코드 참조 0건(선언부만), GUID로 전체 `.unity`/`.prefab` 검색해도 부착 위치 없음 — 어떤 프리팹/씬에도 이 컴포넌트가 존재하지 않아 실질적으로 스폰될 수 없는 고아 클래스 |
| B-D07-04 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 15 | `Launch(Vector2 dir)` | public 메서드지만 프로젝트 전체에서 호출부 0건 (클래스 자체가 B-D07-03) |
| B-D07-05 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossChaseState.cs | 5-56 | `WoodBossChaseState` (클래스 전체) | `IBossState` 구현체지만 프로젝트 전체에서 `new WoodBossChaseState()` 호출부 0건 — 상태머신에 편입되지 못한 상태 클래스. `IdleState`/`WoodBossAttackState`는 전부 `WoodBoss.IdleState`/`WoodBossAttackState`로 전환하며 이 클래스로는 전환하지 않음 |
| B-D07-06 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossAttackState.cs | 19 | `CloseRange` (private const float) | 선언 후 파일 내에서도, 프로젝트 전체에서도 참조 0건 — 사용되지 않는 상수 |
| B-D07-07 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs | 6 | `WoodBossStatsSystem` (클래스, 재확인) | **재확인 결과 — 기존 고아 판정이 오류였음.** `Assets/SaveSystem/Check.md:108`은 문자열 `"WoodBossStatSystem"`(중간 `s` 없음)으로 검색해 참조 0건이라 결론지었으나, 실제 클래스명은 `WoodBossStatsSystem`(중간에 `s` 포함, 파일명과 철자 불일치)이다. 이 철자로 재검색하면 `WoodBossController.cs:19,25`에서 `private WoodBossStatsSystem _woodStats;` 필드 선언 + `GetComponent<WoodBossStatsSystem>()` 호출로 실제 사용 중임을 확인 — **삭제 대상 아님, 기존 STATE.md 기재가 정정되어야 함** |

## D-08 — TODO/FIXME 잔재 및 임시 디버그 코드

### D-08 일반 항목

| ID | File | Line(s) | Kind | Reason |
|----|------|---------|------|--------|
| B-D08-01 | Assets/Enemy/Boss/Script/FloorHandAttack.cs | 29 | Debug.Log | 개발용 상태 추적("바닥 손 공격 시작!") — 제거 권장 |
| B-D08-02 | Assets/Enemy/Boss/Script/hand.cs | 31 | Debug.LogWarning | "Player not found" — 오류 진단용 — 유지 권장 |
| B-D08-03 | Assets/Enemy/Monster_Alpha/Script/bullet.cs | 17, 33, 50 | Debug.Log/LogError | 17/33=Rigidbody2D 누락 오류 진단용(유지 권장), 50=피격 로그(개발용 추적, 제거 권장) |
| B-D08-04 | Assets/Enemy/Monster_Alpha/Script/enemy.cs | 25, 73 | Debug.LogError/Log | 25=Player 태그 미설정 오류 진단용(유지 권장), 73=사망 로그(개발용 추적, 제거 권장) |
| B-D08-05 | Assets/Enemy/Monster_Alpha/Script/EnemyBrain.cs | 140, 146 | Debug.LogWarning/Log | 클래스 자체가 B-D07-01로 죽은 코드 — 실행되지 않으므로 정리 시 클래스와 함께 제거됨 |
| B-D08-06 | Assets/Enemy/Script/EnemyHitBox.cs | 18 | Debug.LogWarning | BossStatsSystem 탐색 실패 — 오류 진단용 — 유지 권장 |
| B-D08-07 | (없음, 0건 — 프로젝트 전체도 0건) | - | TODO | - |

> TODO/FIXME/HACK 주석: 이 범위에서 0건 (프로젝트 전체도 0건).

### D-08 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Kind | Reason |
|----|------|---------|------|--------|
| B-D08-08 | Assets/Enemy/NewBoss/Script/BossController.cs | 93, 100 | 주석처리 코드 | `//if (Anim != null) Anim.SetBool(...)` 2건 — 애니메이션 파라미터 미사용으로 주석 처리된 코드 |
| B-D08-09 | Assets/Enemy/NewBoss/Script/BossStatesSystem.cs | 87, 95 | Debug.Log | 배리어/체력 피격 로그 — 개발용 상태 추적 — 제거 권장 |
| B-D08-10 | Assets/Enemy/NewBoss/Script/States/ChaseStates.cs | 7, 32 | Debug.Log | Enter/Exit 로그 — 개발용 상태 추적 — 제거 권장 |
| B-D08-11 | Assets/Enemy/NewBoss/Script/States/CombatState.cs | 80, 84, 114, 119, 130 | Debug.Log/LogWarning | 80=애니메이션 타임아웃 경고(유지 권장), 나머지 4건=상태 전환/공격 선택 추적(제거 권장) — 전 보스 공용 헬퍼이므로 회귀 위험 높음 |
| B-D08-12 | Assets/Enemy/NewBoss/Script/States/IdleState.cs | 8, 23 | Debug.Log | Enter/Exit 로그 — 개발용 상태 추적 — 제거 권장 |
| B-D08-13 | Assets/Enemy/NewBoss/Script/States/Attacks/HeavyAttack.cs | 19 | Debug.Log | 개발용 상태 추적 — 제거 권장 (CP949 파일) |
| B-D08-14 | Assets/Enemy/NewBoss/Script/States/Attacks/LightAttack.cs | 18 | Debug.Log | 개발용 상태 추적 — 제거 권장 (CP949 파일) |
| B-D08-15 | Assets/Enemy/NewBoss/Script/States/Attacks/RangedPokeAttack.cs | 18 | Debug.Log | 개발용 상태 추적 — 제거 권장 (CP949 파일) |
| B-D08-16 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/Rootspikevisual.cs | 32 | Debug.LogWarning | Pillar/SpikeHead 미할당 경고 — 오류 진단용 — 유지 권장 (CP949 파일) |
| B-D08-17 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 40, 41 | 주석처리 코드 | `// var playerStats = ...` 2건 — PlayerStats 연동 시도가 주석 처리됨 (클래스 자체가 B-D07-03 죽은 코드) |
| B-D08-33 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 37 | Debug.Log | "플레이어 피격!" — 클래스 자체가 죽은 코드라 실행되지 않음 (CP949 파일) |
| B-D08-18 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialAttackState.cs | 49 | Debug.Log | 상태 전환 추적 — 개발용, 제거 권장 |
| B-D08-19 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialDeadState.cs | 15, 32 | Debug.Log/주석처리 코드 | 15=사망 상태 로그(제거 권장), 32=`// boss.Anim?.SetTrigger("Die");` 주석처리 코드 |
| B-D08-20 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialGroggyState.cs | 27, 55, 59, 64, 70, 75 | Debug.Log | 그로기 단계별 진행 로그 6건 — 개발용 상태 추적, 제거 권장 |
| B-D08-21 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialIdleState.cs | 24 | Debug.Log | 상태 전환 추적 — 제거 권장 |
| B-D08-22 | Assets/Enemy/Tutorial/TutorialBoss/TentaclePierceStrategy.cs | 79, 100, 122 | Debug.Log | 전조/공격/피격 로그 3건 — 개발용 추적, 제거 권장 |
| B-D08-23 | Assets/Enemy/Tutorial/TutorialBoss/TentacleSwipeStrategy.cs | 91, 102, 122, 161, 217 | Debug.LogWarning/Log | 91=바닥 감지 실패 경고(유지 권장), 나머지 4건=진행 로그(제거 권장) |
| B-D08-24 | Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs | 153, 275 | Debug.Log/LogWarning | 153=피격 로그(제거 권장), 275=CoreTransform 미연결 경고(유지 권장) |
| B-D08-25 | Assets/Enemy/Tutorial/TutorialBoss/TutorialRootSpikeStrategy.cs | 24, 42, 57 | Debug.LogWarning/Log | 24/42=프리팹 누락·바닥 감지 실패 경고(유지 권장), 57=스파이크 소환 로그(제거 권장) |
| B-D08-26 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/FloorSweepStrategy.cs | 36, 60, 72, 93, 123, 142 | Debug.LogWarning/Log | 36/60=프리팹 누락·바닥 감지 실패 경고(유지 권장), 나머지 4건=진행 로그(제거 권장) |
| B-D08-27 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/IdleState.cs | 14 | Debug.Log | 타겟 발견 로그 — 제거 권장 (CP949 파일) |
| B-D08-28 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/RootSpikeStrategy.cs | 27, 33, 59 | Debug.LogWarning/Log | 27=프리팹 누락 경고(유지 권장), 33/59=진행 로그(제거 권장) (CP949 파일) |
| B-D08-29 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/VineSwingStrategy.cs | 28, 37, 48 | Debug.Log | 차징/타격/피격 로그 3건 — 개발용 추적, 제거 권장 (주석처리 코드는 B-D09 참고) |
| B-D08-30 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossAttackState.cs | 50, 93 | Debug.Log | 공격 선택 로그 2건 — 개발용 추적, 제거 권장 |
| B-D08-31 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs | 51, 64, 66 | Debug.Log/주석처리 코드 | 51/64=사망 시퀀스 로그(제거 권장), 66=`// Anim.SetTrigger("Die");` 주석처리 코드 |
| B-D08-32 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs | 29, 40 | Debug.Log | 체력 변화/사망 로그 2건 — 개발용 추적, 제거 권장 (CP949 파일) |

> `Assets/Enemy/Tutorial/WoodBoss/BossController/State/VineSwingStrategy.cs` 주석처리 코드(4건): 31, 32, 38, 55행 — `// boss.ShowWarningCircle(...)`, `// boss.Anim.SetTrigger("Charge")`, `// boss.Anim.SetTrigger(AnimationName)`, `// boss.HideWarningCircle()` — 애니메이션/경고이펙트 연동이 아직 붙지 않아 주석 처리된 상태.
> `Assets/Enemy/Tutorial/WoodBoss/BossController/State/FloorSweepStrategy.cs:144` 주석처리 코드: `// results[i].GetComponent<IDamageable>()?.TakeDamage(_damage);` — IDamageable 인터페이스 경로 미사용.

## D-10 — 과도하게 긴/복잡한 함수 (권장)

> D-10은 권장 수준 관찰이다. 실제 수정은 사용자 승인 필수 (CONTEXT.md D-10).

### D-10 일반 항목

(없음)

### D-10 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Metric | Reason |
|----|------|---------|--------|--------|--------|
| B-D10-01 | Assets/Enemy/NewBoss/Script/States/CombatState.cs | 68-138 | `Execute` | 70줄 / 분기 8 | 애니메이션 대기 판정 + 쿨다운 체크 + 그로기 전환 + 거리 판정 + 공격 전략 선택·시작 5개 책임이 한 메서드에 혼재. 전 보스(WaterMonster/WaterSpirit/TutorialBoss) 공용 기반 클래스 메서드라 회귀 위험 최고 등급 |
| B-D10-02 | Assets/Enemy/NewBoss/Script/States/CombatState.cs | 158-200 | `SelectWeightedPattern` | 41줄 / 분기 15 | 거리 조건 필터 + 쿨다운 필터 + 직전패턴 배제/감쇠 + 가중치 누적 + 랜덤 선택까지 한 메서드. Phase 7/8에서 SpiritCombatState/WaterMonsterCombatState 양쪽이 공유하는 핵심 헬퍼라 분해 시 양쪽 보스 동시 회귀 가능 |
| B-D10-03 | Assets/Enemy/Tutorial/TutorialBoss/TentacleSwipeStrategy.cs | 68-162 | `AttackRoutine` | 94줄 / 분기 10 | 바닥 Raycast 감지 + 경고 표시 + 스윕 오브젝트 생성 + Lerp 이동 + 매 프레임 히트체크 5단계가 코루틴 하나에 순차 결합 |
| B-D10-04 | Assets/Enemy/Tutorial/TutorialBoss/TentaclePierceStrategy.cs | 68-133 | `AttackRoutine` | 65줄 / 분기 5 | 경고 표시 + OverlapBoxAll AoE 판정 + foreach 데미지 적용 + 후딜레이가 한 코루틴에 순차 결합 |
| B-D10-05 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/FloorSweepStrategy.cs | 31-125 | `AttackRoutine` | 95줄 / 분기 6 | TentacleSwipeStrategy.AttackRoutine(B-D10-03)과 거의 동일한 구조(바닥 감지+경고+스윕+Lerp+히트체크) — D-09 후보로도 교차 관찰됨 |

## D-04 — CP949 인코딩 위험 파일

> 이 파일들은 CP949 인코딩이다. 실제 수정 단계에서는 표준 Read/Edit 왕복이 비-ASCII 바이트를 U+FFFD로 훼손시키므로 `git show HEAD:<path>` + 순수 바이트 스크립트 프로토콜이 필요하다 (CONTEXT.md D-04, STATE.md Phase 11 Plan 3 기록).

| # | File | 비고 |
|---|------|------|
| 1 | Assets/Enemy/Boss/Script/HandCollision.cs | 일반 |
| 2 | Assets/Enemy/Monster_Alpha/Script/patorl.cs | 일반 (B-D07-02 죽은 코드) |
| 3 | Assets/Enemy/NewBoss/Script/States/Attacks/HeavyAttack.cs | 고위험 |
| 4 | Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs | 고위험 |
| 5 | Assets/Enemy/NewBoss/Script/States/Attacks/LightAttack.cs | 고위험 |
| 6 | Assets/Enemy/NewBoss/Script/States/Attacks/RangedPokeAttack.cs | 고위험 |
| 7 | Assets/Enemy/NewBoss/Script/States/CounterState.cs | 고위험 |
| 8 | Assets/Enemy/NewBoss/Script/States/GroggyState.cs | 고위험 |
| 9 | Assets/Enemy/NewBoss/Script/States/IBossState.cs | 고위험 |
| 10 | Assets/Enemy/NewBoss/Script/States/IdleState.cs | 고위험 |
| 11 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/Rootspikevisual.cs | 고위험 |
| 12 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 고위험 (B-D07-03 죽은 코드) |
| 13 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/IdleState.cs | 고위험 |
| 14 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/RootSpikeStrategy.cs | 고위험 |
| 15 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossChaseState.cs | 고위험 (B-D07-05 죽은 코드) |
| 16 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs | 고위험 (B-D07-07 재확인 — 살아있음) |

## D-09 후보 관찰 (raw — Plan 05에서 교차 검증)

- **동명 클래스 `IdleState` 2개**: `Assets/Enemy/NewBoss/Script/States/IdleState.cs:4`(네임스페이스 없음)와 `Assets/Enemy/Tutorial/WoodBoss/BossController/State/IdleState.cs:6`(namespace `WoodBoss`) — 둘 다 `IBossState` 구현, `Enter/Execute/Exit` 골격이 구조적으로 동일(TargetFound 체크 후 상태 전환)하지만 전환 대상이 다름(`ChaseState` vs `WoodBossAttackState`)과 로그 문구가 다름. 네임스페이스로 분리되어 컴파일 충돌은 없으나, `WoodBossController.cs:45`는 `new WoodBoss.IdleState()`로 명시적 정규화가 필요해 유지보수 시 혼동 위험이 있음.
- **`TentaclePierceStrategy.cs:68-133` vs `TentacleSwipeStrategy.cs:68-162`**: 둘 다 "경고 프리팹 생성 → WaitForSeconds → 경고 제거 → 공격 판정 → Debug.Log" 골격이 거의 동일한 `AttackRoutine` 코루틴 구조(각각 B-D10-04/B-D10-03). 공격 판정 방식만 다름(OverlapBoxAll AoE vs Lerp 이동+매 프레임 히트체크).
- **`TentacleSwipeStrategy.AttackRoutine`(B-D10-03) vs `FloorSweepStrategy.AttackRoutine`(B-D10-05)**: 바닥 Raycast 감지 → 경고 → 스윕 오브젝트 Lerp 이동 → 매 프레임 히트체크 구조가 거의 1:1로 동일 (TutorialBoss 계열과 WoodBoss 계열이 각각 독립 구현).
- **`Assets/Enemy/Monster_Alpha/Script/` 이동/추적 로직**: `enemy.cs`(살아있음, HP 상속 + 사격형)와 `EnemyBrain.cs`(B-D07-01 죽은 코드)가 유사한 "플레이어 감지 → 사거리 체크 → 발사/대시" 골격을 가지나, `EnemyBrain`이 죽은 코드이므로 실질적 중복 위험은 없음 — 교차검증 시 배제 권장.

## 스캔 커버리지

| # | File | Lines | Risk | Scanned |
|---|------|-------|------|---------|
| 1 | Assets/Enemy/Boss/Script/BossHealthBarController.cs | 67 | 일반 | yes |
| 2 | Assets/Enemy/Boss/Script/FloorHandAttack.cs | 82 | 일반 | yes |
| 3 | Assets/Enemy/Boss/Script/hand.cs | 100 | 일반 | yes |
| 4 | Assets/Enemy/Boss/Script/HandCollision.cs | 18 | 일반 | yes |
| 5 | Assets/Enemy/Monster_Alpha/Script/bullet.cs | 66 | 일반 | yes |
| 6 | Assets/Enemy/Monster_Alpha/Script/Charge.cs | 138 | 일반 | yes |
| 7 | Assets/Enemy/Monster_Alpha/Script/enemy.cs | 81 | 일반 | yes |
| 8 | Assets/Enemy/Monster_Alpha/Script/EnemyBrain.cs | 162 | 일반 | yes |
| 9 | Assets/Enemy/Monster_Alpha/Script/patorl.cs | 63 | 일반 | yes |
| 10 | Assets/Enemy/NewBoss/Script/BossController.cs | 192 | 고위험 | yes |
| 11 | Assets/Enemy/NewBoss/Script/BossStatesSystem.cs | 106 | 고위험 | yes |
| 12 | Assets/Enemy/NewBoss/Script/States/Attacks/HeavyAttack.cs | 20 | 고위험 | yes |
| 13 | Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs | 7 | 고위험 | yes |
| 14 | Assets/Enemy/NewBoss/Script/States/Attacks/LightAttack.cs | 19 | 고위험 | yes |
| 15 | Assets/Enemy/NewBoss/Script/States/Attacks/RangedPokeAttack.cs | 19 | 고위험 | yes |
| 16 | Assets/Enemy/NewBoss/Script/States/ChaseStates.cs | 35 | 고위험 | yes |
| 17 | Assets/Enemy/NewBoss/Script/States/CombatState.cs | 226 | 고위험 | yes |
| 18 | Assets/Enemy/NewBoss/Script/States/CounterState.cs | 26 | 고위험 | yes |
| 19 | Assets/Enemy/NewBoss/Script/States/GroggyState.cs | 29 | 고위험 | yes |
| 20 | Assets/Enemy/NewBoss/Script/States/IBossState.cs | 9 | 고위험 | yes |
| 21 | Assets/Enemy/NewBoss/Script/States/IdleState.cs | 28 | 고위험 | yes |
| 22 | Assets/Enemy/Script/EnemyHitBox.cs | 28 | 일반 | yes |
| 23 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/RootSpike.cs | 14 | 고위험 | yes |
| 24 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/Rootspikevisual.cs | 54 | 고위험 | yes |
| 25 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 52 | 고위험 | yes |
| 26 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SpriteColliderSync.cs | 47 | 고위험 | yes |
| 27 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialAttackState.cs | 75 | 고위험 | yes |
| 28 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialDeadState.cs | 49 | 고위험 | yes |
| 29 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialGroggyState.cs | 79 | 고위험 | yes |
| 30 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialIdleState.cs | 46 | 고위험 | yes |
| 31 | Assets/Enemy/Tutorial/TutorialBoss/TentaclePierceStrategy.cs | 183 | 고위험 | yes |
| 32 | Assets/Enemy/Tutorial/TutorialBoss/TentacleSwipeStrategy.cs | 246 | 고위험 | yes |
| 33 | Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs | 343 | 고위험 | yes |
| 34 | Assets/Enemy/Tutorial/TutorialBoss/TutorialRootSpikeStrategy.cs | 60 | 고위험 | yes |
| 35 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/FloorSweepStrategy.cs | 150 | 고위험 | yes |
| 36 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/IdleState.cs | 23 | 고위험 | yes |
| 37 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/RootSpikeStrategy.cs | 69 | 고위험 | yes |
| 38 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/VineSwingStrategy.cs | 67 | 고위험 | yes |
| 39 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossAttackState.cs | 99 | 고위험 | yes |
| 40 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossChaseState.cs | 93 | 고위험 | yes |
| 41 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs | 75 | 고위험 | yes |
| 42 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs | 43 | 고위험 | yes |

## 요약

| 카테고리 | 항목 수 | 고위험 항목 수 |
|----------|---------|----------------|
| D-07 | 7 | 5 |
| D-08 | 33 | 26 |
| D-10 | 5 | 5 |
