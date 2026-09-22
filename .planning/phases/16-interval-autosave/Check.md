# Phase 16 — 일정 간격 자동저장 검증 상태

**요약:** 16-01/16-02 구현 및 컴파일 완료. AutoSaveTimer가 플레이 가능 시간 180초마다 현재 슬롯을 저장하고 AutoSaveNotice가 우하단 알림을 표시하도록 연결했다.
정적 회귀 원문 기준 18/22 PASS, 4건 FAIL(검증 명세 오류 [BUG-010](bugs/BUG-010-plan-acceptance-false-positives.md)). 주석/기존 변경을 구분한 대체 검사 22/22 PASS.
Unity 에디터 실측 1차(2026-09-14, Unity MCP)에서 180초 자연 발동과 현재 슬롯 갱신을 파일 증거로 확인했고, 2차(2026-09-14, Unity CLI `unity command eval`)에서 남은 13항목을 전부 측정했다.
Play 모드 29항목 **29 PASS / 0 FAIL / 0 미검증**. 메인메뉴 슬롯 오염 차단(B)과 기존 수동 저장 무회귀(E)를 포함해 전 항목이 실측으로 닫혔다.

## 무엇이 바뀌었나

- AutoSaveTimer.cs 및 .meta: 자기 부트스트랩, 영속 싱글톤, 1초 틱 코루틴, Playing/PlayerStats 가드와 강제 실행/로그 검증 메뉴.
- AutoSaveNotice.cs 및 .meta: 런타임 ScreenSpaceOverlay 캔버스, 우하단 한국어 라벨, unscaled 유지/페이드. 자동저장 성공 후에만 호출.
- SaveLoadManager.cs: Save() 말미 리셋 알림 1문장, 주석/빈 줄 포함 +8/-0줄. 기존 SaveAnywhere/체크포인트/보스/로드 메서드는 그대로다.
- 씬/프리팹 0줄 변경, 설정 0줄 변경. 기존 사용자 변경은 작업 전 SHA-256 기준으로 보존했다.
- Unity 생성 Assembly-CSharp.csproj에 신규 스크립트 Compile 항목 2개를 넣어 실제 컴파일에 포함했다. Git 제외 파일이다.

## 동작 요약 (실행자/사용자가 기대해야 하는 것)

- 간격: 플레이 가능 시간 180초를 1초 틱으로 누적. 일시정지/인벤토리/퍼즐/게임클리어 시간은 scaled 대기에서 멈추고, 플레이어가 없는 메인메뉴는 게이트에서 제외된다.
- 발동 조건: CurrentState == Playing AND PlayerStats.Instance != null, 저장 매니저 존재.
- 저장 내용: SaveAnywhere()와 동일. 현재 씬 기록, 씬이 바뀌면 SpawnPointName 비움. 회복 없음.
- 대상: 현재 슬롯(CurrentSlot) 파일 덮어쓰기. 슬롯 대화상자 없음.
- 체크포인트/보스/일시정지 메뉴 저장 성공 시 카운트 0 리셋.
- 알림: 우하단 자동 저장됨, 1.2초 유지 + 0.6초 페이드아웃.

## 정적 회귀 검사 (2026-09-14, 18/22 PASS)

기준 커밋: 34306e3f7305a4d3f9b457282291bb94a515b7a6. git rev-parse b05668b^로 16-01 첫 코드 커밋의 부모임을 확인했다.
Windows에서는 grep/wc 대신 rg의 일치 행 수를 집계했다. 아래 명령 표의 timer/manager/notice는 위 해당 .cs 경로의 약칭이다.
실행 가능한 전체 명령/인수 및 카운트 로직: [verify-static.cjs](bugs/verify-static.cjs).
재현: node .planning/phases/16-interval-autosave/bugs/verify-static.cjs.
빌드는 증분 캐시가 경고를 생략하지 않도록 --no-incremental을 추가해 기준선과 비교했다.

| # | 결정 | 명령/검사 | 기대값 | 실제값 | 원문 판정 |
|---|---|---|---|---|---|
| 1 | D-01 | rg SaveSlotDialog timer | 0 | 2 | FAIL — BUG-010 |
| 2 | D-01 | rg SelectSlot timer | 0 | 0 | PASS |
| 3 | D-01 | rg _auto.json Assets -g *.cs | 0 | 0 | PASS |
| 4 | D-02 | rg \.bak Assets -g *.cs | 0 | 0 | PASS |
| 5 | D-03 | rg -F Playing gate timer | 1 | 1 | PASS |
| 6 | D-03b | rg GameStateManager\|CurrentState manager | 0 | 0 | PASS |
| 7 | D-03b | rg -F spawn clear guard manager | 1 | 1 | PASS |
| 8 | D-03b | git diff --numstat baseline -- manager | 8 / 0 / Assets/SaveSystem/Script/SaveLoadManager.cs | 8 / 0 / Assets/SaveSystem/Script/SaveLoadManager.cs | PASS |
| 9 | D-03c | rg -F player presence guard timer | 1 | 1 | PASS |
| 10 | D-04 | rg -F interval constant timer | 1 | 1 | PASS |
| 11 | D-05 | rg -F reset hook manager | 1 | 1 | PASS |
| 12 | D-05 | rg NotifySaveWritten Assets -g *.cs | 2 | 3 | FAIL — BUG-010 |
| 13 | D-05b | rg isInCombat\|inCombat\|IsInCombat Assets -g *.cs | 0 | 0 | PASS |
| 14 | D-06 | git diff --numstat baseline -- SaveData.cs | (빈 출력) | (빈 출력) | PASS |
| 15 | D-07 | rg -F direct SaveAnywhere call timer | 1 | 1 | PASS |
| 16 | D-07 | rg SaveAtCheckpoint\|ResetHealthToMax timer | 0 | 1 | FAIL — BUG-010 |
| 17 | D-07b/c | rg direct Save()\|SaveAuto timer | 0 | 0 | PASS |
| 18 | D-10 | rg -F Korean message notice | 1 | 1 | PASS |
| 19 | D-10 | rg -F Show call timer | 1 | 1 | PASS |
| 20 | D-11 | git diff --numstat baseline -- settings files | (빈 출력) | (빈 출력) | PASS |
| 21 | 범위 | git diff --name-only baseline -- Assets | 5 | 22 | FAIL — BUG-010 |
| 22 | 빌드 | dotnet build Assembly-CSharp.csproj --nologo -v quiet --no-incremental | {"exit":0,"warnings":6} | {"exit":0,"warnings":6} | PASS |

원문 FAIL을 PASS로 바꾸지 않았다. 아래 대체 검증으로 실제 구현 조건을 확인했다.

| 원문 # | 대체 검증 | 실제값 | 판정 |
|---|---|---|---|
| 1 | 주석 제외 실제 참조 | false | PASS |
| 12 | 선언+호출만 계수, 주석 제외 | 2 | PASS |
| 16 | 주석 제외 실제 호출 | false | PASS |
| 21 | 기존 미커밋 변경 제외, baseline..HEAD 코드 커밋 범위 | ["Assets/SaveSystem/Script/AutoSaveNotice.cs","Assets/SaveSystem/Script/AutoSaveNotice.cs.meta","Assets/SaveSystem/Script/AutoSaveTimer.cs","Assets/SaveSystem/Script/AutoSaveTimer.cs.meta","Assets/SaveSystem/Script/SaveLoadManager.cs"] | PASS |

대체 판정: **22/22 PASS**. 별도 태스크 인수 검사도 최종 50/50 PASS(계획 코드 일치, 인코딩, GUID, 호출 순서 포함).

추가 실측:
- SaveAnywhere 검색: 원문 7행(주석 포함), 실행 코드 3행(선언 1 + 수동 호출 1 + 타이머 호출 1).
- 알림 금지 의존성 검색: 원문 주석 2행, 실행 코드 참조 0건.
- 16-02 연결 커밋 05e4dec: +8/-0줄(계획 기대값 +7은 오기, 제시 코드와 일치).
- AutoSaveNotice: 2행에 3회 출현(grep -c는 출현 횟수가 아닌 행 수).
- 한국어 UTF-8 문구 바이트 ec9e90eb8f9920eca080ec9ea5eb90a8; U+FFFD 없음. Timer ASCII/CRLF, Notice UTF-8/LF, BOM 없음.
- .meta GUID는 서로 다르고 각각 Assets/Packages/ProjectSettings 전체에서 1건만 존재.
- 작업 전 기존 Assets 1932개 파일 SHA-256 비교: SaveLoadManager의 승인된 훅 변경 외 차이 0건. 기존 미커밋 씬·프리팹·서비스 변경 보존.
- 재빌드 오류 0, 기존 경고 6: CS0108 WaterMonsterStats, CS0114 TutorialBossController 2건, CS0618 CameraBoundsTrigger, CS0649 bullet, CS0414 Stage2CombatState.

## D-02 / D-05b / D-06 / D-08 / D-09 / D-11 (의도적 미구현)

| 결정 | 내용 | 왜 코드 변경 0줄인가 |
|------|------|----------------------|
| D-02 | 덮어쓰기 전 `.bak` 백업 없음 | 게임 안에 백업 복원 UI 가 없어 파일만 늘어난다. 안전성은 D-03 억제 조건과 D-06(체력 미저장)이 담보한다. |
| D-05b | '전투 중'/'보스전 중' 억제 없음 | 프로젝트에 전역 전투 판정 플래그가 없고, D-06 때문에 보스전 중 자동저장이 남기는 해가 없다. 항목 13 이 플래그 신설 0건을 증명한다. |
| D-06 | 체력 하한 보정·저체력 스킵 장치 없음 | `SaveData` 에 현재 체력 필드가 **아예 없다**(`SaveData.cs:20-21`, Phase 15 정책: 로드는 항상 풀피 부활). 저체력 저장으로 사망 루프가 생기는 시나리오가 구조적으로 불가능하다. |
| D-08 | 체크포인트 없이 다음 씬으로 넘어가 자동저장이 걸리면 부활 지점이 '그 씬의 기본 시작 위치' | `SaveAnywhere()` 의 기존 동작을 그대로 수용한 결과다(의도된 동작). 씬을 되돌리지 않으므로 진행은 앞으로만 간다. |
| D-09 | 보스전 중 억제 없음 | 로드가 `LoadSceneAsync` 로 씬을 통째 재생성하므로 보스/몬스터 HP 는 어차피 전부 초기화되고, 격파 기록이 있는 보스만 등장하지 않는다(P15 D-06/D-07). |
| D-11 | `setting.json` 에 on/off·간격 노출 없음 | 자동저장은 항상 켜진 고정 동작이다. 항목 20 이 `SettingsData`/`GameSettingsPanel` 무변경을 증명한다. |


## Unity 에디터 실측 (2026-09-14, Unity MCP)

Unity 에디터 Play 모드에서 직접 측정했다. 수단은 Unity MCP(`Unity_RunCommand` / `Unity_GetConsoleLogs`)이며,
판정은 **세이브 파일의 수정 시각·내용 변화**와 **런타임 오브젝트 실측값**으로 했다. 시작 씬은 `Tutorial Map`, 슬롯은 0.

**측정 환경 (판정에 영향을 주는 조건이므로 명시)**

- 세이브 경로 실측: `C:/Users/chang/AppData/LocalLow/DefaultCompany/AQUA ECLIPSE` (`companyName=DefaultCompany`, `productName=AQUA ECLIPSE`).
- 검증 시작 전 `save.json` / `save_1.json` / `save_2.json` 3개를 세션 스크래치패드에 백업했다(체크리스트 3번). 기준 상태는 전부 `2026-09-13 12:04:39`, `SceneName="Tutorial Map"`, `SpawnPointName=""`.
- **에디터 비포커스 구간에서는 게임이 거의 진행되지 않는다.** `Application.runInBackground=False`(프로젝트 설정 `PlayerSettings.runInBackground=False`)이므로 창이 뒤로 가면 프레임이 멈추고, 벽시계 240초 동안 `Time.time` 이 3.8초만 흘렀다(프레임 537). 자동저장이 발동하지 않은 것은 타이머 결함이 아니라 이 조건 때문이었다.
- 그래서 이후 측정에서는 **런타임에서만** `Application.runInBackground = true` 로 바꿨다. `ProjectSettings` 는 변경하지 않았고(Play 모드 종료 시 원복), 저장소 파일도 건드리지 않았다.
- **Unity MCP 제약 2건:** ① 스크립트에서 `System.Reflection` 사용이 차단돼 `_elapsedSeconds` 직접 판독과 `[ContextMenu]` 기어 메뉴(`Phase16/1`·`Phase16/2`) 호출이 불가능하다 → 항목 5·8은 사용자 확인으로 남는다. **(2차 정정: 이 제약은 Unity MCP 한정이다. `unity command eval` 은 리플렉션을 허용해 두 항목 모두 실측했다.)** ② `Unity_GetConsoleLogs` 가 런타임 `Debug.Log` 를 반환하지 않는다(검증용 마커 `[Phase16Verify]` 를 심어도 조회되지 않음) → 콘솔 문자열 기준(`[SaveLoadManager] Saved to ...`)은 판독 불가이며, 대신 파일 수정 시각·내용으로 판정했다. **(2차 정정: `unity command console` 은 런타임 게임 로그를 스택트레이스까지 반환한다. 콘솔 문자열 기준도 원문으로 확보했다.)**
- 세션 전체 Console: **에러 0건**, 경고 2건. 경고는 Play 진입 시점(18:24:09)의 `The referenced script (Unknown) on this Behaviour is missing!` 2건으로 **Phase 16 와 무관한 기존 씬 문제**다. 신규 스크립트 2개는 정상 임포트됐다(`AutoSaveTimer` guid `d9e6503a…`, `AutoSaveNotice` guid `90640068…`, 클래스 해석 성공, `.meta` 는 임포트 후에도 `git status` 무변경 = 새 GUID 생성 없음).

**D-04 자연 발동 (핵심 측정)**

| 시각(벽시계) | `Time.time` | 사건 | 증거 |
|---|---|---|---|
| 18:28:19 | 4.78 | 측정 기준점 (게이트 `Playing`/`PlayerStats=True`/슬롯 0) | — |
| 18:31:15 | 약 181 | **자동저장 발동** | `save.json` mtime `2026-09-14 18:31:15`, 기준 `2026-09-13 12:04:39` 에서 갱신 |

즉 게이트가 참인 시간 약 176초가 더 흘러 누적 180초에 도달한 시점에 발동했다. `IntervalSeconds=180`, `TickSeconds=1` 실측 확인.
기록 내용은 `SceneName="Tutorial Map"`(현재 씬), `SpawnPointName=""`, 체력 필드 없음. 다른 슬롯(`save_1`/`save_2`)은 `2026-09-13 12:04:39` 그대로 = D-01 준수.
자동저장 직후 체력은 **60/100 그대로** = 무회복 확인(D-07b). `SaveSlotDialog` 오브젝트 0건 = D-01 준수.

**일시정지 (D-03)**

18:32:38 에 `SetState(Paused)` + `timeScale=0` (PauseMenu 와 동일한 호출) 후 18:34:30 까지 관측:

- `Time.time` 이 263.4846 에서 **한 번도 증가하지 않았다** → `WaitForSeconds` 가 스케일 시간이므로 코루틴이 틱 자체를 못 한다. 카운트 정지가 구조적으로 보장된다(항목 14).
- 같은 구간에 `save.json` mtime 은 18:31:15 유지 = 저장 미발생(항목 15). 관측 길이는 112초이며 180초는 아니다 — 다만 위 `Time.time` 정지가 더 강한 증거다.
- 해제 후 `state=Playing`, `timeScale=1` 로 복귀하고 `Time.time` 이 다시 증가했다(항목 16).

**기존 수동 저장 무회귀 (D-03b — 절대 조건)**

| 항목 | 조작 | 결과 |
|---|---|---|
| 20 | `Paused` 상태에서 `SaveLoadManager.Instance.SaveAnywhere()` | mtime 18:35:06 으로 **정상 기록** → `SaveAnywhere()` 안에 상태 게이트가 없음이 런타임에서 증명됐다 |
| 21 | `SaveAtCheckpoint("Phase16TestCP")` | 체력 **60/100 → 100/100 회복**, `SpawnPointName="Phase16TestCP"` 기록 |
| 23 | 위 두 호출 | `SaveSlotDialog` 오브젝트 생성 0건 |

**알림 UI (D-10)**

`AutoSaveNotice.Show()`(타이머가 호출하는 공개 메서드) 를 직접 불러 실측:

| 확인 | 실측값 |
|---|---|
| 캔버스 | `AutoSave Notice Canvas`, `ScreenSpaceOverlay`, `sortingOrder=20000`, `ScaleWithScreenSize`, `refRes=(1920,1080)` |
| 부모 | `DontDestroyOnLoad` 하위(`AutoSave` 자식) → 씬 전환에 파괴되지 않음(항목 31 구조 근거) |
| 위치 | `anchorMin/Max=(1,0)`, `pivot=(1,0)`, `anchoredPosition=(-48,48)`, `sizeDelta=(420,60)` = 우하단 48px |
| 텍스트 | `자동 저장됨` (6자), 32pt, `BottomRight`, 흰색, `outlineWidth=0.2` |
| 입력 차단 없음 | `GraphicRaycaster` 없음, `label.raycastTarget=False` (항목 29) |
| 페이드 | `Show()` 직후 alpha=1 → 유지·페이드 후 alpha=0 확인 (`Hold=1.2`, `Fade=0.6`) |
| `timeScale=0` 중 페이드 | 알림을 띄운 직후 `timeScale=0` 으로 일시정지해도 alpha 가 **0 까지 내려갔다** → `unscaledDeltaTime` 사용 확인(항목 30) |
| 연속 호출 | `Show()` 2회 연속 → 자식 캔버스 1개·라벨 1개 유지, alpha 가 1 로 재시작 = 겹침 없음(항목 32) |

**한글 글리프 — 기존 '알려진 한계' 항목 해소**

`Build()` 가 폰트를 지정하지 않아 라벨의 주 폰트는 TMP 기본값 `LiberationSans SDF` 이고 이 폰트 자체는 한글이 없다(`HasCharacters("자동 저장됨")=False`).
그러나 프로젝트 전역 TMP 설정에 폴백이 하나 등록돼 있다: `TMP_Settings.fallbackFontAssets = [NotoSansKR-Regular SDF]` (`hasKorean=True`).
실제 렌더 결과를 문자 단위로 확인한 결과 한글 5자가 모두 이 폴백으로 해결됐다:

| 문자 | `isVisible` | 해석된 폰트 | glyphIndex |
|---|---|---|---|
| 자 | True | NotoSansKR-Regular SDF | 17311 |
| 동 | True | NotoSansKR-Regular SDF | 12264 |
| (공백) | False | LiberationSans SDF | 3 |
| 저 | True | NotoSansKR-Regular SDF | 17423 |
| 장 | True | NotoSansKR-Regular SDF | 17332 |
| 됨 | True | NotoSansKR-Regular SDF | 12343 |

→ **글리프 누락 없음.** 단, 이는 전역 폴백에 의존하는 결과다. 나중에 TMP 설정에서 `NotoSansKR-Regular SDF` 폴백을 제거하면 이 알림은 깨진다.

**측정을 중단시킨 조건 (남은 항목의 사유)**

18:35:20 에 `SceneManager.LoadScene("1 stage")` 로 씬을 옮긴 직후 **플레이어 루프가 1.83초만 진행되고 멈췄다**
(`frameCount` 75780 고정, `deltaTime=0`, `unscaledDeltaTime=0`, `timeScale=1`, `state=Playing`). `Application.runInBackground` 재설정·토글로도 되살아나지 않았다.
스케일 시간이 흐르지 않으면 `WaitForSeconds` 코루틴이 틱하지 못하므로 **시간 기반 항목을 더 측정할 수 없어 중단**했다.

> **[2차 실측 추가 — 이 중단의 원인이 규명됐다]** 플레이어 루프가 멈춘 것이 아니라 **에디터가 일시정지된 것이다.** 2차에서 `EditorApplication.isPaused` 를 직접 읽어 `True` 임을 확인했다.
> Console 의 **Error Pause 가 켜져 있고**, 씬 로드 직후 아래 표의 `MissingComponentException`([BUG-008](../15-load-timing-and-load-scope/bugs/BUG-008-tutorialboss-animator-missing.md))이 발생해 Play 모드가 자동으로 멈춘다.
> `EditorApplication.isPaused = false` 로 해제하면 즉시 되살아나며, Error Pause 를 끄면 재발하지 않는다. `runInBackground` 와는 무관했다. 이 조건을 제거한 뒤 남은 13항목을 전부 측정했다.

같은 로드 시점에 기존 문제 3건이 함께 찍혔다. **전부 Phase 16 코드와 무관하다:**

| 시각 | 종류 | 내용 |
|---|---|---|
| 18:24:09 | 경고 2 | `The referenced script (Unknown) on this Behaviour is missing!` (Play 진입 시점, 기존 씬 문제) |
| 18:35:20 | 경고 | `The referenced script on this Behaviour (Game Object 'PauseMenuPanel') is missing!` |
| 18:35:20 | 경고 | `Tutorial Boss에 SpriteRenderer가 없습니다` (`HP.cs:47`) |
| 18:35:20 | **에러 1** | `MissingComponentException: There is no 'Animator' attached to the "Tutorial Boss"` (`TutorialIdleState.cs:23` ← `TutorialBossController.cs:180`) = 이미 기록된 [BUG-008](../15-load-timing-and-load-scope/bugs/BUG-008-tutorialboss-animator-missing.md) |

Phase 16 스크립트가 만든 에러·경고는 **0건**이다.

**세이브 파일 처리 (체크리스트 34번)**

측정이 끝난 뒤 백업으로 **3개 슬롯 전부 복구했다**(`SpawnPointName=""` 로 원상 복귀). 측정 중 생성된 상태는
백업 폴더에 `save.json.after-test` 로 따로 보존했다. 프로젝트 파일 변경은 0건이다(`git status ProjectSettings/` 무변경 —
`PlayerSettings.runInBackground` 은 Play 모드 종료와 함께 원래 값 `False` 로 돌아왔다).

## 알려진 한계

- 사망 자동 로드 중 기존 매니저가 Loading 상태를 설정하지 않는 구간에는 Playing/PlayerStats 가드가 통과할 수 있다. 계획에서 이미 수용한 미검증 경쟁 구간이며, 실제 관측 시 별도 런타임 버그를 기록한다. 이번 페이즈에서 로드 경로를 변경하지 않았다. **(2차 실측에서 항목 26·27 로드 경로는 정상 동작을 확인했고 경쟁 구간의 오작동은 관측되지 않았다. 경쟁 자체를 배제하는 측정은 아니다.)**
- 1초 틱은 프레임 스케줄링에 의존한다. 계획의 '최대 1초 늦음'은 보장하지 않는다. **실측 결과 게이트가 참인 시간 약 176초 누적 후 발동해 D-04(180초)와 일치했다** — 다만 측정은 에디터 1회분이다.
- **자동저장 간격은 '게임이 실제로 도는 동안'만 흐른다.** `PlayerSettings.runInBackground=False` 이므로 창이 뒤로 가면 카운트가 멈춘다. 게임 자체가 정지하는 구간이라 데이터 손실 위험은 없지만, "3분마다"는 벽시계가 아니라 플레이 시간 기준이라는 뜻이다.
- **1차 실측의 '벽시계 240초 동안 `Time.time` 3.8초' 관측은 창 포커스 때문이 아니었다 — 원인은 [BUG-008](../15-load-timing-and-load-scope/bugs/BUG-008-tutorialboss-animator-missing.md)이다.** 2차 실측에서 `EditorApplication.isPaused=True` 를 직접 읽어 확인했다: Console 의 **Error Pause 가 켜져 있고**, `Tutorial Map` / `1 stage` 로드 직후 BUG-008 의 `MissingComponentException` 이 발생해 **Play 모드가 자동 일시정지**된다. 1차 실측이 '루프가 멈췄다'며 13항목을 포기한 실제 원인이 이것이다. 자동저장 로직과는 무관하다.
- .meta는 고유 GUID를 직접 생성했다. **Unity 임포트 후에도 `.meta` 는 변경되지 않았다**(`git status` 무변경, guid 유지) — 이 항목은 해소됐다.
- **TMP 글리프 한계는 해소됐다** — 전역 폴백 `NotoSansKR-Regular SDF` 가 한글 5자를 모두 공급한다(문자별 실측). 단 이 폴백을 TMP 설정에서 제거하면 알림이 깨진다.
- **알림의 화면 표시는 해소됐다.** `ScreenSpaceOverlay` 는 카메라 렌더(`source=camera`)에는 안 잡히지만 **합성 백버퍼 캡처(`unity command capture_game_view --source screen`)에는 잡힌다.** 2차 실측에서 우하단 "자동 저장됨" 이 읽히는 것을 1920x1080 캡처로 확인했다(항목 28). 단, 일시정지 중에는 게임 뷰가 다시 그려지지 않아 캡처가 **직전 프레임**을 돌려준다 — `EditorApplication.Step()` 으로 한 프레임 진행시킨 뒤 캡처해야 한다.
- ~~**Unity MCP 로는 기어 메뉴(`[ContextMenu]`)와 런타임 `Debug.Log` 를 쓸 수 없다**~~ — **이 제약은 Unity MCP 한정이며 Unity CLI 에서는 성립하지 않는다.** `mcp__unity-mcp__Unity_RunCommand` 는 `System.Reflection` 네임스페이스를 거부하지만, `unity command eval`(Roslyn)은 리플렉션을 허용하고 `unity command console` 은 런타임 게임 로그를 스택트레이스까지 돌려준다. 2차 실측은 이 경로로 항목 5·8·6 의 콘솔 문자열 기준을 전부 채웠다.

## Play 모드 체크리스트

**진행 상황: 29항목(4~32) 전부 실측 완료 — 29 PASS / 0 부분 / 0 미검증 / 0 FAIL.**
`[x]` 뒤의 (1차)는 2026-09-14 Unity MCP 실측, (2차)는 같은 날 Unity CLI(`unity command eval` / `console` / `capture_game_view`) 실측이다.
통과하지 않은 항목을 통과로 적지 않았다(Phase 9 선례). 번호는 16-03-PLAN.md 원문 번호다.

**사전 준비**
- [x] 1. (1차) Unity 임포트·컴파일 정상 — 에러 0, 클래스 2개 해석(`AutoSaveTimer` / `AutoSaveNotice`), `.meta` guid 변경 없음.
- [x] 2. (1차) 세이브 경로 확인 — `C:/Users/chang/AppData/LocalLow/DefaultCompany/AQUA ECLIPSE`.
- [x] 3. (1·2차) 세이브 3개 백업 완료. 2차도 측정 전 백업하고 측정 후 SHA-256 일치로 복구했다(34번 참조).

**A. 기본 발동 (D-04 / D-01 / D-07)**
- [x] 4. (1차) `AutoSave` 오브젝트 생성 확인 — `DontDestroyOnLoad` 소속, `AutoSaveTimer` + `AutoSaveNotice` 부착.
- [x] 5. (2차) `Phase16/2. Log Timer State` 실행 — 기어 메뉴가 바인딩하는 바로 그 메서드(`DebugLogTimerState`, `ContextMenu` 어트리뷰트 확인)를 리플렉션으로 호출했다. 콘솔 원문:
  `[AutoSaveTimer] elapsed=19/180 canAutoSave=True state=Playing hasPlayerStats=True slot=0`.
  연속 샘플에서 `elapsed` 가 `Time.time` 과 1:1로 증가했다(19.84→19, 21.59→21, 23.33→23).
- [x] 6. (1차) 3분 플레이 후 발동 — 게이트 참 상태로 약 176초 누적 뒤 18:31:15 저장. (2차) 콘솔 문자열도 확보했다: `[SaveLoadManager] Saved to ...\save.json`.
- [x] 7. (1차) `save.json` mtime 18:31:15 갱신, `SceneName="Tutorial Map"` 확인. 다른 슬롯 무변경.
- [x] 8. (2차) `Phase16/1. Force AutoSave Now` — `ContextMenu` 경로 `"Phase16/1. Force AutoSave Now"` 를 어트리뷰트로 확인한 뒤 호출했다.
  `elapsed` 95→0, `save.json` mtime 19:11:51→**19:27:33**, 콘솔 `[SaveLoadManager] Saved to ...\save.json`. 슬롯 대화상자는 생성되지 않았다.

**B. 메인메뉴에서 발동하지 않는다 (D-03c — 가장 중요)**
- [x] 9. (2차) `MainMenu` 로 이동해도 `AutoSave` 오브젝트가 살아 있다(`autoObjAlive=True`).
- [x] 10. (2차) 계획이 예측한 그대로다 — `state=Playing`(정상), `hasPlayerStats=False`, **`canAutoSave=False`**.
  콘솔 원문: `[AutoSaveTimer] elapsed=28/180 canAutoSave=False state=Playing hasPlayerStats=False slot=0`.
  `GameStateManager.HandleSceneLoaded()`(`GameStateManager.cs:81-88`)가 모든 씬 로드에서 `Playing` 을 강제하므로 두 번째 가드가 유일한 방어선임이 런타임에서 확인됐다.
- [x] 11. (2차) `Phase16/1. Force AutoSave Now` → 콘솔에 경고 원문 `[AutoSaveTimer] Gate blocked the write - see Phase16/2 for the reason.` 만 뜨고 **저장은 일어나지 않았다**(`save.json` mtime 19:31:04 유지).
- [x] 12. (2차) **슬롯 오염 차단 확인.** 계획의 "5분 방치"보다 강한 조건으로 측정했다: 카운터를 **179/180**(한 틱 뒤 발동)으로 무장한 뒤 메인메뉴에서 **298초** 방치했다.
  결과 `elapsed` 는 **179에서 단 한 번도 증가하지 않았고**(게이트가 증가 이전에 `continue` 한다), `save.json` 은 mtime 19:31:04 · `SceneName="Tutorial Map"` 그대로였다. **`SceneName` 이 `MainMenu` 로 바뀌지 않았다.**

**C. 일시정지 중 발동하지 않고 카운트도 멈춘다 (D-03)**
- [x] 13. (1차) 18:32:38 `SetState(Paused)` + `timeScale=0` 적용(PauseMenu 와 동일 호출).
- [x] 14. (1차) 18:34:30 까지 `Time.time` 이 263.4846 에서 전혀 증가하지 않았다. 스케일 시간이 멈추므로 `WaitForSeconds` 틱 자체가 불가능하고 카운트도 멈춘다.
- [x] 15. (1차) 같은 구간에 `save.json` mtime 18:31:15 유지 = 저장 미발생.
- [x] 16. (1차) 해제 후 `state=Playing`, `timeScale=1` 복귀 및 `Time.time` 재증가 확인.

**D. 수동 저장이 카운트를 리셋한다 (D-05)**
- [x] 17. (2차) `elapsed=50`(0 아님) 상태에서 일시정지 메뉴의 진행상황 저장을 **실제 UI 경로로** 실행했다 —
  `GameSettingsPanel.OnSaveProgressBtnClick()` → 슬롯 대화상자 열림(`state` Playing→Paused) → 슬롯 2 버튼 `onClick` → 확인 버튼 `"덮어쓰기"` `onClick`.
- [x] 18. (2차) 같은 호출에서 `elapsed` **50 → 0** 으로 리셋됐다. 선택한 슬롯에 기록됐다(`save_1.json` mtime 19:29:34, 콘솔 `Saved to ...\save_1.json`).
- [x] 19. (2차) 체크포인트도 동일 — `Checkpoint.Interact()` → 대화상자 → 슬롯 1 → 덮어쓰기. `elapsed` **90 → 0**, `save.json` mtime 19:31:04 · `SpawnPointName="check"` 기록.

**E. 기존 수동 저장 무회귀 (D-03b — 절대 조건)**
- [x] 20. (1차) `Paused` 상태에서 `SaveAnywhere()` 가 정상 기록됐다 = `SaveAnywhere()` 안에 상태 게이트가 없음이 런타임에서 증명됐다.
  (2차) **슬롯 대화상자 UI 전 구간까지 확인했다** — 제목 `"저장할 슬롯 선택"`, 슬롯 3개, 슬롯 클릭 시 제목이 `"슬롯 2의 데이터를 덮어쓰시겠습니까?"` 로 바뀌고 다른 슬롯이 `interactable=False` 가 되며, `"덮어쓰기"` 후 대화상자가 닫히고 `state` 가 `Playing` 으로 복귀했다.
  **완료 문구는 이 씬에서 표시되지 않는다** — `GameSettingsPanel.saveProgressFeedbackText` 가 미할당이기 때문이며(코드 주석 `// 저장 결과 안내 (선택 - 비워둬도 동작함)`), 콜백 자체는 호출된다. Phase 16 은 씬 0줄 변경이므로 **기존 상태**다.
- [x] 21. (1차) `SaveAtCheckpoint("Phase16TestCP")` → 체력 60/100 → **100/100 회복**, `SpawnPointName` 기록 확인.
- [x] 22. (1차) 자동저장은 체력을 건드리지 않았다 — 60/100 에서 자연 발동 후에도 60/100 유지.
- [x] 23. (1·2차) 자동저장·강제 호출 모두 `SaveSlotDialog` 를 만들지 않았다.

**F. 자동저장 후 실제 로드 (D-07 / D-08)**
- [x] 24. (2차) 체크포인트(`check`, `Tutorial Map`)를 기록한 뒤 `1 stage` 로 이동했고, 새 씬에서 **자연 발동**했다(강제 호출 아님) — 19:39:20 `Saved to ...\save.json`.
- [x] 25. (2차) 저장 직후 `save.json` 내용: `"SceneName": "1 stage"`(새 씬), `"SpawnPointName": ""`(**빈 문자열로 비워짐**). D-07 명세와 일치한다.
- [x] 26. (2차) `PlayerStats.Die()` → 자동 로드 확인. 콘솔 원문 `[SaveLoadManager] Restored stats: 100/100 (maxTotal 200), scene=1 stage, spawnPoint=`.
  **마지막 체크포인트(`Tutorial Map` 의 `check`)로 돌아가지 않고 `1 stage` 의 기본 시작 위치에서 풀피로 부활**했다 — D-08 이 수용한 의도된 동작 그대로다.
- [x] 27. (2차) 메인메뉴 → 이어하기 정상 동작. `SlotSelectPanel.OpenForLoad()` 목록이 **슬롯 1 = `1 stage`**(자동저장 결과), 슬롯 2·3 = `Tutorial Map` 으로 표시됐고 CTA 는 `"이어하기"` 였다.
  슬롯 1 버튼 `onClick` → 19:40:55 `Restored stats: 100/100 (maxTotal 200), scene=1 stage, spawnPoint=` 로 로드됐다.

**G. 알림 UI 세부 (D-10)**
- [x] 28. (2차) **육안 확인 완료.** 실제 자동저장으로 알림을 띄운 뒤 `capture_game_view --source screen` 1920x1080 합성 캡처에서 **우하단 "자동 저장됨" 이 흰 글씨로 선명하게 읽힌다**(좌상단 체력 HUD 와 겹치지 않음).
  라벨 실측: 월드 코너 `bl=(1452,48) tr=(1872,108)`, `alpha=1`, 폰트 `LiberationSans SDF` + 전역 폴백 `NotoSansKR-Regular SDF`, 한글 5자 전부 `visible=True`.
  사라짐도 측정했다: Show 기준 t=0 `alpha=1` → t=1.720s `alpha=0.125` → t=3.422s `alpha=0`. 코드 모델(Hold 1.2s + Fade 0.6s)의 t=1.72 예측값 0.133 과 일치하며 **1.8초 안에 사라진다**(D-10 의 "1~2초").
- [x] 29. (1차) 조작 차단 없음 — 캔버스에 `GraphicRaycaster` 없음, 라벨 `raycastTarget=False`.
- [x] 30. (1차) `timeScale=0`(일시정지) 중에도 alpha 가 0 까지 내려갔다 = `unscaledDeltaTime` 사용 확인, 화면에 얼어붙지 않는다.
- [x] 31. (1차) 씬 전환 후에도 `AutoSave` 오브젝트와 알림 캔버스·라벨이 살아 있었다. (2차) **새 씬에서의 발동 자체도 확인했다** — 항목 24 의 `1 stage` 자연 발동이 알림 경로까지 포함한다.
- [x] 32. (1차) `Show()` 2회 연속 호출 시 캔버스·라벨 1개를 재사용하고 alpha 가 1 로 재시작 = 겹침 없음.

**결과 기록**
- [x] 33. 위 표대로 기록했다. 미검증을 통과로 적지 않았다.
- [x] 34. 백업에서 3개 슬롯 전부 복구 완료. 1차는 `save.json.after-test`, 2차는 `save*.json.phase16-after-test` 로 측정 후 상태를 보존했고,
  복구본이 백업과 **SHA-256 일치**함을 3슬롯 모두 확인했다.

## 2차 실측 수단과 환경 (재현용)

- 수단: **Unity CLI**(`unity 1.0.0-beta.9`) → `unity status`(port 7801, Unity 6000.3.10f1, state ready) → `unity command <name>`.
  사용한 명령: `eval`(Roslyn C#, 리플렉션 허용), `console`(런타임 게임 로그 + 스택트레이스), `capture_game_view --source screen`(오버레이 캔버스 포함 합성 캡처), `editor_status` / `editor_stop`, `wait_for`(장시간 대기), `get_build_settings`.
- **측정 전용 임시 변경 2건 — 둘 다 원복했다.**
  1. `PlayerSettings.runInBackground` 를 측정 동안 `true` 로 두었다(창 포커스와 무관하게 게임이 돌도록). 측정 후 `false` 로 복구했고 `git status --porcelain ProjectSettings/` 가 **빈 출력**임을 확인했다.
  2. Console 의 **Error Pause** 를 껐다(BUG-008 예외마다 Play 가 멈춰 측정이 불가능했다). 측정 후 다시 켰다.
- 프로젝트 파일 영향 0: `Assets/` 의 `git status` 항목 집합이 세션 시작과 동일하고, `AutoSaveTimer.cs`(17:38) / `AutoSaveNotice.cs`(17:34) / `SaveLoadManager.cs`(17:28) / `Tutorial Map.unity`(9/13 11:51) / `1 stage.unity`(9/13 11:51) / `MainMenu.unity`(9/10 20:20) 의 mtime 이 모두 **첫 에디터 명령(19:16:22) 이전**이다.
- 참고: `Assets/_Recovery/0 (3).unity`(untracked, 12MB)는 mtime **19:13** 으로 첫 에디터 명령보다 앞서며 이 검증이 만든 것이 아니다. Unity 의 씬 복구 백업이라 임의로 지우지 않았다.

## 현재 상태

- 16-01 완료, 16-02 완료(각 구현 커밋 2개 및 SUMMARY 커밋).
- 16-03 Task 1 완료: 원문 회귀 18 PASS / 4 FAIL(검증 명세 BUG-010), 대체 회귀 22 PASS. Assets 변경 없음.
- 16-03 Task 2 **완료**: Play 모드 29항목 **29 PASS / 0 FAIL / 0 미검증**.
  1차(Unity MCP)에서 15항목, 2차(Unity CLI)에서 나머지 13항목과 항목 28 의 육안 확인을 채웠다.
  플랜의 완료 조건인 **B 그룹(메인메뉴 미발동)과 E 그룹(수동 저장 무회귀)이 모두 실측 통과**했다.
- 신규 버그 0건. 측정 중 관측된 `MissingComponentException` 은 기존 미해결 [BUG-008](../15-load-timing-and-load-scope/bugs/BUG-008-tutorialboss-animator-missing.md) 이며 Phase 16 과 무관하다.
- 세이브 파일: 백업에서 SHA-256 일치로 복구 완료. 프로젝트 파일·설정 변경 0건.
- Phase 16 은 검증 완료 상태다.
