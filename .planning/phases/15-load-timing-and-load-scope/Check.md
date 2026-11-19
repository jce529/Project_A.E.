# Phase 15 — 로드 시점 및 로드 범위 정의 검증 상태

**요약:** 플레이어 사망 복구, 로드 실패 폴백, 보스 진행도 복원, 체력 불변식 보정이 6개 소스/프리팹에 구현됐다. 정적 회귀 검사는 18개 중 16개가 PASS이며, 2개 FAIL은 구현 회귀가 아니라 각각 기존 설정 저장 경로와 Phase 14 상태 로그 때문에 계획의 기준값이 현재 코드와 어긋난 경우다. Unity Play 모드 실측 5개 묶음은 아직 사용자 확인 전이다.

## 무엇이 바뀌었나

- `Assets/Player/Script/PlayerStats.cs` — D-01/D-02 사망 시 마지막 세이브 자동 로드 또는 현재 씬 재시작, D-08 저장 체력 불변식 보정.
- `Assets/Player.prefab` — D-09 시작 `maxHealth` 100, 성장 상한 `maxTotalHealth` 200 확정.
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — D-04 로드 실패 5분기의 메인메뉴 복귀, D-05 실패 시 세이브 무삭제·무덮어쓰기.
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs` — D-06/D-07 저장된 `WaterSpirit` 격파 상태를 `Awake()`에서 조회해 자기 비활성화.
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — D-06/D-07 저장된 `WaterMonster` 격파 상태를 `Awake()`에서 조회해 자기 비활성화.
- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs` — D-06/D-07 저장된 `TutorialBoss` 격파 상태를 복원하고 보스방 벽을 해제한 뒤 자기 비활성화.

## 정적 회귀 검사 (2026-09-11, 16/18 PASS)

| # | 결정 | 검사 | 기대값 | 실제값 | 판정 |
|---|---|---|---:|---:|---|
| 1 | D-01 | `PlayerStats.cs`의 `public override void Die()` 개수 | 1 | 1 | PASS |
| 2 | D-01 | `PlayerStats.cs`의 `base.Die()` 개수 | 0 | 0 | PASS |
| 3 | D-01 | `PlayerStats.cs`의 `mgr.LoadGame();` 개수 | 1 | 1 | PASS |
| 4 | D-02 | 현재 씬 동기 재시작 호출 개수 | 1 | 1 | PASS |
| 5 | D-03 | `git diff --numstat HEAD -- Assets/map/script/FallZone.cs` | 빈 출력 | 빈 출력 | PASS |
| 6 | D-04 | `SaveLoadManager.cs`의 `AbortLoadToMainMenu(` 개수 | 6 | 6 | PASS |
| 7 | D-04 | `MainMenuSceneName = "MainMenu"` 상수 선언 개수 | 1 | 1 | PASS |
| 8 | D-05 | `SaveLoadManager.cs`의 `File.Delete` 개수 | 0 | 0 | PASS |
| 9 | D-05 | `SaveLoadManager.cs`의 `File.WriteAllText` 개수 | 1 | 2 | **FAIL** — `Save()` 1건 외에 Phase 14 이전부터 존재한 독립 설정 저장 `SaveSettings()` 1건이 있음. `AbortLoadToMainMenu()`에는 쓰기 없음 |
| 10 | D-06/D-07 | `Assets/**/*.cs`의 `IsBossDefeated` 개수 | 4 | 4 | PASS |
| 11 | D-06/D-07 | `Assets/**/*.cs`의 `SaveOnBossDefeated` 개수 | 4 | 4 | PASS |
| 12 | D-07 | TutorialBoss의 `WallToUnlock.UnlockWall();` 개수 | 1 | 1 | PASS |
| 13 | D-08 | 저장 `maxHealth` 상한 보정문 개수 | 1 | 1 | PASS |
| 14 | D-09 | `Player.prefab`의 행 시작 `maxHealth: 100` 개수 | 1 | 1 | PASS |
| 15 | D-09 | `Player.prefab`의 행 시작 `maxTotalHealth: 200` 개수 | 1 | 1 | PASS |
| 16 | D-10 | `MapGimmickState\|_data.Items` C# 검색 결과가 Phase 11 시점과 동일한지 | Phase 11과 동일: 3건 | 현재 5건 / Phase 11(`1fcf28a`) 3건 | **FAIL** — Phase 14가 `DebugLogState()`에 읽기 전용 개수 로그 2건을 추가함. Phase 15 신규 소비자는 0건이며 저장/복원 훅은 여전히 없음 |
| 17 | 전체 | 구현 파일 5개의 UTF-8 `EF BF BD` 바이트 개수 | 파일별 모두 0 | `PlayerStats=0`, `SaveLoadManager=0`, `SpiritStats=0`, `WaterMonsterStats=0`, `TutorialBossController=0` | PASS |
| 18 | 범위 | 15-01/02/03 태스크 커밋 7개의 변경 파일 합집합 | 지정된 6개 파일 | 지정된 6개 파일과 정확히 일치 | PASS |

범위 검사에 사용한 커밋은 `6575989`, `6bf0a58`, `5b037f8`, `1639e4b`, `40d0b37`, `b0d008b`, `8edfad3`이다. 합집합은 위 "무엇이 바뀌었나"의 6개 파일뿐이며, 실행 전부터 존재한 다른 미커밋 변경은 포함하거나 스테이징하지 않았다.

## D-03 / D-10 (의도적 미구현)

- **D-03**: `Assets/map/script/FallZone.cs` 무수정. 낙하는 사망이 아니라 데미지 페널티이며 `PlayerRespawn.RespawnPosition()`으로 위치만 되돌린다. 낙하 데미지로 HP가 0이 되면 그때 `PlayerStats.Die()`의 D-01 경로로 자연히 넘어간다. 검증: 위 표 #5.
- **D-10**: `SaveData.MapGimmickState`와 `SaveData.Items`는 스텁 유지. 두 필드 모두 쓰기 훅이 없어(Phase 11 D-03 / D-03b) 복원할 데이터가 존재하지 않는다. 저장 쪽이 먼저 생겨야 복원을 논할 수 있다. 검증: 위 표 #16. Phase 14의 `DebugLogState()`가 두 컬렉션의 개수를 읽어 로그에 표시하지만 상태를 소비·복원하지는 않으며, Phase 15 신규 소비자는 0건이다.

## Play 모드 체크리스트 (사용자 직접 확인)

### 1) 사망 자동 로드 (D-01)

- [ ] `Tutorial Map` 진입 → 체크포인트 활성화(S키)로 세이브 생성 → Console에 `[SaveLoadManager] Saved to ...` 1건
- [ ] 체크포인트에서 떨어진 위치로 이동한 뒤 적/낙하로 HP를 0으로 만든다
- [ ] Player GameObject가 Hierarchy에서 사라지지 않는다 (`Destroy` 안 됨)
- [ ] 게임오버 UI가 뜨지 않는다
- [ ] 저장된 씬이 다시 로드되고 플레이어가 저장한 체크포인트 위치에 서 있다
- [ ] Console에 `[SaveLoadManager] Restored stats: ` 1건, 체력이 저장 당시 값이다
- [ ] 로드가 **1회만** 발생한다 (`Restored stats:`가 2건 이상 찍히지 않는다 — `_deathHandled` 가드)

### 2) 세이브 없는 사망 → 현재 씬 재시작 (D-02)

- [ ] `Application.persistentDataPath`의 `save.json` / `save_1.json` / `save_2.json`을 모두 다른 폴더로 **이동**한다 (삭제 아님, 복구 가능하게)
- [ ] 새 게임으로 `Tutorial Map` 진입 후 체크포인트를 켜지 않은 채 HP 0으로 만든다
- [ ] 메인메뉴로 나가지 **않고** `Tutorial Map`이 처음부터 다시 시작된다
- [ ] 플레이어 체력이 최대치(100)로 리셋돼 있다
- [ ] 검증 후 옮겨둔 세이브 파일을 원위치로 되돌린다

### 3) 로드 실패 → 메인메뉴 (D-04, D-05)

- [ ] `save.json`을 텍스트 에디터로 열어 **사본을 먼저 백업**한 뒤 내용을 `{` 한 글자로 바꿔 손상시킨다
- [ ] 메인메뉴 → 이어하기 → 슬롯 1 선택
- [ ] Console에 `[SaveLoadManager] Load failed (save file unreadable or malformed: ...) - returning to MainMenu. Save file left untouched.`가 찍힌다
- [ ] 화면이 `MainMenu` 씬으로 돌아온다 (멈춰 있지 않다)
- [ ] `save.json`의 내용이 여전히 `{` 그대로다 — 지워지거나 덮어써지지 않았다 (D-05)
- [ ] 백업본으로 되돌린 뒤, 이번엔 `SceneName` 값을 `"NoSuchScene"`으로 바꾼다 → 이어하기 → Console에 `LoadSceneAsync returned null for scene 'NoSuchScene' - not registered in Build Settings?` 후 `MainMenu` 복귀
- [ ] 검증 후 백업본을 원위치로 되돌린다

### 4) 격파 보스 미등장 (D-06, D-07)

- [ ] `Tutorial Map`에서 TutorialBoss를 격파한다 → Console에 `[SaveLoadManager] Saved to ...`
- [ ] 격파 후 체크포인트를 한 번 더 활성화해 씬/스폰포인트를 갱신한다
- [ ] 메인메뉴로 나갔다가 이어하기로 같은 슬롯을 로드한다
- [ ] TutorialBoss GameObject가 Hierarchy에서 비활성(SetActive false) 상태다 — 보스가 다시 나타나지 않는다
- [ ] 보스방 벽(`WallToUnlock`이 가리키는 오브젝트)과 그 `wallTilemap`이 둘 다 비활성이다 — 통과할 수 있다
- [ ] 클리어 패널(`ClearPanel`)이 다시 뜨지 않고 게임이 Paused로 전환되지 않는다
- [ ] 보스 체력바가 화면에 나타나지 않는다
- [ ] (참고) WaterSpirit / WaterMonster는 현재 배치된 씬이 없어 이번 실측 대상이 아니다 — BUG-003 본체는 별도 페이즈

### 5) 체력 불변식 보정 (D-08, D-09)

- [ ] `save.json`을 백업한 뒤 `"Health": 999, "MaxHealth": 400, "MaxTotalHealth": 200`으로 손으로 고친다
- [ ] 이어하기로 로드한다
- [ ] Console에 `[PlayerStats] RestoreStats corrected out-of-range saved values: 999/400 (maxTotal 200) -> 200/200 (maxTotal 200).`이 찍힌다
- [ ] Inspector의 PlayerStats가 `health 200 / maxHealth 200 / maxTotalHealth 200`이다
- [ ] 새 게임으로 `Tutorial Map` 진입 시 Inspector가 `maxHealth 100 / maxTotalHealth 200`이고 하트 UI가 총 40개 생성·20개 활성 상태다
- [ ] `AddHealth()`를 호출하면 `maxHealth`가 101로 증가한다 (BUG-002 해소 확인)
- [ ] 검증 후 백업본을 원위치로 되돌린다

## 현재 상태

정적 회귀는 **16건 통과 / 2건 실패 / 0건 미검증**이다. 두 FAIL은 각각 계획 #9의 설정 저장 경로 누락과 #16의 Phase 14 진단 로그 누락으로 원인 플랜이 특정됐으며, Phase 15 구현 파일의 회귀나 범위 이탈은 발견되지 않았다. Play 모드는 **0건 통과 / 0건 실패 / 34건 미검증**으로 사용자 실측을 기다린다.
