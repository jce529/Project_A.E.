# Phase 9: 일반 스테이지와 보스 스테이지 진입 시 카메라 크기(줌) 변화 - Context

**Gathered:** 2026-07-30
**Status:** Ready for planning

<domain>
## Phase Boundary

기존 `CameraController.cs`(위치 추적 전용)에 두 가지 카메라 기능을 추가한다:
1. 보스 스테이지 트리거 진입/이탈에 따른 카메라 줌(orthographic size) 전환
2. 카메라 X축 이동 범위 제한(맵 경계 밖으로 못 나가게 클램프)

씬 로드(Portal.cs/SceneManager) 기반 전환이 아니라, 씬 내부에 배치하는 트리거 콜라이더 기반이다.

</domain>

<decisions>
## Implementation Decisions

### 보스 스테이지 진입 감지 및 줌 전환
- **D-01:** 보스 스테이지 진입은 씬 로드가 아니라 **트리거 콜라이더**(`BoxCollider2D`, Is Trigger) 기반으로 감지한다.
- **D-02:** 트리거 콜라이더는 보스마다 **사용자가 에디터에서 수동으로 배치**한다. 보스 GameObject에 자동 부착하지 않는다.
- **D-03:** `OnTriggerExit2D`로 트리거 영역을 벗어나면 **즉시 자동으로 일반 스테이지 줌 값으로 복귀**한다. 보스 처치 등 별도 이벤트는 필요 없다.
- **D-04:** 목표 줌 값(orthographic size)은 **일반 = 5, 보스 = 7**. 사용자가 "일단 세팅하고 보자"고 명시함 — 확정값이 아니라 플레이테스트로 조정될 초기값.
- **D-05:** 줌 값은 **Inspector 필드로 노출**한다 (하드코딩 금지). Play 모드에서 쉽게 튜닝 가능해야 한다.
- **D-06:** 줌 전환은 **부드럽게(Lerp)** 처리한다. 즉시 전환 아님.
- **D-07:** 줌 전환 속도는 기존 위치추적용 `smoothing`(5f) 필드와 **별도의 속도 필드로 분리**한다 (예: `zoomSmoothing`). 용도가 다르므로 값을 공유하지 않는다.
- **D-08:** 이번 Phase 범위는 **트리거 스크립트 + 카메라 줌 로직 구현까지만**. 실제 보스 씬(WaterMonster/WaterSpirit/TutorialBoss 등)에 트리거 콜라이더를 배치하는 에디터 작업은 범위 밖 — 사용자가 추후 직접 배치한다.

### 카메라 X축 이동 범위 제한
- **D-09:** 카메라 **X축 이동 범위를 min/max 값으로 제한**한다 (벽처럼 넘어갈 수 없음). **Y축은 이번 Phase에서 제한하지 않는다** (사용자 명시적 확인).
- **D-10:** 경계값은 `BoxCollider2D` 참조 방식(Phase 4 `mapBounds` 패턴)이 아니라 **`minX`/`maxX` float Inspector 필드로 직접 지정**한다.
- **D-11:** 클램프 계산 시 카메라 중심점만이 아니라 **화면 반폭(`orthographicSize * camera.aspect`)까지 감안**한다 — 줌 값이 달라져도(5 ↔ 7 전환) 맵 경계 밖이 화면에 보이지 않아야 한다.

### Claude's Discretion
- 새 트리거/줌 컴포넌트의 클래스/파일 이름
- 트리거 스크립트가 `CameraController`를 참조하는 방식 (싱글톤 vs `FindObjectOfType` vs Inspector 직접 참조 등)
- 줌 전환·클램프 로직을 `LateUpdate` 내 어느 순서로 적용할지 (위치 추적 → 줌 Lerp → X축 클램프 순서 등 구체 구현)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기존 카메라 코드
- `Assets/Camera/Script/CameraController.cs` — 현재 위치 추적 로직 전체 (수정/확장 대상)

No external specs/ADRs — requirements fully captured in decisions above.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Camera/Script/CameraController.cs` — `Transform target` 을 `Vector3.Lerp`(`smoothing=5f`)로 따라가는 `LateUpdate` 로직만 존재. `orthographicSize`/줌 관련 필드나 `Camera` 컴포넌트 참조는 전혀 없음 — 신규 추가 필요.

### Established Patterns
- `Vector3.Lerp(..., smoothing * Time.deltaTime)` 부드러운 추종 패턴을 `LateUpdate`에서 사용 — 줌 Lerp도 동일 패턴을 따르는 것이 자연스러움.
- Phase 4 `WeatherController.mapBounds` — `BoxCollider2D` 참조 기반 bounds 패턴이 존재하지만, 이번 X축 경계는 **의도적으로 이 패턴을 쓰지 않고** float Inspector 필드로 직접 지정하기로 결정함(D-10).

### Integration Points
- 씬은 스테이지 단위로 이미 분리되어 있음(`1 stage.unity`, `Tutorial Map.unity` 등), 전환은 `Assets/map/script/portal.cs` + `GameManager.cs`(`NextSpawnPointName` + `SceneManager.LoadScene`)가 담당하지만, **이번 기능은 이 씬 전환 경로와 무관**하다 — 보스 트리거는 씬 내부의 새 GameObject/콜라이더로 별도 동작.
- Cinemachine 미사용 — 순수 `UnityEngine.Camera` + 수동 스크립트 구조를 그대로 따른다.

</code_context>

<specifics>
## Specific Ideas

- "일반 5, 보스 7로 일단 세팅하고 보자" — 초기 튜닝값, Inspector 노출 필요성의 근거.
- "카메라가 나갈수 없는 구역을 정하고싶어. 일단 x축을 기준으로 벽처럼 막혀서 안넘어가도록" — X축 전용 하드 클램프 요구, Y축은 명시적으로 범위 밖.

</specifics>

<deferred>
## Deferred Ideas

- 실제 보스 씬(WaterMonster/WaterSpirit/TutorialBoss)에 `BossZoomTrigger` 콜라이더를 배치하는 에디터 작업 — 사용자가 기능 완성 후 직접 진행 (D-08).
- Y축 이동 범위 제한 — 이번 Phase 범위 아님, 필요 시 별도 논의.

</deferred>

---

*Phase: 09-camera-zoom-stage-transition*
*Context gathered: 2026-07-30*
