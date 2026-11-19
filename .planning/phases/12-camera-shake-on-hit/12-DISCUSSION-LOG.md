# Phase 12: 피격 시 카메라 흔들림 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-11
**Phase:** 12-camera-shake-on-hit
**Areas discussed:** 트리거 조건, 흔들림 강도/결, 카메라 파이프라인과의 합성, 튜닝 파라미터 노출 범위

---

## 트리거 조건 (언제 흔들리나)

| Option | Description | Selected |
|--------|-------------|----------|
| 플레이어 피격 시만 | 플레이어가 데미지를 받을 때만 흔들림. `PlayerStats.TakeDamage` 오버라이드에서 트리거 | ✓ |
| 플레이어 + 보스 피격 시 모두 | 보스도 맞을 때 흔들림 — `HP.cs`에 공용 `OnHit` 이벤트 필요 | |
| 플레이어 사망 시에만 | 일반 피격이 아니라 사망 순간에만 강한 흔들림 | |

**User's choice:** 플레이어 피격 시만 (추천 옵션)
**Notes:** 카메라가 플레이어를 따라다니므로 가장 직접적인 피드백이라는 이유로 선택.

---

| Option | Description | Selected |
|--------|-------------|----------|
| PlayerStats.TakeDamage 오버라이드 | `base.TakeDamage(dmg)` 호출 후 `CameraController.Instance.Shake()` 호출만 추가. HP.cs 무수정 | ✓ |
| HP.cs에 공용 OnHit 이벤트 신설 | OnDeath와 비슷한 OnHit 이벤트 추가, CameraController가 구독. 보스도 이벤트를 갖게 되지만 이번 Phase는 미사용 | |

**User's choice:** PlayerStats.TakeDamage 오버라이드 (추천 옵션)

---

| Option | Description | Selected |
|--------|-------------|----------|
| 발동함 | 사망으로 이어지는 마지막 피격에도 흔들림이 자연스럽게 함께 발동 | ✓ |
| 사망 시에는 생략 | health <= 0이면 Die() 호출 직전에 분기해서 흔들림을 건너뜀 | |

**User's choice:** 발동함 (추천 옵션)

---

## 흔들림 강도/결 (Shake Feel)

| Option | Description | Selected |
|--------|-------------|----------|
| 고정 강도 | 데미지량과 무관하게 항상 동일한 강도/지속시간 | ✓ |
| 데미지 비례 강도 | 많이 맞을수록 더 세게 흔들림, 최대치 클램프 필요 | |

**User's choice:** 고정 강도 (추천 옵션)

---

| Option | Description | Selected |
|--------|-------------|----------|
| 랜덤 오프셋 (Perlin/Random) | 매 프레임 임의 방향으로 작은 오프셋을 주고 지수적으로 감쇠 | ✓ |
| 진동(Sine wave) 패턴 | 규칙적인 진동 패턴 — 더 예측 가능하지만 덜 자연스러움 | |

**User's choice:** 랜덤 오프셋 (추천 옵션)

---

| Option | Description | Selected |
|--------|-------------|----------|
| 타이머를 최대치로 리프레시 | 연속 피격 시 항상 일정한 강도로 보임 | ✓ |
| 가산(중첩) — 점점 더 세짐 | 연속타격일수록 더 오래, 더 세게 흔들림 — 상한선 필요 | |

**User's choice:** 타이머를 최대치로 리프레시 (추천 옵션)

---

## 카메라 파이프라인과의 합성

| Option | Description | Selected |
|--------|-------------|----------|
| 항상 흔들림 | 일반/보스 구역 구분 없이 피격 시마다 항상 화면이 흔들림 | ✓ |
| 일반 스테이지에서만 | 보스존(레거시 추종 경로)에서는 흔들림도 끔 — D-15와 일관성 | |

**User's choice:** 항상 흔들림 (추천 옵션)

---

| Option | Description | Selected |
|--------|-------------|----------|
| 경계 뚫고 나가도 됨 | 흔들림은 ApplyBoundsClamp 이후 마지막으로 더해짐 — 맵 밖이 순간적으로 보여도 무방 | ✓ |
| 경계 안에서만 흔들림 (클램프 이후 재적용) | 흔들림 자체를 다시 클램프 — 구현이 더 복잡함 | |

**User's choice:** 경계 뚫고 나가도 됨 (추천 옵션)

---

## 튜닝 파라미터 노출 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 강도 + 지속시간 2개만 | `shakeMagnitude`, `shakeDuration` — 기존 zoomSmoothing류와 같은 단순 float 필드 | ✓ |
| 강도 + 지속시간 + 감쇠곡선(AnimationCurve) | 디자이너가 감쇠 방식을 직접 그릴 수 있음 — 이번 범위엔 과함 | |

**User's choice:** 강도 + 지속시간 2개만 (추천 옵션)

---

## Claude's Discretion

- `Shake()` 메서드 시그니처 (매개변수 없음)
- 감쇠 곡선의 정확한 수식 (선형 vs 지수)
- `Random.insideUnitCircle` vs Perlin noise
- LateUpdate 내 정확한 삽입 위치 (재앵커 블록 이후 — 기술적으로 필수)
- 신규 필드/내부 상태 변수 이름

## Deferred Ideas

- 보스 피격 시에도 카메라 흔들림 (HP.cs 공용 OnHit 이벤트 신설 필요) — 이번 Phase 범위 밖.
- 감쇠 곡선 AnimationCurve Inspector 노출 — 이번 Phase 범위 밖.
