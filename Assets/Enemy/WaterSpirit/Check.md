# WaterSpirit — 패턴 판단 로직 Play 모드 검증 체크리스트

**상태: 08-03 일괄 검증 반영(2026-11-05) — 5/6 PASS(phase-07 07-03 측정 참조 4 + WaterMonster 회귀 1), D-07 Stage 2 사이클 미검증** — WaterMonster 보스가 동일한 CombatState 기반 패턴 판단 로직으로
전환된 뒤, 전체 보스를 한 번에 일괄 Play 모드 검증할 예정이다.
(출처: `.planning/phases/07-boss-attack-pattern-judgment/07-02-PLAN.md`)

## 검증 대상 변경사항

`SpiritCombatState`의 고정 라운드로빈(`_pattern[_patternIndex++ % 5]`)을 제거하고,
`CombatState`의 범용 헬퍼(`PatternCandidate` + `SelectWeightedPattern` + `ForceSelectPattern`)를
이용한 조건부 가중치 랜덤 선택으로 교체했다.

| 패턴 | 가중치 | 거리 조건 |
|---|---|---|
| SpiritCharge | 1.0 | 없음 |
| SpiritExhaustion | 0.6 | 없음 |
| SpiritWakeRepel | 1.0 | `RepelRange`(1.5) 이내만 |
| SpiritFarProjectile | 1.0 | 없음 |

추가로 직전 패턴 연속 사용 금지, 패턴별 쿨다운, `SpiritExhaustion` 직후
`SpiritWakeRepel` 강제 체인이 적용되었다. `Stage2CombatState.cs` /
`WaterMonsterCombatState.cs` / `SpiritController.cs`는 수정하지 않았다.

## 사전 준비

1. Unity 에디터에서 컴파일 에러 0건 확인
2. 물의 정령 보스 씬 열기
3. Console Collapse를 끄고 검색창에 `SpiritCombatState` 입력해 필터링

## 정적 회귀 검사 (2026-10-31, 08-03 Task 1 — 소스 grep, Play 모드 아님)

- PASS: `SpiritCombatState.cs` 의 2-인자 호출 `SelectWeightedPattern(dist, _candidates);` 그대로 유지
  → 기본값 `lastUsedWeightMultiplier = 0f`(완전배제) 로 동작 (`CombatState.cs` 에 기본값 `= 0f` 및
  `if (lastUsedWeightMultiplier <= 0f) continue;` 확인).
- PASS: Phase 8 커밋이 바꾼 `.cs` 는 `CombatState.cs`, `WaterMonsterCombatState.cs` 2개뿐 —
  `SpiritCombatState.cs` / `Stage2CombatState.cs` 는 Phase 8 에서 변경 0건.

정적 회귀는 통과. 아래 Play 모드 항목(특히 D-05 완전배제 회귀)은 **전부 미검증** 상태다.

## 검증 항목

- [x] **D-03 랜덤성**: Play로 30초 이상 교전 → `[SpiritCombatState] 패턴 선택:` 로그가
      매번 같은 순서로 반복되지 않는다. Stop 후 재시작해 두 번째 시퀀스도 기록
      (첫 번째와 달라야 함).
- [x] **D-05 연속 사용 금지**: 로그에서 같은 패턴명이 연속 두 줄로 나오지 않는다.
- [x] **D-04 강제 체인**: `패턴 선택: SpiritExhaustion` 직후 반드시
      `체인 발동: SpiritExhaustion → SpiritWakeRepel (강제)` →
      `[CombatState] 새로운 공격 시작: SpiritWakeRepel` 순서로 찍히고,
      그 다음엔 다시 일반 `패턴 선택:`으로 복귀한다.
- [x] **후보 고갈 스팸 없음**: `사용 가능한 패턴 후보 없음` 로그가 프레임마다
      쏟아지지 않는다 (나와도 1줄만, 곧 공격 재개).
- [ ] **D-07 Stage 2 사이클**: HP 50% 이하로 Stage 2 진입 → `일반 패턴 실행 #1/3~#3/3`
      → `헤비콤보 단계 진입` → 헤비콤보 종료 → 분신 삭제+그로기 전환이 Phase 6과
      동일하게 동작하고, 분신 2 + 진짜 보스 1 = 총 3개가 동시 존재한다.
- [x] **D-08b 물괴물 보스 회귀 없음**: WaterMonster 마이그레이션 완료 후,
      `WaterMonsterCombatState`가 이번 변경 이전과 동일하게 동작한다.

## 결과 기록

(검증 완료 시 여기에 관찰된 패턴 시퀀스 로그 원문과 각 항목 PASS/FAIL을 기록한다.)

### 08-03 검증 결과 (2026-11-05)

WaterSpirit 항목은 phase-07-verify(07-03 패턴 이력 유지) 세션이 같은 에디터에서 `1 stage` 씬으로 240초 x 2회(총 165회 패턴 실행)
실측한 결과를 참조한다. 08-03 에서 중복 검증하지 않았다. 원본 로그: 총괄 노트 폴더 `p7_03_run1_pattern_log.txt`, `p7_03_run2_pattern_log.txt`.

| 항목 | 판정 | 증거 |
|---|---|---|
| D-03 랜덤성 | PASS (07-03 참조) | 두 회차 시퀀스 상이. Run1 `Charge→FarProjectile→Exhaustion→WakeRepel→Exhaustion→WakeRepel…` / Run2 `Exhaustion→WakeRepel→Charge→FarProjectile→Charge→Exhaustion…` |
| D-05 연속 사용 금지(완전배제) | PASS (07-03 참조) | 연속 동일 패턴 0건. CombatState 재진입 약 70회/회차에도 0건. Plan 08-01 헬퍼 확장의 기본값(완전배제) 회귀 없음 |
| D-04 강제 체인 | PASS (07-03 참조) | Exhaustion→WakeRepel 42/42(100%), 그중 39회는 Chase 를 거쳐 CombatState 가 새로 생성된 뒤에도 유지(07-02 에서는 9/16) |
| 후보 고갈 스팸 없음 | PASS (07-03 참조) | 5초 이상 멈춘 사례 없음 |
| D-07 Stage 2 사이클 | 미검증 | 07-03 범위 밖이라 Stage 2 진입을 측정하지 않았고 08-03 에서도 하지 않음. 사람이 HP 50% 이하로 직접 확인 필요 |
| D-08b 물괴물 보스 회귀 없음 | PASS (08-03) | `WaterMonster/Check.md` 결과 기록 참조(8항목 PASS, 텔레포트 1항목 수동 대기). 07-03 이 `CombatState.cs` 에 넣은 `PatternHistory`(기본 Enter 마다 새 이력)가 WaterMonster 동작을 바꾸지 않음을 확인 |