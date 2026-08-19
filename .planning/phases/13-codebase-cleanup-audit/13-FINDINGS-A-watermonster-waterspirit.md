# Phase 13 Audit — Findings A: WaterMonster / WaterSpirit

**Scope:** `Assets/Enemy/WaterMonster/**/*.cs`, `Assets/Enemy/WaterSpirit/**/*.cs`
**Files scanned:** 38
**Risk tier:** 전 범위 회귀 위험 높음 (D-05/D-06 — Play 모드 검증된 보스 상태머신)
**Generated:** 2026-08-19

## D-07 — 죽은 코드

### D-07 일반 항목

(없음)

### D-07 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| A-D07-01 | Assets/Enemy/WaterSpirit/Script/SpiritController.cs | 8 | `ChargeRange` | 프로젝트 전체 참조 1건(선언부뿐) — 문서화된 의도적 유지 (STATE.md Key Decision: 재활용 시 SpiritFarProjectile 영구 선택 불가 발생) — 삭제 권장 아님 |
| A-D07-02 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 263 | `EnterWaterCombat()` | 프로젝트 전체 참조 1건(선언부뿐), 씬/프리팹/anim/controller 참조 0건. 주석("Used by test harnesses")과 달리 실제 호출부가 코드베이스 어디에도 없음 |
| A-D07-03 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 257 | `CoreColor` (getter) | `SetCoreColor()`로 쓰기만 되고 프로젝트 전체에서 읽는 곳이 0건 (getter 자체가 소비되지 않음) |
| A-D07-04 | Assets/Enemy/WaterMonster/Script/Phase3/PuddleExplosionController.cs | 7 | `PuddleExplosionController` (클래스 전체) | 클래스명 프로젝트 전체 참조 1건(선언부뿐), 씬/프리팹/asset 참조 0건 — 코드에서 인스턴스화되지 않고 어떤 프리팹에도 부착되어 있지 않음 |
| A-D07-05 | Assets/Enemy/WaterMonster/Script/Phase3/WaterTeleportState.cs | 2 | `using System.Collections.Generic;` | 사용되지 않는 using — 파일 본문에 List/Dictionary/HashSet/IEnumerable 등 Generic 컬렉션 사용 없음 |
| A-D07-06 | Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs | 2 | `using System.Collections;` | 사용되지 않는 using — 파일 본문에 IEnumerator/ArrayList/Hashtable 등 비-Generic 컬렉션 사용 없음 (Generic 쪽 `List<SpiritController>`는 별도의 `using System.Collections.Generic;`로 커버됨) |

## D-08 — TODO/FIXME 잔재 및 임시 디버그 코드

### D-08 일반 항목

(없음)

### D-08 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Kind | Reason |
|----|------|---------|------|--------|
| A-D08-01 | Assets/Enemy/WaterMonster/Script/Phase2/WeatherController.cs | 21 | Debug.LogError | 오류 진단용 — 유지 권장 (필수 참조 누락 가드) |
| A-D08-02 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterMeleeSwipe.cs | 16 | Debug.Log | 개발용 상태 추적 — 제거 권장 (공격 실행 로그) |
| A-D08-03 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterRangedSpit.cs | 14 | Debug.Log | 개발용 상태 추적 — 제거 권장 (공격 실행 로그) |
| A-D08-04 | Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs | 105, 112 | Debug.Log | 개발용 상태 추적 — 제거 권장 (후보 없음/패턴 선택 로그) |
| A-D08-05 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 56, 87, 92 | Debug.LogError/Log/LogWarning 혼합 | 56=Stats 타입 불일치 오류 진단(유지 권장); 87/92=HitBox 설정 상태 추적(제거 권장 검토) |
| A-D08-06 | Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs | 32, 36, 51 | Debug.Log/LogWarning 혼합 | 32/51=TakeDamage/회복 상태 추적(제거 권장); 36=0 이하 데미지 가드 진단(유지 권장) |
| A-D08-07 | Assets/Enemy/WaterSpirit/Script/SpiritController.cs | 54, 109 | Debug.Log | 개발용 상태 추적 — 제거 권장 (Stage2 진입/차지 히트 로그) |
| A-D08-08 | Assets/Enemy/WaterSpirit/Script/SpiritStats.cs | 22, 28, 37, 46, 59 | Debug.Log/LogWarning 혼합 | 22/28/37/59=피격/사망 상태 추적(제거 권장); 46=SpiritController 컴포넌트 누락 진단(유지 권장) |
| A-D08-09 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritCharge.cs | 27, 35, 60 | Debug.Log | 개발용 상태 추적 — 제거 권장 (텔레포트/돌진 로그) |
| A-D08-10 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritExhaustion.cs | 11 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| A-D08-11 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritFarProjectile.cs | 27, 33 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| A-D08-12 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectileAttack.cs | 15, 30, 34 | Debug.LogWarning/Log 혼합 | 15/34=프리팹/컴포넌트 누락 진단(유지 권장); 30=발사 로그(제거 권장) |
| A-D08-13 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritRepel.cs | 12, 32, 36, 42 | Debug.Log/LogWarning 혼합 | 12/32/42=공격 실행/넉백 상태 추적(제거 권장); 36=HP 컴포넌트 누락 진단(유지 권장) |
| A-D08-14 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritStealth.cs | 36 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| A-D08-15 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritWakeRepel.cs | 18 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| A-D08-16 | Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs | 22, 38, 49, 56 | Debug.Log | 개발용 상태 추적 — 제거 권장 (전투 진입/체인/패턴 선택 로그) |
| A-D08-17 | Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs | 25, 29, 36, 52, 68, 72, 112, 124, 144 | Debug.Log/LogError/LogWarning 혼합 | 29/52=필수 참조(SpiritController/DummyPrefab) 누락 오류 진단(유지 권장); 나머지 7건=분신 스폰/헤비콤보 사이클 상태 추적(제거 권장) |

> TODO/FIXME/HACK 주석: 이 범위에서 0건 (프로젝트 전체도 0건).
> 주석 처리된 코드 블록: `grep -nE '^\s*//.*[;{}]'` 실행 결과 이 범위에서 0건 — 순수 코드 주석(설명 문장)만 존재, 커멘트-아웃된 실제 코드는 없음.

## D-10 — 과도하게 긴/복잡한 함수 (권장)

> D-10은 권장 수준 관찰이다. 실제 수정은 사용자 승인 필수 (CONTEXT.md D-10).

### D-10 일반 항목

(없음)

### D-10 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Metric | Reason |
|----|------|---------|--------|--------|--------|
| A-D10-01 | Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs | 68-114 | `SelectAttackStrategy` | 47줄 / 분기 8 | Enrage 존 스폰 early-return + 텔레포트 상태전환 early-return + 후보 목록 구성 위임 + 가중치 선택 + 후보없음/선택 로그 처리까지 5가지 책임이 한 메서드에 혼재 |

> **측정 결과 참고:** 이 범위에서 40줄 초과 또는 분기 토큰 15 초과 기준을 실제로 충족하는 메서드는 위 1건뿐이다.
> 200줄 이상 파일(`WaterMonsterController.cs` 267줄, `Stage2CombatState.cs` 173줄)도 개별 검사했으나, 두 파일 모두
> 필드/프로퍼티 선언과 다수의 짧은 메서드(각 10~30줄, 분기 2~6개)로 구성되어 있어 파일 길이만큼 개별 메서드가
> 복잡하지는 않았다. 근접 미달 사례로 `PlayerAbsorb.SubscribeInput`(27줄 / 분기 11, switch-case 5분기)와
> `WaterTeleportState.SelectTeleportTarget`(31줄 / 분기 9)이 있으나 두 기준 모두 미충족이라 표에는 넣지 않았다.

## D-09 후보 관찰 (raw — Plan 05에서 교차 검증)

- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs:100-113` 와 `Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs:44-57` — "후보 없음 로그 가드(`_noCandidateLogged`) + 패턴 선택 로그" 보일러플레이트가 두 CombatState 서브클래스에 거의 동일하게 중복. `CombatState` 기반 클래스로 승격 가능해 보임 (Phase 8 D-01a~c/D-06c 는 판단 로직 자체의 의도적 차이를 다루지, 이 로깅 보일러플레이트의 중복은 다루지 않음).
- `Assets/Enemy/WaterSpirit/Script/SpiritController.cs:99-111` (`HandleChargeImpact`) 와 `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectile.cs:37-45` (`HandleHit`) — 레이어 마스크 체크 → HP 컴포넌트 탐색 → `TakeDamage` 호출로 이어지는 유사한 피격판정 골격이 반복 (완전 동일 코드는 아니고 대상 컴포넌트 탐색 방식은 다름).

## 스캔 커버리지

| # | File | Lines | Scanned |
|---|------|-------|---------|
| 1 | Assets/Enemy/WaterMonster/Script/Phase2/PlayerAbsorb.cs | 78 | yes |
| 2 | Assets/Enemy/WaterMonster/Script/Phase2/PuddlePool.cs | 89 | yes |
| 3 | Assets/Enemy/WaterMonster/Script/Phase2/PuddleSpawner.cs | 63 | yes |
| 4 | Assets/Enemy/WaterMonster/Script/Phase2/PuddleStackManager.cs | 68 | yes |
| 5 | Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs | 56 | yes |
| 6 | Assets/Enemy/WaterMonster/Script/Phase2/WeatherController.cs | 58 | yes |
| 7 | Assets/Enemy/WaterMonster/Script/Phase3/PuddleExplosionController.cs | 114 | yes |
| 8 | Assets/Enemy/WaterMonster/Script/Phase3/WaterTeleportState.cs | 129 | yes |
| 9 | Assets/Enemy/WaterMonster/Script/Phase4/SlowDownZone.cs | 24 | yes |
| 10 | Assets/Enemy/WaterMonster/Script/Phase4/SpeedUpZone.cs | 24 | yes |
| 11 | Assets/Enemy/WaterMonster/Script/States/Attacks/GeyserEffect.cs | 27 | yes |
| 12 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterColorPrison.cs | 78 | yes |
| 13 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterColorPrisonZone.cs | 51 | yes |
| 14 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterGeyser.cs | 23 | yes |
| 15 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterJumpLand.cs | 68 | yes |
| 16 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterMeleeSwipe.cs | 35 | yes |
| 17 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterPrisonAttack.cs | 36 | yes |
| 18 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterPrisonMapAoe.cs | 26 | yes |
| 19 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterPrisonProjectile.cs | 33 | yes |
| 20 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterRangedSpit.cs | 25 | yes |
| 21 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterSpitProjectile.cs | 34 | yes |
| 22 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterWaveProjectile.cs | 36 | yes |
| 23 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterWavePush.cs | 51 | yes |
| 24 | Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs | 140 | yes |
| 25 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 267 | yes |
| 26 | Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs | 88 | yes |
| 27 | Assets/Enemy/WaterSpirit/Script/SpiritController.cs | 111 | yes |
| 28 | Assets/Enemy/WaterSpirit/Script/SpiritStats.cs | 75 | yes |
| 29 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritCharge.cs | 62 | yes |
| 30 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritExhaustion.cs | 13 | yes |
| 31 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritFarProjectile.cs | 35 | yes |
| 32 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectile.cs | 45 | yes |
| 33 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectileAttack.cs | 37 | yes |
| 34 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritRepel.cs | 46 | yes |
| 35 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritStealth.cs | 40 | yes |
| 36 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritWakeRepel.cs | 21 | yes |
| 37 | Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs | 74 | yes |
| 38 | Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs | 173 | yes |

## 요약

| 카테고리 | 항목 수 | 고위험 항목 수 |
|----------|---------|----------------|
| D-07 | 6 | 6 |
| D-08 | 17 | 17 |
| D-10 | 1 | 1 |
