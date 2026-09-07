# SaveSystem — Newtonsoft.Json 기반 세이브/로드 매니저 Play 모드 검증 체크리스트

Phase 11 (D-01 ~ D-06) 최종 검증 문서. UI 가 범위 밖(D-04)이라 저장/로드를 눌러볼 메뉴가 없으므로,
`SaveLoadManager` 컴포넌트의 인스펙터 톱니바퀴(Context Menu)로 저장/로드/새게임을 직접 호출해
Play 모드에서 실측 검증한다. 이 문서는 `Assets/Camera/Check.md` 의 서술 형식(정적 검사 표 + Play
모드 체크리스트 + 결과 기록 섹션)을 그대로 따른다.

## 검증 대상 변경사항

- `Packages/manifest.json` — Newtonsoft.Json 3.2.2 직접 의존성 고정 (Phase 11 Plan 1)
- `Assets/SaveSystem/Script/SaveData.cs` — 신규. 저장 스키마 POCO (`SceneName`/`SpawnPointName` 문자열,
  `PlayerStatsSaveData`, `BossProgress`/`MapGimmickState` Dictionary 스텁, `Items` List 스텁)
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — 신규. `DontDestroyOnLoad` 싱글톤(씬 배치 불필요,
  `RuntimeInitializeOnLoadMethod` 부트스트랩 자동 생성), 메모리 캐시, `save.json` 단일 슬롯 I/O,
  코루틴 기반 `LoadSceneAsync` + 스탯 복원. 이번 플랜에서 ContextMenu 검증 훅 4개 추가.
- `Assets/Player/Script/PlayerStats.cs` — `RestoreStats(float, float, float)` additive 메서드 추가
- 저장 트리거 5곳 (Group A / Group B, 아래 참고)

## 저장 트리거 — Group A / Group B (두 아키텍처)

| Group | 보스/트리거 | 파일 | 통합 지점 |
|---|---|---|---|
| (체크포인트) | Checkpoint (S키) | `Assets/map/script/Checkpoint.cs` | S키 활성화 로직 안에서 `SaveLoadManager.Instance.SaveAtCheckpoint(gameObject.name)` 직접 호출 |
| **Group A** | TutorialBoss | `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs` | `HP.OnDeath` 이벤트가 이미 구독되어 있는 `HandleDeath()` 안에 삽입 |
| **Group A** | WoodBoss | `Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs` | 동일하게 `HP.OnDeath` → `HandleDeath()` 경로 안에 삽입 |
| **Group B** | WaterSpirit | `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs` | `OnDeath` 이벤트 자체가 없음 — `BossStatsSystem.Die()` 오버라이드 본문 안에 직접 삽입 |
| **Group B** | WaterMonster | `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` | 동일하게 `BossStatsSystem.Die()` 오버라이드 본문 안에 직접 삽입 |

Group A 와 Group B 는 서로 다른 코드 경로이기 때문에, 한쪽만 저장되고 다른 쪽은 조용히 저장이
안 되는 실패가 컴파일 단계에서는 절대 드러나지 않는다. 이것이 Play 모드 체크리스트에서
**두 그룹을 각각 최소 1종씩** 반드시 확인해야 하는 이유다.

## 사전 준비

1. Unity 에디터를 열고 컴파일이 끝날 때까지 대기한다. Console 에 **에러 0건**인지 확인한다.
2. 아무 씬에서나 Play 를 시작한다 — `SaveLoadManager` 는 `RuntimeInitializeOnLoadMethod` 로
   자동 생성되므로 씬에 미리 배치할 필요가 없다.
3. Hierarchy 최상단(DontDestroyOnLoad 섹션 포함)에서 `SaveLoadManager` GameObject 를 선택한다.
4. Inspector 에서 `Save Load Manager (Script)` 컴포넌트의 톱니바퀴(gear) 아이콘을 클릭하면
   `Phase11/1. Save Now`, `Phase11/2. Load Game`, `Phase11/3. New Game (memory only)`,
   `Phase11/4. Log State` 4개 메뉴가 보인다.
5. 저장 파일 경로 찾는 법: `Phase11/4. Log State` 실행 → Console 에 절대 경로가 출력된다
   (`Application.persistentDataPath`, Windows 기준
   `%userprofile%/AppData/LocalLow/<회사명>/<제품명>/save.json`).

## 섹션 1) 정적 회귀 검사

아래 15개 명령을 실제로 실행하고 결과를 기록했다. 실행 환경: 이 저장소 루트, 커밋 `f4ac2b4`
기준 (Task 1 커밋 직후). 비교 baseline 커밋(check #15): `efcfc19` (Phase 11 시작 직전 마지막 커밋).

| # | 명령 | 기대값 | 실측값 | 판정 |
|---|------|--------|--------|------|
| 1 | `grep -c '"com.unity.nuget.newtonsoft-json": "3.2.2"' Packages/manifest.json` | 1 | 1 | PASS |
| 2 | `grep -c "SaveLoadManager.Instance.SaveAtCheckpoint" Assets/map/script/Checkpoint.cs` | 1 | 1 | PASS |
| 3 | `grep -rn "SaveLoadManager.Instance.SaveOnBossDefeated" Assets/ \| wc -l` | 4 | 4 | PASS |
| 4 | `grep -rno 'SaveOnBossDefeated("[A-Za-z]*")' Assets/ \| sort -u -t: -k3 \| wc -l` | 4 (bossId 4종이 서로 다름) | 4 (TutorialBoss, WoodBoss, WaterSpirit, WaterMonster) | PASS |
| 5 | `grep -rnE "\basync\b\|\bawait\b" Assets/SaveSystem/ \| wc -l` | 0 (코루틴 전용 컨벤션) | 0 | PASS |
| 6 | `grep -c "PlayerSpawner.targetSpawnPointName = spawnPointName;" Assets/SaveSystem/Script/SaveLoadManager.cs` | 1 | 1 | PASS |
| 7 | `grep -cE "float +x\|float +y\|Vector2\|Vector3" Assets/SaveSystem/Script/SaveData.cs` | 0 (D-05: 원시 좌표 저장 금지) | 0 | PASS |
| 8 | `grep -cP "[^\x00-\x7F]" Assets/map/script/Checkpoint.cs` | 9 (CP949 보존) | 9 | PASS |
| 9 | `grep -cP "[^\x00-\x7F]" Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs` | 7 (CP949 보존) | 7 | PASS |
| 10 | `grep -cP "[^\x00-\x7F]" Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs` | 92 | 92 | PASS |
| 11 | `grep -cP "[^\x00-\x7F]" Assets/Enemy/WaterSpirit/Script/SpiritStats.cs` | 8 | 8 | PASS |
| 12 | `grep -cP "[^\x00-\x7F]" Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` | 8 | 8 | PASS |
| 13 | `grep -cP "[^\x00-\x7F]" Assets/Player/Script/PlayerStats.cs` | 12 | 12 | PASS |
| 14 | `git status --porcelain Assets/Script/HP.cs Assets/Enemy/NewBoss/Script/BossStatesSystem.cs Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs Assets/map/script/portal.cs Assets/map/script/GameManager.cs Assets/Script/MainMenuUI.cs` | 출력 없음 (불가침 6파일) | 출력 없음 | PASS |
| 15 | Phase 11 이 수정한 7개 파일(manifest.json, PlayerStats.cs, Checkpoint.cs, TutorialBossController.cs, WoodBossController.cs, SpiritStats.cs, WaterMonsterStats.cs)의 `git diff --numstat efcfc19` 삭제 라인 합계 | 0 (전부 순수 삽입) | 0 (manifest.json 1/0, PlayerStats.cs 13/0, Checkpoint.cs 5/0, TutorialBossController.cs 5/0, WoodBossController.cs 4/0, SpiritStats.cs 5/0, WaterMonsterStats.cs 5/0 — 전부 삽입만, 삭제 0) | PASS |

**15개 항목 전부 PASS.**

## 섹션 2) Play 모드 체크리스트 — 저장 (D-01, D-02)
- [v] Play 진입 직후 Hierarchy 에 `SaveLoadManager` GameObject 가 자동 생성되어 있고 DontDestroyOnLoad 섹션에 있다 (씬에 수동 배치하지 않았음에도)
- [v] 컨텍스트 메뉴 `Phase11/4. Log State` 실행 → Console 에 `path=...save.json exists=False` (첫 실행 시)
- [v] `1 stage` 씬에서 체크포인트에 들어가 S키를 누른다 → Console 에 `[SaveLoadManager] Saved to <경로>` 가 출력된다
- [v] 출력된 경로(`Application.persistentDataPath`, Windows 기준 `%userprofile%/AppData/LocalLow/<회사명>/<제품명>/save.json`)를 탐색기로 열어 `save.json` 이 실제로 생성되었는지 확인한다
- [v] `save.json` 내용에 `"SceneName": "1 stage"` 와 `"SpawnPointName": "<S키를 누른 체크포인트 GameObject 이름>"` 이 들어 있다
- [v] `save.json` 에 `"PlayerStats"` 의 Health/MaxHealth/MaxTotalHealth 가 당시 실제 체력과 일치한다
- [v] `save.json` 에 `"BossProgress": {}`, `"MapGimmickState": {}`, `"Items": []` 스텁이 존재한다 (D-03, D-03b)
- [v] 체크포인트 저장 이후 그냥 돌아다니는 동안 Console 에 `Saved to` 로그가 추가로 찍히지 않는다 (플레이 중 파일 I/O 0회)

## 섹션 3) Play 모드 체크리스트 — 보스 격파 자동 저장 (D-01, 두 아키텍처)
- [ ] **Group A / TutorialBoss** (`HP.OnDeath` → `HandleDeath()` 경로): 튜토리얼 보스 처치 → Console 에 `Saved to` 출력 → `save.json` 의 `BossProgress` 에 `"TutorialBoss": true` 추가
- [ ] **Group A / WoodBoss** (`HP.OnDeath` → `HandleDeath()` 경로): 우드 보스 처치 → `BossProgress` 에 `"WoodBoss": true` 추가 (클리어 패널이 뜨기 전에 저장이 끝나야 함)
- [ ] **Group B / WaterSpirit** (`BossStatsSystem.Die()` 오버라이드 경로 — 이벤트 없음): 물의 정령 처치 → `BossProgress` 에 `"WaterSpirit": true` 추가 — **이 항목이 Group B 의 조용한 실패를 잡는 핵심 체크다**
- [ ] **Group B / WaterMonster** (`BossStatsSystem.Die()` 오버라이드 경로 — 이벤트 없음): 물괴물 처치 → `BossProgress` 에 `"WaterMonster": true` 추가
- [ ] 보스 격파 저장 후에도 `SceneName` / `SpawnPointName` 이 **직전 체크포인트 값 그대로**다 (보스 위치로 바뀌지 않음 — 연구 Open Question 1 확정 사항)
- [ ] 물의 정령 분신(IsDummy)이 사라질 때는 저장이 발생하지 않는다 (Console 에 추가 `Saved to` 없음)

## 섹션 4) Play 모드 체크리스트 — 로드 (D-05, 비동기 씬 로드)
- [ ] `MainMenu` 씬에서 Play 시작 → `SaveLoadManager` 컨텍스트 메뉴 `Phase11/2. Load Game` 실행
- [ ] 씬이 저장된 `SceneName` 으로 전환되고, 전환 중 프레임이 멈추지 않는다 (`LoadSceneAsync`)
- [ ] 전환 완료 후 플레이어가 저장된 체크포인트 위치에 서 있다 (`PlayerSpawner` 경로 재사용)
- [ ] Console 에 `[SaveLoadManager] Restored stats: <health>/<maxHealth> (maxTotal ...), scene=..., spawnPoint=...` 가 출력되고 값이 `save.json` 과 일치한다 — `PlayerStats.Instance is null after scene load` 에러가 **없어야** 한다 (연구 Pitfall 5)
- [ ] 체력 UI 가 복원된 체력을 반영한다 (`ClampHealth()` 가 `onHealthChangedCallback` 을 호출하므로)
- [ ] 저장 파일이 없는 상태에서 `Phase11/2. Load Game` 실행 → `LoadGame aborted - no save file at ...` 경고만 뜨고 씬 전환이 일어나지 않는다

## 섹션 5) Play 모드 체크리스트 — 새 게임 (D-06)
- [ ] `save.json` 이 존재하는 상태에서 `Phase11/3. New Game (memory only)` 실행
- [ ] Console 에 `NewGame - memory reset only, file untouched.` 출력
- [ ] 디스크의 `save.json` 파일 내용과 수정 시각이 **변하지 않았다** (D-06 핵심)
- [ ] 이후 체크포인트 S키를 누르면 그 시점에 파일이 새 내용으로 덮어써진다

## 섹션 6) 알려진 한계 / 범위 밖
- 메인 메뉴 "이어하기" 버튼 연동은 이번 페이즈 범위 밖 (D-04). `LoadGame()` 은 컨텍스트 메뉴로만 호출 가능하다.
- 다중 슬롯 미지원 (D-02).
- `MapGimmickState` 와 `Items` 는 스키마만 존재하고 실제로 채워 넣는 코드가 없다 (D-03, D-03b).
- `Assets/Scenes/InGame.unity` 는 Build Settings 에 등록되어 있으나 파일이 존재하지 않는 스테일 엔트리다. `MainMenuUI.OnClickStart()` 가 이 씬을 로드하려 하며 실패한다 — **기존 이슈이며 이번 페이즈에서 수정하지 않았다** (D-04 UI 범위 밖). 새 게임 기본 씬은 `1 stage` 로 설정되어 있다.
- `Portal.cs` / `GameManager.NextSpawnPointName` / `WoodBossStatSystem.cs` 는 고아 코드이며 이번 페이즈에서 언급만 하고 건드리지 않았다.

## 결과 기록

정적 회귀 검사(11-04-PLAN.md Task 2, 2026-08-10)는 위 섹션 1의 15개 항목 전부 통과했다:
Newtonsoft.Json 버전 고정, 5개 저장 트리거 연결(체크포인트 1 + 보스 4종 서로 다른 bossId),
무-async/await(코루틴 전용), 원시 좌표 미사용(D-05), CP949 인코딩 보존 6파일, 불가침 파일 6종
무변경, Phase 11 전체 수정 파일 순수 삽입(삭제 0줄).

Play 모드 실측 검증(Task 3)은 아직 수행되지 않았다 — 위 섹션 2~5 체크리스트는 사용자가 직접
Unity 에디터에서 확인해야 한다. PASS 로 허위 기록하지 않는다.

---

> **참고 (2026-08-27):** 이전에 이 자리에는 "Phase 14 — 키바인딩 keybind.json 위임 검증" 섹션이
> 있었다. 해당 Phase 14(키바인딩을 전용 `keybind.json`으로 저장)는 quick task `260827-h5y`가
> 구현한 `setting.json`(`SettingsData`) 통합 설정 방식으로 대체되며 폐기되어, 그 구현 코드와
> 함께 이 검증 섹션도 제거했다. 키바인딩은 이제 `setting.json`의 `InputBindingsJson` 필드로
> 저장된다.

## Phase 14 — 세이브 슬롯 확장 (3슬롯) 검증

### (A) 검증 대상 변경사항

- `Assets/SaveSystem/Script/SaveLoadManager.cs` — `SlotCount`(3), `CurrentSlot`, `GetSavePath(int)`, `SelectSlot(int)`, `HasSaveFile(int)`, `PeekSlotData(int)`, `NewGameInSlot(int)`, `LoadSlot(int)` 추가, `SavePath` static → instance 전환, `Phase14/` ContextMenu 훅 4개 추가
- `Assets/Script/SlotSelectPanel.cs` — 신규
- `Assets/Script/OverwriteConfirmPanel.cs` — 신규
- `Assets/Script/MainMenuUI.cs` — `OnClickStart`/`OnClickLoad` D-01~D-03 재배선
- 0줄 변경 계약: `SaveData.cs`, `Checkpoint.cs`, `TutorialBossController.cs`, `SpiritStats.cs`, `WaterMonsterStats.cs`

### (B) 슬롯 파일 매핑

| 슬롯 | 파일명 | 비고 |
|---|---|---|
| 0 | `save.json` | Phase 11 파일명 그대로. 기존 플레이어 데이터가 자동으로 슬롯 0이 됨 — 마이그레이션 코드 0줄 |
| 1 | `save_1.json` | 신규 |
| 2 | `save_2.json` | 신규 |

경로: `Application.persistentDataPath` (Windows: `%userprofile%/AppData/LocalLow/<회사명>/<제품명>/`)

### (C) 씬 배선 가이드 — `Assets/Scenes/MainMenu.unity` (사용자 수동 작업)

기존 MainMenu Canvas를 사용한다. Canvas Scaler는 `Scale With Screen Size`, Reference Resolution은 1920x1080, `Match Width Or Height`는 **0.5**로 유지한다.

1. Canvas 아래에 전체 스트레치 `SlotSelectPanel`을 만들고 Image 색을 `{r: 0.06, g: 0.08, b: 0.12, a: 0.96}`로 설정한 뒤 `SlotSelectPanel` 컴포넌트를 추가한다.
2. 자식 `Title` TMP를 만들고 NotoSansKR-Regular, 32, 흰색으로 설정한다. 텍스트는 비워 둔다.
3. `CardRow`에 HorizontalLayoutGroup을 추가하고 Padding 48, Spacing 32, Middle Center로 설정한다. Title과의 세로 간격은 64다.
4. `SlotCard0/1/2`를 순서대로 만든다. 배경 `{r: 0.078, g: 0.102, b: 0.141, a: 0.90}`, Highlighted `{r: 0.149, g: 0.451, b: 0.749, a: 1}`, Min Height 96, 내부 Padding 24, Spacing 8로 설정한다. 각 카드에 Label(20), Body(16), Cta(20) TMP를 둔다.
5. 전체 스트레치 `OverwriteConfirmPanel`을 만들고 기본 비활성화한다. 배경은 `{r: 0.06, g: 0.08, b: 0.12, a: 0.96}`, Padding 24, Title 32, Body 16, 간격 16이다. ConfirmButton은 Min Height 64, Padding 16, Normal `{r: 0.55, g: 0.18, b: 0.18, a: 1}`, Highlighted `{r: 0.72, g: 0.28, b: 0.28, a: 1}`, Pressed `{r: 0.38, g: 0.12, b: 0.12, a: 1}`로 설정한다. CancelButton은 빨강이 아닌 보조색을 쓴다.

| 컴포넌트 | 필드 | 연결 대상 |
|---|---|---|
| `MainMenuUI` | `Slot Select Panel` | `SlotSelectPanel` GameObject |
| `MainMenuUI` | `New Game Scene Name` | `Tutorial Map` |
| `MainMenuUI` | `Load Game Button` | 기존 Load Game 버튼 |
| `SlotSelectPanel` | `Title Text` | `Title` TMP |
| `SlotSelectPanel` | `Slot Buttons` (size 3) | `SlotCard0/1/2` Button |
| `SlotSelectPanel` | `Slot Label Texts` (size 3) | 각 카드 Label TMP |
| `SlotSelectPanel` | `Slot Body Texts` (size 3) | 각 카드 Body TMP |
| `SlotSelectPanel` | `Slot Cta Texts` (size 3) | 각 카드 Cta TMP |
| `SlotSelectPanel` | `Overwrite Confirm Panel` | `OverwriteConfirmPanel` GameObject |
| `OverwriteConfirmPanel` | 텍스트 필드 4개 | 각 TMP |

| 버튼 | OnClick 대상 | 메서드 | 인자 |
|---|---|---|---|
| `SlotCard0` | `SlotSelectPanel` | `OnClickSlot(int)` | **0** |
| `SlotCard1` | `SlotSelectPanel` | `OnClickSlot(int)` | **1** |
| `SlotCard2` | `SlotSelectPanel` | `OnClickSlot(int)` | **2** |
| 뒤로 버튼 | `SlotSelectPanel` | `OnClickBack()` | - |
| `ConfirmButton` | `OverwriteConfirmPanel` | `OnClickConfirm()` | - |
| `CancelButton` | `OverwriteConfirmPanel` | `OnClickCancel()` | - |

`SlotSelectPanel`과 `OverwriteConfirmPanel`은 비활성 상태로 저장한다. 카드와 다이얼로그 문구는 스크립트 상수에서 채우므로 Inspector에 한글을 직접 입력하지 않는다.

### (D) 정적 회귀 검사

| # | 명령 | 기대값 | 실측값 | 판정 |
|---|---|---|---|---|
| 1 | `SlotCount = 3` 검색 | 1 | 1 | PASS |
| 2 | `SaveFileName = "save.json"` 검색 | 1 | 1 | PASS |
| 3 | `File.(Move\|Copy\|Delete)` 검색 | 0 | 0 | PASS |
| 4 | `File.WriteAllText` 검색 | 2 | 2 | PASS |
| 5 | `_data = ` 검색 | 3 | 3 | PASS |
| 6 | SaveLoadManager 비 ASCII 검색 | 0 | 0 | PASS |
| 7 | MainMenuUI 비 ASCII 검색 | 0 | 0 | PASS |
| 8 | MainMenuUI `LoadGame()` 검색 | 0 | 0 | PASS |
| 9 | SlotSelectPanel `LoadGame()` 검색 | 0 | 0 | PASS |
| 10 | `overwriteConfirmPanel.Open(slot, StartNewGameInSlot);` 검색 | 1 | 1 | PASS |
| 11 | SlotSelectPanel `NewGameInSlot` 검색 | 1 | 1 | PASS |
| 12 | 0줄 변경 계약 파일 Git 상태 | 빈 출력 | 빈 출력 | PASS |
| 13 | `TotalBossCount = 3` 검색 | 1 | 1 | PASS |

### (E) Play 모드 체크리스트

*D-07 마이그레이션 안전성*
- [ ] 기존 `save.json` 또는 백업본이 persistentDataPath에 있다
- [ ] 이어하기 슬롯 1 카드에 기존 진행도가 보인다
- [ ] `Phase14/1. Log All Slots`의 slot 0 경로가 `save.json`이다
- [ ] 슬롯 0 로드 시 기존 씬, 스폰포인트, 체력이 복원된다

*D-06 슬롯 독립성*
- [ ] Slot 1 선택 후 저장하면 `save_1.json`이 생성된다
- [ ] Slot 1 저장이 `save.json`을 바꾸지 않는다
- [ ] Slot 2 저장 시 `save_2.json`이 생기며 슬롯 0/1은 무영향이다

*D-01 이어하기*
- [ ] 하나라도 데이터가 있으면 이어하기가 활성화된다
- [ ] 전부 비었을 때만 이어하기가 비활성화된다
- [ ] 이어하기 클릭 시 즉시 로드하지 않고 슬롯 화면이 뜬다
- [ ] 빈 슬롯 카드는 보이지만 클릭할 수 없다
- [ ] 데이터 카드를 누르면 해당 슬롯 상태가 로드된다

*D-02 / D-03 새시작*
- [ ] 빈 슬롯이 있으면 슬롯 화면 없이 `Tutorial Map`으로 진입한다
- [ ] 가장 낮은 번호의 빈 슬롯을 사용한다
- [ ] 세 슬롯이 모두 차면 슬롯 선택 화면이 뜬다
- [ ] 새 게임에는 이전 슬롯 진행도가 따라오지 않는다

*D-04 / D-05 덮어쓰기 확인*
- [ ] 점유 슬롯을 새 게임 대상으로 고르면 확인창이 뜬다
- [ ] 본문이 "이 슬롯을 덮어쓰고 새 게임을 시작하시겠습니까?"이다
- [ ] 확인 버튼이 "덮어쓰고 시작"이며 빨간색이다
- [ ] 취소 시 파일 수정시각이 변하지 않는다
- [ ] 확인 직후에도 기존 파일은 다음 저장 전까지 유지된다

*회귀*
- [ ] 체크포인트 저장이 현재 슬롯 파일에 기록된다
- [ ] 보스 격파 저장이 현재 슬롯 파일에 기록된다
- [ ] 설정 저장은 슬롯과 무관하게 `setting.json`에 기록된다

### 결과 기록

- 검증 일자:
- Unity 버전:
- Console 오류:
- 미확인 또는 실패 항목:
