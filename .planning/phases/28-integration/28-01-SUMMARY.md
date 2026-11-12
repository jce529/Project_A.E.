# Phase 28 Plan 01 Summary — 통합 정합성 + 아트 상태 연결

정적 점검만 수행(Play/컴파일/Unity CLI 미실행). 브랜치 `claude/phase-28-integration-e29556`.

## 1. 병합 (충돌 0, 모두 자동 병합)
순서 23→24→25→26→27, `--no-ff`. 병합한 브랜치 끝 커밋: 23=b43dd2c, 24=ad7cef2, 25=dce04e2, 26=ad50fba, 27=71f77b9. `주창은`/`main` 병합 안 함.

## 2. 키 대조표 (전수)
쓰기=W, 읽기=R. 키는 SetGimmick 가 소문자 정규화하며 모든 키가 소문자.

### s1.* (Phase 24 단독 소유, 27 무관)
| 키 | W | R | 비고 |
|---|---|---|---|
| s1.lower.entered … s1.reward.entered (8개) | Stage1Trigger / Stage1SlideResult (Stage1Progress.Set) | Stage1Progress.AllSet(requires), Stage1ProgressGate | 일치. 슬라이드 실패 시 slide.success/upper.entered/platform.done 해제 |
| s1.reward.entered | Stage1Trigger | (없음) | 끝 키: 1스테이지→2-1 전환을 일으키는 코드 없음 (§5) |

### s2.a.* (2-1: 25 상태머신 + 27 기믹)
| 키 | W | R | 상태 |
|---|---|---|---|
| s2.a.area{1-6}.visited | Stage21Progress.EnterArea (AreaTrigger) | Stage21Progress.CanEnterArea | 25 내부 일치 |
| s2.a.area{1-6}.cleared | Stage2Events.RaiseAreaCleared 수신 시(25) | (없음) | **고아**: 발행자 없음. **수정**: StoneTabletHint `clearedKey` 추가, 2-1 석판에 `s2.a.area5.cleared` 지정하면 기록됨. 1~4, 6번은 쓸 곳도 읽는 곳도 없어 무해 |
| s2.a.tablet.unlocked | StoneTabletHint(27, 전멸 시) | Stage21Progress.CanInvestigateTablet (호출자 없음), StoneTabletHint | 일치. 25도 OnAreaCleared 로 쓰지만 발행자 없어 미동작(무해) |
| s2.a.tablet.read | StoneTabletHint.Open (readKey 기본값) | (없음) | 일치(25 OnTabletOpened 도 같은 키) |
| s2.a.lantern.solved | LanternPuzzle(27) | Stage21Progress(IsLanternSolved/CanEnterStage22/TryRemoveFog) | **일치 (핵심 연결)** |
| s2.a.fog.removed | Stage21Progress.TryRemoveFog (25) | Stage21FogController | 25 단독, 일치 |
| s2.a.telescope.viewed | Telescope(27) | (없음) | 고아(의도): 아트/연출용 |
| s2.a.tree.transformed | TreeMonsterTransform(27) | (없음) | 고아(의도): 아트/연출용 |

### s2.b.* (2-2: 26 상태머신 + 27 기믹)
| 키 | W | R | 상태 |
|---|---|---|---|
| s2.b.waterway.changed | WaterwayLever(27) | Stage22Progress.Check(5→8), Stage22Gate | **일치** |
| s2.b.waterfall.stopped | FloodgateWaterfall(27) | Stage22Progress.CanAccessTablet, StoneTabletHint.requiredFlagKey | **일치** |
| s2.b.tablet.read | StoneTabletHint(27) — Inspector readKey 설정 시에만 | Stage22Progress.Check(8→9) | 기본값이 `s2.a.tablet.read` 라 **2-2 인스턴스 설정 필수**(§8). 코드 불일치는 아님 |
| s2.b.tablet.unlocked | StoneTabletHint(unlockedKey 설정 시) | StoneTabletHint | 2-2 인스턴스는 unlockedKey 도 s2.b 로 설정 |
| s2.b.loop.completed | Stage22ProgressMachine.TryEnter (9→1) | (없음) | 읽는 곳 없음. 아트/엔딩 연결용. Current=None 인 로드 직후엔 9→1 이 기록되지 않는 한계 |
| s2.b.waterwheel.running / s2.b.lift.raised | WaterWheel / RisingLift(27) | (없음) | 고아(의도): 26 은 미사용 |

## 3. 고친 불일치 (2건, `Assets/map/script/Stage2/StoneTabletHint.cs`)
1. **2-2 석판 영구 잠금 버그**: `areaEnemies` 가 비면 `Unlocked` 가 영영 false → 2-2 석판은 적 전멸 조건이 없고 `requiredFlagKey`(= s2.b.waterfall.stopped)만 쓰므로 열 수 없었음. `areaEnemies` 가 비고 `requiredFlagKey` 가 있으면 Start 에서 Unlocked=true.
2. **고아 키 `s2.a.area5.cleared`**: 발행자 없던 25의 구역 전멸 키를 맞추기 위해 선택 필드 `clearedKey` 추가(비우면 기존 동작).
키 문자열 자체의 오타/불일치는 없었음(25/26 ↔ 27 상수 전수 대조).

## 4. 시그니처/이벤트 (25·26 ↔ 27)
- 25·26 은 27 클래스를 코드로 참조하지 않는다. `Stage2Events` / `Stage21Progress` / `Stage22Progress` / `Stage22ProgressMachine` 는 25·26 폴더 밖에서 호출 0건(grep). 연결은 오직 **플래그 키**이므로 시그니처 불일치 컴파일 오류 없음.
- `Stage2Events.RaiseLanternSolved/RaiseAreaCleared/RaiseTabletOpened` 는 발행자 없는 죽은 경로(삭제하지 않음, 플래그 경로가 정본).
- 27 의 `Changed(bool)` 이벤트 구독자 없음.
- 외부 심볼 존재 확인: `IPlayerInteractable.CanInteract/Interact(PlayerInteraction)`, `HP.OnDeath(Action)`, `InputHandler.Instance/OnPauseEvent`, `GameStateManager.Instance/GameState.Puzzle/SetState`, `PlayerRespawn.UpdateCheckpoint/RespawnPosition`, `SaveLoadManager.SetGimmick/GetGimmick(string,bool)`. asmdef 없음 → 어셈블리 경계 문제 없음.

## 5. 진행 경로 점검 (시작 → 1 → 2-1 → 2-2 → 9 → 1)
| 구간 | 키/트리거 | 막힘/되돌리기 | 결과 |
|---|---|---|---|
| 시작→1스테이지 | NewGame → defaultSceneName "1 stage" | - | OK |
| 1스테이지 내부 | s1.lower → platform → upper → slide.success → stepping → waterfall → split → reward | requires 로 순서 강제. 슬라이드 실패→ResetToLower+체크포인트 리스폰, 장판부터 재시도 | 키 일치 |
| **1→2-1 전환** | s1.reward.entered 이후 | 전환 코드 없음 | **끊김(미해결)**: 기존 `SignpostPortal` 또는 `Stage1Trigger.Report()` 후 UnityEvent 로 연결 필요(씬 작업) |
| 2-1 구역 1~6 | Stage21AreaTrigger 선형, 6→Stage21Stage22Gate | 건너뛰면 onBlocked. 석등 미해결 시 출구 onBlocked | OK |
| **2-1→2-2** | Stage21Stage22Gate.onAllowed | 석등 풀 때까지 차단 | 전환 호출 필요. `2 stage.unity` 만 존재(2-2 별도 씬 없음, Build Settings 미등록) → 같은 씬 내 이동이면 Z1 위치 이동으로 대체 |
| 2-2 구역 1→7→5→8→9→1 | Stage22ZoneTrigger + Stage22Gate | 5→8 물길 변경 필요, 8→9 석판 read 필요, 7→8 항상 차단, 9→1 허용, 비인접 이동 차단 | 키 일치. 레버를 되돌리면 5→8 블로커 재활성(통과 후엔 무관) |
| 9→1 | TryEnter(Z9→Z1) | s2.b.loop.completed 기록 | 이후 흐름은 범위 밖 |

## 6. 세이브/로드 복원
- `LoadGame()` 은 `_data = loaded` 를 **씬 로드 전**에 설정하므로 씬의 Awake/Start/OnEnable 에서 `GetGimmick` 이 복원된 값을 읽는다. 별도 복원 코드 불필요.
- 복원 소비자: Stage1ProgressGate(OnEnable), Stage21FogController(OnEnable/Start), Stage22Gate(OnEnable+폴링), 27 전 기믹(Start), FlagStateVisual(OnEnable).
- 복원 안 되는 것(의도/한계): 석등 개별 ON/OFF(해결 시만 영구), 2-2 현재 구역, TreeMonsterTransform 타이머, 석판 UI 열림 상태.
- 플래그는 메모리 캐시이며 디스크 반영은 `Save()`(체크포인트/보스/자동저장) 때만. 체크포인트 이후 세운 플래그는 사망→LoadGame 시 롤백(씬도 초기화되므로 일관).
- Stage1Progress.OnFlagChanged 는 로드 시 발생하지 않지만 Gate 는 OnEnable 에서 직접 읽으므로 문제 없음.

## 7. 아트 상태 연결 컴포넌트 — `Assets/map/script/FlagStateVisual.cs`
- 키 4개 `activeKey`(작동) / `disabledKey`(비활성) / `changedKey`(변화) / `interactedKey`(상호작용), 나머지는 기본. 우선순위 Disabled > Changed > Interacted > Active > Default.
- `arts[]` 항목: state, sprite(SpriteRenderer 교체), animatorTrigger(Animator.SetTrigger), showOnly[](해당 상태에서만 켜질 오브젝트). 빈 칸은 무시 → 아트 없이도 안전.
- OnEnable 즉시 평가(세이브 복원), 이후 0.25s 폴링. `Current`, `StateChanged`, `Refresh()` 공개.
- Animator 트리거 이름은 아트 컨트롤러 파라미터와 일치해야 한다.

### 연결 대상 목록 (아트 도착 후 Inspector 에서 FlagStateVisual 부착)
| 오브젝트 | 상태 | 키 |
|---|---|---|
| 1스테이지 장판/징검다리/폭포 | Active | `s1.platform.done`, `s1.stepping.done`, `s1.waterfall.done` (구간별) |
| 슬라이드 | Changed | `s1.slide.success` |
| 2-1 안개 | Changed | `s2.a.fog.removed` |
| 2-1 석등 | Active | `s2.a.lantern.solved` (개별 ON/OFF 는 LanternSwitch on/offVisual) |
| 2-1 석판 | Active/Interacted | active=`s2.a.tablet.unlocked`, interacted=`s2.a.tablet.read` |
| 2-1 망원경 | Interacted | `s2.a.telescope.viewed` |
| 2-1 가고일(나무) | Changed | `s2.a.tree.transformed` |
| 2-2 물길 레버/물길 | Changed | `s2.b.waterway.changed` |
| 2-2 수문/폭포 | Changed | `s2.b.waterfall.stopped` |
| 2-2 물레방아 | Active | `s2.b.waterwheel.running` |
| 2-2 승강대 | Active | `s2.b.lift.raised` |
| 2-2 석판 | Active/Interacted | active=`s2.b.waterfall.stopped`, interacted=`s2.b.tablet.read` |
| 2-2 9→1 복귀 연출 | Changed | `s2.b.loop.completed` |

- 27 컴포넌트의 on/offVisual, waterwayA/B 등 기존 SetActive 토글과 중복되지 않게, 한 오브젝트에는 한 방식만 쓴다.
- "비활성" 의미의 플래그 키는 현재 없다. `disabledKey` 는 필요 시 신규 키를 합의해 사용.

## 8. 필요한 Inspector 연결 (씬 미수정, 사용자 작업)
- `1 stage.unity`: 24-01-SUMMARY "Inspector 연결 목록" 1~4(Stage1Trigger 7곳, Stage1SlideResult 1개 + lowerCheckpoint, Gate, 체크포인트 배치) + **보상방→2스테이지 전환 연결**.
- `2 stage.unity`(2-1): 구역 트리거 1~6(Stage21AreaTrigger, 안개 구역 removesFog), 출구 Stage21Stage22Gate(onAllowed→2-2 이동), Stage21FogController, 석등 3개(LanternSwitch+콜라이더)+LanternPuzzle(answer), StoneTabletHint(areaEnemies=5번 구역 적 HP, **clearedKey=`s2.a.area5.cleared`**), Telescope, TreeMonsterTransform.
- 2-2: Stage22ProgressMachine 빈 오브젝트 1개, 구역 1~9 Stage22ZoneTrigger(isTrigger), Stage22Gate 3개(5→8, 8→9, 7→8 blocker), WaterwayLever, FloodgateWaterfall, WaterWheel/RisingLift, **석판 StoneTabletHint: unlockedKey=`s2.b.tablet.unlocked`, readKey=`s2.b.tablet.read`, requiredFlagKey=`s2.b.waterfall.stopped`, areaEnemies 비움**.
- Build Settings: `2 stage.unity` 미등록(현재 MainMenu, 1 stage, Tutorial Map).
- Player 태그 "Player", 모든 트리거 isTrigger, Player 에 `PlayerRespawn`(슬라이드 실패 경로).

## 9. 컴파일 오류 가능 지점 (병합 접점)
1. `SetGimmick/GetGimmick` 호출처: Stage1Progress, Stage21Progress, Stage22Progress/Machine, Gimmick2Store. 시그니처 `(string,bool)` / `(string,bool=false)` 일치 확인함. 첫 컴파일에서 가장 먼저 볼 곳.
2. `Stage2Events` 의 `internal static` 메서드 — 같은 어셈블리라 OK, asmdef 추가 시 깨짐.
3. 전역 이름 충돌: Telescope, WaterWheel, LanternSwitch, RisingLift, Stage1Trigger, FlagStateVisual 은 저장소 내 유일(grep).
4. .meta 는 에디터가 생성(전 브랜치 공통).
5. 한 파일에 여러 타입(Stage22Progress.cs 의 enum 2개, Stage21Progress.cs 의 static 클래스 3개)은 MonoBehaviour 가 아니므로 OK.
6. 신규/수정 파일 인코딩: FlagStateVisual 은 ASCII+CRLF, StoneTabletHint 는 원래 UTF-8 CRLF 유지.

## 10. 최종 검증 체크리스트 (Play, 순서대로)
A. 컴파일/임포트
- [ ] 콘솔 컴파일 오류 0 (§9 우선). 새 스크립트 .meta 생성 → 커밋 대상 확인.

B. 씬 연결 (§8 목록대로 Inspector)
- [ ] 1 stage / 2 stage 연결 완료, Build Settings 에 2 stage 등록, 1→2-1 · 2-1→2-2 전환 연결.

C. 1스테이지 (Phase 24 미검증)
- [ ] 순서를 건너뛰면 플래그가 안 서고, 순서대로면 s1.* 8개가 차례로 true.
- [ ] 슬라이드 성공 → slide.success, 징검다리 열림.
- [ ] 슬라이드 실패 → 3개 플래그 해제, 하층 체크포인트 리스폰, 장판부터 재시도 가능.
- [ ] Stage1ProgressGate 가 문/발판을 플래그대로 켜고 끈다.

D. 2-1 (Phase 25 + 27 미검증)
- [ ] 구역 1→6 선형, 건너뛰면 onBlocked.
- [ ] 석등 정답 패턴 → lantern.solved, 석등 고정/잠금.
- [ ] 5번 적 전멸 → tablet.unlocked(+area5.cleared) 후에만 석판 UI 열림, 닫기 버튼/Pause 로 닫힘, 다시 열림.
- [ ] 안개 구역 진입: 석등 해결 후에만 안개 영구 제거.
- [ ] 6번 출구: 석등 미해결 시 차단, 해결 후 2-2 이동.
- [ ] 망원경 UI 열림/닫힘, 입력 상태 Puzzle↔Playing 복귀. 나무 2초 체류 변신, 2초 전 이탈 시 타이머 초기화.

E. 2-2 (Phase 26 + 27 미검증)
- [ ] 1→7→5 이동 가능, 비인접·7→8 차단.
- [ ] 레버 전 5→8 차단, 레버 후 통과. 수문 정지 후 석판 열림(2-2 석판 잠금 버그 수정 확인), 열람 후 8→9 허용, 9→1 허용 + s2.b.loop.completed.
- [ ] 물레방아 가동 → 승강대 상승, 물길/폭포 비주얼 전환.

F. 세이브→재로드
- [ ] 체크포인트 저장 후 LoadGame: 안개 제거, 석등 고정, 수문/레버/물레방아 상태, s1.* 게이트, 석판 상태 복원.
- [ ] 체크포인트 이후 세운 플래그가 사망 후 롤백되는 것이 일관적인지 확인.
- [ ] NewGame 시 모든 플래그 초기화.

G. 아트 (도착 후)
- [ ] FlagStateVisual 을 §7 표대로 부착, 플래그 변화 시 스프라이트/Animator 트리거 전환, 로드 직후 올바른 상태로 시작.

H. 알려진 한계 확인(의도)
- [ ] 로드 후 2-2 현재 구역 None → 첫 트리거 구역 수락; 9→1 loop 플래그는 같은 세션에서 9를 거쳤을 때만 기록.

## 11. 남은 이슈
- 1→2-1, 2-1→2-2 씬 전환 코드 부재(Inspector/UnityEvent 로 연결, 2-2 별도 씬 없음).
- Stage2Events 발행자 없음 / `.cleared` 1~4,6 / 25 의 죽은 경로 정리 여부는 총괄 결정.
- 비활성(Disabled) 의미의 플래그 키 미정(아트와 합의).
- STATE/ROADMAP/REQUIREMENTS 미수정(총괄 반영).
- 모델 라우팅: jev_ask 는 입력 형식 오류로 실패, 정적 대조·병합·컴포넌트 모두 Claude(sonnet) 직접 수행(Codex 한도 폴백 불필요).

## 12. Unity 에디터 작업 (총괄 지시 변경 반영)
**방식:** 이 worktree 프로젝트를 별도 에디터 인스턴스(Unity 6000.3.10f1, pipeline 포트 7801)로 열어 `unity command eval_file` 로 생성했다. 메인 체크아웃 에디터(pid 41988, 포트 7800)와 그 AssetImportWorker 는 건드리지 않았고, 모든 명령에 `--project-path` 로 worktree 를 지정했다. 작업 후 worktree 에디터만 `EditorApplication.Exit` 로 종료(pid 39284 종료 확인, 메인 pid 41988 생존). 에디터가 건드린 `EditorBuildSettings.asset`, `ShaderGraphSettings.asset`, `.vscode/settings.json`, 생성된 `.slnx` 는 커밋 대상에서 제외(복원/삭제).

**컴파일 확인(증거):** 첫 임포트(Library 약 2.7GB 신규 생성) 후 `console_status` → `compilationFailed:false`, `recompile_status` → `{"status":"idle","failed":false,"errors":[]}`, 콘솔 "CS####" 오류 0건. §9 병합 접점(SetGimmick/GetGimmick 시그니처 등) 컴파일 통과로 확인됨. 콘솔 error 60건은 전부 임포트 잡음: URP/2d.tooling 패키지 파일의 Windows 경로 길이 초과(`DirectoryNotFoundException`, `Asset import failed`), `Assets/Tile/Ground.prefab` 임포트 경고, `Library/Unused` Broken PPtr(신규 Library 정리 대상). 우리 코드와 무관. 단, 플레이 모드 동작은 검증하지 않음.

**생성물 (기존 씬 미수정, additive 씬 + 프리팹):**
| 프리팹 | 씬 | 내용 |
|---|---|---|
| `Assets/Prefabs/MapProgress/Stage1Progress.prefab` | `Assets/Scenes/MapProgress_Stage1Progress.unity` | Stage1Trigger 7개(키/requires 순서 설정, BoxCollider2D isTrigger), Stage1SlideResult 1개, ArtState 4개 |
| `Assets/Prefabs/MapProgress/Stage21Progress.prefab` | `Assets/Scenes/MapProgress_Stage21Progress.unity` | Stage21AreaTrigger 1~6, Stage21Stage22Gate, Stage21FogController("Fog"), LanternSwitch×3 + LanternPuzzle(lanterns 연결, answer 기본값), StoneTabletHint(clearedKey=s2.a.area5.cleared, hintPanel 연결), Telescope(viewPanel 연결), TreeMonsterTransform(tree/monster 연결), ArtState 5개 |
| `Assets/Prefabs/MapProgress/Stage22Progress.prefab` | `Assets/Scenes/MapProgress_Stage22Progress.unity` | Stage22ProgressMachine, Stage22ZoneTrigger Z1~Z9(zone 설정), Stage22Gate 3개(5→8, 8→9, 7→8; blocker 연결), WaterwayLever(A/B·visual 연결), FloodgateWaterfall(waterfall/visual 연결), RisingLift(platform/low/high), WaterWheel(wheelVisual·lift 연결), StoneTabletHint(unlockedKey=s2.b.tablet.unlocked, readKey=s2.b.tablet.read, requiredFlagKey=s2.b.waterfall.stopped, areaEnemies 비움), ArtState 6개 |
ArtState_* = `FlagStateVisual` 부착 빈 오브젝트(§7 표대로 키만 지정, 스프라이트/애니메이터 슬롯은 비어 있음). 프리팹 직렬화에서 키/zone/from/to/참조 값이 설정됨을 YAML 로 확인(null 참조 0).
사용법: 해당 씬 인스턴스를 기존 씬(`1 stage`, `2 stage`)에 프리팹으로 끌어다 놓고 구역 트리거를 실제 위치로 옮긴다(현재는 x축 8 간격 임시 배치). 

**기존 씬에 반드시 직접 해야 하는 최소 목록 (프리팹으로 대체 불가):**
1. 각 트리거/구역/석판/석등/레버 등 임시 위치를 실제 맵 위치로 이동, 시각 자식(On/Off, WaterwayA/B, Tree/Monster, Blocker 크기 등)을 실제 오브젝트로 교체하거나 연결.
2. 1스테이지: Stage1SlideResult.lowerCheckpoint 에 하층 `Checkpoint` Transform 지정, 전금재 슬라이드 성공/실패 UnityEvent → OnSuccess()/OnFail(), 필요 시 Stage1ProgressGate 배치(문/발판).
3. 2-1: StoneTabletHint.areaEnemies 에 5번 구역 적 HP 지정(비우면 영구 잠금), 안개 구역 Stage21AreaTrigger 에 removesFog 체크 및 Fog 를 실제 안개 오브젝트로, 출구 Stage21Stage22Gate.onAllowed → 2-2 이동, 구역 5 전투 석판.
4. 씬 전환: 1→2-1, 2-1→2-2 연결 및 `2 stage.unity` Build Settings 등록(EditorBuildSettings 는 의도적으로 미수정).
5. 플레이어 태그 "Player", 플레이어에 PlayerRespawn 존재 확인.
6. `.meta`: 모든 신규 스크립트의 .meta 를 이 worktree 에디터가 생성·커밋했다(프리팹이 해당 GUID 를 참조하므로 반드시 함께 병합해야 함). 메인 에디터에서 같은 스크립트 .meta 를 따로 만들었다면 GUID 충돌 → 이 브랜치의 .meta 를 정본으로 사용.
