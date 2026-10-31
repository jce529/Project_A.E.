# Phase 07 Plan 02: Play 모드 검증 Summary

**검증 일자:** 2026-10-31 · **방식:** Unity CLI(`eval_file`)로 에디터 Play 중 `EditorApplication.update` 샘플러를 걸어 `CombatState` 의 `LastUsedPatternType` / `_currentAttack` 를 리플렉션으로 읽어 변화 시점마다 기록 (Phase 13 정리로 `패턴 선택`/`체인 발동` 등 Debug.Log 는 코드에서 제거됨). 소스·씬 수정 없음.
**환경 보정:** 플레이어 체력 고정/중력 0 고정(`hold.cs`), `runInBackground=true`. 원본 로그는 공유 노트 폴더의 `p7_run1_pattern_log.txt`, `p7_run3_stage2_log.txt`, `p7_run2_stage2_loop_log.txt`.

## 판정

| 항목 | 판정 | 근거 |
|---|---|---|
| D-03 랜덤성 | **PASS** | Run 1(240초, 90회 실행) 시퀀스 `Charge FarProjectile FarProjectile FarProjectile Charge FarProjectile Charge ...`, Run 2 시퀀스 `FarProjectile Charge FarProjectile Charge FarProjectile Exhaustion WakeRepel Exhaustion WakeRepel Charge WakeRepel ...` — 고정 순환(Charge→Exhaustion→WakeRepel→FarProjectile)이 아니며 두 회차 순서가 다름 |
| D-05 연속금지 | **PASS (조건부)** | 같은 CombatState 세션 안에서 동일 패턴 연속 실행 0건(20회 전환 모두 다른 패턴). 단, 세션이 재진입되면 추적이 초기화되어 같은 패턴이 이어짐(아래 발견 1) |
| D-04 강제 체인 | **PARTIAL / FAIL(재진입 시)** | 같은 세션 안에서는 Exhaustion 직후 WakeRepel 9/9 (100%). 그러나 Exhaustion 16회 중 7회는 Exhaustion 직후 보스가 Chase→CombatState 로 재진입해 `LastUsedPatternType` 이 초기화되어 WakeRepel 이 아닌 다른 패턴(FarProjectile 5, Charge 1, Exhaustion 1)이 이어짐 |
| 후보 고갈 정지 | **PASS** | 보스가 `cur=-` 로 수 초간 멈춘 사례 없음 (공격 간 간격 ≈1.1~3초 일정) |
| D-07 Stage 2 사이클 | **NOT VERIFIED** | HP 55% 감소로 Stage 2 진입, 진짜 1 + 분신 2 = 3개 확인(`spirits=3`). 그러나 3회 → 헤비콤보 → 그로기 → 분신 재스폰까지는 관찰하지 못함: 일반 패턴 `cnt` 가 1 에서 멈춘 채 플레이어와 거리가 벌어지면(Exhaustion 후 d 가 2.6→4.3 로 서서히 증가, FarProjectile 후 d=12) Chase 를 거쳐 `Stage2CombatState` 가 재진입해 `cnt=0` 으로 초기화됨. 이 보정 환경(플레이어 고정)에서는 재현되나 실제 플레이에서의 빈도는 미확인 |
| D-08b 물괴물 회귀 | **NOT VERIFIED** | Phase 8 이후 일괄 검증 예정 (지시에 따름) |

## 관찰 로그 발췌 (Run 1, 변화 시점 기록: `시각 State|last=…|cur=…`)

```
28.38 SpiritCombatState|last=SpiritCharge|cur=SpiritCharge d=4.0
31.38 SpiritCombatState|last=SpiritExhaustion|cur=SpiritExhaustion d=3.1
33.88 ChaseState|last=-|cur=- d=2.0          <- Exhaustion 직후 재진입
33.89 SpiritCombatState|last=-|cur=- d=2.0   <- last 초기화
33.90 SpiritCombatState|last=SpiritFarProjectile|cur=SpiritFarProjectile d=12.0   (WakeRepel 아님)
36.42 SpiritCombatState|last=SpiritCharge|cur=SpiritCharge d=4.2
39.45 SpiritCombatState|last=SpiritCharge|cur=SpiritCharge d=5.9   <- 재진입 후 Charge 연속
42.45 SpiritCombatState|last=SpiritExhaustion|cur=SpiritExhaustion d=3.4
44.98 SpiritCombatState|last=SpiritFarProjectile|cur=SpiritFarProjectile d=12.0   (재진입 후, WakeRepel 아님)
```

전체 시퀀스(Run 1): Charge FarProjectile FarProjectile FarProjectile Charge FarProjectile Charge FarProjectile FarProjectile FarProjectile Charge Exhaustion FarProjectile Charge Charge Exhaustion FarProjectile FarProjectile Charge Charge Charge FarProjectile FarProjectile Exhaustion WakeRepel Charge FarProjectile Exhaustion WakeRepel FarProjectile Exhaustion WakeRepel FarProjectile Exhaustion WakeRepel ...

Stage 2 (Run 3) 발췌:
```
13.55 Stage2CombatState|cnt=1|last=SpiritExhaustion|cur=SpiritExhaustion|spirits=3|d=2.6
16.03 Stage2CombatState|cnt=1|last=SpiritExhaustion|cur=-|spirits=3|d=4.3
16.07 Stage2CombatState|cnt=0|last=-|cur=-|spirits=3|d=2.3          <- 재진입, cnt/last 초기화
17.25 Stage2CombatState|cnt=1|last=SpiritFarProjectile|cur=SpiritFarProjectile|d=12.0
19.78 Stage2CombatState|cnt=0|last=-|cur=-|spirits=3|d=2.3          <- 다시 초기화
```

## 발견 (코드 미수정, 기록만)

1. **`CombatState.Enter` 가 `LastUsedPatternType` 과 쿨다운을 매번 초기화한다** (`CombatState.cs:64-65`, 주석: "세션 단위 리셋"). SpiritCharge/FarProjectile 은 텔레포트하고 보스는 `dist > AttackRange+1` 이면 Chase 로 나갔다가 `new CombatState/SpiritCombatState` 로 재진입하므로, 전투 중 "세션"이 매우 짧다. 그 결과 (a) 재진입 경계에서 연속금지(D-05)가 무력화, (b) Exhaustion 이 재진입 직전이면 D-04 강제 체인이 소실. 로그상 Exhaustion 7/16 이 체인 소실.
2. Stage 2 도 같은 이유로 일반 패턴 카운터(`_patternsExecuted`)가 재진입 때 0 으로 돌아가 사이클이 진행되지 않을 수 있음(+ 재진입마다 `Enter` 에서 Clone 연출/분신 스폰 재실행). Phase 6 와의 동등성은 확인 못 함.
3. 위는 플레이어를 고정 위치에 둔 보정 환경에서의 관찰이다. 실제 플레이어 이동 시 빈도는 미확인.

## 후속 조치 제안

- `/gsd:plan-phase 7 --gaps`: `LastUsedPatternType` / 쿨다운을 `Enter` 에서 지우지 않고 보스(컨트롤러 또는 공유 컨텍스트)에 유지하거나, Chase 복귀 시 CombatState 인스턴스를 재사용하는 방안 검토. 이는 Phase 8(WaterMonster 마이그레이션)의 쿨다운 계약과 충돌할 수 있어 Phase 8 계획과 함께 판단 필요.
- D-07 은 플레이어를 보스에 붙여 두고(거리 ≤3.5 유지) 사람이 직접 Stage 2 한 사이클을 확인하는 수동 검증 권장.

## 환경 메모

- `save.json` sha256 이 기준(`7df6fe74…`)과 달라 `f480eeb8…` 이며 검증 후에도 동일. 자동저장이 쓴 것으로 보이며 파일은 건드리지 않음.
- Play 는 종료함(`editor_stop`). 씬 파일은 저장하지 않음. 커밋하지 않음.

## Self-Check

- 코드 변경 없음 (`git status` 는 `.agents/ .codex/ AGENTS.md` 미추적 항목만 — 이번 작업과 무관).
- 미관찰 항목(D-07, D-08b)은 NOT VERIFIED 로 기록.
