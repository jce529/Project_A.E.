# Phase 9: 일반 스테이지와 보스 스테이지 진입 시 카메라 크기(줌) 변화 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-07-30
**Phase:** 09-camera-zoom-stage-transition
**Areas discussed:** 스테이지 진입 감지 방식, 줌 목표 수치, 전환 연출 방식, 적용 범위 및 복귀 시점, 카메라 X축 이동 범위 제한(사용자 추가 제기)

---

## 스테이지 진입 감지 방식

| Option | Description | Selected |
|--------|-------------|----------|
| 씬 단위 (씬 로드 시 고정) | 각 씬에 일반/보스 플래그를 미리 지정, 씬 로드 직후 적용 | |
| 트리거 콜라이더 기반 | 보스 구역에 BossRoomTrigger 배치, 진입 시 줌 전환 | ✓ |

**User's choice:** 트리거 콜라이더 기반
**Notes:** 씬이 이미 스테이지 단위로 분리되어 있지만, 트리거 방식을 선택함으로써 한 씬 안에 일반/보스 구간이 공존해도 대응 가능.

| Option | Description | Selected |
|--------|-------------|----------|
| 에디터에서 수동 배치 (사용자) | 각 보스 구역에 BoxCollider2D(Is Trigger)를 사용자가 직접 배치 | ✓ |
| 보스 GameObject에 자동 부착 | BossController Awake 시 자동으로 대형 콜라이더 생성 | |

**User's choice:** 에디터에서 수동 배치
**Notes:** 보스마다 범위가 다를 수 있어 자동화보다 수동 배치가 적합하다고 판단.

| Option | Description | Selected |
|--------|-------------|----------|
| 자동 복귀 (Exit 시 즉시 되돌림) | OnTriggerExit2D 시 즉시 일반 줌으로 복귀 | ✓ |
| 복귀 안 함 (보스 처치까지 유지) | 별도 이벤트가 있어야 복귀 | |

**User's choice:** 자동 복귀 (Exit 시 즉시 되돌림)

---

## 줌 목표 수치

| Option | Description | Selected |
|--------|-------------|----------|
| 구체 수치로 직접 지정 | 사용자가 정확한 값을 지정 | ✓ |
| 배율로 지정 (현재값 기준 x배) | 상대적 배율 | |
| Claude 재량 (임시값 적용 후 플레이테스트로 조정) | | |

**User's choice:** 구체 수치로 직접 지정 → 일반 = 5, 보스 = 7
**Notes:** "일반 5, 보스 7로 일단 세팅하고 보자" — 확정값이 아니라 초기 튜닝값.

| Option | Description | Selected |
|--------|-------------|----------|
| Inspector 필드로 노출 (권장) | Play 모드 중 조정 가능 | ✓ |
| 코드 상수로 고정 | 변경 시 코드 수정 필요 | |

**User's choice:** Inspector 필드로 노출

---

## 전환 연출 방식

| Option | Description | Selected |
|--------|-------------|----------|
| 부드럽게 전환 (Lerp) | 기존 위치 추적과 동일한 방식 | ✓ |
| 즉시 전환 | 트리거 진입/퇴장 순간 값이 바로 바뀜 | |

**User's choice:** 부드럽게 전환 (Lerp)

| Option | Description | Selected |
|--------|-------------|----------|
| 별도 속도 필드로 분리 (권장) | zoomSmoothing 같은 신규 필드 | ✓ |
| 기존 smoothing 값 재사용 | 필드 추가 없이 위치 추적과 공유 | |

**User's choice:** 별도 속도 필드로 분리

---

## 적용 범위 및 복귀 시점

| Option | Description | Selected |
|--------|-------------|----------|
| 기능만 구현, 배치는 사용자가 나중에 (권장) | 이번 Phase는 스크립트/로직까지만 | ✓ |
| 특정 보스 씬 1곳에 실제 배치까지 Phase 범위에 포함 | 에디터 배치 작업까지 포함 | |

**User's choice:** 기능만 구현, 배치는 사용자가 나중에

---

## 카메라 X축 이동 범위 제한 (사용자가 논의 중 추가 제기)

**User's free-text:** "카메라가 나갈수 없는 구역을 정하고싶어. 일단 x축을 기준으로 벽처럼 막혀서 안넘어가도록 만들고싶어."

| Option | Description | Selected |
|--------|-------------|----------|
| Phase 9에 함께 포함 (권장) | 동일 CameraController.cs 작업이므로 이번 Phase에 포함 | ✓ |
| 별도 Phase로 분리 | Phase 10으로 따로 추가 | |

**User's choice:** Phase 9에 함께 포함

| Option | Description | Selected |
|--------|-------------|----------|
| min/max X 숫값 Inspector 필드 | minX, maxX float 직접 입력 | ✓ |
| BoxCollider2D 기반 (Phase 4 mapBounds 패턴) | 에디터에 배치한 콜라이더의 bounds 참조 | |

**User's choice:** min/max X 숫값 Inspector 필드

| Option | Description | Selected |
|--------|-------------|----------|
| 카메라 중심점만 클램프 (간단) | transform.position.x 만 제한 | |
| 화면 반폭까지 감안 (정밀) | orthographicSize * aspect 까지 계산해 맵 밖이 안 보이게 함 | ✓ |

**User's choice:** 화면 반폭까지 감안 (정밀)

| Option | Description | Selected |
|--------|-------------|----------|
| 예, Y축은 제한 없음 (이번 Phase 범위 아님) | | ✓ |
| Y축도 동일한 방식으로 함께 제한 | | |

**User's choice:** 예, Y축은 제한 없음

---

## Claude's Discretion

- 신규 트리거/줌 컴포넌트의 클래스/파일 이름
- 트리거 스크립트가 CameraController를 참조하는 방식
- LateUpdate 내 로직 적용 순서 (위치 추적 → 줌 Lerp → X축 클램프 등)

## Deferred Ideas

- 실제 보스 씬(WaterMonster/WaterSpirit/TutorialBoss)에 트리거 콜라이더를 배치하는 에디터 작업 — 사용자가 추후 직접 진행
- Y축 이동 범위 제한 — 이번 Phase 범위 아님
