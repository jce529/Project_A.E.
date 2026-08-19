# Camera — 스테이지 줌 전환 & X축 이동 제한 Play 모드 검증 체크리스트

**상태: 검증 생략 (사용자 결정)** — 09-03-PLAN.md 체크포인트에서 정적 검사(Task 1)는 통과했으나,
Unity Play 모드 실측 검증(Task 2)은 사용자가 명시적으로 생략하기로 결정하여 수행되지 않았다.
아래 체크리스트 항목은 실측되지 않았으므로 미체크 상태로 남긴다. 런타임 동작(부드러운 줌 전환,
이탈 자동 복귀, 줌 7 상태에서의 X축 클램프)은 아직 사람이 눈으로 확인하지 않았다는 뜻이다.

## 검증 대상 변경사항

`Assets/Camera/Script/CameraController.cs` (위치 추종 전용 → 줌 + X축 클램프 추가, 기존 로직은 삽입만 하고 무변경)
`Assets/Camera/Script/BossZoomTrigger.cs` (신규 — 보스 구역 트리거)

| Inspector 필드 | 기본값 | 의미 | 결정 |
|---|---|---|---|
| `normalZoom` | 5 | 일반 스테이지 orthographic size | D-04 / D-05 |
| `bossZoom` | 7 | 보스 구역 orthographic size | D-04 / D-05 |
| `zoomSmoothing` | 3 | 줌 Lerp 속도 (위치 추종 `smoothing`=5 와 별개) | D-06 / D-07 |
| `minX` | -1000 | 카메라 화면 좌측 한계 (월드 X) | D-09 / D-10 |
| `maxX` | 1000 | 카메라 화면 우측 한계 (월드 X) | D-09 / D-10 |

`LateUpdate` 실행 순서: 위치 추종 Lerp → 줌 Lerp → X 클램프.
클램프는 `minX + halfWidth` ~ `maxX - halfWidth` 범위이며 `halfWidth = orthographicSize * aspect` (D-11).
**Y축은 의도적으로 제한하지 않는다 (D-09).**

`minX`/`maxX` 기본값이 넓은(-1000/1000) 이유: 기존 씬의 카메라 동작을 바꾸지 않기 위해서다.
실제 값은 씬마다 사용자가 직접 튜닝한다.

## 사전 준비

1. Unity 에디터를 열고 컴파일이 끝날 때까지 대기한다. Console 에 **에러 0건**인지 확인한다.
2. `Assets/Scenes/1 stage.unity` 를 연다.
3. Hierarchy 에서 빈 GameObject 생성 → 이름 `TempBossZoneTest` 로 변경.
4. `TempBossZoneTest` 에 `BoxCollider2D` 추가 → **Is Trigger 체크** → Size 를 플레이어가 확실히
   드나들 만한 크기로 설정(예: X=10, Y=10).
5. `TempBossZoneTest` 에 `BossZoomTrigger` 컴포넌트를 추가한다.
6. 플레이어가 지나다니는 경로 위에 `TempBossZoneTest` 를 배치한다.
7. Hierarchy 에서 `Main Camera` 선택 → Inspector 에 신규 필드 5개가 보이는지 확인한다.

> 이 `TempBossZoneTest` 오브젝트는 **검증용 임시 오브젝트**다. 검증이 끝나면 삭제해도 되고,
> 실제 보스 구역에 트리거를 배치하는 작업은 D-08 에 따라 사용자가 별도로 진행한다.
> (검증 후 씬을 저장하지 않으면 임시 오브젝트는 남지 않는다.)

## 검증 항목

- [ ] **컴파일 & 필드 노출 (D-05 / D-10)**: Console 에러 0건이고, `Main Camera` Inspector 에
      `Normal Zoom`(5) / `Boss Zoom`(7) / `Zoom Smoothing`(3) / `Min X`(-1000) / `Max X`(1000)
      5개 필드가 모두 보인다.
- [ ] **기존 동작 회귀 없음**: Play 시 카메라가 예전과 동일하게 플레이어를 따라다닌다
      (시작 순간 튀거나 줌이 갑자기 변하지 않는다).
- [ ] **진입 줌 전환 (D-01 / D-04 / D-06)**: 플레이어가 `TempBossZoneTest` 안으로 들어가면
      Main Camera 의 `Size` 가 5 → 7 로 **점프 없이 부드럽게** 증가한다 (Inspector 의 Camera >
      Size 값이 몇 프레임에 걸쳐 변하는 것으로 확인).
- [ ] **이탈 자동 복귀 (D-03)**: 트리거 밖으로 나오면 별도 조작 없이 `Size` 가 7 → 5 로 되돌아온다.
- [ ] **재진입 안정성**: 트리거를 빠르게 여러 번 드나들어도 줌이 5/7 사이에서 정상 동작하고
      중간 값에 멈추거나 에러가 나지 않는다.
- [ ] **Inspector 튜닝 가능 (D-05)**: Play 중 `Boss Zoom` 을 9 로 바꾸고 다시 트리거에 들어가면
      9 로 확대된다 (하드코딩이 아님을 확인).
- [ ] **줌 속도 분리 (D-07)**: Play 중 `Zoom Smoothing` 을 1 로 낮추면 줌 전환만 느려지고,
      플레이어 추종 속도(`Smoothing` = 5)는 그대로다.
- [ ] **X축 클램프 — 일반 줌 (D-09 / D-10 / D-11)**: `Min X` / `Max X` 를 실제 맵 좌우 끝 좌표에
      맞춰 설정한 뒤, 플레이어가 맵 좌/우 끝까지 이동해도 화면에 맵 경계 바깥(빈 공간)이 보이지 않는다.
- [ ] **X축 클램프 — 보스 줌 (D-11 핵심)**: 같은 `Min X` / `Max X` 값에서 **보스 줌(7) 상태로**
      맵 좌/우 끝까지 이동해도 경계 바깥이 보이지 않는다. (줌 7 은 화면이 더 넓어 여기서 새기 쉽다 —
      반드시 확인할 것.)
- [ ] **Y축 무제한 (D-09)**: 플레이어가 위/아래로 이동할 때 카메라 Y 는 여전히 제한 없이 따라간다.
- [ ] **경계가 화면보다 좁을 때(튜닝 노트)**: `Max X - Min X` 가 화면 폭(= `Size * aspect * 2`)보다
      좁게 설정되면 카메라가 한쪽으로 붙는다. 이는 방어 코드를 넣지 않기로 한 알려진 동작이며,
      이 경우 `Min X` / `Max X` 를 더 넓게 잡으면 된다. 보스 줌(7) 기준으로 여유가 있는지 확인한다.

## 튜닝 값 기록

| 씬 | Min X | Max X | 비고 |
|---|---|---|---|
| 1 stage.unity | | | |
| Tutorial Map.unity | | | |

## 결과 기록

정적 회귀 검사(09-03-PLAN.md Task 1)는 9개 항목 전부 통과했다: LateUpdate 순서, 반폭 공식,
Y축 미클램프, 싱글톤 형태(DontDestroyOnLoad 없음), 줌 속도 분리(zoomSmoothing), 줌 값 필드화,
BossZoomTrigger Enter/Exit 가드, 인코딩/삽입 전용 게이트, 커밋 경로 범위(Assets/Camera/ 한정).

Play 모드 실측 검증(Task 2, 위 체크리스트)은 사용자 결정으로 생략되었다 (2026-07-30).
따라서 부드러운 줌 전환, 이탈 자동 복귀, 재진입 안정성, Inspector 실시간 튜닝, 줌 5/7 양쪽에서의
X축 클램프 동작은 실측으로 확인되지 않은 상태다. 실제 보스 씬에 트리거를 배치하기 전, 최소 1회는
Play 모드에서 이 체크리스트를 직접 확인할 것을 권장한다.

# Phase 10 - 데드존 3종 (Base Deadzone / Dynamic Offset / Peeking) Play 모드 검증 체크리스트

## Phase 10 검증 대상 변경사항

`Assets/Camera/Script/CameraController.cs` (Phase 9 줌/클램프 위에 데드존 3종 레이어링, 기존 로직 무변경)
신규 파일 없음. `PlayerController.cs` / `InputHandler.cs` 는 0줄 변경.

| Inspector 필드 | 기본값 | 의미 | 결정 |
|---|---|---|---|
| `deadzoneWidth` | 3 | 데드존 박스 가로(월드 유닛). 카메라 X 이동을 게이트 | D-01 / D-02 |
| `deadzoneHeight` | 2 | 데드존 박스 세로. **카메라 Y 하드컷 게이트** (Gizmo 전용 → 실동작 승격) | D-01 / 가정 A1 폐기 (260804-q6h) |
| `maxOffsetDistance` | 1.5 | 경계를 밀 때 진행 방향으로 앞서 나가는 거리 | D-05 |
| `offsetSmoothTime` | 0.35 | 오프셋 SmoothDamp 시간 | D-07 |
| `offsetHoldDuration` | 0.4 | 멈춘 뒤 오프셋을 유지하는 시간(초) | D-06 |
| `peekThreshold` | 0.5 | 피킹 발동까지 입력을 유지해야 하는 시간(초) | D-13 (사용자 값) |
| `peekDistance` | 3 | 피킹 시 카메라 Y 이동 거리 | D-13 |
| `peekSmoothTime` | 0.35 | 피킹 나갈 때 SmoothDamp 시간 | D-13 |
| `peekReturnSmoothTime` | 0.12 | 피킹 복귀(취소) SmoothDamp 시간 - 더 빠름 | D-13 |
| `idleSpeedThreshold` | 0.05 | 이 속도(유닛/초) 이하면 "정지"로 간주 | D-10 |
| `peekCancelSpeed` | 12 | 이 속도 이상이면 피킹 즉시 취소 (대시 20 / 달리기 7 사이) | D-11 |

`LateUpdate` 실행 순서: 레거시 추종 Lerp -> **보스/일반 분기** -> 줌 Lerp -> X 클램프 -> 데드존 앵커 재동기화.
일반 스테이지 합성식: `카메라X = 데드존중심X - 박스오프셋X`, `카메라Y = 데드존중심Y + 피킹Y` (260804-q6h 에서 추종베이스Y Lerp → 데드존중심Y 하드컷으로 교체).
**보스 구역(`SetBossZoom(true)`)에서는 3종 전부 비활성화되고 Phase 9 레거시 Lerp 만 동작한다 (D-15).**

## Phase 10 사전 준비

1. Unity 에디터를 열고 컴파일 완료를 기다린다. Console 에 **에러 0건**인지 확인한다.
2. `Assets/Scenes/1 stage.unity` 를 연다.
3. Hierarchy 에서 `Main Camera` 선택 -> Inspector 에 위 표의 11개 신규 필드가 모두 보이는지 확인한다.
4. Scene 뷰를 열어둔 채로 Play 한다 (Gizmo 박스를 보려면 Scene 뷰가 필요하다).
5. 보스 구역 테스트는 Phase 9 체크리스트의 `TempBossZoneTest`(BoxCollider2D + Is Trigger + `BossZoomTrigger`) 오브젝트를 그대로 사용한다.

## Phase 10 검증 항목

### 1) Base Deadzone (D-01 / D-02 / D-03 / D-14)

- [ ] **박스 Gizmo 표시**: Scene 뷰에 카메라 위치를 중심으로 노란 박스(가로 3 / 세로 2)가 보인다.
- [ ] **박스 안에서 완전 정지**: 플레이어를 박스 폭 안에서 좌우로 조금씩 움직이면 **카메라가 전혀 움직이지 않는다** (Main Camera Transform X 고정).
- [ ] **경계 밀기 = 즉시 추종**: 플레이어가 박스 좌/우 끝을 넘어가면 카메라가 지연 없이(하드컷) 따라붙고 플레이어가 경계에 붙은 채로 이동한다. 카메라가 뒤늦게 스르륵 따라오면 잘못 구현된 것이다 (D-14 위반).
- [ ] **Inspector 튜닝**: Play 중 `deadzoneWidth` 를 6 으로 키우면 정지 구간이 눈에 띄게 넓어진다.
- [ ] **줌 무관 고정 크기 (D-02)**: 보스 구역에 들어가 줌이 7 이 되어도 Gizmo 박스 크기는 그대로다.

### 2) Dynamic Asymmetrical Deadzone (D-04 ~ D-07)

- [ ] **진행 방향 시야 확보 (부호 검증 - 가장 중요)**: 오른쪽으로 계속 달리면 플레이어가 화면 왼쪽 편에 놓이고 **오른쪽(진행 방향)이 더 넓게 보인다**. 반대로 보이면 부호가 뒤집힌 것이다 (10-02-PLAN 가정 A2 위반).
- [ ] **왼쪽 대칭**: 왼쪽으로 달리면 왼쪽이 더 넓게 보인다.
- [ ] **부드러운 전환 (D-07)**: 방향을 바꿀 때 시야가 순간이동하지 않고 `offsetSmoothTime` 에 걸쳐 이동한다.
- [ ] **정지 후 유지 (D-06)**: 달리다 멈추면 오프셋이 즉시 사라지지 않고 약 0.4초 유지된 뒤 서서히 복귀한다.
- [ ] **정지 후 정중앙 안착 (checkpoint 수정, `07f9db3`)**: 달리다 완전히 멈추고 유지시간+복귀시간(약 0.75초)이
      지나면, 카메라가 진행 방향 반대쪽에 치우쳐 멈추지 않고 **플레이어가 화면 정중앙에 온다** (달리기 직전
      정지 상태와 동일한 프레이밍). 이전 버그: 정지 후 오프셋이 0으로 빠지면서 진행 방향이 오히려 더 좁아
      보였음 — 좌우 모두 확인.
- [ ] **경계를 밀 때만 발동 (D-05)**: 박스 안에서 좌우로 조금씩 움직이는 동안에는 오프셋이 생기지 않는다.

### 3) Input-based Peeking (D-08 ~ D-13)

- [ ] **위 피킹**: 땅에 선 채 멈춰서 위 방향키를 0.5초 이상 유지하면 카메라가 천천히 위로 올라간다.
- [ ] **아래 피킹**: 같은 조건에서 아래 방향키를 유지하면 카메라가 아래로 내려간다.
- [ ] **임계 시간 (D-13)**: 위 방향키를 0.2초만 눌렀다 떼면 카메라가 움직이지 않는다.
- [ ] **이동 시 취소**: 피킹 중 좌우로 움직이면 피킹이 풀리고 원위치로 복귀한다.
- [ ] **공중 불가 (D-12)**: 점프 중 위 방향키를 유지해도 피킹이 발동하지 않는다.
- [ ] **급증 취소 (D-11)**: 피킹 중 대시하면 즉시 취소되며 빠르게 복귀한다. 피격(넉백)에서도 동일하다.
- [ ] **입력 잠금 가드 (D-09)**: `movementLocked` 가 true 인 상황(컷신/구속)에서 위 방향키를 유지해도 피킹이 발동하지 않는다. 재현이 어려우면 Play 중 `PlayerController` 의 `Movement Locked` 를 Inspector 에서 강제 체크해 확인한다.
- [ ] **씬 전환 후 정상 동작 (구독 누수)**: 다른 씬에 갔다 돌아온 뒤에도 피킹이 정상 동작하고 Console 에 파괴된 오브젝트 관련 경고가 없다. 반응이 두 배로 세지면 구독이 중복된 것이다.

### 4) 보스 구역 비활성화 & Phase 9 회귀 (D-15 / D-16 / D-17)

- [ ] **보스 구역 진입**: `TempBossZoneTest` 안에 들어가면 줌이 5 -> 7 로 부드럽게 전환되고(Phase 9 동작 유지), **카메라가 데드존 없이 플레이어를 계속 부드럽게 따라다닌다** (정지 구간 없음).
- [ ] **보스 구역 내 피킹 불가**: 보스 구역 안에서 위/아래 입력을 유지해도 카메라가 시야 이동하지 않는다.
- [ ] **이탈 복귀**: 보스 구역을 나오면 데드존/오프셋/피킹이 다시 동작하고, 전환 순간 카메라가 **튀지 않는다**.
- [ ] **X 클램프 유지 (D-17)**: `Min X` / `Max X` 를 맵 좌우 끝에 맞춘 뒤, 데드존/오프셋/피킹이 걸린 상태로 맵 끝까지 가도 화면에 맵 바깥이 보이지 않는다.
- [ ] **맵 끝 복귀 반응성**: 맵 좌/우 끝에서 되돌아올 때 카메라가 한참 멈춰 있다가 뒤늦게 따라오지 않는다 (클램프 후 앵커 재동기화 확인).
- [ ] **Y축 동작 (가정 A1 폐기됨)**: 가정 A1("Y축 데드존 없음")은 quick task `260804-q6h` 에서 폐기되었다. 일반 스테이지 점프/낙하 시의 카메라 Y 동작은 아래 **5) Y축 하드컷 데드존** 섹션에서 검증한다. (옛 기대값이던 "Y 는 스무딩으로 따라온다"는 더 이상 정상 동작이 아니다. 이 섹션에서는 **보스 구역 안에서만** Y 가 레거시 Lerp 로 부드럽게 따라오는지 확인한다.)

### 5) Y축 하드컷 데드존 (quick task 260804-q6h — 가정 A1 폐기)

`deadzoneHeight`(기본 2)가 Gizmo 표시 전용에서 **실제 카메라 Y 게이트**로 승격되었다. `UpdateDeadzoneCenterY()` 는
X축 `UpdateDeadzoneCenter()` 와 동일 계열의 하드컷이며(DY-01), 기존 `_followBaseY` 의 `smoothing` Lerp 는 제거되었다.
동적 오프셋/피킹과 완전 독립이고 공중/지상 분기가 없으며(DY-02), 보스 구역에서는 비활성이다(DY-03).
피킹 Y 는 이 결과 위에 그대로 얹힌다(DY-04).

- [ ] **박스 안에서 Y 완전 정지**: 박스 세로 범위 안에서 작은 단차를 오르내려도 `Main Camera` Transform Y 가 고정이다.
- [ ] **위 경계 밀기 = 즉시 추종 (하드컷)**: 점프해서 박스 상단 경계를 넘으면 카메라가 지연 없이 따라붙고, 플레이어가 상단 경계에 붙은 채로 올라간다. 스르륵 늦게 따라오면 Lerp 가 남아 있는 것이다 (DY-01 위반).
- [ ] **아래 경계 대칭**: 낙하로 하단 경계를 넘으면 동일하게 즉시 따라온다.
- [ ] **플레이어가 항상 박스 안 (이 작업의 목적)**: Scene 뷰에서 **노란 박스 세로 범위 밖에 플레이어가 서 있는 장면이 더 이상 나오지 않는다** (2026-08-04 사용자 보고 이슈 해소 확인).
- [ ] **Gizmo Y 중심 반영**: Play 중 노란 박스가 카메라 Y 가 아니라 데드존 중심 Y 에 그려진다. 피킹 중에는 박스가 화면 세로 중앙에서 피킹 거리만큼 어긋나 보이는 것이 **정상**이다 (X축에서 오프셋이 걸렸을 때와 동일한 구조).
- [ ] **Inspector 튜닝**: Play 중 `Deadzone Height` 를 6 으로 키우면 Y 정지 구간이 눈에 띄게 넓어진다.
- [ ] **피킹과 독립 (DY-04)**: 위/아래 피킹이 정상 동작하고, 피킹이 끝나면 박스 중심은 그대로인 채 카메라만 되돌아온다 (박스가 피킹을 따라 끌려가면 되먹임 버그).
- [ ] **동적 오프셋과 독립 (DY-02)**: 좌우로 달려 X 오프셋이 걸린 상태에서도 Y 데드존 동작이 달라지지 않는다.
- [ ] **공중/지상 구분 없음 (DY-02)**: 점프 중에도 Y 데드존이 지상과 완전히 동일하게 동작한다 (공중 전용 분기 없음).
- [ ] **보스 구역 비활성 (DY-03)**: 보스 구역 안에서는 Y 가 기존 Phase 9 레거시 Lerp 로 부드럽게 따라오고, 진입/이탈 시 카메라 Y 가 크게 튀지 않는다.
- [ ] **점프 체감 (튜닝 포인트)**: 하드컷이라 높은 점프에서 Y 가 딱딱하게 느껴질 수 있다. 거슬리면 `Deadzone Height` 를 키워 대응한다. **스무딩 재도입은 DY-01 위반이므로 금지** — 필요하면 별도 재논의 항목으로 남긴다.

## Phase 10 튜닝 값 기록

| 씬 | deadzoneWidth | maxOffsetDistance | offsetHoldDuration | peekDistance | 비고 |
|---|---|---|---|---|---|
| 1 stage.unity | | | | | |
| Tutorial Map.unity | | | | | |

## Phase 10 결과 기록

### 정적 회귀 검사 (10-04-PLAN.md Task 2, 2026-08-04)

| # | 검사 | 기대값 | 실제값 | 판정 |
|---|---|---|---|---|
| 1 | 인코딩 게이트 (`LC_ALL=C grep -c '[^[:print:][:space:]]'`) | 5 | 5 | PASS |
| 2 | 삭제 라인 총량 (10-01~10-03 누적) | 3줄, 비-ASCII 0 | 3줄 (`p.x = _deadzoneCenterX;` / `if (!_isBossZone) _deadzoneCenterX = transform.position.x;` / `p.y = _followBaseY;`), 비-ASCII 0 | PASS* |
| 3 | 읽기 전용 파일 무수정 (`PlayerController.cs`/`InputHandler.cs`) | 0줄 | 0줄 (status 0줄, diff --name-only vs ef6f164 0줄) | PASS |
| 4 | `LateUpdate` 실행 순서 | 레거시Lerp<보스분기<줌Lerp<마지막ApplyXClamp<앵커재동기화 | 303 < 306 < 308 < 310 < 313 | PASS |
| 5 | `ApplyNormalStageCamera` 내부 순서 | 속도계산<Deadzone<Offset<Peek<...<`_lastTargetPos`갱신 | 189 < 190 < 191 < 192 < ... < 199 | PASS |
| 6 | 하드컷 보존 (`UpdateDeadzoneCenter` 본문에 Lerp/SmoothDamp 없음) | 0 | 0 | PASS |
| 7 | 금지 심볼 (`isDashing`/`isKnockedBack`/`Rigidbody`/`minY`/`maxY`/`DontDestroyOnLoad`/`Cinemachine`) | 전부 0 | `isDashing`=0, `isKnockedBack`=0, `Rigidbody`=0, `minY`=0, `maxY`=0, `DontDestroyOnLoad`=1, `Cinemachine`=0 | PASS** |
| 8 | 구독 대칭 및 캐시 (`OnMoveEvent +=`/`-=`/`GetComponent<PlayerController>()`) | 1, 1, 1 | 1, 1, 1 | PASS |
| 9 | `BossZoomTrigger.cs` 무변경 및 `SetBossZoom` 시그니처 | 0줄, 1 | 0줄, 1 | PASS |

\* **검사 2 비고 (baseline 커밋 선택 오류 계열)**: `git diff ef6f164 -- CameraController.cs`를 한 번에 실행하면 삭제 라인이 `0`으로 나온다 (236줄 전부 삽입으로만 매칭됨 — Phase 10 Plan 1~3 SUMMARY에 기록된 것과 동일한 계열의 diff 알고리즘 특성이며 실제 회귀가 아니다). 대신 10-01~10-03 각 태스크 커밋을 그 직전 커밋과 개별 diff하여 합산했다: `5a36816`=0, `95592bb`=0, `717e37f`=0, `f24d53a`=2, `5d5b55e`=0, `b4ee51a`=1, 합계 3줄. 삭제된 3줄 내용이 플랜 명세와 정확히 일치하고 전부 ASCII임을 확인했다.

\*\* **검사 7 비고 (`DontDestroyOnLoad`=1)**: 112행 주석 `// InputHandler is DontDestroyOnLoad while this camera is scene local, ...`은 10-03-PLAN.md 170행에 명시된 원문 그대로다 — `InputHandler`(다른 클래스)의 실제 동작을 설명하는 주석이며, `CameraController` 자신이 `DontDestroyOnLoad`를 호출하지 않는다는 원래 취지(Phase 9 결정, "씬 로컬 싱글톤")는 그대로 유지된다. 코드에 `DontDestroyOnLoad(...)` 호출은 여전히 0건이다. 이 게이트의 리터럴 문자열 카운트 기대값(0)이 10-03에서 이미 커밋된 정당한 주석과 충돌하는, Phase 9 Plan 1(`09-01`)에서도 발견된 것과 동일한 "플랜 자체 검증 스크립트 설계 오류" 패턴이다. `CameraController.cs`는 이 플랜에서 수정하지 않았다.

**9개 검사 전부 실질적으로 PASS.** 정적 검사만 완료되었으며, **Play 모드 미검증** 상태다 (Task 3 참고).

### Play 모드 실측 결과 (10-04-PLAN.md Task 3, 2026-08-04)

- **확인 항목**: "Y축 동작 (가정 A1)" (161행)
- **판정**: 이슈 보고 (FAIL 아님, 설계 범위 밖 요청 — 코드는 명세대로 정확히 동작함)
- **사용자 관찰**: Play 모드에서 실제 플레이해본 결과, 캐릭터가 노란 데드존 박스 세로 범위 밖(아래)에 서 있는 장면을 확인. 원인 진단 결과 "B"(Y축은 애초에 하드컷 게이트가 없고 `deadzoneHeight`는 Gizmo 표시 전용이며, Y는 기존 `_followBaseY` Lerp 스무딩만 따라간다 — `CameraController.cs` 28-33행 주석과 일치)가 맞다고 확인.
- **사용자 요청 (원문)**: "내가 직접 플레이해보니까 Y축 방향으로도 데드존이 있는게 더 좋을 것 같아"
- **판단**: 코드는 10-01-PLAN 잠금 가정 A1("Y축 데드존 없음")을 정확히 구현하고 있으므로 회귀가 아니다. 그러나 이 가정 자체를 사용자가 재논의하고 싶어하므로, **Y축 데드존 도입은 Phase 10 범위 밖의 신규 요구사항**으로 별도 gap-closure 플랜에서 다룬다 (10-04-PLAN.md 체크포인트 규칙: "이 체크포인트에서 즉흥 코드 수정 금지").
- **이 플랜에서의 조치**: `CameraController.cs` 무수정. 위 "Y축 동작 (가정 A1)" 체크박스는 미체크로 남김 (PASS 허위 기록 금지).

## 부가 기능: X 경계(minX/maxX) Gizmo 확인 (에디터 모드, Play 불필요)

Phase 10 체크리스트 검증 중 사용자 요청으로 추가됨 (`fa427d7`). `Assets/Camera/Script/CameraController.cs`의
`OnDrawGizmos`에 minX/maxX 위치의 빨간 세로선 2개를 추가해, Play 없이 에디터 모드에서도 카메라 좌우 한계를
바로 확인할 수 있게 했다.

- [ ] **선 표시**: `1 stage.unity`(또는 개발용 씬)에서 Hierarchy → `Main Camera` 선택 시, Scene 뷰에
      **빨간 세로선 2개**가 보인다 (minX 위치, maxX 위치 각각 1개).
- [ ] **기본값 확인**: `Min X`/`Max X` Inspector 기본값이 `-1000`/`1000`이면, 두 선이 화면에서 아주
      멀리(맵 밖) 떨어져 보이거나 아예 뷰포트 밖에 있다 (정상 — 아직 씬별로 튜닝 전).
- [ ] **실시간 반응**: Inspector에서 `Min X`/`Max X` 값을 맵 좌우 끝 좌표로 바꾸면, Scene 뷰의 빨간 선이
      **Play 하지 않아도 즉시** 새 위치로 이동한다.
- [ ] **Play 모드에서도 유지**: Play 중에도 두 선이 계속 보이고(데드존 노란 박스와 함께), 카메라가 선을
      넘어가지 않는지 눈으로 대조할 수 있다.
- [ ] **런타임 영향 없음**: 에디터 전용 Gizmo이므로 빌드/런타임 동작에는 영향이 없다 (코드 리뷰로 확인 —
      `OnDrawGizmos`는 Unity 에디터에서만 호출됨).

## quick task 260804-q6h 결과 기록

### 정적 회귀 검사 (2026-08-04)

| # | 검사 | 기대값 | 실제값 | 판정 |
|---|---|---|---|---|
| 1 | 인코딩 게이트 (`LC_ALL=C grep -c '[^[:print:][:space:]]'`) | 5 | 5 | PASS |
| 2 | 삭제 라인 비-ASCII | 0 | 0 (삭제 11줄, 전부 ASCII) | PASS |
| 3 | `_followBaseY` 잔존 | 0 | 0 | PASS |
| 4 | `deadzoneHeight` 등장 | 3 | 3 | PASS |
| 5 | `UpdateDeadzoneCenterY` 본문 Lerp/SmoothDamp/오프셋·피킹 심볼 | 0 | 0 | PASS |
| 6 | 레거시 Lerp / X 재앵커 라인 삭제 | 0, 0 | 0, 0 | PASS |
| 7 | `minY`/`maxY`/`IsGrounded` | 0 / 0 / 1 | 0 / 0 / 1 | PASS |
| 8 | 읽기 전용 파일 무수정 (`PlayerController.cs`/`InputHandler.cs`/`BossZoomTrigger.cs`) | 0줄 | 0줄 | PASS |
| 9 | 수정 파일 목록 | `CameraController.cs`, `Check.md` 2개 | `CameraController.cs`, `Check.md` 2개 | PASS |

**Play 모드 검증 상태**: 미검증 (위 "5) Y축 하드컷 데드존" 체크리스트는 사용자가 직접 확인해야 한다. PASS 로 허위 기록 금지.)

---

# quick task 260805-m41 — 구역별 카메라 X 경계 (CameraBoundsTrigger)

> **이 섹션의 캐시/복원 방식(MX-05)은 quick task 260805-q2u 에서 폐기되었다.** 아래 내용은 이력 보존용이며,
> 현행 동작은 문서 맨 아래 "quick task 260805-q2u" 섹션을 따른다.

## 변경사항

- `CameraController.SetXBounds(float min, float max)` 신규 — `minX`/`maxX` 에 단순 대입만 한다(MX-01).
  클램프 계산은 기존 `ApplyXClamp()` 가 매 프레임 그대로 수행하므로 신규 로직이 없다.
- `Assets/Camera/Script/CameraBoundsTrigger.cs` 신규 — `BossZoomTrigger` 와 동일 패턴(Player 태그 가드 +
  `CameraController.Instance` 싱글톤). `zoneMinX` / `zoneMaxX` 두 개의 Inspector 필드를 가진다(MX-03).
- **구역 = 범위 한정 오버라이드 (MX-05)**: 진입 시 **직전에 활성이던 경계를 캐시**한 뒤 구역 경계를 적용하고,
  이탈 시 그 캐시 값으로 **되돌린다**. `BossZoomTrigger` 가 이탈 시 일반 줌으로 복귀하는 것과 같은 성격이다.
  캐시는 **트리거 인스턴스당 단일 슬롯**이며 스택/히스토리/구역 매니저는 없다(의도적 단순화).
- 구역 안에서 스폰해 진입 이벤트가 발생하지 않은 트리거는 이탈 시 **아무것도 복원하지 않는다**
  (`_hasCachedPrev` 가드). 가드가 없으면 `0 / 0` 이 적용되어 카메라가 한 지점에 고정된다.

## 알려진 한계 (코드로 방어하지 않음 — 레벨 디자인으로 회피)

- **[해소됨 — 260805-q2u]** 이탈 시 항상 고정 기본 경계로 복귀하도록 바뀌어 stale 복원 자체가 발생하지
  않는다. 단, 구역을 겹치면 어느 트리거의 Exit 가 먼저 오느냐에 따라 잠깐 기본 경계로 떨어졌다 다시
  잡히므로 여전히 겹치지 않게 배치하는 편이 좋다.
- **구역 사이의 빈틈**에서는 직전 구역 진입 전의 경계(보통 씬 기본값)로 돌아간다. 이는 설계상 정상이며,
  방과 방 사이가 곧바로 이어지길 원하면 트리거를 맞붙여 배치한다. → 260805-q2u 의 **타일링 지침** 참조
- **구역 폭 < 카메라 가로폭**이면 `Mathf.Clamp` 의 min 이 max 를 넘어 카메라가 한 지점에 고정된다.
  코드에서 막지 않는다(설계상 designer 책임) — 아래 설정 5단계 참조.

## 수동 에디터 설정 (사용자 작업 — 코드로는 하지 않음, MX-04)

구역 하나당 아래를 반복한다.

1. Hierarchy 에서 빈 GameObject 생성 → 이름 예: `CameraBounds_Room01`.
2. `Box Collider 2D` 추가 → **`Is Trigger` 체크**.
3. Collider 의 Size/Offset 을 해당 방 전체를 덮도록 조정한다. **다른 구역 트리거와 겹치지 않게** 배치한다
   (겹침은 위 "알려진 한계"의 stale 복원 원인이다). 방과 방을 곧바로 잇고 싶으면 맞붙이되 겹치지는 않게 한다.
4. `Camera Bounds Trigger` 스크립트 추가 → `Zone Min X` / `Zone Max X` 에 그 방의 좌/우 월드 X 좌표 입력.
5. **구역 폭 >= 카메라 가로폭** 을 지킨다. 카메라 가로폭 = `2 * orthographicSize * aspect`
   (일반 스테이지 `normalZoom = 5`, 16:9 기준 약 17.8 월드 유닛). 이보다 좁으면 클램프가 한 점으로
   무너져 카메라가 고정된다 — 코드에서 막지 않는다(설계상 designer 책임).
6. `Main Camera` 의 `Min X` / `Max X` Inspector 값이 **기본(fallback) 경계**가 된다. 어떤 구역에도 속하지
   않을 때 이 값이 쓰이므로, 맵 전체를 감싸는 넉넉한 값으로 맞춰 둔다.
7. 이스터에그 확장 구역은 **같은 스크립트에 넓은 값만** 넣으면 된다. 별도 컴포넌트 불필요.
8. 보스 구역은 `BossZoomTrigger`(줌)와 `CameraBoundsTrigger`(경계)를 **각각 독립적으로** 붙인다.
   한 GameObject 에 둘 다 붙여도 되고 따로 둬도 된다 — 서로 간섭하지 않는다.

## Play 모드 검증 항목

### 6) 구역별 카메라 X 경계 (quick task 260805-m41)

> **폐기됨 — 아래 7) 로 대체.**

- [ ] **구역 진입 시 경계 교체**: 방 A 에서 방 B 로 넘어가면 `Main Camera` Inspector 의 `Min X` / `Max X`
      값이 방 B 트리거의 `Zone Min X` / `Zone Max X` 로 바뀐다 (Play 중 Inspector 실시간 확인).
- [ ] **구역 이탈 시 복원 (MX-05)**: 방 B 를 빠져나오면 `Min X` / `Max X` 가 **진입 직전 값**(보통 씬 기본값)
      으로 되돌아간다. 값이 그대로 남아 있으면 `OnTriggerExit2D` 가 없거나 태그 가드에서 걸린 것이다.
- [ ] **캐시 순서 확인**: 구역 이탈 후 복원된 값이 **그 구역 자신의 값과 다르다**. 같다면 진입 시
      `SetXBounds` 를 호출한 **뒤에** 캐시한 것이다 (MX-05 순서 위반).
- [ ] **다음 프레임 클램프**: 값이 바뀐 직후 카메라가 새 경계 안쪽으로 즉시(1프레임) 들어온다.
      별도 재클램프 코드 없이 기존 `ApplyXClamp()` 만으로 동작하는지 확인 (MX-01 근거).
- [ ] **좌우 하드 클램프**: 방 안에서 좌/우 끝까지 달리면 카메라가 `Zone Min X`/`Zone Max X` 에서 정확히
      멈추고 플레이어만 계속 이동한다.
- [ ] **경계에서 되돌아올 때 즉시 반응**: 클램프된 상태에서 반대 방향으로 걸으면 카메라가 지연 없이
      따라온다 (D-17 재앵커가 여전히 동작하는지 = 데드존 회귀 확인).
- [ ] **인접 구역 연속 통과**: 방 A ↔ B 를 빠르게 여러 번 오가도 경계가 튀지 않고 매번 들어간 방의 값이
      적용되며, 나올 때마다 직전 값으로 복원된다.
- [ ] **구역 안 스폰 후 이탈 (안전장치)**: 플레이어가 트리거 **안에서 시작**한 뒤 밖으로 걸어나가도
      카메라가 한 지점에 고정되지 않는다 (`_hasCachedPrev` 가드가 `0 / 0` 복원을 막는지 확인).
- [ ] **이스터에그 확장 구역**: 넓은 값을 넣은 구역에 들어가면 카메라가 훨씬 멀리까지 따라가고,
      나오면 원래 좁은 경계로 복원된다.
- [ ] **보스 구역과 독립**: `BossZoomTrigger` 구역에 들어가 줌이 바뀌어도 경계는 그대로이고,
      줌 전환 중에도 클램프가 깨지지 않는다 (`ApplyXClamp` 가 현재 `orthographicSize` 를 쓰므로
      화면이 넓어지면 유효 이동 범위가 좁아지는 것이 정상).
- [ ] **Player 태그 가드**: 적/투사체가 트리거를 드나들어도 경계가 바뀌거나 복원되지 않는다.
- [ ] **Gizmo 반영**: Play 중 `Main Camera` 의 빨간 세로선 2개가 현재 구역 경계 위치로 이동하고,
      이탈 시 원래 위치로 돌아온다 (`CameraController.OnDrawGizmos` 가 필드를 그대로 그리므로 자동 반영).
- [ ] **Phase 9/10 회귀 없음**: 데드존/동적 오프셋/피킹/줌 동작이 이전과 동일하다.

## quick task 260805-m41 결과 기록

### 정적 회귀 검사 (2026-08-05)

| # | 검사 | 기대값 | 실제값 | 판정 |
|---|---|---|---|---|
| 1 | `CameraController.cs` 삭제 라인 (순수 삽입) | 0 | 0 | PASS |
| 2 | `CameraController.cs` 인코딩 게이트 (비-ASCII 라인) | 5 | 5 | PASS |
| 3 | `minX` / `maxX` 등장 라인 (`CameraController.cs`) | 7 / 7 | 7 / 7 | PASS |
| 4 | `SetXBounds` 본문 `;` 개수 / 금지 심볼 | 2 / 0 | 2 / 0 | PASS |
| 5 | `CameraBoundsTrigger.cs` 비-ASCII / 라인 수 | 0 / <=56 | 0 / 54 | PASS |
| 6 | Enter/Exit 정의 + 태그 가드 (MX-05) | 1 / 1 / 2 | 1 / 1 / 2 | PASS |
| 7 | 캐시 순서 (캐시 행 < 적용 행 < 복원 행, 가드 행 < 복원 행) | 성립 | 40 < 43 < 52, 51 < 52 | PASS |
| 8 | private 필드 개수 / `List<`,`Stack<`,`static ` | 3 / 0 | 3 / 0 | PASS |
| 9 | `public` 시작 줄 (클래스 1 + 필드 2) | 3 | 3 | PASS |
| 10 | 읽기 전용 파일 무수정 (`BossZoomTrigger.cs`/`PlayerController.cs`/`InputHandler.cs`/`*.unity`) | 0줄 | 0줄 | PASS |
| 11 | 커밋 파일 목록 | 3개 | 3개 (`CameraController.cs`, `CameraBoundsTrigger.cs`, `Check.md`) | PASS |

**Play 모드 검증 상태**: 미검증 (위 "6) 구역별 카메라 X 경계" 체크리스트 13개는 사용자가 씬에 트리거를
직접 배치한 뒤 확인해야 한다. PASS 로 허위 기록 금지.)

---

# quick task 260805-q2u — 구역 타일링 + 부드러운 경계 전환 (260805-m41 재설계)

## 무엇이 바뀌었나

| 항목 | 260805-m41 (이전) | 260805-q2u (현행) |
|---|---|---|
| 경계 전환 | 프레임 단위로 즉시 덮어씀 (순간이동) | `boundsSmoothing` 속도로 매 프레임 Lerp (부드러운 전환) |
| `minX`/`maxX` 의미 | 런타임에 `SetXBounds` 가 직접 덮어쓰는 라이브 값 | 스테이지 **고정 기본** 경계, 런타임에 절대 쓰이지 않는 폴백 |
| `SetXBounds` 가 쓰는 대상 | `minX`/`maxX` (Inspector 필드 그 자체) | `_targetMinX`/`_targetMaxX` (Inspector 필드는 불변) |
| 이탈 시 복귀값 | 트리거가 캐시해둔 **진입 직전 값** | 항상 `CameraController.Instance.minX`/`maxX` (고정 기본값) |
| 트리거 상태 | `_prevMinX`/`_prevMaxX`/`_hasCachedPrev` 3개 인스턴스 필드 보유 | 인스턴스 상태 필드 0개 (완전 무상태) |
| 겹침 stale | 구역이 겹치면 낡은 값으로 복원될 수 있음 | 이탈 시 항상 고정 기본값으로 복귀하므로 stale 복원 자체가 없음 |

## Inspector 필드

| 필드 | 기본값 | 의미 | 결정 |
|---|---|---|---|
| `minX` | -1000 | 스테이지 **고정 기본** 좌측 경계 (어떤 zone 에도 없을 때의 폴백) | Q2-01 |
| `maxX` | 1000 | 스테이지 고정 기본 우측 경계 | Q2-01 |
| `boundsSmoothing` | 3 | 경계 전환 Lerp 속도 (`zoomSmoothing` 과 동일 성격, `smoothing`/`zoomSmoothing` 과 별개) | Q2-02 |
| `zoneMinX`/`zoneMaxX` (트리거) | -1000/1000 | 이 구역이 적용할 좌/우 월드 X | Q2-06 |

## 내부 구조

`_targetMinX`/`_targetMaxX` (`SetXBounds` 가 쓰는 목표값)와 `_currentMinX`/`_currentMaxX` (매 프레임
`boundsSmoothing` 으로 Lerp 되는 라이브 값)의 2단 구조다. `ApplyXClamp` 는 항상 `_current*` 만 소비한다.

갱신된 `LateUpdate` 순서: 레거시 추종 Lerp -> 보스/일반 분기 -> 줌 Lerp -> **경계 Lerp** -> X 클램프 ->
데드존 앵커 재동기화.

`Start()` 에서는 `_targetMinX`/`_targetMaxX`/`_currentMinX`/`_currentMaxX` 네 필드를 전부 `minX`/`maxX`
로 시드한 뒤 `ApplyXClamp()` 를 호출한다. **순서가 핵심** — 시드가 첫 클램프보다 먼저여야 한다. 그렇지
않으면 두 필드가 기본값 0 인 채로 첫 클램프가 실행되어, 프레임 1 에서 카메라가 x=0 부근으로 끌려간다.

Gizmo 규칙(Q2-05): Play 모드에서는 빨간 세로선 2개가 `_currentMinX`/`_currentMaxX` (라이브 값)를 그리고,
에디터 모드에서는 `minX`/`maxX` (기본값)를 그린다 — 기존 노란 데드존 박스가 `Application.isPlaying` 으로
분기하는 것과 동일한 패턴이다.

## 레벨 디자인 지침 (Q2-07 — 코드가 아니라 배치로 푸는 문제)

- **타일링**: 벽이 있는 모든 구간마다 그 위치에 맞춘 `CameraBoundsTrigger` 를 이어붙인다. "일반 구역"이라고
  트리거를 비워두면 그 구간에서는 최외곽 기본 경계가 적용되어 **카메라가 벽 너머를 미리 비춘다.**
  `minX`/`maxX` 는 어떤 zone 에도 속하지 않을 때의 최외곽 폴백일 뿐, "정상 상태"가 아니다.
- **Y 범위 분리**: `CameraBoundsTrigger` 는 `BoxCollider2D` 라 X 뿐 아니라 **Y 범위도 가진다.** 같은 X
  구간이라도 위쪽 통로(벽 없음)와 아래쪽 통로(벽 있음)에 **Y 범위가 겹치지 않는 별도 트리거 박스**를
  두면 층별로 다른 경계가 자동 적용된다. **코드 변경 불필요 — 트리거 박스 배치만 하면 된다 (사용자
  수동 작업).**
- **이어붙이되 겹치지는 않게**: 방과 방을 맞붙이면 전환이 끊기지 않는다. 겹치면 Exit/Enter 순서에 따라
  한 프레임 기본 경계로 튀었다 잡힐 수 있다(전환이 Lerp 라 눈에 잘 안 띄지만 권장하지 않음).
- **구역 폭 >= 카메라 가로폭**(`2 * orthographicSize * aspect`, 일반 줌 5·16:9 기준 약 17.8) 유지.
  기존 m41 섹션의 8단계 수동 설정 절차는 그대로 유효하므로 참조 링크만 남기고 재작성하지 않는다.
  (참조: 위 "수동 에디터 설정 (사용자 작업 — 코드로는 하지 않음, MX-04)")

## 알려진 한계

- 전환이 Lerp 이므로 `boundsSmoothing` 이 낮으면 좁은 구역에 빠르게 진입할 때 **경계가 아직 다 좁혀지기
  전에** 카메라가 잠깐 벽 너머를 비출 수 있다. 대응: `boundsSmoothing` 을 올리거나 트리거를 벽보다
  앞쪽에서 시작.
- 구역 폭 < 화면 폭이면 여전히 클램프가 한 점으로 무너진다 (설계상 designer 책임, 방어 코드 없음).
- 물리 충돌 기반 카메라 차단은 **논의 끝에 기각** — 재도입은 별도 재논의 항목.

## Play 모드 검증 항목

### 7) 구역 타일링 & 부드러운 경계 전환 (quick task 260805-q2u)

- [ ] **부드러운 전환**: 구역 진입 시 카메라 좌우 여백이 **점프 없이** 몇 프레임에 걸쳐 좁아진다.
- [ ] **`boundsSmoothing` 튜닝**: Play 중 1 로 낮추면 경계 전환만 느려지고 줌/추종 속도는 그대로다.
- [ ] **`Min X`/`Max X` 불변**: Play 중 구역을 드나들어도 `Main Camera` Inspector 의 `Min X`/`Max X` 값이
      **전혀 변하지 않는다** (변하면 Q2-01 위반 = 옛 방식이 남아 있는 것).
- [ ] **이탈 시 기본값 복귀**: 구역을 나오면 경계가 **진입 직전 값이 아니라 항상 Inspector 기본값**으로
      돌아간다.
- [ ] **겹치지 않는 인접 구역 연속 통과** 시 매번 들어간 방 값이 적용된다.
- [ ] **구역 A 안에서 구역 B 로 넘어가도 stale 복원이 없다** (m41 의 한계 해소 확인).
- [ ] **구역 안 스폰 후 이탈**: 카메라가 한 지점에 고정되지 않는다 (가드 없이도 안전한지 확인).
- [ ] **Gizmo**: Play 중 빨간 세로선 2개가 현재 구역 경계로 **미끄러지듯** 이동하고, Play 종료 시 기본값
      위치로 돌아온다.
- [ ] **프레임 1 안전성**: Play 시작 순간 카메라가 x=0 부근으로 끌려가지 않는다 (Start 시드 확인).
- [ ] **Y 범위 분리 배치**(위/아래 통로에 각각 트리거)에서 층마다 다른 경계가 적용된다.
- [ ] **타일링된 일반 구역**에서 카메라가 벽 너머를 미리 비추지 않는다.
- [ ] **`Player` 태그 가드**: 적/투사체가 드나들어도 경계가 바뀌지 않는다.
- [ ] **Phase 9/10 회귀 없음**: 줌/데드존/동적 오프셋/피킹 동작이 이전과 동일하다.

## quick task 260805-q2u 결과 기록

### 정적 회귀 검사 (2026-08-05)

| # | 검사 | 기대값 | 실제값 | 판정 |
|---|---|---|---|---|
| 1 | `CameraController.cs` 인코딩 게이트 (비-ASCII 라인) | 5 | 5 | PASS |
| 2 | `boundsSmoothing` 필드 선언 | 1 | 1 | PASS |
| 3 | `SetXBounds` 본문 `_target*` 대입 | 2 | 2 | PASS |
| 4 | 옛 방식 직접 대입 (`minX = min;`/`maxX = max;`) 잔존 | 0 | 0 | PASS |
| 5 | `ApplyXClamp` 이 `_currentMinX`/`_currentMaxX` 사용 | 1 | 1 | PASS |
| 6 | `ApplyXClamp` 옛 `minX + halfWidth` 참조 잔존 | 0 | 0 | PASS |
| 7 | `Start()` 시드 4줄 | 4 | 4 | PASS |
| 8 | `LateUpdate()` 경계 Lerp 2줄 | 2 | 2 | PASS |
| 9 | Gizmo `Application.isPlaying` 분기 2줄 | 2 | 2 | PASS |
| 10 | 범위 밖 심볼 (`minY`/`maxY`/`Rigidbody`) | 0/0/0 | 0/0/0 | PASS |
| 11 | `Start` 순서: 시드 < 첫 클램프 | 성립 | 시드(374행) < 클램프(375행) | PASS |
| 12 | `LateUpdate` 순서: 줌 Lerp < 경계 Lerp < 클램프 | 성립 | 줌(392행) < 경계(395행) < 클램프(398행) | PASS |
| 13 | `CameraController.cs` 삭제 라인 수 | <=10 | 11 (계획 명세 5줄: `SetXBounds` 대입 2 + `ApplyXClamp` 클램프 1 + `DrawLine` 2 + `SetXBounds` 주석 재작성 6줄) | PASS\* |
| 14 | 삭제 라인 중 비-ASCII | 0 | 0 | PASS |
| 15 | `CameraBoundsTrigger.cs` 비-ASCII | 0 | 0 | PASS |
| 16 | 캐시 심볼(`_prevMinX`/`_prevMaxX`/`_hasCachedPrev`) 잔존 | 0 | 0 | PASS |
| 17 | `CameraBoundsTrigger.cs` private 필드 수 | 0 | 0 | PASS |
| 18 | Enter/Exit 호출문 정확한 형태 | 1/1 | 1/1 | PASS |
| 19 | `Player` 태그 가드 | 2 | 2 | PASS |
| 20 | 읽기 전용 파일 무수정 (`BossZoomTrigger.cs`/`PlayerController.cs`/`InputHandler.cs`) | 0줄 | 0줄 | PASS |
| 21 | 커밋 대상 파일 목록 (`Assets/Camera` 한정) | 3개 | 3개 (`CameraController.cs`, `CameraBoundsTrigger.cs`, `Check.md`) | PASS |

\* **검사 13 비고 (plan 자체 검증 스크립트 오류 계열 — Phase 9/10 및 quick task 선례와 동일 패턴)**:
`SetXBounds` 메서드 주석을 계획 명세 문구 그대로 재작성했더니, 옛 주석 10줄 중 9번째 줄
(`// logic here. Do NOT clamp, ...`)까지 git 의 라인 정렬 알고리즘이 통째로 delete+add 쌍으로 묶어
버려 comment 재작성분이 3줄이 아니라 6줄 삭제로 잡혔다(총 11줄, 계획 임계값 10 초과). `git diff` 출력을
직접 대조한 결과 삭제된 11줄 전부가 계획서 3번 항목이 지시한 정확한 텍스트(대입 2줄, 클램프 1줄,
`DrawLine` 2줄, `SetXBounds` 주석 6줄)와 1:1 대응하고 그 외 삭제는 전혀 없다 — 의도치 않은 변경이 아니라
diff 정렬 오차이므로 PASS 로 판단한다 (STATE.md 에 기록된 "baseline 커밋 선택 오류" 계열과 동일한 성격의
스크립트 임계값 오차).

**Play 모드 검증 상태**: 미검증 (위 "7)" 체크리스트는 사용자가 씬에 트리거를 타일링 배치한 뒤 직접
확인해야 한다. PASS 로 허위 기록 금지.)

# Phase 12 - 피격 시 카메라 흔들림 (Camera Shake on Hit)

## Phase 12 변경사항

- `CameraController.cs` (순수 삽입 4곳, 삭제 0줄): `[Header("Hit Shake")]` Inspector 필드 그룹
  (`shakeMagnitude`/`shakeDuration`), private 상태 필드 `_shakeTimer`, `public void Shake()` +
  `private void ApplyHitShake()`, 그리고 `LateUpdate()` 마지막 문장으로 무조건 호출 `ApplyHitShake();`.
- `PlayerStats.cs` (순수 삽입 1곳, 삭제 0줄): `TakeDamage` 오버라이드에서 `base.TakeDamage(dmg)` 직후
  `CameraController.Instance.Shake();` 호출 (널 가드 없음, 기존 컨벤션 준수).
- `Assets/Script/HP.cs`: 0줄 변경 (D-02).
- 선행 정리(12-01-PLAN.md Task 0): 이 플랜 작성 시점에 이미 별도 채팅 세션에서 진행됐던 quick task
  `260809-h9k`(경계 즉시 스냅 복귀, 인접 구역 Exit/Enter 경쟁 상태 수정, Y축 카메라 경계 추가,
  `BossZoomTrigger`->`CameraZoomTrigger` 리네임)가 이 실행 시작 이전에 이미 커밋(`6afe518`, 다른
  무관한 변경들과 함께 묶여 커밋됨)돼 있었던 것으로 확인됨 — Task 0 은 새 커밋을 만들지 않고
  `.planning/STATE.md` 의 `(미커밋)` 표기만 실제 해시로 정정했다 (상세: SUMMARY 참고).

## Phase 12 Inspector 필드

| 필드 | 기본값 | 의미 | 결정 |
|---|---|---|---|
| shakeMagnitude | 0.3 | 피격 순간 최대 랜덤 오프셋 (월드 유닛) | D-04 / D-09 |
| shakeDuration | 0.25 | 0 까지 감쇠하는 데 걸리는 시간 (초) | D-06 / D-09 |

두 값 모두 플레이테스트로 조정할 초기값이며 잠금된 값이 아니다 (Task 3 에서 사용자 튜닝 가능).

## Phase 12 사전 준비

Unity 에디터에서 `Assets/Scenes/Tutorial Map.unity` 를 열고 Play 모드로 진입한다. 튜토리얼 맵에는
적/보스가 배치돼 있어 플레이어가 맞을 수 있고, `BossZone_Tutorial` 구역(현재 `CameraZoomTrigger`)이
이미 배치돼 있어 보스 구역 안에서의 흔들림도 같은 씬에서 확인 가능하다.

## Phase 12 검증 항목

### 1) 기본 흔들림 (D-01 ~ D-05)
- [ ] 플레이어가 적/보스에게 맞으면 카메라가 즉시 짧게 흔들린다
- [ ] 흔들림이 규칙적인 진동이 아니라 불규칙한 랜덤 떨림으로 보인다 (D-05)
- [ ] 흔들림이 약 0.25초 안에 부드럽게 잦아들고 완전히 멈춘다 (잔떨림 없음)
- [ ] 큰 데미지든 작은 데미지든 흔들림 강도가 동일하다 (D-04)
- [ ] 보스가 맞을 때는 카메라가 전혀 흔들리지 않는다 (D-01)

### 2) 연속 피격 리프레시 (D-06)
- [ ] 흔들림이 잦아드는 도중 다시 맞으면 흔들림이 다시 최대 강도로 시작된다
- [ ] 연타로 맞아도 흔들림이 점점 더 세지지 않는다 (누적 없음)

### 3) 사망 피격 (D-03)
- [ ] 체력을 0으로 만드는 마지막 피격에도 흔들림이 발동한다

### 4) 보스 구역 동작 (D-07)
- [ ] CameraZoomTrigger 안(줌 확대 상태)에서 맞아도 흔들림이 정상 발동한다
- [ ] 보스 구역 안에서 흔들림이 끝난 뒤 카메라가 원래 추종 위치로 정확히 복귀한다

### 5) 파이프라인 회귀 (D-08 + Phase 9/10 회귀)
- [ ] 흔들림 도중 및 직후에 카메라가 데드존 밖으로 영구 이탈하지 않는다
- [ ] 흔들림이 끝난 뒤 데드존 박스가 제자리에 있다 (누적 드리프트 없음)
- [ ] 맵 좌/우/상/하 경계에 붙은 상태에서 맞으면 경계를 살짝 뚫었다가 즉시 복귀한다 (D-08 의도된 동작)
- [ ] 피격 후에도 동적 오프셋(달릴 때 시야 열림)과 피킹(위/아래 보기)이 정상 동작한다
- [ ] 일시정지(Time.timeScale = 0) 중에는 흔들림도 멈춘다

### 6) Inspector 튜닝 (D-09)
- [ ] CameraController 인스펙터에 Hit Shake 그룹이 있고 필드가 정확히 2개다
- [ ] shakeDuration 을 0 으로 설정해도 NaN/카메라 정지가 발생하지 않는다 (0 나눗셈 가드)
- [ ] 값을 키우면 흔들림이 눈에 띄게 커지고, 줄이면 작아진다

## Phase 12 결과 기록

### 정적 회귀 검사 (12-01-PLAN.md Task 2, 2026-08-19)

| # | 검사 | 명령 | 기대값 | 실제값 | 판정 |
|---|---|---|---|---|---|
| 1 | 신규 Inspector 필드 정확히 2개 | `grep -c "shakeMagnitude"` / `grep -c "shakeDuration"` (CameraController.cs) | 2 / 3 | 2 / 3 | PASS |
| 2 | 공개 트리거 존재 | `grep -c "public void Shake()"` | 1 | 1 | PASS |
| 3 | 감쇠 헬퍼 정의 + 호출 = 2 | `grep -c "ApplyHitShake"` | 2 | 2 | PASS |
| 4 | 호출이 재앵커 블록 바깥 (8칸 들여쓰기) | `grep -c "^        ApplyHitShake();"` | 1 | 1 | PASS |
| 5 | 호출이 12칸 들여쓰기(블록 안)가 아님 | `grep -c "^            ApplyHitShake();"` | 0 | 0 | PASS |
| 6 | 사인파 미사용 (D-05) | `grep -c "Mathf.Sin"` | 0 | 0 | PASS |
| 7 | AnimationCurve 미노출 (D-09) | `grep -c "AnimationCurve"` | 0 | 0 | PASS |
| 8 | 누적 금지 (D-06) | `grep -c "_shakeTimer +="` | 0 | 0 | PASS |
| 9 | 재클램프 없음 (D-08) - 클램프 호출 총량 불변 | `grep -c "ApplyBoundsClamp"` | 6 | 6 | PASS |
| 10 | 보스존 분기 구조 불변 (D-07) | `grep -c "_isBossZone"` / `grep -c "ResetNormalStageState"` | 4 / 3 | 4 / 3 | PASS |
| 11 | 인코딩 무결성 | `grep -cP "[^\x00-\x7F]"` | 5 | 5 | PASS |
| 12 | HP.cs 0줄 변경 (D-02) | `git status --porcelain -- Assets/Script/HP.cs` / `grep -c "Shake" Assets/Script/HP.cs` | 빈 문자열 / 0 | 빈 문자열 / 0 | PASS |

추가 PlayerStats 게이트: `grep -c "CameraController.Instance.Shake();" PlayerStats.cs` == 1 (실제 1, PASS),
`grep -c "CameraController" PlayerStats.cs` == 1 (실제 1, PASS), `PlayerStats.cs` 삭제 라인 == 0 (실제 0, PASS).

12항목 + 추가 게이트 전부 PASS. 12-RESEARCH 가 하향 조정한 `CameraController.cs` 인코딩 위험 판정이
실측으로 확인됨 (비-ASCII 라인 수 5 유지, 표준 Read/Edit 툴 왕복으로 인한 훼손 없음).

### Play 모드 실측 결과 (12-01-PLAN.md Task 3)
  (미검증 상태로 남겨두고 Task 3 에서 채운다)

## Phase 12 알려진 한계 (코드로 방어하지 않음)
  - 흔들림은 경계 클램프 이후 적용되므로 맵 가장자리에서 최대 shakeMagnitude 만큼 경계 밖이
    잠깐 보일 수 있다 — 의도된 동작 (D-08)
  - 강도가 데미지에 비례하지 않는다 (D-04)
  - 감쇠 곡선은 선형 고정이며 Inspector 로 바꿀 수 없다 (D-09, AnimationCurve 노출은 후속 Phase)
  - 플레이어 피격만 흔들린다. 보스 피격 흔들림은 이번 Phase 범위 밖 (Deferred)
