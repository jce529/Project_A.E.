# Phase 07 Plan 03: Play 모드 재검증 Summary

**검증 일자:** 2026-11-05 · **브랜치:** 주창은(HEAD, 병합 커밋 2067d3c 기준) · **코드 수정:** 없음 (07-03 구현은 이미 반영돼 있었고 검증만 수행)
**방식:** Unity CLI(`unity command eval_file`)로 에디터 Play 중 `EditorApplication.update` 샘플러를 걸어 `SpiritController.PatternHistory.LastUsed` 와 `CombatState._currentAttack`(리플렉션)을 읽고 변화 시점마다 기록. 보정 스크립트가 플레이어 HP를 매 프레임 최대로 복구하고, 중력 0·속도 0으로 고정하며, 보스와 5 유닛 넘게 벌어지면 보스 왼쪽 3 유닛으로 되돌린다(Chase 복귀가 자주 일어나는 07-02 환경 재현). `Application.runInBackground=true` 필요(없으면 프레임이 멈춤). 씬 `1 stage`, 240초 x 2회(실측 약 259초), 회차마다 Play를 새로 시작.
**원본 로그:** 공유 노트 폴더 `p7_03_run1_pattern_log.txt`, `p7_03_run2_pattern_log.txt`.

## 판정

| 항목 | 판정 | 근거 |
|---|---|---|
| D-04 Exhaustion→WakeRepel 강제 체인 | **PASS** | Run1 20/20, Run2 22/22 (100%). 이 중 39회는 Exhaustion 직후 Chase→새 SpiritCombatState 재진입을 거쳤는데도 `last=SpiritExhaustion` 이 유지되어 WakeRepel 이 이어짐. 07-02 기준선은 16회 중 7회 소실 |
| D-05 연속 동일 패턴 금지 | **PASS** | 두 회차 합 165회 실행에서 연속 동일 패턴 0건. CombatState 재진입은 Run1 72회, Run2 69회였고 경계 포함. 07-02 기준선은 세션 경계 연속 24건 |
| 후보 고갈 정지 | **PASS** | `cur=-` 로 5초 이상 멈춘 구간 없음. 쿨다운이 재진입 사이에도 유지됨에도 정지 증가 없음 |
| D-03 랜덤성 | **PASS (회귀 없음)** | 두 회차 시퀀스가 다름. Run1 `Exhaustion WakeRepel Charge FarProjectile Charge Exhaustion WakeRepel Charge FarProjectile Exhaustion ...`, Run2 `Charge FarProjectile Exhaustion WakeRepel Exhaustion WakeRepel Exhaustion WakeRepel Charge Exhaustion ...`. 실행 횟수 Run1: Charge 25 / Exhaustion 20 / WakeRepel 20 / FarProjectile 17, Run2: WakeRepel 23 / Exhaustion 22 / Charge 20 / FarProjectile 18 |
| D-07 Stage 2 사이클 | **NOT VERIFIED** | 07-03 plan 범위 밖(Stage2 `_patternsExecuted` 초기화·분신 재스폰은 후속). Stage 2 진입 자체를 측정하지 않았음 |
| D-08b 물괴물 회귀 | **NOT VERIFIED** | 이 세션 범위 밖. Phase 8 검증 세션(08-03)이 WaterMonster 실측으로 확인 예정. 기본 `AcquireHistory` 는 `new PatternHistory()` 라 동작이 같아야 함 |

참고: 위임받은 작업 설명에는 D-07 이 "CombatState 재진입 사이 패턴 이력 유지"로 적혀 있었으나, 07-03 PLAN 에서 그 항목은 D-04/D-05 로 표기되고 D-07 은 Stage 2 사이클이다. 이력 유지는 위 D-04·D-05 로 확인했고, D-07 은 plan 대로 NOT VERIFIED 로 둔다.

## 로그 발췌 (Run 2, 재진입 경계에서 이력 유지)

```
15.02 SpiritCombatState|last=SpiritExhaustion|cur=SpiritExhaustion
15.03 SpiritCombatState|last=SpiritExhaustion|cur=-
17.53 ChaseState|last=SpiritExhaustion|cur=-
19.93 SpiritCombatState|last=SpiritExhaustion|cur=-            <- 새 CombatState, last 유지
19.94 SpiritCombatState|last=SpiritWakeRepel|cur=SpiritWakeRepel   <- 강제 체인
21.44 ChaseState|last=SpiritWakeRepel|cur=-
22.08 SpiritCombatState|last=SpiritWakeRepel|cur=-
22.08 SpiritCombatState|last=SpiritExhaustion|cur=SpiritExhaustion  <- WakeRepel 직후 다른 패턴(연속 아님)
```

07-02 의 같은 상황은 `ChaseState|last=-` → `SpiritCombatState|last=-` 로 초기화되어 FarProjectile 이 이어졌다.

## 남은 이슈

1. D-07: Stage 2 Chase 재진입 시 카운터 초기화·분신 재스폰(07-02 발견 2)은 미해결, 수동 확인 또는 후속 plan 필요. `PatternHistory.Clear()` 는 `OnStage2Trigger` 에서만 호출되므로 Stage 2 직후 첫 패턴 동작은 별도 확인 필요.
2. 측정은 플레이어 고정 보정 환경이다. 실제 플레이에서의 이동 시 동작은 사람 확인 권장(사용자 검증 대기).
3. Phase 8 WaterMonster 회귀(D-08b)는 08-03 세션이 확인한다.

## 환경 메모

- Play 종료함(`editor_stop`), 씬 저장 안 함. 씬 파일의 기존 변경(Phase 22 작업)은 건드리지 않음.
- 작업 폴더 `.planning/STATE.md` 가 병합 충돌(UU) 상태로 남아 있다. 이 세션은 수정하지 않음(총괄 담당).

## Self-Check

- 코드 변경 없음. 판정은 모두 측정값 기반이며 미측정 항목은 NOT VERIFIED.
