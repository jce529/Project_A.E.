---
phase: 08-watermonster-combatstate
plan: 03
subsystem: boss-ai
tags: [verification, play-mode, combatstate, watermonster, waterspirit, tutorialboss]
requires: [08-01, 08-02]
requirements-completed: [D-01a, D-01c, D-03a, D-04a, D-04b, D-06a, D-06b, D-06c]
requirements-partial: [D-05a]
completed: 2026-11-05
---

# Phase 08 Plan 03: 3종 보스 일괄 Play 모드 검증 Summary

**WaterMonster 는 Unity CLI Play 모드 실측으로 9개 항목 중 8개 PASS, D-05 텔레포트 1개는 수동 확인 대기. WaterSpirit 은 phase-07-verify(07-03) 실측 결과를 참조해 D-07 Stage 2 를 제외하고 PASS. TutorialBoss 는 `CombatState` 를 상속하지 않아 2026-09-29 기록을 인용했다. 코드 변경 없음.**

## 방법
- Task 1(정적 회귀 12/12 PASS, 2026-10-31)은 이전에 완료됐다.
- Task 2 는 Unity CLI `eval` 로 수행했다. WaterMonster 는 어느 씬에도 배치돼 있지 않아 빈 씬에 `WaterMonster.prefab` + `Player.prefab` + 바닥을 런타임에 생성했고, 에디터 샘플러가 상태/현재 공격/거리/페이즈·광폭화 플래그/HP 를 변화 시점마다 기록했다. 플레이어는 고정 거리(2.0 / 3.3)에 두고 HP 를 회복시켰다.
- 소스에 `패턴 선택:` 로그가 없어(체크리스트 문구와 다름) 샘플러의 `atk=` 전환을 패턴 선택으로 취급했다.
- 같은 에디터를 쓰는 phase-07-verify 와 순서를 조율해(07 먼저, "에디터 비었음" 수신 후 08-03) 충돌 없이 진행했다. 끝난 뒤 Play 를 종료하고 원래 씬(`1 stage`, dirty 아님)으로 복구했다.

## 결과
| 보스 | 항목 | 판정 |
|---|---|---|
| WaterMonster | D-02 랜덤성, D-01 감쇠, D-04 45초 잠금, D-03/D-04b 광폭화 배율, D-06a 거리 분기, D-06b/c 페이즈 전환, 후보 고갈 스팸 없음, WaterSpirit 회귀 없음 | PASS (8) |
| WaterMonster | D-05 사전 가드(장판 스폰/텔레포트) | **수동 확인 대기** |
| WaterSpirit | D-03, D-05, D-04, 후보 고갈, D-08b | PASS (5, 앞 4개는 07-03 참조) |
| WaterSpirit | D-07 Stage 2 사이클 | **미검증** |
| TutorialBoss | 전 항목 | PASS (2026-09-29 기록 인용, 재실측 안 함) |

핵심 증거(상세는 각 `Check.md`와 노트 폴더 로그):
- WavePush 비광폭화 간격 46.0~81.5초(최소 ≥45), 광폭화 간격 22.50 / 22.51 / 22.50 / 22.50초.
- 근접 30회 중 RangedSpit 0, 원거리 103회 중 MeleeSwipe/JumpLand 0.
- P2 전환(t=46.76) 후 PrisonAttack 0 → 51.24 첫 PrisonMapAoe, P3 전환(t=83.58) 후 MapAoe 0 → 96.87 ColorPrison.
- 같은 패턴 연속 12~14%(0 아님, 금지가 아니라 감쇠).
- WaterSpirit(07-03): 165회 패턴 실행 연속 동일 0건, 강제 체인 42/42.
- 로그 원문: 총괄 노트 폴더 `p8_03_run{1,2,3}_*_log.txt`, `p7_03_run{1,2}_pattern_log.txt`.

## 남은 이슈
1. **D-05 텔레포트/장판 (수동 확인 대기)** — 실제 파괴 불가 웅덩이 2개 이상 상황이 필요하다. 임시 `PuddleStackManager` 로 카운트만 2 로 강제하면 보스가 패턴 선택을 멈추고 `WaterMonsterCombatState` 에 머물렀다(가드는 작동, `WaterTeleportState` 정상 이동은 미확인). 광폭화 장판 스폰도 ColorPrison 존과 구분되지 않아 판정하지 못했다.
2. **D-07 WaterSpirit Stage 2 사이클** — 07-03 범위 밖이라 아무도 측정하지 않았다. 사람이 HP 50% 이하로 확인 필요.
3. **WavePush 잠금이 CombatState 재진입 시 초기화됨(관찰)** — Chase 를 거쳐 3.6초 만에 재사용된 사례. 07-03 `PatternHistory` 기본(Enter 마다 새 이력) 때문이며, 08-02 이전 `_lastWaveTime` 도 상태 인스턴스 필드였던 것으로 보여 회귀는 아닐 가능성이 높다. 이전 빌드와 직접 비교는 하지 않았다. 45초 잠금을 재진입에도 유지하려면 WaterMonster 가 `AcquireHistory` 를 override 해야 한다(D-04a 해석 문제, 별도 결정 필요).
4. **WaterColorPrison 존 잔존 — 확인 완료(2026-11-05): 정상 경로 누수 없음, 예외/비활성화 경로는 구조적으로 샐 수 있음.**
   - 코드: 정상 경로에서는 `ColorPrisonSequence` 가 3.5초 뒤 `spawnedZones` 를 `Destroy` 하고 `ActiveZones.Clear()` 한다. 정리 코드에 `try/finally` 가 없다.
   - 136개의 원인: 처음 테스트 씬에 `CameraController` 가 없어 `PlayerStats.TakeDamage` 끝의 `CameraController.Instance.Shake()`(PlayerStats.cs:75)가 NullReference 를 던졌고, 코루틴이 `Destroy` 전에 중단됐다(테스트 씬 특유). 앞서 "PlayerStats 초기화/UI 참조"라고 적은 추정은 틀렸고, 정확한 원인은 `CameraController` 부재다.
   - 재측정: `CameraController` 를 추가하고 Player 프리팹(체력 100만으로 고정, 존 밖 배치라 80 데미지 경로 통과 추정)으로 `WaterColorPrison` 을 4초 간격 24회 시전 → 시전 중 4개(`ColorPrisonZoneCount`=4), 종료 후 **0개**. 즉 실제 보스 씬에서 정상 진행 중 누적 누수는 없다. 직접 호출(`ExecuteAttack`)이라 광폭화 자연 시전과 타이밍만 다르다.
   - 실제로 새는 경로: 시전 1.5초 뒤 보스 GameObject 를 `SetActive(false)` 했더니 존 4개가 6초 뒤에도 잔존(코루틴이 보스 MonoBehaviour 에서 돌아 비활성화 시 중단). `WaterMonsterStats.Die()` 가 `SetActive(false)` 를 쓰므로 3.5초 시전 창 안에서 보스가 처치되면 씬 전환 전까지 존이 남는다. 사용자가 본 "너무 많다"는 이 경로 또는 예외 경로일 수 있으나 실제 보스 씬에서는 재현하지 못했다.
   - 수정 후보(코드는 수정하지 않음, 사용자 결정): (a) `ColorPrisonSequence` 정리를 `try/finally` 로 감싸기 — 예외 경로 방지, 비활성화에는 무효 (b) 존 정리를 `WaterMonsterController.OnDisable` 에서 처리하거나 존 자체에 수명(`Destroy(go, 3.5f+α)`)을 줘서 코루틴과 분리 — 두 경로 모두 방지. 범위는 `WaterColorPrison.cs`(+선택적으로 `WaterColorPrisonZone.cs`) 한 파일 내외로 08-03 범위 밖.
5. 광폭화 중 MeleeSwipe 가 50% 를 차지(자가 쿨다운 0.7초로 가장 짧음). 08-02 Pitfall 6 의 결과이며 밸런스 판단은 사람 몫.
6. 측정 중 자동저장이 `%LocalLow%/DefaultCompany/AQUA ECLIPSE/save.json` 에 세이브를 썼다.

## 완료 기준 대비
- 세 `Check.md` 결과 기록과 이 SUMMARY 는 채웠고 항목마다 판정과 증거가 있다.
- 단, 계획의 자동 검증(`grep -c '^- \[ \]' == 0`)은 **통과하지 않는다**: WaterMonster D-05 와 WaterSpirit D-07 이 의도적으로 `[ ]` 로 남아 있다(수동 대기/미검증을 통과로 처리하지 않기 위해).
- STATE.md / ROADMAP.md 는 수정하지 않았다(총괄이 반영).
