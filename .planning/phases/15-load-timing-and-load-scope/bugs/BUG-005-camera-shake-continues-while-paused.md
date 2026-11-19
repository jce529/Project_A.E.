# BUG-005: 일시정지 중 카메라 흔들림 지속

- 심각도: 중간
- 상태: 해결됨 (2026-09-10 수정 + Play 모드 재검증 완료)
- 발견일: 2026-09-10
- 영향 범위: Phase 12 피격 카메라 흔들림, 일시정지 화면

## 현상

피격 흔들림이 활성화된 상태에서 `Time.timeScale = 0`으로 일시정지해도 카메라 위치가 매 프레임
계속 바뀐다. 게임 시간은 멈췄지만 화면은 랜덤하게 떨리므로 Phase 12 체크리스트의 “일시정지 중에는
흔들림도 멈춘다” 기대와 다르다.

## 근거

- `Assets/Camera/Script/CameraController.cs:224` — `ApplyHitShake()` 진입점.
- `Assets/Camera/Script/CameraController.cs:227` — 타이머 감소에는 `Time.deltaTime`을 사용하므로
  일시정지 중 `_shakeTimer`가 줄지 않는다.
- `Assets/Camera/Script/CameraController.cs:231` — 타이머 감소 여부와 무관하게 매 프레임 새
  `Random.insideUnitCircle` 오프셋을 적용한다.
- `Assets/Camera/Check.md:535` — 일시정지 중 흔들림 정지가 명시된 검증 항목.

## 재현 절차

1. `Tutorial Map`을 Play 모드로 연다.
2. `CameraController.Shake()`를 호출해 흔들림을 시작한다.
3. `Time.timeScale = 0`으로 설정한다.
4. 여러 프레임을 진행하며 `Main Camera` Transform 위치를 기록한다.

## 기대 결과

- `Time.timeScale == 0`인 동안 카메라 Transform 위치가 흔들림 때문에 바뀌지 않는다.
- 재개 후 남아 있던 흔들림 시간이 정상적으로 이어지거나, 합의된 정책대로 종료된다.

## 실제 결과

Unity 6000.3.10f1에서 `Time.deltaTime=0`인 5개 연속 프레임의 카메라 XY가 모두 바뀌었다.

| 프레임 | X | Y |
|---:|---:|---:|
| 기준 | 269.168400 | -39.621850 |
| 1 | 269.354200 | -39.388280 |
| 2 | 268.912200 | -39.643610 |
| 3 | 269.217400 | -39.355800 |
| 4 | 269.106200 | -39.879640 |
| 5 | 269.177700 | -39.597630 |

## 원인

`ApplyHitShake()`는 `_shakeTimer <= 0`만 검사한다. 일시정지 중에는 `_shakeTimer -= Time.deltaTime`가
0만큼 감소하지만, 이후 랜덤 오프셋 계산과 Transform 적용은 그대로 실행된다. 결과적으로 흔들림이
끝나지 않은 채 무기한 새 오프셋이 적용된다.

## 수정 방향

- `Time.deltaTime <= 0f`일 때 오프셋 적용 전에 반환해 일시정지 프레임에서 Transform을 건드리지 않는다.
- 재개 후 기존 타이머를 이어갈지 즉시 종료할지는 현재 체크리스트 문구상 “멈췄다가 이어짐”이 가장
  작은 변경이다.
- 수정은 `ApplyHitShake()` 내부에 한정하고 데드존·경계 클램프·보스 구역 분기는 변경하지 않는다.

## 완료 조건

- [x] `Time.timeScale=0`인 연속 프레임에서 카메라 위치가 고정된다.
- [x] 재개 후 흔들림이 정상 감쇠하고 기준 위치로 복귀한다.
- [x] 일반 피격 흔들림, 연속 피격 리프레시, `shakeDuration=0` 회귀가 없다.
- [x] Unity Console 컴파일/실행 오류가 0건이다.
- [x] 검증 일자와 수정 커밋을 아래에 기록한다.

## 해결 기록

- 검증 일자: 2026-09-10
- 수정 커밋: `e81edbf`
- 비고: Unity MCP 실측으로 재현 후 수정 및 재검증 완료. 보스 관련 검증은 수행하지 않음. 상세는 아래 참고.

### 수정 내용 (2026-09-10)

`Assets/Camera/Script/CameraController.cs` `ApplyHitShake()` 에 일시정지 프레임 조기 반환 2줄 추가.
`_shakeTimer` 는 건드리지 않으므로 남은 흔들림 시간은 그대로 보존되고 재개 시 이어진다.
데드존·경계 클램프·보스 구역 분기는 변경하지 않았다.

```csharp
if (_shakeTimer <= 0f) return;
// A paused frame (Time.timeScale = 0) advances no game time, so the countdown cannot
// progress and the camera must not be moved either - the shake resumes with the game.
if (Time.deltaTime <= 0f) return;
_shakeTimer -= Time.deltaTime;
```

### 재검증 결과 (2026-09-10, Unity 6000.3.10f1, Tutorial Map)

| # | 항목 | 조건 | 결과 | 판정 |
|---|---|---|---|---|
| 1 | 일시정지 중 정지 | `Shake()` 후 `timeScale=0`, 1604 프레임 경과 | 위치 변동 0.000000 | PASS |
| 2 | 일시정지 중 정지 (장시간) | `shakeDuration=3`, 499 프레임 경과 | 위치 변동 0.000000 | PASS |
| 3 | 흔들림 동작 | `shakeDuration=60`, `timeScale=1` | offset 0.179308 | PASS |
| 4 | 흔들림 도중 일시정지 | 위 3에서 `timeScale=0` | 기준 위치 스냅 후 고정 | PASS |
| 5 | 재개 후 이어짐 | 위 4에서 `timeScale=1` | offset 0.199466 재개 | PASS |
| 6 | `shakeDuration=0` 가드 | `Shake()` 호출 | offset 0.000000, NaN 없음 | PASS |
| 7 | Console | 전 과정 | Error/Exception/Assert 0건 | PASS |

수정 전 기록(위 "실제 결과" 표)에서는 동일 조건 5프레임이 모두 변동했다. 수정 후에는 1604프레임에서
소수점 6자리까지 완전히 동일하다.

부수 관찰: 일시정지 시 카메라는 흔들림 오프셋을 남긴 채 얼지 않고 기준 추종 위치로 스냅되어 고정된다.
추종 로직이 매 프레임 위치를 다시 쓰고 흔들림만 건너뛰기 때문이다.
