# Phase 16 — 일정 간격 자동저장 검증 상태

**요약:** 16-01/16-02 구현 및 컴파일 완료. AutoSaveTimer가 플레이 가능 시간 180초마다 현재 슬롯을 저장하고 AutoSaveNotice가 우하단 알림을 표시하도록 연결했다.
정적 회귀 원문 기준 18/22 PASS, 4건 FAIL(검증 명세 오류 [BUG-010](bugs/BUG-010-plan-acceptance-false-positives.md)). 주석/기존 변경을 구분한 대체 검사 22/22 PASS.
Unity Play 모드 실측은 전부 미검증이며, 16-03 Task 2 사용자 체크포인트에서 대기한다.

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


## 알려진 한계

- 사망 자동 로드 중 기존 매니저가 Loading 상태를 설정하지 않는 구간에는 Playing/PlayerStats 가드가 통과할 수 있다. 계획에서 이미 수용한 미검증 경쟁 구간이며, 실제 관측 시 별도 런타임 버그를 기록한다. 이번 페이즈에서 로드 경로를 변경하지 않았다.
- 1초 틱은 프레임 스케줄링에 의존한다. 계획의 '최대 1초 늦음'은 보장하지 않는다. 180회 대기에 프레임 지연이 누적될 수 있으므로 실제 3분 근처 발동 시각은 A 그룹에서 측정한다.
- .meta는 고유 GUID를 직접 생성했다. Unity 임포트 후 추가 메타 필드가 생기면 확인한다. 이번 세션에서 Unity 에디터 검증은 수행하지 않았다.
- 한국어 바이트는 검증했지만 TMP 기본 폰트의 글리프와 화면 배치는 G 그룹에서 확인해야 한다.

## Play 모드 체크리스트 (사용자 확인 필요)

현재 모든 항목 미검증. 아래 번호는 16-03-PLAN.md 원문 번호다. 실제 대상 파일은 CurrentSlot에 따라 save.json / save_1.json / save_2.json으로 바꿔 확인한다.

**사전 준비**
- [ ] 1. Unity 에디터를 연다. 신규 스크립트 2개가 임포트되며 컴파일된다. **Console 에 컴파일 에러가 0건**인지 먼저 확인한다. (`.meta` 는 이미 만들어 커밋했으므로 guid 가 새로 생기지 않아야 한다. `.meta` 가 수정되면 그 변경은 커밋한다.)
- [ ] 2. 세이브 파일 위치를 알아 둔다: `%USERPROFILE%\AppData\LocalLow\<회사명>\<프로젝트명>\save.json` (슬롯 0). 파일의 **수정 시각**과 `"SceneName"` 값을 이 검증 동안 계속 확인하게 된다.
- [ ] 3. 검증 전에 현재 `save.json` / `save_1.json` / `save_2.json` 을 다른 폴더로 복사해 **백업**해 둔다 (자동저장이 덮어쓴다).

**A. 기본 발동 (D-04 / D-01 / D-07)**
- [ ] 4. 게임 씬(예: `Tutorial Map`)에서 Play 를 누르고, Hierarchy 에 `AutoSave` 오브젝트가 생성됐는지 확인한다.
- [ ] 5. `AutoSave` 선택 → `AutoSaveTimer` 컴포넌트 기어 메뉴 → **`Phase16/2. Log Timer State`** 실행. `canAutoSave=True`, `state=Playing`, `hasPlayerStats=True`, `elapsed` 이 초당 1씩 늘고 있는지 확인한다.
- [ ] 6. 그대로 **3분 이상 플레이**한다(가만히 서 있어도 된다). 3분 근처에 우하단에 `자동 저장됨` 이 뜨고 1~2초 안에 사라지는지, Console 에 `[SaveLoadManager] Saved to ...` 가 찍히는지 확인한다.
- [ ] 7. `save.json` 의 수정 시각이 방금으로 갱신됐고 `"SceneName"` 이 **현재 플레이 중인 씬 이름**인지 확인한다.
- [ ] 8. (빠른 반복용) 기어 메뉴 → **`Phase16/1. Force AutoSave Now`** 를 누르면 3분을 기다리지 않고 같은 경로가 실행된다. 이후 항목은 이걸로 확인해도 된다.

**B. 메인메뉴에서 발동하지 않는다 (D-03c — 가장 중요)**
- [ ] 9. 게임 중 메인메뉴로 나간다(또는 MainMenu 씬에서 Play 시작).
- [ ] 10. `AutoSave` 선택 → `Phase16/2. Log Timer State` → **`canAutoSave=False`**, `hasPlayerStats=False` 여야 한다. (`state=Playing` 으로 보이는 것은 정상이다 — 그래서 두 번째 가드가 있다.)
- [ ] 11. `Phase16/1. Force AutoSave Now` 를 눌러 본다 → Console 에 `[AutoSaveTimer] Gate blocked the write` 경고가 뜨고 **저장은 일어나지 않아야 한다.**
- [ ] 12. 메인메뉴에서 5분 이상 방치한 뒤 `save.json` 의 수정 시각이 **그대로**이고 `"SceneName"` 이 `MainMenu` 로 바뀌지 **않았는지** 확인한다. → 바뀌었다면 즉시 중단하고 보고할 것 (슬롯 오염 버그).

**C. 일시정지 중 발동하지 않고 카운트도 멈춘다 (D-03)**
- [ ] 13. 게임 중 `Phase16/2` 로 `elapsed` 값을 적어 둔다. ESC 로 일시정지한다.
- [ ] 14. 일시정지 상태로 1분 이상 둔 뒤 다시 `Phase16/2` 실행 → `elapsed` 가 **거의 그대로**이고 `canAutoSave=False`, `state=Paused` 여야 한다.
- [ ] 15. 일시정지 중 `Phase16/1. Force AutoSave Now` → 경고만 뜨고 저장되지 않아야 한다.
- [ ] 16. 일시정지를 해제하면 `elapsed` 가 다시 늘어난다.

**D. 수동 저장이 카운트를 리셋한다 (D-05)**
- [ ] 17. `Phase16/2` 로 `elapsed` 를 확인한 뒤(0 이 아닌 값), 일시정지 메뉴에서 **진행상황 저장**을 실행한다(슬롯 선택 대화상자 → 저장).
- [ ] 18. 다시 `Phase16/2` → `elapsed` 가 **0 근처**로 리셋됐는지 확인한다.
- [ ] 19. 체크포인트에서도 같은 것을 확인한다: 체크포인트 상호작용(저장) 후 `elapsed` 가 0 근처다.

**E. 기존 수동 저장 무회귀 (D-03b — 절대 조건)**
- [ ] 20. 일시정지 메뉴 → 진행상황 저장이 **여전히 정상 동작**하는지 확인한다(슬롯 대화상자가 뜨고, 선택한 슬롯에 저장되고, 완료 문구가 뜬다). → 이게 막히면 D-03b 위반이다. 즉시 보고.
- [ ] 21. 체크포인트 상호작용 저장도 정상 동작하고 **체력이 풀회복**되는지 확인한다(체크포인트만의 동작이다).
- [ ] 22. 자동저장(`Phase16/1`)으로는 **체력이 회복되지 않는지** 확인한다 — 체력을 일부 깎은 상태에서 강제 자동저장 후 체력 변화가 없어야 한다.
- [ ] 23. 자동저장 시 **슬롯 선택 대화상자가 뜨지 않는지** 확인한다 (D-01).

**F. 자동저장 후 실제 로드 (D-07 / D-08)**
- [ ] 24. 체크포인트를 밟은 뒤 다른 씬으로 이동해서 자동저장을 일으킨다(`Phase16/1`).
- [ ] 25. `save.json` 의 `"SceneName"` 이 새 씬이고 `"SpawnPointName"` 이 **빈 문자열**인지 확인한다.
- [ ] 26. 플레이어를 죽여 자동 로드가 걸리게 한다 → 새 씬의 **기본 시작 위치**에서 풀피로 부활하는지 확인한다(D-08 이 수용한 의도된 동작이다. 마지막 체크포인트로 돌아가지 않는 것이 정상이다).
- [ ] 27. 메인메뉴 → 이어하기 → 해당 슬롯 로드도 정상 동작하는지 확인한다.

**G. 알림 UI 세부 (D-10)**
- [ ] 28. 알림이 **우하단**에 뜨고 글자가 읽히는지, 1~2초 안에 사라지는지 확인한다.
- [ ] 29. 알림이 떠 있는 동안 **버튼 클릭/조작이 막히지 않는지** 확인한다.
- [ ] 30. 알림이 떠 있는 동안 ESC 로 일시정지해도 알림이 화면에 얼어붙지 않고 사라지는지 확인한다.
- [ ] 31. 씬을 전환한 뒤에도 자동저장 알림이 정상적으로 뜨는지 확인한다(캔버스가 씬과 함께 파괴되지 않아야 한다).
- [ ] 32. 연속으로 `Phase16/1` 을 두 번 누르면 알림이 겹치지 않고 다시 처음부터 표시되는지 확인한다.

**결과 기록**
- [ ] 33. 통과/실패를 보고한다. **통과하지 않은 항목을 통과로 보고하지 않는다** (Phase 9 선례: 검증을 생략했으면 "생략"으로 남긴다).
- [ ] 34. 검증이 끝나면 3단계에서 백업한 세이브 파일을 원래대로 복구할지 결정한다.

## 현재 상태

- 16-01 완료, 16-02 완료(각 구현 커밋 2개 및 SUMMARY 커밋).
- 16-03 Task 1 완료: 원문 회귀 18 PASS / 4 FAIL(검증 명세 BUG-010), 대체 회귀 22 PASS. Assets 변경 없음.
- 16-03 Task 2: 사용자 Play 모드 결과 대기. A~G 실행 점검 29항목(4~32) 중 통과 0 / 실패 0 / 미검증 29. 사전 준비·결과 기록 항목도 미확인.
- 세이브 파일 백업/복구 여부: 사용자 실측 미시작으로 미결정. 이번 세션에서 세이브 파일을 읽거나 쓰지 않았다.
- Phase 16 완료 처리와 최종 단계 검증은 Play 모드 결과 수신 이후 진행한다.
