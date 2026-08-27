# 검증 상태 종합 (라이브/Play 모드 검증이 남은 항목)

이 문서는 여러 phase 폴더와 `Assets/` 아래에 흩어진 Check.md/UAT 파일들을 한눈에 보기 위한
색인이다. 각 phase의 상세 체크리스트는 여전히 각자의 `Assets/.../Check.md` 가 원본이며, 이 문서와
각 `.planning/phases/N-*/Check.md` 는 그 원본을 요약/링크한 것이다. 2026-08-19 기준.

## 요약 표

| Phase | 이름 | 코드 상태 | 정적 검사 | Play 모드 검증 | 우선순위 |
|---|---|---|---|---|---|
| 7 | 보스 공격 패턴 판단 로직 리팩토링 | 완료 | 완료 | **미착수** (보류) | 낮음 (8과 함께) |
| 8 | WaterMonster CombatState 마이그레이션 | 완료 | **미실행** | **미착수** (보류) | 낮음 |
| 9 | 카메라 줌/스테이지 전환 | 완료 | 완료 (7/7) | **일부 생략** (사용자 결정) | 선택 |
| 10 | 카메라 데드존 3종 | 완료 | 완료 (9/9, 2026-08-04) | **미착수** (보류, 41항목) | 낮음 |
| 11 | 세이브/로드 매니저 | 완료 (ContextMenu 훅 제외) | 완료 (15/15, 11-01~03분) | **미착수** (보류) | 낮음 |
| 12 | 피격 시 카메라 흔들림 | 완료 | 완료 (12/12, 2026-08-19) | **응답 대기 중** | **활성 — 지금 진행 중** |

## 지금 당장 필요한 것

**Phase 12 Task 3** — 유일하게 활성 상태인 항목. `Assets/Camera/Check.md` Phase 12 섹션(6개
소섹션)을 Unity Play 모드에서 확인하고 결과를 알려주면 phase가 마무리된다.

## 보류 중인 것 (사용자가 부를 때까지 진행 안 함)

- **Phase 7 + 8**: WaterSpirit / TutorialBoss / WaterMonster 3종 패턴 판단 로직을 한 번에 일괄
  검증하기로 사용자가 결정함. Phase 8 은 정적 회귀 검사조차 아직 실행 전이라 Play 모드보다 그것부터
  필요하다.
- **Phase 9**: 완료 처리는 됐으나 UAT 5항목이 `09-HUMAN-UAT.md` 에 `pending` 으로 남아 있음.
  필수는 아니고 권장 사항.
- **Phase 10**: 정적 검사는 이미 통과했지만 Play 모드 체크리스트(41항목, 5개 소섹션)가 전부
  미체크. 코드 자체는 안정적으로 완료된 상태.
- **Phase 11**: `SaveLoadManager.cs` 에 ContextMenu 검증 훅을 먼저 추가해야 Play 모드 검증이
  가능하다 (11-04-PLAN.md 범위).

## 참고 — 실제로는 낡은 문서 (재검증 불필요)

`Assets/Camera/Check.md` 의 quick task `260805-m41` / `260805-q2u` 섹션은 체크박스가 미체크로
남아 있지만, 그 구현 자체가 이후 `260809-h9k` 에서 폐기·대체됐고 `260809-h9k` 는 사용자와 함께
Play 모드 검증을 이미 마쳤다 (STATE.md 참고). 혼동해서 이 오래된 섹션을 다시 검증할 필요 없다.

## 각 phase 상세 문서

- `.planning/phases/07-boss-attack-pattern-judgment/Check.md`
- `.planning/phases/08-watermonster-combatstate/Check.md`
- `.planning/phases/09-camera-zoom-stage-transition/Check.md`
- `.planning/phases/10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller/Check.md`
- `.planning/phases/11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json/Check.md`
- `.planning/phases/12-camera-shake-on-hit/Check.md`

## 실제 체크리스트 원본 (Assets/ 아래, 코드와 함께 관리)

- `Assets/Camera/Check.md` — Phase 9 / 10 / 12 + quick task 3종
- `Assets/Enemy/WaterSpirit/Check.md` — Phase 7
- `Assets/Enemy/Tutorial/TutorialBoss/Check.md` — Phase 7
- `Assets/Enemy/WaterMonster/Check.md` — Phase 8
- `Assets/SaveSystem/Check.md` — Phase 11
