# Phase 18: 인벤토리 시스템 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-09-21
**Phase:** 18-inventory-system-depends-on-phase-17
**Areas discussed:** 슬롯/스택 구조, 인벤토리 소유/배치, 월드 아이템 획득 연동, 추가/제거/사용 API 동작

---

## 슬롯/스택 구조

| Option | Description | Selected |
|--------|-------------|----------|
| 고정 상수 (예: 20슬롯, 슬롯당 99개) | 코드 하드코딩 상수. 프로젝트의 '순수 로직' 원칙에 부합 | ✓ |
| Inspector 노출 필드 | SerializeField로 노출해 튜닝 가능하게 | |
| 무제한 (Dictionary<id,count>만) | 로드맵의 '고정 슬롯+스택'과 충돌 | |

| Option | Description | Selected |
|--------|-------------|----------|
| Dictionary<string itemId, int count> | 슬롯 순서 없이 종류별 개수만 관리 | |
| 슬롯 배열(List<InventorySlot>) — ItemData+count | 실제 슬롯 위치/순서 존재, 향후 UI에서 의미를 가질 수 있음 | ✓ |

**User's choice:** 고정 상수 슬롯/스택 수 + `List<InventorySlot>` 슬롯 배열 구조.
**Notes:** 정확한 슬롯 수/스택 수치는 계획 단계에서 확정하기로 함.

---

## 인벤토리 소유/배치

| Option | Description | Selected |
|--------|-------------|----------|
| Player GameObject의 새 컴포넌트 (Inventory : MonoBehaviour) | PlayerStats/PlayerInteraction과 동일 패턴 | ✓ |
| PlayerStats에 통합 (필드로 추가) | 별도 컴포넌트 없이 기존 클래스 확장 | |

**User's choice:** 새 `Inventory` 컴포넌트.
**Notes:** 영속성(세이브/로드)은 Phase 19 범위이므로 이번 phase는 메모리 상태만 다루고
DontDestroyOnLoad 여부는 신경쓰지 않기로 확인(단일 옵션으로 제시, 질문 자체는 재확인 없이 진행).

---

## 월드 아이템 획득 연동

| Option | Description | Selected |
|--------|-------------|----------|
| Checkpoint.cs와 동일한 IPlayerInteractable 패턴 (interact 키) | 기존 PlayerInteraction 인프라 재사용 | ✓ |
| 트리거 접촉 시 자동 획득 | 새로운 감지 경로 필요 | |

**User's choice:** IPlayerInteractable 패턴.

| Option | Description | Selected |
|--------|-------------|----------|
| 획득 실패 — 아이템은 월드에 그대로 남음 | 데이터 유실 없음, 가장 안전한 기본값 | ✓ |
| 획득 실패 — 사용자에게 알림(Debug.Log) | 실패 자체는 동일, 로그로 명시 | |

**User's choice:** 획득 실패 시 월드에 그대로 남음.
**Notes:** 인벤토리 가득 찬 상태에서 파괴하지 않는 것이 원칙.

---

## 추가/제거/사용 API 동작

| Option | Description | Selected |
|--------|-------------|----------|
| Phase 17과 동일한 ContextMenu 검증 훅 | UI 없이 Play 모드에서 검증하는 기존 확립된 패턴 | ✓ |
| 간단한 키바인딩/테스트용 트리거 추가 | 임시 키 입력으로 첫 슬롯 사용 | |

**User's choice:** ContextMenu 검증 훅 패턴 (Phase 17과 일관).

| Option | Description | Selected |
|--------|-------------|----------|
| 슬롯을 완전히 비우고 리스트에서 제거하지 않음 | 고정 길이 슬롯 배열 유지, 슬롯 위치 불변 | ✓ |
| 사용된 아이템 엔트리를 리스트에서 제거(뒤 슬롯이 압축) | 가변 길이, 슬롯 위치가 이동할 수 있음 | |

**User's choice:** 슬롯을 비우되 제거하지 않음(고정 위치 유지).

---

## Claude's Discretion

- 정확한 슬롯 개수/최대 스택 수치.
- `InventorySlot` 타입 설계(클래스/구조체, nested 여부).
- `TryAddItem`/`RemoveItem`/`UseItem` 메서드 시그니처.
- 동일 아이템 스택 병합 순서(기존 슬롯 우선 채움 vs 새 슬롯).
- `WorldItem`의 `CanInteract`/`Interact` 구체 구현과 프리팹 구성.

## Deferred Ideas

- 인벤토리 UI(그리드, 드래그앤드롭, 아이콘 표시) — 추후 UI phase.
- 세이브/로드 연동 — Phase 19.
- 아이템 정렬/자동 정리 — 필요 시 백로그.
