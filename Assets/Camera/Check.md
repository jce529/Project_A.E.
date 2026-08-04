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
