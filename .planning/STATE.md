---
gsd_state_version: 1.0
milestone: v2.0
milestone_name: milestone
status: executing
last_updated: "2026-09-21T00:35:26.860Z"
last_activity: "2026-09-21 - merged 주창은 (Phase 14 slot wiring, Phase 15 완료 처리, Phase 16 interval-autosave COMPLETE, BUG-009 fix, Phase 999.1 backlog) into Item after renumbering Item's Phase 16-18 -> 17-19"
progress:
  total_phases: 19
  completed_phases: 11
  total_plans: 47
  completed_plans: 41
  percent: 72
---

# GSD State

## Current Milestone

**물의_정령_보스_구현 (v2.0)**

## Current Position

Phase: 17
Phase 17 (iitem-itemdata-scriptableobject-id-useeffect-ui) — 17-01 COMPLETE. 17-02 Task 1(Unity 6000.3.10f1 배치모드 임포트 — 컴파일 에러 0/Item 임포트 에러 0/missing script 0, Assets/Item/ 무변경 커밋 없음) + Task 2(Assets/Item/Check.md 작성, commit a47a39a) 완료, Task 3(Play 모드 실측 체크포인트) 사용자 응답 대기 중
Phase 12 (camera-shake-on-hit) — EXECUTING (Task 3 Unity MCP 비보스 실측 통과, BUG-005 수정·재검증 완료, 사망 전환 마지막 피격만 수동 관찰 대기)
Phase 13 (codebase-cleanup-audit) — COMPLETE (5/5 plans, 보고서 전용, Assets 0줄 변경) + 후속 정리 라운드 COMPLETE (D-07/D-08 고위험 포함 전량 처리, 2026-08-20)
Phase 14 (save-slot-expansion) — 14-01/02 완료(슬롯화 API + 슬롯 UI), 14-03(MainMenu 배선/정적 회귀/Play 검증)은 비보스 핵심 통과, UI 일부·보스 회귀만 대기 (주창은 브랜치에서 구현, 병합으로 반영)
Phase 15 (load-timing-and-load-scope) — 15-01/02/03 구현 완료, 15-04(Play 모드 검증)는 미종결(2026-09-19 확인: ROADMAP.md 헤딩 누락 상태로 방치돼 있었음, 백필함). 15-UAT.md `status: partial`, BUG-007(씬 전환 후 InputHandler 유실, major) 미해결.
Phase 16 (interval-autosave) — COMPLETE (3/3 plans, Play 모드 29/29 PASS, 주창은 브랜치에서 구현, 병합으로 반영)
Plan: Not started (Phase 17 기준)
Status: Executing Phase 17; Phase 12/14/15는 각자 별도 체크포인트 대기 중
**워킹트리 리스크**: 미커밋 삭제 48건(`Assets/Player/Script/AttackBox.cs`, `Assets/Script/TakeDmg.cs`, `Assets/Enemy/Monster_Alpha/Script/EnemyBrain.cs` 등)은 2026-09-08 커밋 `a9c427f`("feat: update save slots and main menu, remove unused scripts, add system docs")로 이미 정식 반영되어 해소됨 — 두 브랜치의 공통 조상에 포함된 상태였다. `.dead-code-backup/pre-cleanup-20260903-1430.zip`은 git 히스토리에 커밋된 적이 없어 이 저장소 상태와 무관. 루트 `*.sh` 스크립트 5종(2026-08-20자)과 `.agy/settings.json`(2026-08-27자)은 여전히 untracked — 용도 미확인.
Last activity: "2026-09-21 - Item 브랜치의 Phase 16-18을 17-19로 재번호(주창은의 Phase 16 interval-autosave와 번호 충돌 방지) 후 주창은 병합"

Progress: [████████░░] 72% (Item Phase 17 + 주창은 Phase 14/16 반영 병합, 정확한 plan 합산은 다음 상태 갱신에서 재계산 필요)

## Phase Status

| Phase | Name | Status | Completed |
|-------|------|--------|-----------|
| 5 | 보스 기반 엔티티 및 스테이지 1 공격 패턴 | Complete | 2026-04-30 |
| 6 | 스테이지 전환 및 스테이지 2 은신·분신 시스템 | Complete | 2026-04-30 |
| 7 | 보스 공격 패턴 판단 로직 리팩토링 (1/2 plans) | In Progress (07-02 Play 모드 검증 체크포인트 대기) | - |
| 8 | WaterMonster 보스 CombatState 마이그레이션 (2/3 plans) | In Progress (08-03 정적 회귀+Play 모드 일괄 검증 대기) | - |
| 9 | 일반/보스 스테이지 카메라 줌 변화 | Complete (Play 모드 실측 미검증, UAT 보류) | 2026-07-30 |
| 10 | 카메라 데드존 3종 (Base/Dynamic Asymmetrical/Peeking) (3/4 plans) | In Progress (비보스 핵심 실측 통과, 수동 시각 확인·보스 항목 대기) | - |
| 11 | Newtonsoft.Json 세이브/로드 매니저 (3/4 plans) | In Progress (비보스 I/O·로드·새게임 실측 통과, UI 일부·보스 항목 대기) | - |
| 12 | 피격 시 카메라 흔들림 (0/1 plans) | In Progress (Task 3 비보스 실측 통과, BUG-005 수정·재검증 완료, 사망 피격·보스 항목 대기) | - |
| 13 | 코드베이스 정리 감사 (프로젝트 폴더 전수 스캔) | Complete (5/5 plans + 후속 정리 라운드 D-07/D-08 전량 실행 완료, D-09/D-10은 백로그) | 2026-08-20 |
| 14 | 세이브 슬롯 확장 (슬롯 3개, 슬롯별 진행도) (2/3 plans) | In Progress (비보스 전부 통과 — 3슬롯 독립성 + Load 버튼 전 구간 실측, 체력 UI·보스 회귀만 대기) | - |
| 15 | 로드 시점 및 로드 범위 정의 (3/4 plans) | In Progress (15-04 정적 회귀+Play 모드 검증 대기, BUG-007 미해결) | - |
| 16 | 일정 간격 자동저장 (3/3 plans) | Complete (정적 회귀 22/22 대체 PASS, Play 모드 29/29 PASS, Assets·ProjectSettings 0줄 변경) | 2026-09-14 |
| 17 | 아이템 코어: IItem + ItemData ScriptableObject (1/2 plans) | In Progress (17-02 Task 1~2 완료, Task 3 Play 모드 체크포인트 사용자 응답 대기) | - |

## Performance Metrics

| Phase-Plan | Duration | Tasks | Files | Date |
|------------|----------|-------|-------|------|
| 05-01 | - | 2 | 3 | 2026-04-30 |
| 05-02 | - | 2 | 4 | 2026-04-30 |
| Phase 10 P01 | 6min | 3 tasks | 1 files |
| Phase 10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller P02 | 6min | 2 tasks | 1 files |
| Phase 10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller P03 | 10min | 2 tasks | 1 files |
| Phase 11 P01 | 15min | 3 tasks | 3 files |
| Phase 11 P02 | 8min | 2 tasks | 1 files |
| Phase 11 P03 | 15min | 3 tasks | 5 files |
| Phase 15 P01 | 6min | 3 tasks | 2 files |
| Phase 15 P02 | 17min | 2 tasks | 1 files |
| Phase 17 P01 | 25min | 2 tasks | 9 files |

## Performance Metrics

| Phase-Plan | Duration | Tasks | Files | Date |
|------------|----------|-------|-------|------|
| 07-01 | 5min | 2 | 2 | 2026-07-27 |
| 08-01 | 15min | 2 | 1 | 2026-07-29 |
| 08-02 | 20min | 2 | 2 | 2026-07-29 |
| 09-01 | 5min | 2 | 1 | 2026-07-30 |
| 09-02 | 5min | 2 | 2 | 2026-07-30 |
| 09-03 | 5min | 1/2 | 1 | 2026-07-30 |
| quick-260805-m41 | ~20min | 3 | 3 | 2026-08-05 |

## Accumulated Context

### Key Decisions

- Every actionable load failure returns through `AbortLoadToMainMenu` and the centralized `MainMenuSceneName` constant; both synchronous and coroutine failures therefore have a visible outcome (Phase 15 Plan 2).
- Load failure recovery never writes or deletes the existing save file, preserving it for manual recovery (Phase 15 Plan 2).
- The no-save `LoadGame` guard remains a warning and return because no save is a valid new-game and death-restart condition handled by Phase 15 Plan 1 (Phase 15 Plan 2).
- PlayerStats owns player-death recovery by overriding HP.Die without calling the base implementation; a save reloads, while no save restarts the active scene (Phase 15 Plan 1).
- Restored health treats maxTotalHealth as the anchor and clamps maxHealth then health with a floor of 1 before assignment (Phase 15 Plan 1).
- 물의 정령은 WaterMonsterController와 별도 독립 엔티티로 구현 (`SpiritController : BossController`)
- 분신은 별도 GameObject로, 동일 상태머신 구조에 `isDummy` 플래그로 데미지 분기
- 코드 공유는 BossController / BossStatsSystem 기반 클래스 수준으로만 제한
- 애니메이션·이펙트 없이 순수 로직·상태머신만 구현 (v3.0+에서 연동 예정)
- SpiritCombatState 의 고정 라운드로빈 배열을 CombatState 범용 PatternCandidate 헬퍼 기반 조건부 가중치 랜덤으로 교체 (Phase 7 Plan 1)
- SpiritController.ChargeRange 는 기존 데드 필드로 재활용하지 않고 유지 (재활용 시 SpiritFarProjectile 영구 선택 불가 발생)
- 코드 공유는 BossController / BossStatsSystem 기반 클래스 수준으로만 제한
- CombatState.SelectWeightedPattern 에 별도 오버로드 대신 기본값 있는 3번째 파라미터(lastUsedWeightMultiplier = 0f) 추가 — SpiritCombatState.cs 의 기존 2-인자 호출부가 한 글자도 안 바뀌어야 하므로(Phase 7 D-05a 완전배제 회귀 방지) (Phase 8 Plan 1)
- BuildCandidates() 를 Enter() 가 아니라 SelectAttackStrategy 매 호출마다 재구성 — 페이즈가 전투 도중 바뀌는 WaterMonster 는 SpiritCombatState 의 Enter()-1회-캐싱 패턴을 복사할 수 없다 (Phase 8 Plan 2, D-06c)
- WaterWavePush 의 45초 특수 잠금은 PatternCandidate.cooldownOverride 로 전달 — strategy.Cooldown(3f) 에 의존하면 잠금이 조용히 3초로 축소되는 회귀가 된다 (Phase 8 Plan 2, D-04a)
- CameraController.cs 의 신규 삽입 주석에서 "DontDestroyOnLoad" 리터럴 문자열을 피하고 "Not persisted across scene loads"로 대체 — 09-01-PLAN.md 자체의 액션 텍스트(해당 문자열을 포함한 주석 지정)와 인수 기준(같은 문자열 카운트 0 요구)이 상충했기 때문 (Phase 9 Plan 1)
- BossZoomTrigger 는 필드 0개 유지 - 줌 값은 CameraController Inspector 소유(D-04/D-05), 트리거는 어느 보스 구역에나 드롭 가능해야 함(D-02) (Phase 9 Plan 2)
- Phase 9 Plan 3 Task 2 (Unity Play 모드 실측 검증)는 사용자가 명시적으로 생략하기로 결정 — Check.md 에 PASS 로 허위 기록하지 않고 "검증 생략" 상태와 미체크 항목 그대로 남김 (Phase 9 Plan 3)
- CameraController.cs Task 1 삽입 주석에서 "deadzoneHeight" 리터럴 문자열을 피하고 "the height field below"로 대체 — Task 3 검증 게이트(deadzoneHeight 카운트==2)와 상충했기 때문, Phase 9 Plan 1의 DontDestroyOnLoad 사례와 동일 패턴 (Phase 10 Plan 1)
- CameraController.cs 카메라 X 합성은 `_deadzoneCenterX - _currentBoxOffsetX` 이고 오프셋은 `-(pushDir * maxOffsetDistance)` (잠금 가정 A2) — 오른쪽으로 달리면 카메라가 오른쪽으로 앞서 나가 진행 방향 시야가 열린다 (Phase 10 Plan 2)
- Phase 10 Plan 2 Task 2 의 `git diff ef6f164` 삭제 라인 수 게이트(==2)는 baseline 커밋 선택 오류로 문자 그대로는 항상 0 이 나온다 — Plan 10-01 이 순수 삽입 diff 였으므로 그 위에서 다시 수정되는 라인은 ef6f164 기준으로는 애초에 없던 라인의 일부로 뭉쳐 보임. 대신 직전 커밋(717e37f) 기준 `git diff HEAD` 로 검증해 정확히 2줄(둘 다 ASCII)임을 확인 — Phase 9 Plan 1의 DontDestroyOnLoad, Phase 10 Plan 1의 deadzoneHeight 사례와 같은 계열의 "계획 자체 검증 스크립트 오류" 패턴 (Phase 10 Plan 2)
- Phase 10 Plan 3 의 두 태스크 모두 `git diff ef6f164` 삭제 라인 게이트가 0 을 반환 (Task 1 기대값 2, Task 2 기대값 3) — 같은 baseline 커밋 선택 오류 계열. 대신 직전 커밋(Task 1: 434a3e0, Task 2: 5d5b55e) 기준으로 검증해 각각 0줄/1줄(전부 ASCII)임을 확인, 플랜 자체 서술("이 플랜에서 1줄을 수정")과 일치 (Phase 10 Plan 3)
- CameraController.cs peekCancelSpeed 기본값 12 는 PlayerController.runSpeed(7)와 dashSpeed(20) 사이에 위치 — 평상시 달리기는 피킹을 취소하지 않고 대시/피격만 취소하도록, isDashing/isKnockedBack 에 public 접근자를 추가하지 않고 이동량 급증 프록시만으로 구분 (D-11) (Phase 10 Plan 3)
- gsd-tools.cjs 의 `state update-progress` 명령은 대소문자 무시 정규식이 STATE.md 본문의 "Progress:" 필드보다 frontmatter YAML 의 "progress:" 키를 먼저 매치하고 `\s*` 가 개행까지 삼켜버려, 본문 Progress 줄이 갱신되지 않는 기존 버그를 발견 (frontmatter 는 재구성 로직이 디스크에서 다시 계산하므로 자체 치유되지만 본문 줄은 그대로 남음) — 공용 도구 스크립트라 이 플랜 범위에서 수정하지 않고, STATE.md 의 Progress 줄/frontmatter percent 값만 직접 보정함 (Phase 10 Plan 3)
- Y축 데드존은 `_followBaseY`(Lerp)를 완전히 대체하는 `_deadzoneCenterY`(하드컷)로 구현하고, X축 `UpdateDeadzoneCenter()`와 병합하지 않고 별도 함수 `UpdateDeadzoneCenterY()`로 분리 — 병합 시 Y가 `_deadzonePushSign`을 오염시켜 X축 Dynamic Offset의 방향 신호를 깨뜨리기 때문(DY-02). `LateUpdate` 끝에 Y 재앵커 라인은 추가하지 않음 — Y에는 클램프가 없어 수학적으로 항등(no-op)이기 때문 (quick task 260804-q6h)
- `CameraController.SetXBounds(min, max)`는 순수 필드 대입만 한다 — 기존 `ApplyXClamp()`가 매 프레임 `minX`/`maxX`를 소비하므로 신규 클램프 로직 불필요(MX-01). 이전 경계 캐시는 `CameraController`가 아니라 `CameraBoundsTrigger` 인스턴스가 소유(MX-05) — 구역마다 독립적인 단일 슬롯 캐시로, 스택/구역 매니저는 의도적으로 만들지 않음. 구역은 영구 핸드오프가 아니라 **범위 한정 오버라이드**이며, 진입 직전 경계를 캐시했다가 이탈 시 복원한다(사용자 결정, 2026-08-05). 겹치는 구역에서의 stale 복원은 코드로 방어하지 않고 `Check.md`에 알려진 한계로만 문서화 (quick task 260805-m41) — **이 캐시/복원 방식은 260805-q2u 에서 폐기됨**
- `minX`/`maxX`를 런타임 불변 "스테이지 고정 기본 경계"로 재정의하고, `SetXBounds`가 대신 쓰는 `_targetMinX`/`_targetMaxX`를 신규 `boundsSmoothing`(기본 3, `zoomSmoothing`과 동일 성격)으로 `_currentMinX`/`_currentMaxX`에 매 프레임 Lerp — `ApplyXClamp`와 Gizmo 빨간선 모두 `_current*`만 소비(Q2-01~Q2-05). `CameraBoundsTrigger`의 캐시/복원(MX-05) 로직을 전부 삭제하고 이탈 시 항상 `CameraController.Instance`의 고정 `minX`/`maxX`로 복귀하도록 바꿔, 구역이 겹치거나 순서가 꼬여도 stale 값이 복원될 수 없게 구조적으로 해소(Q2-06). 레벨 디자인은 벽이 있는 모든 구간에 트리거를 타일링하고, `BoxCollider2D`의 Y 범위로 층별 트리거를 분리 배치하는 방식으로 코드 변경 없이 해결(Q2-07) (quick task 260805-q2u) — **`boundsSmoothing` Lerp는 260809-h9k 에서 폐기됨**
- `boundsSmoothing` Lerp를 완전히 제거하고 `SetXBounds`가 `_currentMinX`/`_currentMaxX`에 직접 대입하도록 되돌림(260805-m41과 동일한 즉시 스냅 방식, `_targetMinX`/`_targetMaxX` 필드 삭제) — 사용자가 구역 경계 전환이 "벽처럼 딱 막히는 느낌"이 아니라고 지적, Lerp가 있으면 좁은 구역 진입 시 경계가 다 좁혀지기 전에 벽 너머가 잠깐 보이는 게 Check.md에 이미 알려진 한계였음. minX/maxX(스테이지 기본) 자체는 원래도 즉시 클램프였으므로 이제 구역 경계도 동일한 체감이 됨 (quick task 260809-h9k)
- `CameraBoundsTrigger.OnTriggerExit2D`가 무조건 스테이지 기본 경계로 복귀하는 기존 로직(Q2-06)에 구조적 허점 발견: 구역을 서로 맞붙여 타일링하면(Q2-07 권장 방식) 한 구역의 Exit과 이웃 구역의 Enter가 같은 물리 프레임에 발생할 수 있는데, Unity가 두 콜백의 호출 순서를 보장하지 않아 Exit이 Enter보다 늦게 처리되면 방금 적용된 이웃 구역의 좁은 경계를 기본값으로 덮어써버림 (사용자가 "복도→보스룸으로 걸어 들어가기만 해도 다시 안 됨" 증상으로 발견). `OnTriggerExit2D`에서 복귀 전 다른 모든 `CameraBoundsTrigger`의 콜라이더와 `Collider2D.IsTouching`으로 겹침을 확인해, 플레이어가 아직 다른 구역 안에 있으면 복귀를 건너뛰도록 가드 추가 — `IsTouching`은 콜백 처리 순서와 무관하게 물리 엔진의 현재 겹침 상태를 직접 읽으므로 순서 문제에 안전함. 구역을 하나만 쓸 때는 이 경쟁 상태가 절대 드러나지 않아 지금까지 발견되지 않았음 (quick task 260809-h9k)
- Y축 카메라 경계(`minY`/`maxY`)를 X와 완전히 동일한 구조로 추가 — 이전까지 "Y는 의도적으로 클램프하지 않는다"(D-09, Phase 9~10 내내 유지된 결정)였으나 사용자가 명시적으로 뒤집음. `ApplyXClamp`를 `ApplyBoundsClamp`로 합쳐 X/Y 동시 클램프, `CameraBoundsTrigger`는 구역 콜라이더의 Y 범위(`b.min.y`/`b.max.y`)를 그대로 Y 경계로도 사용(Y 전용 `useCustomBounds` 없음, 요청 없었음). D-17 재앵커 라인에 Y 버전 추가 필수 — 안 하면 Y 클램프 도입 후 피킹 오프셋이 `_deadzoneCenterY`에 매 프레임 누적되는 회귀 발생. **주의**: 기존 트리거 박스(예: `BossZone_Tutorial`, 세로 15유닛)는 트리거 감지 용도로만 세로 크기를 잡아뒀는데 이제 그 크기가 Y 클램프 범위로도 쓰여서, orthoSize 5 기준 실제 카메라 Y 이동 여유가 5유닛밖에 안 남음 — 방마다 세로 크기를 다시 확인해야 함 (quick task 260809-h9k)
- `Tutorial Map.unity`에 `CameraBounds_Corridor_BeforeBoss` 구역을 신규 배치(X 125~278.43, 문 앞 복도) — `BossZone_Tutorial`과 맞붙여 Q2-07 타일링 예시를 실제로 처음 적용. 방 밖에서 보스룸 내부가 미리 보이던 문제(전용 트리거 없이 스테이지 기본 경계(125~339)가 보스룸 X범위를 통째로 포함했던 게 원인)를 해소. Play 모드에서 사용자와 함께 직접 검증 완료 (quick task 260809-h9k)
- Newtonsoft.Json 을 `manifest.json` 에 3.2.2 로 직접 의존성 고정 (11-CONTEXT.md 의 "3.2.1" 기재는 연구 단계 오류로 확인, `Library/PackageCache` 실제 캐시 버전 3.2.2 기준) (Phase 11 Plan 1)
- `SaveData`/`PlayerStatsSaveData` POCO 스키마 신설 — 위치는 좌표가 아니라 SceneName+SpawnPointName 문자열로 모델링(D-05), 보스진행도/맵기믹은 `Dictionary<string, bool>` 스텁(D-03), 아이템은 빈 `List<string>` 스텁(D-03b) (Phase 11 Plan 1)
- `PlayerStats.RestoreStats(float, float, float)` 를 additive 전용 공개 메서드로 추가 — setter 프로퍼티 대신 이 메서드가 유일한 외부 쓰기 경로이며, `maxTotalHealth` -> `maxHealth` -> `health` -> `ClampHealth()` 순서를 지켜야 저장된 체력이 낡은 maxHealth 로 잘리지 않음 (Phase 11 Plan 1)
- `SaveLoadManager.cs` 의 "coroutine, not async/await" 설명 주석을 두 차례 재작성 — task-level word-boundary grep 게이트와 plan-level plain-substring grep 게이트("Assets/SaveSystem/" 전체, async|await 부분일치)를 동시에 만족시키려면 "asynchronous"/"awaiting" 같은 파생어도 피해야 했음, Phase 9 Plan 1 DontDestroyOnLoad·Phase 10 Plan 1 deadzoneHeight 사례와 동일 계열 (Phase 11 Plan 2)
- `SaveOnBossDefeated(bossId)` 는 보스 격파 시점에 새 스폰포인트를 만들지 않고 `BossProgress` 딕셔너리만 갱신 — 부활 지점은 항상 마지막 체크포인트 활성화가 저장한 씬/스폰포인트를 재사용 (RESEARCH Open Question 1 해결안) (Phase 11 Plan 2)
- 체크포인트 1곳 + 보스 4종(TutorialBoss/WoodBoss/WaterSpirit/WaterMonster) 격파 지점에 `SaveLoadManager.Instance.SaveAtCheckpoint`/`SaveOnBossDefeated` 호출 삽입 완료 — Group A(HP.OnDeath 이미 구독 중인 TutorialBoss/WoodBoss)는 기존 `HandleDeath()` 본문에, Group B(이벤트 자체가 없는 WaterSpirit/WaterMonster)는 `BossStatsSystem.Die()` 오버라이드 본문에 직접 삽입 (Phase 11 Plan 3)
- CP949 인코딩 파일(Checkpoint.cs, WoodBossController.cs) 편집 시 표준 Read/Edit 툴 조합은 UTF-8 왕복 과정에서 파일 전체의 비-ASCII 바이트를 U+FFFD로 조용히 훼손시킴 — `grep -cP "[^\x00-\x7F]"` 줄 수 카운트 게이트로는 감지 불가. `git show HEAD:<path>` 로 원본 바이트를 추출해 순수 바이트 단위 스크립트로 삽입하는 방식으로 전환 (Phase 9 Plan 1/Phase 10 Plan 1 계열의 인코딩 사고와 동일 범주) (Phase 11 Plan 3)
- 작업 트리에 이 플랜과 무관한 기존 스테이지된 변경(카메라 스크립트 rename)이 이미 있는 상태에서 `git add <file>` 후 pathspec 없는 `git commit`을 실행하면 인덱스 전체가 커밋됨 — `git reset --soft HEAD~1` + pathspec 명시 커밋으로 즉시 수정, 이후 전 태스크 커밋에 `-- <path>` 명시 (Phase 11 Plan 3)
- Phase 13은 보고서 전용 phase — `Assets/` 0줄 변경을 각 플랜의 인수 기준(`git status --porcelain Assets` 빈 출력)으로 강제했다. 실제 삭제/리팩토링은 `13-AUDIT-REPORT.md`의 체크박스를 사용자가 승인한 뒤 별도 작업으로 진행한다 (CONTEXT.md D-01/D-02, CLAUDE.md 3번 원칙의 범위 한정 예외). agy CLI로 5개 plan 위임을 시도했으나 파일 30개 이상 규모의 grep 집약적 스캔에서 `--print-timeout` 40분·detached 프로세스로도 반복 timeout — 사용자 승인 하에 Claude Code(fork 5개)가 직접 Read/Grep/Write로 수행하는 것으로 전환. 이 과정에서 agy가 한 차례 존재하지 않는 심볼/줄번호를 지어낸 fragment를 커밋한 사실이 발견되어 즉시 실제 소스 대조 후 재작성했다 (Phase 13)
- CP949(비-UTF-8) 인코딩 .cs 파일이 프로젝트 전체에 46개 존재한다 — 그동안 Phase 9~12에서 개별적으로 발견해온 `Checkpoint.cs`/`WoodBossController.cs` 2개는 빙산의 일각이었다. 분포: Enemy(NewBoss/Tutorial/Boss/Monster_Alpha) 16 / Player 4 / Script+map+ImportedAsset 26 / Camera·SaveSystem·Editor·WaterMonster·WaterSpirit 0. 전수 목록은 `13-AUDIT-REPORT.md` `## D-04` 섹션 (Phase 13)
- 프로젝트 전체 TODO/FIXME/HACK 주석이 0건이고, `Debug.Log*`는 92개 파일에 226건 — D-08 카테고리는 사실상 Debug.Log 정리와 주석처리 코드 정리 두 갈래다. 또한 기존 `Assets/SaveSystem/Check.md`에 "고아 코드"로 기록되어 있던 `WoodBossStatSystem.cs`는 재확인 결과 오판정으로 확인됨(파일명 `WoodBossStatSystem` vs 실제 클래스명 `WoodBossStatsSystem` 철자 불일치로 과거 검색이 0건을 반환) — `WoodBossController.cs`에서 실제로 사용 중이며 삭제 대상이 아니다 (Phase 13)
- 사용자가 `13-AUDIT-REPORT.md` D-08 전체를 카테고리 단위로 일괄 승인(오류 진단용 로그 제외, 나머지 추적용 Debug.Log 전량 제거) + D-D07-17~20(Phase 1~2 1회용 에디터 셋업 도구 4종: BuildPhase2Assets/BuildWaterMonsterAssets/Phase1CLI/PlaceWaterMonsterInScene) 삭제를 승인 — CP949 혼재 위험 때문에 순수 바이트 스크립트로 149줄 자동 삭제 + 1줄 수술적 수정(InputHandler.cs:124, 로그가 `OnPauseEvent?.Invoke()`와 한 줄에 묶여 있어 전체삭제 시 이벤트 구독까지 소실될 뻔함). 보고서 태깅과 실제 코드가 어긋난 `PlayerInputHandler.cs:11`(사실은 널 가드 오류 로그)은 배치에서 제외. `SaveLoadManager.cs`(라인별 구분 불가)와 `HP.cs`(Phase 11/12 "0줄 변경" 계약 파일)는 개별 검토로 남김. 상세 로그는 `13-AUDIT-REPORT.md` `## 2026-08-20 처리 완료 로그` 참고 (Phase 13, 2026-08-20)
- **(향후 정책, Phase 13 이후에도 적용)** GSD phase를 완료할 때 개발용 추적 Debug.Log는 기본적으로 전량 제거한다. 널 체크/컴포넌트 미발견 등 실제 오류 진단용 `Debug.LogError`/`Debug.LogWarning`은 예외로 유지한다. 로그가 다른 실행 로직과 한 줄/한 문에 묶여 있으면 줄 전체 삭제 대신 로그 호출부만 제거하는 수술적 수정을 우선한다 (2026-08-20 사용자 결정)

### Active TODOs

- (권장, 필수 아님) Phase 9: 실제 보스 씬에 트리거를 배치하기 전, `Assets/Camera/Check.md` 의 Play 모드
  체크리스트를 최소 1회 직접 확인할 것 — 09-03 에서 정적 검사만 통과했고 런타임 검증은 생략됨.

- Phase 10 gap: 사용자가 Play 모드 실측 중 Y축 데드존 부재를 확인하고 도입을 요청함 (2026-08-04).
  **코드 반영 완료, Play 모드 검증 대기** — quick task `260804-q6h` (commit `d3cc065`)에서 `_followBaseY`(Lerp)를
  `_deadzoneCenterY`(하드컷)로 교체하고 `UpdateDeadzoneCenterY()`를 X축과 동일 계열로 신설했다. 정적 회귀 검사
  9항목은 전부 통과했으나 Play 모드 실측은 아직 미수행 — `Assets/Camera/Check.md` "5) Y축 하드컷 데드존" 섹션의
  체크리스트 11개 항목을 사용자가 직접 확인해야 한다. 상세 기록: `.planning/quick/260804-q6h-y-cameracontroller-cs/260804-q6h-SUMMARY.md`.

- Phase 7 Plan 2 (07-02-PLAN.md): Play 모드 검증 체크포인트 보류 중. WaterMonster 보스가
  CombatState 기반 패턴 판단 로직으로 마이그레이션된 뒤, WaterSpirit/TutorialBoss/WaterMonster
  전체를 한 번에 일괄 검증할 예정 (사용자 결정). 체크리스트: `Assets/Enemy/WaterSpirit/Check.md`,
  `Assets/Enemy/Tutorial/TutorialBoss/Check.md`

- Phase 8 Plan 3 (08-03-PLAN.md): 정적 회귀 검사 + WaterSpirit/TutorialBoss/WaterMonster 3종
  일괄 Play 모드 검증 체크포인트 (Unity 컴파일 확인은 이 실행 환경에서 불가 — 08-03 에서 수행)

- ~~quick task 260805-m41 gap: 씬 배치 + Play 모드 검증 대기~~ — **해소됨 (260809-h9k)**. `Tutorial Map.unity`에
  `BossZone_Tutorial` + 신규 `CameraBounds_Corridor_BeforeBoss`를 배치하고 사용자와 함께 Play 모드에서 직접
  검증 완료 (구역 진입/이탈, 인접 구역 연속 통과, 즉시 스냅, Y축 클램프 전부 확인). `1 stage.unity` 등 다른 씬에는
  아직 트리거가 배치되지 않았으니 그쪽은 여전히 `Assets/Camera/Check.md` "7)" 섹션 가이드를 따라 사용자가
  수동 배치해야 함 (MX-04).

- Phase 13 감사 보고서(`13-AUDIT-REPORT.md`) D-07/D-08은 `## 회귀 위험 높음` 고위험 항목 포함 전량 개별 승인·실행 완료 (2026-08-20). D-09(중복 로직) 9건, D-10(긴 함수, 권장 등급) 12건은 여러 보스/공용 클래스에 걸친 리팩토링이라 회귀 위험이 커 이번 라운드에서 의도적으로 미착수 — 백로그로 남김, 착수 시 보고서 체크박스 재사용. `## 회귀 위험 높음` 섹션에서 실제로 수정된 파일(SaveLoadManager.cs/BossController.cs/TutorialDeadState.cs/PlayerAttack.cs)은 Play 모드 재검증이 아직 안 됨.

### Blockers

(없음)

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260804-q6h | Y축 데드존(하드컷) 추가 - CameraController.cs | 2026-08-04 | d3cc065 | [260804-q6h-y-cameracontroller-cs](./quick/260804-q6h-y-cameracontroller-cs/) |
| 260805-m41 | 구역별 카메라 X 경계 - SetXBounds + CameraBoundsTrigger | 2026-08-05 | c9d5b7c | [260805-m41-cameracontroller-setxbounds-min-max-boss](./quick/260805-m41-cameracontroller-setxbounds-min-max-boss/) |
| 260805-q2u | 구역 타일링 + 부드러운 경계 전환 (X bounds Lerp 재설계) | 2026-08-05 | 8103c3a | [260805-q2u-x-cameracontroller-cameraboundstrigger](./quick/260805-q2u-x-cameracontroller-cameraboundstrigger/) |
| 260809-h9k | 경계 즉시 스냅 복귀 + 인접 구역 Exit/Enter 경쟁 상태 수정 + Y축 카메라 경계 추가 + Tutorial Map 구역 배치·실측 검증 | 2026-08-09 | 6afe518 | (문서 없음 - 채팅 세션에서 직접 진행) |
| 260912-kih | 플레이어 최근접 상호작용 및 현재 키 UI (컴파일 통과, Play 미검증) | 2026-09-12 | 737216f | [260912-kih-centralize-interaction-detection-on-play](./quick/260912-kih-centralize-interaction-detection-on-play/) |
| 260913-git | 세이브 포인트 및 ESC 저장 슬롯 선택 (컴파일 통과, Play 미검증) | 2026-09-13 | 53311c2 | [260913-git-save-point-esc-slot-selection](./quick/260913-git-save-point-esc-slot-selection/) |

### Roadmap Evolution

- Phase 7 added: 보스 공격 패턴 판단 로직 리팩토링 — CombatState 공유 기반에 TutorialBoss 스타일(거리/쿨다운/연속금지 조건부 판단)의 재사용 가능한 패턴 선택 로직을 도입하고, WaterSpirit 보스(Stage 1 SpiritCombatState 및 Stage 2 Stage2CombatState)에 적용한다.
- Phase 9 added: 일반 스테이지와 보스 스테이지 진입 시 카메라 크기(줌) 변화
- Phase 10 added: 카메라 데드존 기법 3종 (Base Deadzone, Dynamic Asymmetrical Deadzone, Input-based Peeking) 구현 — Phase 9 CameraController에 레이어링
- Phase 11 added: Newtonsoft.Json 기반 싱글톤 세이브/로드 매니저 — DontDestroyOnLoad, 메모리 캐싱, 체크포인트/보스 격파 시점 저장, 씬+좌표/스탯/보스진행도/맵기믹/아이템 데이터 모델, 비동기 씬 로드 후 좌표 이동
- Phase 12 added: 피격 시 카메라 흔들림 (Camera Shake on Hit)
- Phase 13 added: 프로젝트 폴더를 돌면서 의미 없는 코드나, 주석, 리펙토링이 필요한 코드 살펴보는 페이즈
- Phase 14 added (구): 키바인딩(Keybinding)을 keybind.json으로 저장하고 SaveLoadManager에 위임 — **폐기됨(2026-08-27)**: quick task `260827-h5y`가 키바인딩을 포함한 설정 전체를 `setting.json`(SettingsData) 통합 방식으로 먼저 구현해, keybind.json 전용 설계를 대체함. 구현 커밋 2개(`36f76af`/`0c51c26`)는 원격 통합 결정에 따라 rebase로 제거, 로드맵/플래닝 문서에서도 phase 자체를 삭제.
- Phase 50 added (임시 번호, 다른 기기와 동기화 후 재번호 필요 - 그 기기는 이미 Phase 13까지 완료함): 세이브 슬롯 확장 - 슬롯 2개 추가(총 3슬롯), 슬롯별 독립 세이브 데이터 및 진행도 저장/표시 — **재번호 완료(2026-08-27)**: 위 구 Phase 14가 폐기되며 번호가 비어, 이 phase를 14로 재번호(폴더 `.planning/phases/50-2-3/` → `.planning/phases/14-save-slot-expansion/`)
- Phase 15 added: 로드 시점 및 로드 범위 정의 — 세이브 데이터를 언제 로드할지(플레이어 사망/체크포인트 부활/이어하기)와 로드 시 어디까지 복원할지(보스 진행도·맵 기믹·체력 불변식)를 확정한다. BUG-002의 로드 측 항목(역전된 저장 데이터 보정)과 BUG-003의 보스 진행도 복원 의존성을 이 페이즈로 이관.
- Phase 16 added: 일정 간격 자동저장 — Phase 15 논의 중 "세이브가 없어 로드할 게 없는 상황"을 줄이자는 취지로 제안됨. 저장 시점을 늘리는 새 기능이라 Phase 15(로드 범위)와 분리해 별도 페이즈로 추가. (주창은 브랜치에서 진행, 3/3 완료 후 이 병합으로 Item에 반영)
- Phase 17 added (Item 브랜치, 최초 15로 잘못 생성됨 → 즉시 재번호, 병합 시 주창은의 Phase 16과 번호가 겹쳐 17로 재번호): 아이템 코어 — `IItem` 인터페이스(`Item` 브랜치에서 선행 완료, `Assets/Item/Script/IItem.cs`) + `ItemData` ScriptableObject(id, 종류(소모품/진행아이템), UseEffect 파라미터). 데이터 레이어만, UI 제외. **재번호 사유**: `.planning/phases/15-load-timing-and-load-scope/`가 이미 완료된 상태(15-01~15-04 SUMMARY.md 존재, `PlayerStats.cs`/`SaveLoadManager` 사망 시 로드 처리)로 실재했으나 ROADMAP.md에 `### Phase 15` 헤딩이 누락되어 있었음 — `gsd-tools phase add`가 ROADMAP.md 헤딩만 스캔해 다음 번호를 15로 잘못 계산. 폴더/헤딩을 15→16으로 git mv 및 수정.
- Phase 18 added: 인벤토리 시스템 — 고정 슬롯 수 + 스택 기반 자료구조, 추가/제거/사용 API, `PlayerInteraction` 연동 월드 아이템 획득. Depends on Phase 17.
- Phase 19 added: 아이템 저장/로드 연동 — `SaveData.Items`(`List<string>` 스텁)를 `List<ItemSaveEntry>{itemId,count}`로 교체, `SaveVersion` 마이그레이션, `SaveLoadManager` 인벤토리 캡처/복원 로직. Depends on Phase 18 및 기존 `SaveLoadManager`.
- **누락 백필 완료**: `.planning/phases/15-load-timing-and-load-scope/`(Phase 11/14 의존)의 `### Phase 15` 헤딩을 ROADMAP.md에 추가함. **주의**: 완료된 페이즈가 아님 — 15-UAT.md `status: partial`, Play 모드 실측 0/34건 미검증, BUG-007(씬 전환 후 InputHandler 유실, major)이 미해결 상태로 남아 있음. 15-04-PLAN.md(Play 모드 체크포인트, autonomous:false)가 미종결이라 Plans 체크박스도 미완료로 표시함.
- Phase 17 Plan 01: `ItemData.cs`의 D-06 설명 주석("No displayName / icon / description...")이 같은 플랜의 금지-필드 grep 인수 기준과 리터럴로 충돌해("displayName"/"icon"/"description" 자체가 매치됨) "No name/image/blurb fields..."로 재작성 — Phase 9/10/11의 DontDestroyOnLoad/deadzoneHeight/async-await 자기모순 게이트와 동일 계열. 필드/로직 변경 없음 (Phase 17 Plan 1)
- Phase 17 Plan 01 실행 시 배정된 워크트리(`worktree-agent-ab70cb16e75fe20bc`)가 `Item` 브랜치보다 120 커밋 뒤처진 무관한 브랜치를 체크아웃 중이었음(플랜/`Assets/Item/` 트리 전부 부재) — 워크트리 브랜치가 `Item`의 순수 조상(고유 커밋 0개)이라 안전하게 `Item`(`79caaf2`)로 fast-forward 머지해 해결. `Assets/Item.meta`/`Script.meta`/`IItem.cs.meta` 3개는 메인 체크아웃에만 untracked로 존재해 절대경로로 바이트 그대로 복사(내용 무수정) 후 Task 1에 포함 (Phase 17 Plan 1)
- **2026-09-21 병합**: `Item` 브랜치의 Phase 16(아이템 코어)/17(인벤토리)/18(아이템 세이브·로드)를 17/18/19로 재번호한 뒤 `주창은` 브랜치(Phase 14 슬롯 배선, Phase 15 완료 처리, Phase 16 구간 자동저장 COMPLETE, BUG-009 수정, Phase 999.1 오디오 중앙화 백로그)를 병합. 코드 파일은 겹치지 않아 충돌 없었고, `.planning/ROADMAP.md`/`STATE.md` 서술만 수동으로 합침.

## Session Continuity

- 2026-09-14: Phase 16 COMPLETE (16-01/02/03). 16-03 Task 2 Play 모드 실측 29/29 PASS via Unity CLI (commit 6703d34); B(메인메뉴 미발동)·E(수동 저장 무회귀) 포함 전 항목 실측. Assets·ProjectSettings 0줄 변경, 세이브 SHA-256 복구 확인. 다음 Play 모드 검증 전에 Console 의 Error Pause 를 끌 것 — BUG-008 예외로 Play 가 자동 정지한다. 상세는 `.planning/phases/16-interval-autosave/.continue-here.md`.


- Last session: Completed 15-02-PLAN.md (2026-09-10); next implementation target is 15-03-PLAN.md.
- 이전 마일스톤: v1.0 보스_물괴물_구현 (Phase 1~4 완료, 2026-04-16)
- 새 마일스톤 Phase 5부터 번호 이어서 시작
- 로드맵 원본: `.planning/ROADMAP.md`
- 요구사항: `.planning/REQUIREMENTS.md`
- 마지막 세션: Completed 09-03-PLAN.md (2026-07-30, Play 모드 검증은 사용자 결정으로 생략). 다음 재개 지점: Phase 9 검증(gsd-verifier)
- 마지막 세션: Completed 10-01-PLAN.md (2026-08-04, Base Deadzone + `_isBossZone` 분기 구조 + Gizmo). 다음 재개 지점: Phase 10 Plan 2 (10-02-PLAN.md, Dynamic Asymmetrical Deadzone)
- 마지막 세션: Completed 10-02-PLAN.md (2026-08-04, Dynamic Asymmetrical Deadzone — `_currentBoxOffsetX` SmoothDamp + hold timer + `_deadzonePushSign`). 다음 재개 지점: Phase 10 Plan 3 (10-03-PLAN.md, Input-based Peeking)
- 마지막 세션: Completed 10-03-PLAN.md (2026-08-04, Input-based Peeking — `InputHandler.OnMoveEvent` 구독 라이프사이클 + `UpdatePeekOffset` SmoothDamp, `PlayerController.cs`/`InputHandler.cs` 무수정). 다음 재개 지점: Phase 10 Plan 4 (10-04-PLAN.md)
- 마지막 세션: Completed quick task 260804-q6h (2026-08-04, Y축 하드컷 데드존 — `_followBaseY` Lerp를 `_deadzoneCenterY` 하드컷으로 교체, `UpdateDeadzoneCenterY()` 신설, commit `d3cc065`). 정적 회귀 검사 9항목 전부 PASS, Play 모드 미검증. 다음 재개 지점: `Assets/Camera/Check.md` "5) Y축 하드컷 데드존" 체크리스트 Play 모드 실측
- 마지막 세션: Completed quick task 260805-m41 (2026-08-05, 구역별 카메라 X 경계 — `CameraController.SetXBounds(min, max)` 순수 대입 신규 + `CameraBoundsTrigger.cs` 신규(BossZoomTrigger 패턴 미러링, 진입 시 이전 경계 캐시 / 이탈 시 복원, MX-05), commit `c9d5b7c`). 정적 회귀 검사 11항목 전부 PASS, 씬 배치(MX-04) + Play 모드 둘 다 미수행. 다음 재개 지점: `Assets/Camera/Check.md` "6) 구역별 카메라 X 경계" 8단계 수동 배치 후 13개 체크리스트 Play 모드 실측 (**MX-05 캐시/복원 방식은 260805-q2u 로 대체됨**)
- 마지막 세션: Completed quick task 260805-q2u (2026-08-05, 구역 타일링 + 부드러운 경계 전환 — `minX`/`maxX`를 런타임 불변 고정 기본 경계로 재정의, `boundsSmoothing` 신규 필드 + `_targetMinX/_targetMaxX` → `_currentMinX/_currentMaxX` Lerp 2단 구조 도입(zoomSmoothing 미러링), `CameraBoundsTrigger`의 캐시/복원 로직(MX-05) 전부 제거하고 이탈 시 항상 고정 기본값 복귀로 전환, commit `8103c3a`). 정적 회귀 검사 21항목 전부 PASS(1건은 diff 정렬 오차로 문서화), 씬 배치 + Play 모드 둘 다 미수행. 다음 재개 지점: `Assets/Camera/Check.md` "7) 구역 타일링 & 부드러운 경계 전환" 체크리스트 Play 모드 실측 (타일링 + Y 범위 분리 배치 후)
- 마지막 세션: Completed 11-01-PLAN.md (2026-08-10, Newtonsoft.Json 직접 의존성 고정(3.2.2) + `SaveData`/`PlayerStatsSaveData` POCO 스키마 신규 + `PlayerStats.RestoreStats` additive 메서드, commits `a1b14ed`/`82510fd`/`1b26ecc`). 다음 재개 지점: Phase 11 Plan 2 (11-02-PLAN.md, SaveLoadManager)
- 마지막 세션: Completed 11-02-PLAN.md (2026-08-10, `SaveLoadManager` DontDestroyOnLoad 싱글톤 신규 — 부트스트랩(`RuntimeInitializeOnLoadMethod`), 공개 저장 API(`Save`/`SaveAtCheckpoint`/`SaveOnBossDefeated`/`HasSaveFile`/`NewGame`), `LoadGame` 코루틴 기반 씬 로드+스탯 복원, commits `dbde39c`/`e83203b`). 다음 재개 지점: Phase 11 Plan 3 (11-03-PLAN.md, Checkpoint/보스 4종 통합)
- 마지막 세션: Completed 11-03-PLAN.md (2026-08-10, 체크포인트 1곳 + 보스 4종(TutorialBoss/WoodBoss/WaterSpirit/WaterMonster) 격파 지점에 `SaveLoadManager` 호출 삽입 — Group A(HP.OnDeath)는 `HandleDeath()`에, Group B(이벤트 없음)는 `Die()` 오버라이드에 직접 삽입, commits `7e2960e`/`e36a76c`/`1fcf28a`). CP949 인코딩 훼손 위험과 git 인덱스 오염을 실행 중 발견해 즉시 수정(SUMMARY 참고). 다음 재개 지점: Phase 11 Plan 4 (11-04-PLAN.md)
- 마지막 세션: Phase 11 Play 모드 검증 부분 완료(체크포인트/로드/새게임 확인, 보스 4종 격파 저장은 사용자가 추후 확인 예정) + Phase 12(피격 시 카메라 흔들림) 로드맵 추가 + discuss-phase 완료(2026-08-11, `12-CONTEXT.md`/`12-DISCUSSION-LOG.md`). 결정 요약: 플레이어 피격 시만(D-01), `PlayerStats.TakeDamage`에서 `CameraController.Instance.Shake()` 호출(D-02), 고정 강도 랜덤 오프셋 감쇠(D-04~D-06), 보스존 포함 항상 흔들림 + 경계 클램프 이후 최종 적용(D-07/D-08), Inspector 노출은 `shakeMagnitude`/`shakeDuration` 2개만(D-09). 다음 재개 지점: `/gsd:plan-phase 12`
- 마지막 세션(2026-08-27): quick task `260827-h5y`(PlayerPrefs → `setting.json` 전환) Task 1~3 완료(commits `ea05191`/`d42ea6f`/`ef745bb`/`0dfcd0d`), Task 4(Unity 컴파일 + 저장 버튼 OnClick 연결 + Play 모드 실측)는 사용자 확인 대기 중. 별도로 세이브 슬롯 확장을 Phase 50(임시 번호 — 다른 기기가 이미 Phase 13까지 진행해서 충돌 방지용으로 큰 번호 임시 예약, 동기화 후 재번호 필요)으로 로드맵에 추가하고 discuss-phase 완료(`50-CONTEXT.md`/`50-DISCUSSION-LOG.md`, 2026-08-27 재번호 후 `.planning/phases/14-save-slot-expansion/14-CONTEXT.md`/`14-DISCUSSION-LOG.md`로 이동). 결정 요약: 이어하기는 항상 슬롯 선택 화면(D-01), 새시작은 빈 슬롯 있으면 자동 시작·다 차있으면 슬롯 화면으로(D-02/D-03), 덮어쓰기는 항상 확인창(D-04/D-05), 슬롯별 별도 파일(D-06, 기존 save.json 유실 금지가 절대 기준·정확한 마이그레이션 방식은 연구 단계에서 결정). 다음 재개 지점: quick task Task 4 사용자 검증 완료 후, `/gsd:plan-phase 14`
- 마지막 세션(2026-08-27): Phase 14 `/gsd:plan-phase 14` 완료 — `14-UI-SPEC.md` + `14-01/02/03-PLAN.md` 3개 작성(Wave 1 `14-01` SaveLoadManager 슬롯화, Wave 2 `14-02` SlotSelectPanel/OverwriteConfirmPanel 신규, Wave 3 `14-03` MainMenuUI 재배선+씬 배치, 자율/자율/비자율). 같은 날 원격 `origin/주창은`의 `setting.json` 통합 커밋을 머지(`83d082d`)하고 구 Phase 14(keybind.json)를 폐기·제거(`b6b41bc`, `4392d3e`). **`14-UI-SPEC.md`/`14-01/02/03-PLAN.md`와 이를 반영한 `ROADMAP.md` 변경분은 이 시점 이후 커밋되지 않고 워킹트리에 미커밋 상태로 남아 있음** — 코드 실행은 아직 0%. 다음 재개 지점: 미커밋 산출물 커밋 여부 결정 후 `/gsd:execute-phase 14` (Wave 1 `14-01`부터)
- 마지막 세션(2026-09-20): Completed 17-01-PLAN.md — 프로젝트 최초 ScriptableObject `ItemData`(`Assets/Item/Script/ItemData.cs`) 신규, `IItem` 직접 구현, `id`/`type`/`effectType`/`amount` 4필드, `UseEffect`가 Consumable/Heal에서 기존 `PlayerStats.Heal(float)`을 널 가드 없이 재사용하고 Progression은 no-op, 에디터 전용 `ContextMenu` 검증 훅 포함. 예시 `.asset` 2개(`HealthPotion`=Consumable/Heal/20, `AncientKey`=Progression) `Assets/Item/`에 손수 작성, 둘 다 `ItemData.cs.meta`의 guid(`501b19c5008706d0a3f2bf69aacf52f6`)를 참조. commits `f53e5e9`/`33a3113`. 정적 회귀 검사(플랜 자체 6항목) 전부 PASS. Unity 에디터 실임포트/컴파일/Play 모드 검증은 계획대로 Plan 17-02로 이연됨. 다음 재개 지점: `/gsd:execute-phase` 17-02 (Unity 배치모드 임포트 게이트 + Check.md + Play 모드 체크포인트)
- 마지막 세션(2026-09-20): 17-02-PLAN.md Task 1~2 완료. Task 1: Unity 6000.3.10f1 배치모드 임포트(`-batchmode -quit -nographics -projectPath`) 실행, 종료 코드 0, `Assets/` 경로 `error CS` 0건·`Assets/Item` 임포트 에러 0건·missing script 0건 전부 확인. `git status --porcelain Assets/Item` 빈 출력 — 17-01이 손으로 쓴 YAML이 Unity 표준 재직렬화 결과와 완전히 일치해 커밋할 변경 없음(정상 결과). `ProjectSettings/EditorBuildSettings.asset` 개행 정규화(내용 diff 없음)만 범위 밖에서 관측되어 커밋하지 않고 Check.md에 기록. Task 2: `Assets/Item/Check.md`(+ `.meta`, guid `05511fb05942d6634e84eccc8a623d1f`) 신규 작성 — 정적 회귀 결과 8항목 전부 PASS + Play 모드 체크리스트 10항목(미체크) + 임포트 부작용 절 + 알려진 한계. commit `a47a39a`. Task 3(Play 모드 실측 체크포인트, `checkpoint:human-verify` blocking)에서 정지 — 사용자가 `Assets/Item/Check.md`의 10항목을 Unity Play 모드에서 직접 확인하고 결과를 알려줘야 함. 다음 재개 지점: 사용자의 Play 모드 검증 결과 수신 후 Task 3 완료(Check.md 체크박스 갱신 + 결과 기록 절 채움) → 17-02-SUMMARY.md 작성 → Phase 17 완료 처리
