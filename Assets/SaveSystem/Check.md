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

# Phase 14 — 키바인딩 keybind.json 위임 검증

## 검증 대상 변경사항
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — keybind 파일 I/O API 4종 추가
  (`KeybindFileName` / `KeybindPath` / `HasKeybindFile()` / `SaveKeybindings(string)` / `LoadKeybindings()`)
  + `Phase14/5. Log Keybind State` ContextMenu 훅. 순수 삽입(삭제 0줄).
- `Assets/Player/Script/InputHandler.cs` — `SaveBindingOverrides()` / `LoadBindingOverrides()` 내부 구현을
  로컬 설정 저장소에서 `SaveLoadManager` 경유 파일 I/O 로 교체. 저장소 키 상수 제거.
  25줄 삭제 / 37줄 삽입.
- `Assets/Player/Script/Menu/ControlsSettingsPanel.cs` — **0줄 변경** (호출부 시그니처 불변).

## 결정 요약 (14-CONTEXT.md)

| ID | 결정 |
|---|---|
| D-01 | SaveLoadManager 에는 **파일 I/O만** 위임. Input System 직렬화(`SaveBindingOverridesAsJson` / `LoadBindingOverridesFromJson`)는 InputHandler 가 계속 소유 |
| D-02 | 디스크에 JSON 파일을 쓰는 코드는 프로젝트 전체에서 `SaveLoadManager.cs` 한 곳에만 존재 (이번 phase 가 이 원칙의 첫 적용 사례) |
| D-03 | 기존 로컬 설정 저장소의 리바인딩 값은 마이그레이션하지 않는다 — keybind.json 기준으로 새로 시작 |
| D-04 | `keybind.json` 은 `save.json` 과 완전 독립. `NewGame()`/세이브 삭제가 키 설정을 건드리지 않는다 |

## 사전 준비
1. Unity 에디터를 열고 컴파일 완료를 기다린다. Console **에러 0건** 확인 (이 실행 환경에서 확인 불가한 유일한 항목).
2. 아무 씬에서나 Play 시작 — `SaveLoadManager` 는 부트스트랩으로 자동 생성된다.
3. Hierarchy 의 `DontDestroyOnLoad` 섹션에서 `SaveLoadManager` 를 선택 → Inspector 톱니바퀴 →
   `Phase14/5. Log Keybind State` 실행 → Console 에 `keybindPath=<절대경로> exists=False length=0` 출력.
   이 경로가 검증 내내 열어볼 파일 위치다
   (Windows: `%userprofile%/AppData/LocalLow/<회사명>/<제품명>/keybind.json`).
4. 일시정지 메뉴 > 컨트롤 탭(`ControlsSettingsPanel`)이 배치된 씬에서 진행한다.

## 섹션 1) 정적 회귀 검사

| # | 명령 | 기대값 | 실측값 | 판정 |
|---|------|--------|--------|------|
| 1 | `grep -c 'public const string KeybindFileName = "keybind.json";' Assets/SaveSystem/Script/SaveLoadManager.cs` | 1 | 1 | PASS |
| 2 | `grep -c 'public static string KeybindPath' Assets/SaveSystem/Script/SaveLoadManager.cs` | 1 | 1 | PASS |
| 3 | `grep -c 'public void SaveKeybindings(string json)' Assets/SaveSystem/Script/SaveLoadManager.cs` | 1 | 1 | PASS |
| 4 | `grep -c 'public string LoadKeybindings()' Assets/SaveSystem/Script/SaveLoadManager.cs` | 1 | 1 | PASS |
| 5 | `grep -c 'SaveLoadManager.Instance.SaveKeybindings(json);' Assets/Player/Script/InputHandler.cs` | 1 | 1 | PASS |
| 6 | `grep -c 'SaveLoadManager.Instance.LoadKeybindings();' Assets/Player/Script/InputHandler.cs` | 1 | 1 | PASS |
| 7 | `grep -c 'PlayerPrefs' Assets/Player/Script/InputHandler.cs` | 0 (D-03) | 0 | PASS |
| 8 | `grep -rn "InputBindings" --include=*.cs Assets/ \| wc -l` | 0 (D-03, 구 저장소 키 흔적 없음) | 0 | PASS |
| 9 | `grep -rn "File.WriteAllText\|File.ReadAllText" --include=*.cs Assets/ \| grep -v "SaveSystem/Script/SaveLoadManager.cs" \| wc -l` | 0 (D-02) | 0 | PASS |
| 10 | `grep -ci "keybind" Assets/SaveSystem/Script/SaveData.cs` | 0 (D-04, 세이브 스키마 오염 없음) | 0 | PASS |
| 11 | `git status --porcelain Assets/Player/Script/Menu/ControlsSettingsPanel.cs \| wc -l` | 0 (호출부 0줄 변경) | 0 | PASS |
| 12 | `node -e "const fs=require('fs');const b=fs.readFileSync('Assets/Player/Script/InputHandler.cs','latin1').split('\r\n');console.log(b.filter(l=>/[^\x00-\x7F]/.test(l)).length, b.length)"` | `25 209` (인코딩 무결성 + 최종 줄 수) | `25 209` | PASS |

## 섹션 2) Play 모드 체크리스트 — 저장 (D-01, D-02)
- [ ] Play 시작 → 일시정지 → 컨트롤 탭 → `Jump` 버튼 클릭 → 다른 키(예: `K`)를 누른다
- [ ] 오버레이가 닫히고 라벨이 `K` 로 갱신된다
- [ ] `Phase14/5. Log Keybind State` 실행 → `exists=True` 이고 `length` 가 0 보다 크다
- [ ] 사전 준비 3번의 절대 경로에 **`keybind.json` 파일이 실제로 생성**되어 있고, 메모장으로 열면
  `{"bindings":[...]}` 형태의 JSON 이며 방금 바꾼 키가 들어 있다
- [ ] Console 에 `Failed to write keybind file` 에러가 **없다**

## 섹션 3) Play 모드 체크리스트 — 복원 (D-01)
- [ ] Play 를 정지했다가 다시 시작한다
- [ ] 컨트롤 탭을 열면 `Jump` 라벨이 여전히 `K` 다 (`InputHandler.Awake()` → `LoadBindingOverrides()` 경로)
- [ ] 실제로 `K` 를 눌렀을 때 캐릭터가 점프한다 (라벨만 바뀐 게 아니라 바인딩이 실제 적용됨)
- [ ] 원래 키(`Space`)로는 점프하지 않는다
- [ ] Console 에 `SaveLoadManager.Instance is null` 경고/에러가 **없다** (부트스트랩 순서 확인)

## 섹션 4) Play 모드 체크리스트 — 초기화 & 파일 없음 (D-01)
- [ ] 컨트롤 탭의 초기화 버튼 클릭 → 모든 라벨이 기본값으로 돌아간다
- [ ] `keybind.json` 이 기본 상태로 덮어써진다 (파일 수정 시각 갱신)
- [ ] 재시작 후에도 기본 키로 유지된다
- [ ] Play 정지 후 `keybind.json` 을 **직접 삭제**하고 다시 Play → 기본 키로 정상 동작하고
  Console 에 에러가 없다 (파일 없음 = 기본값 사용 경로)
- [ ] `keybind.json` 내용을 `{{{ 깨진 텍스트` 로 바꾸고 Play → `Failed to read keybind file` 또는
  Input System 경고가 뜨더라도 **게임이 기본 키로 계속 실행**된다 (크래시 없음)

## 섹션 5) Play 모드 체크리스트 — save.json 독립성 (D-04)
- [ ] 키를 `K` 로 바꿔 `keybind.json` 을 만든 상태에서 `Phase11/3. New Game (memory only)` 실행 →
  `keybind.json` 의 내용과 수정 시각이 **변하지 않는다**
- [ ] `Phase11/1. Save Now` 로 `save.json` 을 새로 쓴 뒤에도 `keybind.json` 이 변하지 않는다
- [ ] `save.json` 을 직접 삭제하고 Play → 키 설정은 여전히 `K` 로 유지된다 (D-04 핵심)

## 섹션 6) 알려진 한계 / 범위 밖
- 기존 로컬 설정 저장소에 남아 있는 예전 리바인딩 값은 마이그레이션하지 않는다 (D-03).
  출시 전 단계이므로 의도된 동작이며, 이전에 키를 바꿔둔 개발 환경에서는 **첫 실행 시 키가 기본값으로 리셋된 것처럼 보인다.**
- 사운드/그래픽/게임 설정 패널(`SoundSettingsPanel` / `GraphicsSettingsPanel` / `GameSettingsPanel`)과
  `AudioManager` 는 여전히 로컬 설정 저장소를 쓴다 — **키바인딩이 아니므로 이번 phase 범위 밖**이다.
  D-02 원칙을 이들에게 확대 적용할지는 별도 결정 사항.
- `keybind.json` 은 단일 프로필만 지원한다 (프리셋/다중 프로필 없음).
- `InputHandler.cs` 의 한글 주석은 Phase 14 이전부터 이미 훼손된 상태다(U+FFFD 658개).
  이번 편집은 그 훼손을 **늘리지 않았을 뿐** 복구하지 않았다 — 복구는 별도 작업.

## 결과 기록
- 정적 12항목: 실행 완료, 모두 PASS (2026-08-20).
- Play 모드 실측 검증: 미수행 (Task 2 대기)
