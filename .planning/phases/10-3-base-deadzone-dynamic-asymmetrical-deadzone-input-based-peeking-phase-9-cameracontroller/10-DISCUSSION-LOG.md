# Phase 10: 카메라 데드존 3종 기법 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-04
**Phase:** 10-camera-deadzone-dynamic-offset-peeking
**Areas discussed:** 데드존 박스 정의, 이동방향 감지 방식, 피킹 입력·취소 조건, 기존 파이프라인 통합 순서

---

## 데드존 박스 정의

| Option | Description | Selected |
|--------|-------------|----------|
| 월드 유닛 고정 (Inspector float) | Phase 9 minX/maxX와 동일 스타일, 계산 없음 | ✓ |
| 화면 비율 기반 (%) | orthographicSize/aspect로 계산, 줌 비율 항상 동일 | |

| Option | Description | Selected |
|--------|-------------|----------|
| 줌 전환 시 고정 월드 크기 유지 | 로직 단순, Phase 9 줌 전환과 완전 독립 | ✓ |
| 화면 비율 기반 자동 스케일 | 화면상 체감 크기 일정 유지 | |

| Option | Description | Selected |
|--------|-------------|----------|
| 가로/세로 별도 값 (deadzoneWidth/Height) | 좌우·상하 이동 특성이 다른 플랫포머에 적합 | ✓ |
| 정사각형 하나의 값 | 필드 1개로 단순 | |

| Option | Description | Selected |
|--------|-------------|----------|
| OnDrawGizmos로 박스 윤곽 표시 | Scene 뷰 튜닝용, 런타임 영향 없음 | ✓ |
| 시각화 없음 | 체감으로만 확인 | |

**User's choice:** 월드 유닛 고정 + 줌 무관 고정 크기 + 가로/세로 분리 + Gizmo 시각화
**Notes:** 없음

---

## 이동방향 감지 방식 (동적 비대칭 오프셋)

| Option | Description | Selected |
|--------|-------------|----------|
| target.position 프레임 델타로 추정 | PlayerController 미수정, 카메라 스크립트만으로 구현 | ✓ |
| PlayerController에 public 접근자 추가 | 더 정확하지만 Player 스크립트 수정 필요 | |

| Option | Description | Selected |
|--------|-------------|----------|
| 멈춘 후 일정 시간 유지 후 천천히 복귀 | 타이머 추가 필요 | ✓ |
| 정지 즉시 복귀 시작 (SmoothTime만으로 완만하게) | 타이머 불필요 | |

| Option | Description | Selected |
|--------|-------------|----------|
| 데드존 경계를 밀고 있을 때만 발동 | 별도 임계값 없이 기존 데드존 판정 재사용 | ✓ |
| 별도 임계값(순간 이동량 기준)으로 판정 | 데드존과 무관하게 더 일찍 오프셋 켜질 수 있음 | |

**User's choice:** 프레임 델타 추정 + 정지 후 유지-복귀 + 데드존 경계 기준 발동
**Notes:** 없음

---

## 피킹 입력·취소 조건

**중간 논의 (자유 형식):** 사용자가 "InputHandler 직접 구독 vs PlayerController 경유" 트레이드오프를 질문. Claude가 코드 근거(`InputHandler.cs` 이벤트 버스 설계, `PlayerController.cs`의 private 필드 구조)를 들어 InputHandler 직접 구독이 이 코드베이스의 정석 경로임을 설명. 사용자가 이어서 "이벤트씬(컷신/사망/맵 이동)에서 입력이 제한되는 경우에도 카메라가 반응하는 것 아니냐"고 재질문 — 실제로 `PlayerController.OnMove()`(106-108행)가 `movementLocked` 체크 없이 무조건 `moveInput`을 갱신함을 확인, 유효한 우려로 판명. 해결책으로 `movementLocked`(이미 public) 가드 + `OnEnable`/`OnDisable` 대칭 구독/해제를 제시, 사용자가 이 방향 확정.

| Option | Description | Selected |
|--------|-------------|----------|
| 카메라가 InputHandler.OnMoveEvent 직접 구독 + movementLocked 가드 + 대칭 구독/해제 | PlayerController 미수정, 잠금 상태 존중 | ✓ (재질문 후 확정) |
| PlayerController 경유 | Player 스크립트 수정 필요 | |

| Option | Description | Selected |
|--------|-------------|----------|
| target.position 프레임 델타가 거의 0일 때 정지로 판단 | 동적 오프셋과 동일 방식 재사용 | ✓ |
| PlayerController에 public 속도/IsIdle() 추가 | 더 정확하지만 Player 스크립트 수정 필요 | |

| Option | Description | Selected |
|--------|-------------|----------|
| 이동량 급증만으로 프록시 감지 (대시/피격 원인 구분 안 함) | 추가 접근자 불필요 | ✓ |
| PlayerController에 IsBusy() 같은 public 접근자 추가 | 정확하지만 Player 스크립트 수정 필요 | |

| Option | Description | Selected |
|--------|-------------|----------|
| 사용자 제시값(0.5초 / 임의 거리) 그대로 Inspector 노출 | 플레이테스트로 조정 | ✓ |
| 지금 정하지 않고 논의 후 결정 | | |

| Option | Description | Selected |
|--------|-------------|----------|
| IsGrounded() 포함 (원안대로) | 이미 public, 재사용만 하면 됨 | ✓ |
| 제외 (공중에서도 피킹 허용) | | |

**User's choice:** InputHandler 직접 구독(movementLocked 가드 포함) + 프레임 델타 정지판단 + 이동량 급증 프록시 취소 + 원안 수치 그대로 + IsGrounded 포함
**Notes:** InputHandler vs PlayerController 트레이드오프에 대한 자유 질의응답이 이 영역의 핵심 결정을 만들었음 (위 중간 논의 참고).

---

## 기존 파이프라인 통합 순서

**중간 논의 (자유 형식):** 첫 질문("데드존이 기존 위치추종 Lerp를 어떻게 대체?")에 대해 사용자가 "chat하자"며 즉답 대신 논의를 요청. Claude가 "Lerp로 데드존 경계를 쫓아가면 빠른 이동 시 박스와 카메라 사이 간격이 벌어져 '박스 안 완전 정지' 취지가 깨진다"는 기술적 쟁점을 제시하고, 대안으로 "데드존은 하드컷, 오프셋/피킹만 SmoothDamp"를 제안. 사용자가 이 대안 확정.

| Option | Description | Selected |
|--------|-------------|----------|
| 데드존 하드컷 + 오프셋/피킹만 SmoothDamp | 박스 경계 누수 없음, 레이어 역할 명확 | ✓ (자유 논의 후 확정) |
| 가상 타겟 계산 후 기존 Lerp로 쫓아감 (옵션 A) | 구조 단순, 기존 Lerp 재사용 | |
| 완전히 새로운 위치 계산 경로로 전환 (옵션 B) | Phase 9 "삽입 전용" 원칙과 충돌 가능 | |

| Option | Description | Selected |
|--------|-------------|----------|
| 보스 구역에서는 데드존/오프셋/피킹 전부 비활성화, 레거시 Lerp로 복귀 | 보스전 카메라 거동이 Phase 9와 동일하게 유지 | ✓ |
| 보스 구역에서도 그대로 동시 동작 | 구현 단순하지만 보스전 카메라 예측 불가능해질 수 있음 | |
| 피킹만 비활성화, 데드존/오프셋은 유지 | 절충안 | |

| Option | Description | Selected |
|--------|-------------|----------|
| X클램프는 데드존+오프셋+피킹 적용 후 최종 위치에 마지막 적용 | Phase 9 원칙(최신 상태 반영) 유지 | ✓ |
| 데드존/오프셋은 클램프 이전 기준으로만 계산, 클램프는 별도 처리 | 로직 복잡해지고 경계 근처 시거리 발생 가능 | |

**User's choice:** 데드존 하드컷 + 오프셋/피킹 SmoothDamp + 보스 구역 전체 비활성화(레거시 복귀) + 클램프는 최종 위치 기준 마지막 적용
**Notes:** 첫 번째 질문은 즉답 대신 Claude가 기술적 트레이드오프(Lerp 누수 문제)를 먼저 제시하는 방식으로 풀림 (위 중간 논의 참고).

---

## Claude's Discretion

- 새 필드/메서드 명명, LateUpdate 내부 헬퍼 분리 방식
- SmoothDamp velocity 임시 변수 관리 방식
- Gizmo 색상/스타일
- "이동량 급증" 프록시 임계값, 오프셋 유지시간 파라미터 기본값 (Inspector 노출은 필수)

## Deferred Ideas

- Y축 데드존/오프셋/피킹 확장 여부 — 사용자 수식이 X축(+수직 피킹)만 다뤄서 이번 Phase 범위 밖으로 명시적 보류
- Y축 카메라 이동 범위 제한(minY/maxY) — Phase 9에서 이미 이월된 항목, 계속 범위 밖
