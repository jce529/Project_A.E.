# Phase 28 Plan 03 Summary — 자동 검증 결과 (Play 포함)

28-01-SUMMARY §10 체크리스트 A~G 중 에이전트가 자동 판정 가능한 항목을 실행했다. worktree 프로젝트의 별도 에디터(포트 7802)에서만 수행, 메인 에디터(pid 41988)와 기존 씬(`1 stage`, `2 stage`, `Tutorial Map`)은 건드리지 않았다.

## 총괄 결과
| 구분 | 건수 |
|---|---|
| 자동 PASS | **55** (Play 항목 50 + 컴파일/콘솔 1 + 렌더=콜라이더 재확인 1 + 추가 씬 3종 Play 스모크 3) |
| 자동 FAIL | **0** |
| 수동 확인 대기 | **10** (아래 목록) |
| 고친 버그 | **0** (코드 수정 없음) |

증거: `evidence/28-03-play.log`(항목별 PASS/측정값 한 줄, 첫 항목 B1a 는 중복 실행으로 2줄), `evidence/console-open.json`, `evidence/console-play-test-scene.json`(Play 중 error 0), `evidence/28-03-bounds-recheck.md`.

## 테스트 방법과 한계 (정직한 범위)
- 추가 씬 3종은 비어 있어(Player/SaveLoadManager 없음) 로직 검증용 임시 씬 `Assets/Scenes/_P28Test.unity`(3개 프리팹 인스턴스를 y=0/30/60 에 배치 + 합성 Player[Rigidbody2D, 콜라이더, PlayerStats, PlayerRespawn] + GameStateManager + Stage1ProgressGate 픽스처)를 만들어 Play 했다. 씬/Build Settings 등록은 임시였고 **삭제·복원 완료**(커밋 안 함). 추가 씬 3종 자체는 Play 스모크(오류 0)만 별도로 했다.
- 구역 트리거(Stage1/Area/Zone/TreeMonster/Gate)는 합성 Player 를 물리적으로 이동시켜 실제 `OnTriggerEnter2D` 로 발동시켰다. 상호작용 기믹은 `PlayerInteraction`(키 입력) 대신 `Interact(null)`/`Open()` 직접 호출이다(입력 경로는 수동 확인 대기).
- 석판 areaEnemies 는 프리팹에 비어 있어, 임시 HP 2개를 가진 픽스처 석판으로 전멸 전/후를 확인했다.
- 에디터가 비포커스일 때 플레이 루프가 멈춰 `Application.runInBackground=true` 를 걸어 해결(프로젝트 설정은 변경 안 함).
- 첫 세이브 시도(F1)는 NewGame 이 이미 변경된 오브젝트 메모리 상태를 되돌리지 않아 테스트 설정이 무효였음(제품 버그 아님). 씬을 새로 로드한 뒤 재실행해 PASS. 무효 시도의 로그 줄은 삭제했다.

## A. 컴파일 / 콘솔
- [x] **PASS** 컴파일 오류 0: 에디터 오픈 직후 `compilationFailed:false`, CS 오류 0, 콘솔 전체 = warning 1건(Input Manager 지원중단 안내)뿐, error 0. Play(3종 스모크 + 테스트 씬 전 과정)에서도 error 0.
- 이전 보고의 "내용 못 본 error 1건": **정체 확인 불가(재현 안 됨)**. 직전 세션 종료 직전에만 관찰됐고(에디터 종료 중 발생 추정), 같은 동작(씬 열기·Scene 캡처·종료)을 반복해도 error 0. 우리 코드 원인 증거 없음 → 수정 없음.

## B. 씬 연결 (수동 대기)
- [ ] **수동 대기** 기존 `1 stage`/`2 stage` 에 프리팹 배치와 위치/Inspector 연결(28-01 §12 최소 목록): Stage1SlideResult.lowerCheckpoint, StoneTabletHint.areaEnemies(2-1), removesFog 구역, 전금재 슬라이드 이벤트→OnSuccess/OnFail 등.
- [ ] **수동 대기** 1→2-1, 2-1→2-2 씬 전환 연결, `2 stage.unity` Build Settings 등록(전환/LoadGame 이 쓰려면 필요).

## C. 1스테이지 (Phase 24)
- [x] **PASS** C1 순서를 건너뛰면 플래그 안 섬: 역순 방문 후 s1.* 전부 false (A1).
- [x] **PASS** C2 순서대로면 8개 차례로 true, CurrentStep=7 (A2): lower→platform→upper→slide.success→stepping→waterfall→split→reward.
- [x] **PASS** C3 Stage1ProgressGate: upper.entered 시 on/off 전환, 해제 시 원복 (A3a/A3b).
- [x] **PASS** C4 슬라이드 실패: slide.success/upper.entered/platform.done 해제, lower.entered 유지(A4a), 플레이어가 lowerCheckpoint (-40,-40) 로 리스폰(A4b).
- [x] **PASS** C5 실패 후 재시도: PlatformEnd 재방문 시 platform.done 다시 true (A5). 관찰: `Stage1Trigger.oneShot` 의 `enabled=false` 는 물리 메시지를 막지 않아 사실상 효과 없음(무해, 설계 메모).

## D. 2-1 (Phase 25 + 27)
- [x] **PASS** D1 구역 선형: area3 먼저 → blocked(area3.visited=false), 1→6 순서대로 entered=6 (B1a/B1b).
- [x] **PASS** D2 석등: [T,T,T] 오답은 미해결, [T,F,T] 정답 → lantern.solved, 3개 잠금 (B4a/B4b).
- [x] **PASS** D3 안개: 석등 해결 전 TryRemoveFog=false(안개 유지), 해결 후 true·fog.removed·안개 비활성 (B3a/B3b).
- [x] **PASS** D4 6번 출구: 해결 전 blocked, 해결 후 allowed (B2a/B2b).
- [x] **PASS** D5 석판: 프리팹 인스턴스(적 미지정)는 잠금 유지(B5a), 적 2마리 중 1마리 사망 시 잠금, 전부 사망 시 tablet.unlocked + s2.a.area5.cleared, 열기→panel on·tablet.read·상태 Puzzle, 닫기→Playing 복귀, 재열림 가능 (B5b~B5e).
- [x] **PASS** D6 망원경: 열기→IsViewing·viewed 플래그·Puzzle, 닫기→Playing, timeScale 1 복귀 (B6).
- [x] **PASS** D7 나무: 1.0s 체류 후 이탈 → Timer 0 으로 초기화·변신 안 함(1.00s 측정), 2.5s 체류 → 변신(플래그, 나무 off, 몬스터 on) (B7a/B7b).

## E. 2-2 (Phase 26 + 27)
- [x] **PASS** E1 1→7 순차, 비인접 7→3 NotAdjacent 차단, 7→8 차단, Gate_7to8 blocker 항상 on, 7→6→5 복귀 허용 (C1, C2a~C2c, C3a).
- [x] **PASS** E2 레버 전 5→8 WaterwayNotChanged 차단(Gate_5to8 blocker on), 레버 후 flag·blocker off·5→8 허용 (C3b~C3d).
- [x] **PASS** E3 수문 정지→waterfall.stopped·폭포 off, **2-2 석판 잠금 버그 수정 확인**: 정지 전 CanInteract=false, 정지 후 Unlocked=true·CanInteract=true (C4b, C4c).
- [x] **PASS** E4 석판 열람이 s2.b.tablet.read 에 기록(s2.a.* 오염 없음), 8→9 열람 전 TabletNotRead 차단, 열람 후 Gate_8to9 blocker off·8→9 허용 (C4a, C4d~C4f).
- [x] **PASS** E5 9→1 허용 + s2.b.loop.completed (C5).
- [x] **PASS** E6 물레방아 가동→running·lift.raised, 승강대가 3초 내 y 70→74(HighPoint)로 상승 (C6).

## F. 세이브 → 재로드
저장 폴더 `%LocalLow%/DefaultCompany/AQUA ECLIPSE/` 전체를 임시 폴더로 백업(md5 기록) 후 테스트, **종료 후 폴더 통째 원복 및 md5 9개 파일 완전 일치 확인**(테스트가 `save.json` 을 덮어썼고 원복됨).
- [x] **PASS** F1 체크포인트 `SaveAtCheckpoint`→`Save()` 가 23개 플래그를 디스크에 기록 (s1 8, 2-1 area 6·lantern·fog·telescope, 2-2 lever·floodgate·wheel·lift·tablet.read·loop).
- [x] **PASS** F2 사망 경로: `PlayerStats.Die()` → `LoadGame()` → 씬 재로드(activeScene=_P28Test) (F3a).
- [x] **PASS** F3 체크포인트 시점 23개 플래그 전부 `GetGimmick` 으로 복원, 불일치 0 (F3b).
- [x] **PASS** F4 체크포인트 이후 세운 플래그(s2.a.tree.transformed, 임의 키)는 로드 시 롤백(false), 나무 on·몬스터 off (F4).
- [x] **PASS** F5 객체 상태 재적용 13개 검사 전부 통과: 안개 숨김, 석등 [T,F,T] 잠금+Solved, 레버 changed+물길 B on/A off, 수문 stopped·폭포 off·GateOpen, 물레방아 running·승강대 HighPoint, 2-2 석판 열림 가능, Stage1ProgressGate on, Gate 5→8·8→9 blocker off, 7→8 blocker on.
- [x] **PASS** F6 NewGame: 모든 플래그 false, 딕셔너리 비움(F6a); 씬 재로드 시 기본 상태(안개 on, 석등 off·해제, 물길 A, 폭포 동작, 물레방아 정지, Gate blocker on, S1 gate 기본) 9개 검사 통과(F6b).

## G. 아트 상태 연결 / 렌더=콜라이더
- [x] **PASS** G1 `FlagStateVisual` 15개: 초기 Current=Default, 전부 WhiteSquare·알파 1 (arts[] 비어 있음).
- [x] **PASS** G2 플래그 변경에 상태 추종: lift.raised→Active, slide.success→Changed, 오류 없음(arts 비어 sprite 불변).
- [x] **PASS** G3 우선순위: 2-1 석판 ArtState 가 unlocked(Active)+read(Interacted) 동시일 때 Interacted.
- [x] **PASS** G4 렌더=콜라이더 전수 재확인 1회: 플레이스홀더 59, 콜라이더 37, 불일치 0 (오차 0.001, 실측 0.0000), 표 `evidence/28-03-bounds-recheck.md`.
- [x] **PASS** G5 추가 씬 3종 Play 스모크: 3종 모두 Play 진입, 프레임 진행, 컴파일 실패 없음, 콘솔 error 0.
- 아트 자체(스프라이트 교체/Animator 트리거)는 아트 도착 후 확인 → 수동 대기.

## 수동 확인 대기 목록 (통과 처리 안 함)
1. 보이는 모양·크기감(플레이스홀더 배치가 맵에서 읽히는지), 조작감.
2. 실제 플레이어 입력 경로: `PlayerInteraction`(E키·반경 2·프롬프트)로 석등/레버/수문/물레방아/석판/망원경 상호작용. (테스트는 `Interact(null)` 직접 호출)
3. 석판/망원경 UI 패널 레이아웃과 닫기 버튼, Pause 입력으로 닫기(`InputHandler.OnPauseEvent`).
4. 아트(노민기) 도착 후 FlagStateVisual 상태별 arts[] 채움과 Animator 트리거 이름.
5. 1→2-1, 2-1→2-2 전환 연결, `2 stage.unity` Build Settings 등록.
6. 기존 씬 Inspector 연결과 위치 배치(프리팹 인스턴스, lowerCheckpoint, areaEnemies, removesFog 구역, 전금재 슬라이드 이벤트).
7. 실제 플레이어 오브젝트(Layer/Tag/콜라이더 크기)로 트리거 반응 — 합성 Player 로만 확인함.
8. 실제 체크포인트 상호작용(`SaveSlotDialog` 슬롯 선택)과 로드 후 `PlayerSpawner` 스폰 위치 이동.
9. 설계 판단 필요(수정 안 함): 로드 후 2-2 현재 구역 `None` → 첫 트리거 구역이 무엇이든 수락, 9를 거치지 않으면 `s2.b.loop.completed` 미기록; `Stage1Trigger.oneShot` 사실상 무효; 2-1 석판은 areaEnemies 를 지정하지 않으면 영구 잠금.
10. 2-1 에서 `removesFog` 를 체크할 구역 지정(현재 모든 구역 미체크, 안개 제거는 TryRemoveFog 직접 호출로만 검증).

## 정리 확인
- Play 종료, worktree 에디터 종료(pid 확인), 메인 에디터 pid 41988 생존·미접촉, 임시 씬 `_P28Test` 삭제, Build Settings(`ProjectSettings`)/`.vscode`/`.slnx` 원복·삭제, 세이브 폴더 원복(md5 일치).
