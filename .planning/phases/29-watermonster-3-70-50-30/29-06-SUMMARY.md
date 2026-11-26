---
phase: 29-watermonster-3-70-50-30
plan: 06
subsystem: WaterMonster verification (TestScene, Unity CLI)
requirements-completed: [D-01, D-02, D-03, D-04, D-05, D-06, D-07, D-08, D-09, D-10, D-11, D-12, D-13]
files:
  modified:
    - Assets/Enemy/WaterMonster/Check.md
    - docs/ssot/systems/boss-combat.md
  created:
    - .planning/phases/29-watermonster-3-70-50-30/29-06-SUMMARY.md
tasks-executed: [1, 2, 3]
tasks-pending: [4]   # checkpoint:human-verify — OD-1~OD-9 기본값 승인 완료(2026-11-05, 040b4e9). 남은 것: V1~V6 시각 확인, 재활성화 gap 처리 결정, TestScene 저장/폐기
verify:
  task1: T1_OK
  task2: T2_OK
  task3: T3_OK
  task4: 미실행(사용자 확인 대기)
static: {S1: PASS, S2: PASS, S3: PASS, S4: PASS, S5: PASS}
play: {P1: PASS, P2: PASS, P3: PASS, P4: PASS, P5: PASS, P6: PASS, P7: PASS, P8: PASS, P9: PASS, P10: PASS, P11: PASS, P12: PASS, P13: PASS, P14: PASS, P15: PASS, P16: PASS, P17: PASS, P18: PASS, P19: PASS, P20: PASS, P21: PASS, P22: PASS}
fails: []
code-changes: 없음(.cs/.prefab/.unity/.controller 0건)
---

# 29-06 실행 요약 — Phase 29 실측 검증 (Task 1~3)

## 결과
- Unity 컴파일: `scriptCompilationFailed=False`, Editor Console `error CS` 0건, 신규 타입 로드 확인.
- 정적 S1~S5 전부 PASS (Spirit 계열 CombatState 소스 무변경 — Spirit 회귀는 S1 로 판정, 재실측 안 함).
- Play 실측 P1~P22 전부 PASS. 항목별 수치·로그 파일명은 `Assets/Enemy/WaterMonster/Check.md` "Phase 29" 섹션.
- D 대응: D-01 P3/P9/P10, D-02 P1/P2/P4, D-03 P1/P4/P5, D-04 S2/P8/P10, D-05 P3/P9, D-06 P2/P3/P6/P7/P13/P14, D-07 P16, D-08 P17, D-09 P18, D-10 P19, D-11 P20, D-12 P11/P21, D-13 P22 — 모두 PASS.
- SSOT `docs/ssot/systems/boss-combat.md`: 70/50/30 페이즈, 전환 전 그로기(`WaterMonsterGroggyState`), 누적 후보 표, 단일/겹침(상한 2), `WaterMonsterPuddleSystem.Ensure/Bind/Unbind`, 트리거 12종(`Attack_PrisonAoe` 포함), 비활성 기능(파일 유지), 회복 규칙 미변경, 근거 줄 번호 현행화.

## 방법
- `Assets/Scenes/TestScene.unity` Play 후 `WaterMonster.prefab` 런타임 Instantiate(플레이어 오른쪽 2칸). `unity command run_script` 로 PlayerLoop `PreLateUpdate` 에 샘플러·시험 실행기를 삽입(매 프레임, 코루틴 실행 직후), 값 변화 시점만 `.planning/tmp/p23/P<n>.log` 기록, 판정 요약 `results.log`. 스크립트 원본 `.planning/tmp/p23/p23lib.cs.txt`(검증 명령의 `*.cs` 필터에 걸리지 않도록 확장자 변경).
- 플레이어 HP 1,000,000(리플렉션), 피해는 감소량으로 측정. 위치는 매 프레임 고정(pin). 강제 패턴은 보스 `IdleState`(VisionRange -1) 고정 후 `new <전략>().ExecuteAttack(boss)`.

## 측정 중 수정한 것(측정 스크립트만)
- P22 1차: 판정 코드가 생성 5초 뒤 이동한 파도 위치를 읽어 NG → 생성 시점 x 로 고쳐 재측정 PASS(로그 값은 두 실행 동일, 제품 코드 문제 아님).
- P4 1차: 다른 사용자가 같은 Play 세션에서 조작해 보스가 피해(20씩)를 받아 3페이즈로 넘어감 → 무효(`P4_contaminated_run1.log`), 재측정 PASS.

## 관찰(판정 외, gap 후보)
- 2·3페이즈 보스를 `SetActive(false→true)` 하면 `BoundBoss` 는 복구되지만 웅덩이 시스템 `IsActive=False`·웅덩이 0개로 남아 웅덩이 패턴 후보를 잃는다(`Bind` 가 `IsActive=false` 로 초기화, `PhaseEntered` 재발생 없음). 추정 수정 위치: `WaterMonsterPuddleSystem.Bind`(보스 `CurrentPhase>=2` 면 재활성) 또는 `WaterMonsterController.OnEnable`.
- 강제 실행 패턴은 ReadyAt 을 남기지 않아 같은 패턴이 곧바로 다시 선택될 수 있다(검증 방식 특성).
- `WaterMonster.controller` 에는 새 트리거 12종이 없다(아트 대기). `TrySetTrigger` 가 건너뛰므로 경고는 없음.

## 환경 메모
- 시작 상태(11:05Z): Play 꺼짐, `Tutorial Map.unity` 열림·dirty 아님, 컴파일 정상. Input System 장치는 Phase 21 가상 장치(P21VirtualKeyboard/Mouse 8개)뿐이었음.
- 11:16Z 경 Play 세션이 외부에서 재시작됐고, 11:21Z 사용자가 에디터를 재시작(PID 22268→35612, TestScene 이 열린 채). 재시작 후 Input System 정상(Keyboard, Mouse 2개).
- 11:23~11:30Z 사용자가 같은 에디터에서 Play 를 조작함. 그때 "플레이어가 움직이지 않음"은 이 plan 의 위치 고정 훅(p23 Play 세션 안에서만 동작) 때문이었고, Play 종료·도메인 리로드로 사라졌다. 재개 전 확인: 훅 0개, AppDomain 데이터 없음, PlayerController 활성, PrisonDebuff 없음.
- 측정 종료 후 훅 해제(`Uninstall`), `runInBackground=false`, Play 종료.
- **씬 복구 미완**: 끝난 시점 TestScene 이 열려 있고 `isDirty=True` 다. 이 dirty 는 p23 측정 전(11:26Z, 사용자 Play 조작 직후)부터 있던 것으로 이 plan 이 만든 변경이 아니다. `Tutorial Map.unity` 를 다시 열면 그 미저장 변경이 버려지므로 저장도 폐기도 하지 않고 그대로 두었다 — 사용자가 TestScene 변경을 저장/폐기할지 정한 뒤 `Tutorial Map.unity` 를 열어야 한다.
- 자동저장이 `%LocalLow%/DefaultCompany/AQUA ECLIPSE/save.json` 에 기록됐다(측정 중 여러 번). 측정 보스는 사망하지 않아 `WaterMonster` 처치 기록은 남지 않았다.
- 이 plan 과 무관한 미커밋 변경(Spirit, Phase 21 `InputSystem_Actions.inputactions`, `Assets/Player/Interaction.md`, 씬, TextMesh Pro)은 건드리지 않았다.

## 다음
- Task 4(checkpoint:human-verify): **OD-1~OD-9 는 2026-11-05 기본값 승인 완료**(Check.md `### Open Decisions 결과`, 040b4e9). 남은 것은 (1) V1~V6 시각 확인 결과를 받아 Check.md V 줄 갱신, (2) 위 "관찰" 의 재활성화 gap 을 별도 plan 으로 만들지 결정, (3) TestScene 저장/폐기 후 `Tutorial Map.unity` 복구.
