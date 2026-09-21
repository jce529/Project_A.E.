# Phase 16: 아이템 코어 (IItem + ItemData ScriptableObject) - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-09-19
**Phase:** 16-iitem-itemdata-scriptableobject-id-useeffect-ui
**Areas discussed:** IItem 구현 위치, UseEffect 파라미터 설계, 아이템 ID 스킴, 스키마 범위(필드 목록)

---

## IItem 구현 위치

| Option | Description | Selected |
|--------|-------------|----------|
| ItemData가 직접 IItem 구현 | class ItemData : ScriptableObject, IItem. 데이터와 효과 로직이 한 파일에 공존 | ✓ |
| 별도 런타임 클래스가 IItem 구현 | ItemData는 순수 데이터만, 별도 런타임 구현체가 참조 | |

**User's choice:** ItemData가 직접 IItem 구현
**Notes:** 프로젝트에 ScriptableObject 선례가 전혀 없어 이 phase가 첫 패턴을 확립함.

---

## UseEffect 파라미터 설계

### 구현 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 범용 효과 필드만 정의, UseEffect는 최소 구현 | switch로 분기만 하고 실제 PlayerStats 연동은 추후 | |
| 필드 + 실제 효과까지 끝까지 구현 | 최소 소모품 1종(체력 회복)까지 PlayerInteraction 경유 실제 적용 | ✓ |

**User's choice:** 필드 + 실제 효과까지 끝까지 구현

### 종류별 분기

| Option | Description | Selected |
|--------|-------------|----------|
| 소모품만 effectType로 실제 효과 적용, 진행아이템은 빈 구현 | 소모품: Heal 등 실제 적용. 진행아이템: no-op | ✓ |
| 둘 다 같은 effectType 구조로 처리 | 종류 구분 없이 통일된 effectType enum(Heal/Unlock/None) | |

**User's choice:** 소모품만 effectType로 실제 효과 적용, 진행아이템은 빈 구현

### 효과 종류 개수

| Option | Description | Selected |
|--------|-------------|----------|
| Heal 하나만 | effectType enum에 Heal 1개, PlayerStats.Heal(amount) 재사용 | ✓ |
| Heal + None 2가지 | 효과 없는 소모품(테스트/플레이스홀더용)도 허용 | |

**User's choice:** Heal 하나만

**Notes:** `PlayerStats.Heal(float amount)`가 이미 존재해 재사용 가능함을 코드 스카우트로 확인.

---

## 아이템 ID 스킴

| Option | Description | Selected |
|--------|-------------|----------|
| string, 수동 입력 | [SerializeField] string id, 에디터에서 직접 입력 | ✓ |
| string, 에셋 이름 기반 자동 생성 | OnValidate/에디터 스크립트로 자동 채움 | |

**User's choice:** string, 수동 입력
**Notes:** Phase 18의 SaveData.Items → List<ItemSaveEntry>{itemId: string, count} 전환과 타입 호환.

---

## 스키마 범위 (필드 목록)

### 필드 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 표시용 메타데이터도 포함 | displayName/icon/description 추가 | |
| 최소 필드만 (id/type/effect) | UI 없는 시점에 표시용 필드를 둘 이유 없음 | ✓ |

**User's choice:** 최소 필드만 (id/type/effect)

### 예시 에셋

| Option | Description | Selected |
|--------|-------------|----------|
| 예시 에셋 1~2개 생성 | 소모품(Heal) 1개 + 진행아이템 1개 .asset 실제 생성 | ✓ |
| 클래스 정의만, 에셋은 미생성 | CreateAssetMenu만 달아두고 실제 생성은 사용자 몫 | |

**User's choice:** 예시 에셋 1~2개 생성

---

## Claude's Discretion

- ItemType/effectType enum 네이밍 및 파일 배치 (nested vs 별도 파일)
- CreateAssetMenu 속성의 menuName/fileName 문자열
- PlayerInteraction에서 PlayerStats를 얻는 구체적 방법(GetComponent 캐싱 여부)
- 예시 에셋 2개의 구체적 이름/수치

## Deferred Ideas

- 표시용 메타데이터(displayName/icon/description) — Phase 17 또는 이후 UI phase
- 아이템 ID 중복/형식 검증 도구 — 필요 시 Editor 검증 스크립트
- 소모품 effectType 확장 — 필요해지는 시점에 enum 값만 추가
- (참고, 이번 phase 범위 밖) Phase 15 CONTEXT.md의 "자동저장을 Phase 16으로 분리" 기록이 재번호 이후
  현재 로드맵에 반영되지 않은 상태로 발견됨 — 별도 백로그 확인 필요
