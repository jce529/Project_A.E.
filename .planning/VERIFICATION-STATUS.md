# 검증 상태 종합 (라이브/Play 모드 검증이 남은 항목)

이 문서는 여러 phase 폴더와 `Assets/` 아래에 흩어진 Check.md/UAT 파일들을 한눈에 보기 위한
색인이다. 각 phase의 상세 체크리스트는 여전히 각자의 `Assets/.../Check.md` 가 원본이며, 이 문서와
각 `.planning/phases/N-*/Check.md` 는 그 원본을 요약/링크한 것이다. 2026-09-10 기준.

## 요약 표

| Phase | 이름 | 코드 상태 | 정적 검사 | Play 모드 검증 | 우선순위 |
|---|---|---|---|---|---|
| 7 | 보스 공격 패턴 판단 로직 리팩토링 | 완료 | 완료 | **미착수** (보류) | 낮음 (8과 함께) |
| 8 | WaterMonster CombatState 마이그레이션 | 완료 | **미실행** | **미착수** (보류) | 낮음 |
| 9 | 카메라 줌/스테이지 전환 | 완료 | 완료 (7/7) | **일부 생략** (사용자 결정) | 선택 |
| 10 | 카메라 데드존 3종 | 완료 | 완료 (9/9, 2026-08-04) | **비보스 핵심 통과** (Gizmo/체감 제외) | 낮음 |
| 11 | 세이브/로드 매니저 | 완료 | 완료 (15/15, 11-01~03분) | **비보스 핵심 통과** (UI 일부 제외) | 낮음 |
| 12 | 피격 시 카메라 흔들림 | 완료 + BUG-005 수정 | 완료 (12/12, 2026-08-19) | **비보스 통과** (BUG-005 재검증 완료, 사망 피격만 수동 관찰 대기) | 낮음 |
| 14 | 세이브 슬롯 확장 | 완료 + MainMenu 배선 확인됨 | 완료 (13/13 기록) | **비보스 전부 통과** (3슬롯 독립성 + Load 버튼 전 구간) | 낮음 |

## 지금 당장 필요한 것

**없음 (비보스 범위 기준).** 2026-09-10 Unity MCP 실측에서 확인된 Phase 12 Task 3 gap(BUG-005,
`Time.timeScale=0`에도 랜덤 오프셋 적용)은 같은 날 `ApplyHitShake()`에 일시정지 프레임 가드를 추가해
수정했고, 1604프레임 위치 고정 / 재개 후 이어짐 / `shakeDuration=0` 가드 / Console 0건까지 재검증을
마쳤다. 상세는 `bug/BUG-005-camera-shake-continues-while-paused.md` 와 `Assets/Camera/Check.md`.

남은 것은 전부 수동 관찰(아래 보류 목록)이거나 보스 관련이라 사용자 요청으로 보류 상태다.
`bug/` 의 BUG-002(체력 상한 역전, 의도값 결정 필요) / BUG-004(MainMenuUI 중복 컴포넌트)는 사용자
판단이 필요한 항목이고, BUG-001은 2026-09-10 재조사에서 런타임 차단 요인이 아님이 확인됐다.
BUG-003은 보스(WaterMonster) 관련이라 보류 대상이다.

## 보류 중인 것 (사용자가 부를 때까지 진행 안 함)

- **Phase 7 + 8**: WaterSpirit / TutorialBoss / WaterMonster 3종 패턴 판단 로직을 한 번에 일괄
  검증하기로 사용자가 결정함. Phase 8 은 정적 회귀 검사조차 아직 실행 전이라 Play 모드보다 그것부터
  필요하다.
- **Phase 9**: 완료 처리는 됐으나 UAT 5항목이 `09-HUMAN-UAT.md` 에 `pending` 으로 남아 있음.
  필수는 아니고 권장 사항.
- **Phase 10**: X/Y 데드존, Dynamic Offset, Peeking, XY Bounds와 BoundsTrigger의 비보스 핵심은
  통과했다. Gizmo 표시, Inspector 체감 튜닝, 실제 키보드 입력, 씬 전환 구독은 수동 확인 대상이다.
- **Phase 11**: 슬롯 UI 실제 로드, 씬/스폰/스탯 복원, memory-only 새게임과 후속 체크포인트 기록을
  통과했다. 체력 UI와 파일 없음 ContextMenu 경고는 미확인이고, 보스 저장은 보류한다.
- **Phase 14**: 전체 빈 슬롯 UI/이어하기 비활성, 최저 빈 슬롯 자동 선택, 슬롯 1/2 독립 저장,
  데이터 카드 로드, 설정 독립 저장까지 통과했다. 2026-09-10 추가 실측으로 **원본 슬롯 0 카드 표시와
  실제 로드도 확인 완료** — Load 버튼의 씬 PersistentCall을 그대로 발화시켜 버튼 → 슬롯 패널 →
  `OnClickSlot(n)` → `LoadSlot(n)` → 씬 로드·스폰·체력 복원까지 전 구간 동작을 확인했다
  (`Assets/SaveSystem/Check.md`의 "Load 버튼 배선 실측" 참고). 남은 미확인은 체력 UI 표시뿐이다.

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
