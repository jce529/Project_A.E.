# Phase 12: 피격 시 카메라 흔들림 (Camera Shake on Hit) - Context

**Gathered:** 2026-08-11
**Status:** Ready for planning

<domain>
## Phase Boundary

기존 `CameraController.cs`(Phase 9~10까지: 위치추종/데드존 파이프라인 + 줌 Lerp + XY 경계 클램프)에
플레이어 피격 시 카메라를 짧게 흔드는 이펙트를 레이어링한다. 흔들림은 최종 카메라 위치에 더해지는
독립적인 시각 피드백이며, 기존 추종/데드존/줌/클램프 로직 자체는 건드리지 않는다.

</domain>

<decisions>
## Implementation Decisions

### 트리거 조건
- **D-01:** 흔들림은 **플레이어가 피격당할 때만** 발동한다. 보스 등 다른 엔티티가 맞을 때는 흔들리지 않는다 (범위 밖 — Deferred 참고).
- **D-02:** 연결 지점은 `PlayerStats.TakeDamage(float dmg)` 오버라이드 (`Assets/Player/Script/PlayerStats.cs:54`)다. `base.TakeDamage(dmg)` 호출 뒤 `CameraController.Instance.Shake()`를 호출한다. `HP.cs`(보스도 상속하는 베이스 클래스)는 **무수정**이며 공용 `OnHit` 이벤트도 신설하지 않는다.
- **D-03:** 사망(`Die()`)으로 이어지는 마지막 피격에도 흔들림이 함께 발동한다 — `TakeDamage` 내부에서 호출하므로 별도 분기 없이 자연스럽게 포함된다.

### 흔들림 강도/결
- **D-04:** 강도는 **고정값**이다. 데미지량에 비례하지 않는다 — `PlayerStats`에 데미지 상한선이 없어 비례 강도는 극단값 처리가 추가로 필요하기 때문.
- **D-05:** 움직임 방식은 **랜덤 오프셋**(예: `Random.insideUnitCircle * 현재강도`)이며, 시간에 따라 감쇠(decay)한다. 규칙적인 진동(Sine wave)은 사용하지 않는다.
- **D-06:** 연속 피격으로 이전 흔들림이 아직 감쇠 중일 때 새 타격이 들어오면, **지속시간 타이머를 최대치로 리프레시**한다 (가산/누적 방식 아님) — 연속 피격 시 강도가 계속 세지는 것을 방지.

### 카메라 파이프라인과의 합성
- **D-07:** 보스 구역(`_isBossZone` true, 줌 확대 상태)에서도 흔들림은 **항상 적용**된다. Phase 9의 D-15("보스 구역에서는 데드존/오프셋/피킹 전부 비활성화")는 흔들림에는 적용되지 않는다 — 흔들림은 데드존 파이프라인의 일부가 아니라 그 위에 얹히는 독립 레이어이기 때문.
- **D-08:** 흔들림 오프셋은 `ApplyBoundsClamp()`(X/Y 경계 클램프) **이후에 최종 적용**되며, 클램프를 약간 뚫고 나가는 것을 허용한다. 흔들림 적용 후 다시 클램프하지 않는다.

### 튜닝 파라미터 노출 범위
- **D-09:** Inspector 노출 필드는 **`shakeMagnitude`(강도, 월드 유닛)와 `shakeDuration`(지속시간, 초) 2개만**이다. 감쇠 곡선을 `AnimationCurve`로 노출하는 것은 이번 Phase 범위 밖 — 기존 `zoomSmoothing`/`deadzoneWidth`류의 단순 float 필드 패턴을 그대로 따른다.

### Claude's Discretion
- `Shake()` 메서드 시그니처 — 강도가 고정값(D-04)이므로 매개변수 없는 오버로드로 충분 (`public void Shake()`).
- 감쇠 곡선의 정확한 수식 (선형 vs 지수) — Inspector에 노출하지 않기로 했으므로(D-09) 코드 내부에서 결정.
- 흔들림 오프셋 계산에 `Random.insideUnitCircle` vs Perlin noise 중 선택.
- **LateUpdate 내 정확한 삽입 위치 — 반드시 지켜야 할 기술 제약**: 현재 `LateUpdate()`는 위치추종/데드존 → 줌 Lerp → `ApplyBoundsClamp()` → `_deadzoneCenterX`/`_deadzoneCenterY` 재앵커(마지막 블록, `transform.position`을 읽어 되먹임) 순서다. 흔들림 오프셋은 이 **재앵커 블록 이후**에 더해져야 한다 — 재앵커보다 먼저 섞이면 흔들림 값 자체가 매 프레임 데드존 중심(`_deadzoneCenterX/Y`)에 누적되어 카메라 추적 로직이 오염되는 회귀가 발생한다 (D-08의 "클램프 이후 최종 적용"과 같은 이유).
- 신규 필드/내부 상태 변수의 정확한 이름.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 카메라 코드
- `Assets/Camera/Script/CameraController.cs` — 수정/확장 대상. `LateUpdate()`(405~428행)가 흔들림 삽입 지점, CP949 인코딩 파일이므로 삽입 전용/byte-safe 편집 필요 (Phase 9~11에서 반복 확인된 제약).
- `.planning/phases/09-camera-zoom-stage-transition/09-CONTEXT.md` — 카메라 씬 로컬 싱글톤(`CameraController.Instance`, `DontDestroyOnLoad` 미사용) 원칙, LateUpdate 순서의 최초 확정 근거.
- `.planning/phases/10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller/10-CONTEXT.md` — 데드존/오프셋/피킹 레이어링 방식, 보스존 분기(D-15) 및 재앵커 로직(D-17)의 상세 근거.

### 연결 지점
- `Assets/Player/Script/PlayerStats.cs` — `TakeDamage(float dmg)` 오버라이드(54행)가 `Shake()` 호출 지점.
- `Assets/Script/HP.cs` — 베이스 클래스, 참고용 읽기 전용 (무수정, D-02).

No external specs/ADRs — requirements fully captured in decisions above.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `CameraController.Instance` 싱글톤 — `Shake()`를 public 메서드로 추가해 `PlayerStats.TakeDamage`에서 직접 호출 가능.
- 기존 `[Header("...")]` + Inspector float 필드 그룹 패턴(`zoomSmoothing`, `deadzoneWidth` 등) — `shakeMagnitude`/`shakeDuration`도 동일한 스타일의 새 `[Header("Hit Shake")]` 그룹으로 추가.

### Established Patterns
- `LateUpdate` 실행 순서: 위치추종/데드존 → 줌 Lerp → `ApplyBoundsClamp()` → 데드존 재앵커. 흔들림은 이 전체 파이프라인의 "마지막 레이어"로 추가되는 첫 사례.
- CP949 인코딩 삽입 전용 편집 — `CameraController.cs`, `PlayerStats.cs`(UTF-8, 문제 없음) 중 `CameraController.cs`만 byte-safe 삽입 필요.
- 씬 로컬 싱글톤(no `DontDestroyOnLoad`) — 흔들림 관련 내부 상태(타이머 등)도 씬 전환 시 자연히 리셋됨, 별도 정리 로직 불필요.

### Integration Points
- `PlayerStats.TakeDamage` (Assets/Player/Script/PlayerStats.cs:54) → `CameraController.Instance.Shake()` 단일 호출 추가.
- `CameraController.LateUpdate()` 끝부분(재앵커 이후) → 흔들림 오프셋을 `transform.position`에 최종 가산.

</code_context>

<specifics>
## Specific Ideas

특별히 제시된 구체적 레퍼런스나 예시는 없음 — 표준적인 접근으로 진행.

</specifics>

<deferred>
## Deferred Ideas

- 보스(또는 다른 엔티티) 피격 시에도 카메라 흔들림 — 이번 Phase는 플레이어 피격만 다룬다(D-01). 필요해지면 `HP.cs`에 공용 `OnHit` 이벤트를 추가하는 방식으로 확장 가능하나, 이번 Phase에서는 구현하지 않는다.
- 감쇠 곡선을 `AnimationCurve`로 Inspector 노출 — 이번 Phase 범위 밖(D-09), 필요시 후속 Phase에서 논의.

</deferred>

---

*Phase: 12-camera-shake-on-hit*
*Context gathered: 2026-08-11*
