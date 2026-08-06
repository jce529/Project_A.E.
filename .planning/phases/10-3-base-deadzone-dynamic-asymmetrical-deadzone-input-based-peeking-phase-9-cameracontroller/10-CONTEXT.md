# Phase 10: 카메라 데드존 3종 기법 (Base Deadzone / Dynamic Asymmetrical Deadzone / Input-based Peeking) - Context

**Gathered:** 2026-08-04
**Status:** Ready for planning

<domain>
## Phase Boundary

기존 `CameraController.cs`(Phase 9까지: 위치추종 Lerp + 줌 Lerp + X축 클램프)에 3가지 카메라 기법을 레이어링한다:
1. Base Deadzone — 플레이어 주변 가상 박스 안에서는 카메라 완전 정지, 경계를 밀고 나갈 때만 이동
2. Dynamic Asymmetrical Deadzone — 이동 방향 반대편으로 데드존 박스 중심이 부드럽게 오프셋되어 진행 방향 시야 확보
3. Input-based Peeking — 정지+접지+수직입력 유지 시 카메라가 천천히 위/아래로 시야 이동, 이동/대시/피격 시 즉시 취소

일반 스테이지에서만 동작하며, 보스 구역(Phase 9 `SetBossZoom(true)`) 진입 중에는 이 3개 기법 전부 비활성화되고 기존 레거시 추종 로직으로 완전히 복귀한다.

</domain>

<decisions>
## Implementation Decisions

### 데드존 박스 정의
- **D-01:** 박스 크기는 월드 유닛 고정 Inspector float. 가로/세로 별도 필드(`deadzoneWidth`, `deadzoneHeight`) — 메트로배니아 특성상 좌우/상하 이동 폭이 다르므로 분리.
- **D-02:** 보스 줌(7) 전환 중에도 데드존 박스는 고정 월드 크기를 유지한다. 줌 비율에 따라 스케일하지 않는다 (다만 D-15에 따라 보스 구역에서는 애초에 데드존 자체가 꺼진다).
- **D-03:** `OnDrawGizmos`로 박스 윤곽을 표시한다 (에디터 전용, 런타임 성능 영향 없음). Play 모드 튜닝 시 Scene 뷰에서 바로 확인 가능해야 한다.

### 이동방향 감지 (동적 비대칭 오프셋)
- **D-04:** `PlayerController.cs`는 수정하지 않는다. 카메라가 `target.position`의 프레임 간 델타로 이동방향을 추정한다 (moveInput/rigid는 private이라 직접 접근 불가, Phase 9의 "카메라 스크립트만 수정" 원칙 유지).
- **D-05:** 오프셋 발동 기준은 별도 속도 임계값이 아니라 **데드존 경계를 실제로 밀고 있을 때만**이다 (D-01 박스와 연동).
- **D-06:** 정지 후에도 오프셋은 즉시 줄어들지 않고, 일정 시간 유지된 뒤 서서히 복귀한다 (유지시간 파라미터 별도 필요, 플레이테스트로 조정할 초기값).
- **D-07:** 오프셋 자체 전환은 `SmoothDamp`로 부드럽게 처리한다 (D-14의 데드존 하드컷과 대비되는 레이어).

### 피킹 입력 및 취소 조건
- **D-08:** 수직 입력은 카메라가 `InputHandler.Instance.OnMoveEvent`를 **직접 구독**해서 읽는다 (`PlayerController.cs` 미수정). `InputHandler`는 원래 "외부 스크립트가 구독하는 이벤트 버스"로 설계되어 있어 이 경로가 정석.
- **D-09:** 입력 잠금 가드 — `target.GetComponent<PlayerController>().movementLocked`(이미 public)를 확인해, 잠금 중(컷신/사망/구속 등)에는 `OnMoveEvent`가 흘려보내는 원시 입력을 무시한다. `OnMoveEvent`는 `movementLocked` 여부와 무관하게 항상 발화하므로 이 가드가 필수. `InputHandler`는 `DontDestroyOnLoad`, `CameraController`는 씬 로컬(파괴됨)이므로 `OnEnable`에서 구독, `OnDisable`에서 반드시 해제해 구독 누수를 막는다.
- **D-10:** 정지 판단(`Velocity==0`)은 `target.position` 프레임 델타가 거의 0인지로 근사한다 (D-04와 동일 방식 재사용, `PlayerController.cs` 미수정).
- **D-11:** 피킹 즉시 취소 조건(대시/피격 등)도 원인을 구분하지 않고 **이동량 급증**만으로 프록시 감지한다 (`isDashing`/`isKnockedBack`은 private, 새 접근자 추가하지 않음).
- **D-12:** `IsGrounded()` 조건을 원안 그대로 포함한다 (이미 public 메서드, 그대로 재사용).
- **D-13:** `Threshold`(0.5초)와 `PeekDistance`는 사용자 제시값을 초기값으로 Inspector에 노출한다 (Phase 9의 `normalZoom`/`bossZoom` 패턴과 동일하게 플레이테스트 튜닝 대상).

### 기존 파이프라인 통합
- **D-14:** 일반 스테이지에서는 데드존을 **하드컷**으로 즉시 계산한다 (Lerp 없이, 카메라가 데드존이 허용하는 경계 위치로 즉시 스냅). 그 위에 동적 비대칭 오프셋(SmoothDamp)과 피킹 오프셋(SmoothDamp)만 부드럽게 가산한다. 데드존 자체를 Lerp로 쫓아가면 빠른 이동 시 박스 경계와 카메라 사이에 간격이 벌어져 "박스 안에서는 완전 정지"라는 취지가 깨지기 때문.
- **D-15:** 보스 구역 진입 중(`SetBossZoom(true)` 활성 상태)에는 데드존/동적오프셋/피킹을 **전부 비활성화**하고, 기존 Phase 9의 `Vector3.Lerp(transform.position, target.position + offset, smoothing)` 레거시 로직으로 완전히 복귀한다.
- **D-16:** 줌 Lerp(`normalZoom`/`bossZoom`/`zoomSmoothing`)는 두 경로(일반/보스) 모두에서 기존 그대로 동작한다 — 변경 없음.
- **D-17:** X축 클램프(`minX`/`maxX`, `ApplyXClamp()`)는 데드존+오프셋+피킹이 모두 적용된 최종 위치(또는 보스 구역 레거시 위치)에 대해 **마지막으로** 적용된다. Phase 9의 "클램프는 그 프레임의 최신 상태를 반영" 원칙을 그대로 유지.

### Claude's Discretion
- 새 필드/메서드의 정확한 이름, `LateUpdate` 내부를 헬퍼 메서드로 어떻게 분리할지
- `SmoothDamp` velocity 임시 변수 관리 방식
- Gizmo 색상/스타일
- "이동량 급증" 프록시의 정확한 임계값, 오프셋 유지시간 파라미터의 기본 수치 (전부 Inspector 노출 필수, 플레이테스트로 조정될 초기값)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기존 카메라 코드
- `Assets/Camera/Script/CameraController.cs` — Phase 9까지 구현된 싱글톤/줌/X클램프 전체 (이번 Phase의 수정/확장 대상)
- `Assets/Camera/Script/BossZoomTrigger.cs` — `SetBossZoom(bool)` 호출부, D-15의 "보스 구역 판정" 연동 참고
- `.planning/phases/09-camera-zoom-stage-transition/09-CONTEXT.md` — Phase 9 선행 결정(D-01~D-11), LateUpdate 순서 확정 근거

### 플레이어 코드 (읽기 전용 참조 — 수정하지 않음)
- `Assets/Player/Script/InputHandler.cs` — `OnMoveEvent` 이벤트 정의부 (D-08 구독 대상), `DontDestroyOnLoad` 싱글톤 구조
- `Assets/Player/Script/PlayerController.cs` — `movementLocked`(46행, public), `IsGrounded()`(234행, public), `OnMove()`(106-108행, `movementLocked` 체크 없이 무조건 `moveInput` 갱신 — D-09 가드가 필요한 근거)

No external specs/ADRs — requirements fully captured in decisions above.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `CameraController.Instance` 싱글톤, `SetBossZoom(bool)`, `normalZoom`/`bossZoom`/`zoomSmoothing` 필드, `ApplyXClamp()` — 그대로 재사용
- `InputHandler.Instance.OnMoveEvent` (`Action<Vector2>`) — 구독만 추가하면 즉시 사용 가능
- `PlayerController.movementLocked` (public bool), `PlayerController.IsGrounded()` (public method) — 이미 공개되어 있어 새 접근자 불필요

### Established Patterns
- `LateUpdate` 실행 순서: 위치추종 → 줌 Lerp → X클램프 (Phase 9 고정) — 이번 Phase는 "위치추종" 단계 내부를 일반/보스 두 경로로 분기하는 것이 핵심 변경 지점
- 씬 로컬 싱글톤(no `DontDestroyOnLoad`) — `CameraController`는 씬마다 파괴/재생성되므로, `DontDestroyOnLoad`인 `InputHandler` 이벤트 구독은 `OnEnable`/`OnDisable`에서 대칭적으로 처리해야 함 (`PlayerController.cs` 73/82행이 참고 사례)
- CP949 인코딩 삽입 전용 편집 (Phase 9와 동일 제약 예상 — 실행 단계에서 재확인 필요)

### Integration Points
- `CameraController.cs`의 `LateUpdate` 내 기존 위치추종 Lerp 라인이 이번 Phase의 주 분기점 — 보스 구역 여부에 따라 레거시 Lerp 경로 vs 데드존+오프셋+피킹 경로로 갈라짐
- `PlayerController.cs`, `InputHandler.cs`는 읽기 전용 참조만 하고 파일 자체는 수정하지 않음 (D-04, D-08, D-09, D-10, D-11)

</code_context>

<specifics>
## Specific Ideas

사용자가 제시한 원본 수식/로직 (구현 시 그대로 반영):

**1. Base Deadzone**
- `X_player < X_min` → 카메라가 좌측 경계를 밀어낸 거리만큼 좌측 이동
- `X_player > X_max` → 카메라가 우측 경계를 밀어낸 거리만큼 우측 이동
- 그 외(박스 내부 안착) → 카메라 타겟 좌표 연산 무시(정지 유지)
- **D-14 결정에 따라 이 계산 자체는 Lerp 없이 하드컷으로 수행**

**2. Dynamic Asymmetrical Deadzone**
- `TargetOffset = -(MoveDirection × MaxOffsetDistance)`
- `CurrentOffset = Mathf.SmoothDamp(CurrentOffset, TargetOffset, ref Velocity, SmoothTime)`
- 우측으로 달리면 오프셋이 좌측(-)으로 이동하여 우측 시야가 열림

**3. Input-based Peeking**
- 조건식: `Velocity == 0 AND IsGrounded == true AND Input.Y != 0`
- 타이머 `t` 증가, `t > Threshold`(예 0.5초) 이상일 때 활성화
- `TargetPeeking = Input.Y × PeekDistance`
- 이탈 시(조건 불만족) 즉시 `t = 0`, `TargetPeeking = 0`으로 초기화 후 빠른 `SmoothDamp`로 복귀

**최종 통합 공식:**
`FinalCameraPosition = BaseDeadzonePosition + CurrentOffset(동적 데드존) + PeekingOffset(수동 시야)`

(단, 보스 구역에서는 이 공식 전체가 비활성화되고 D-15의 레거시 Lerp로 대체됨)

</specifics>

<deferred>
## Deferred Ideas

- Y축 데드존/오프셋/피킹 확장 여부 — 이번 논의에서 다루지 않음. 사용자 수식은 X축(및 수직 피킹만) 중심으로 제시되었으므로, Y축 데드존이 필요한지는 이번 Phase 범위 밖. 실행 중 애매하면 재논의 필요.
- Phase 9에서 이미 이월된 Y축 카메라 이동 범위 제한(minY/maxY) — 여전히 범위 밖.

</deferred>

---

*Phase: 10-camera-deadzone-dynamic-offset-peeking*
*Context gathered: 2026-08-04*
