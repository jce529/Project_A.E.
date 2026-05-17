# Phase 3: 폭발 기믹 연계 및 보스 순간이동 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-16
**Phase:** 03-explosion-gimmick-teleport
**Areas discussed:** 연쇄 폭발 시퀀스, 폭발 후 웅덩이 처리, WaterTeleportState 동작, 텔레포트 전환 조건

---

## 연쇄 폭발 시퀀스

| Option | Description | Selected |
|--------|-------------|----------|
| 순차 연쇄 폭발 | 각 웅덩이에서 차례로 AoE 발생 (도미노 효과) | |
| 동시 전체 폭발 | 모든 Indestructible 웅덩이 동시 폭발 | ✓ |
| 단일 중앙 AoE | 맵 중앙에서 1회 폭발 | |

**경고 딜레이:** 경고 후 폭발 선택, **2초** 딜레이 (튜닝 가능)

**User's choice:** 동시 전체 폭발 + 경고 2초

---

## 폭발 후 웅덩이 처리

| Option | Description | Selected |
|--------|-------------|----------|
| 파괴 후 카운트 리셋 | Indestructible 웅덩이 Pool Return + 카운트 0 리셋 | ✓ |
| 웅덩이 유지 + 카운트만 리셋 | 웅덩이는 남고 스택만 0으로 | |
| 웅덩이 유지 + 카운트 유지 | 다음 임계치까지 계속 쌓임 | |

**User's choice:** 파괴 후 카운트 리셋 — 사이클 반복형

---

## WaterTeleportState 동작

| Option | Description | Selected |
|--------|-------------|----------|
| 랜덤 웅덩이 | 무작위 Indestructible 웅덩이 선택 | |
| 농락형 포지셔닝 | 거리 반비례 선택 (가까우면 멀리, 멀면 가까이) | ✓ |
| 플레이어에게 가장 가까운 | 공격적 접근 | |

**텔레포트 연출:** 사라짐 + 이펙트 + 나타남 (0.2~0.3초 딜레이)
**도착 후 동작:** 즉시 CombatState 복귀

**User's choice:** 농락형 포지셔닝 — 근거리 상황이면 가장 먼 웅덩이로 이동 후 원거리 패턴, 원거리 상황이면 가장 가까운 웅덩이로 이동 후 근접 패턴

---

## 텔레포트 전환 조건

**User's choice (자유 입력):** 각 공격 패턴 선택마다 텔레포트가 후보로 포함 — `SelectAttackStrategy`에 통합. Indestructible ≥ 2개 이상일 때만 텔레포트 후보에 포함. 쿨다운 수치는 플래너에게 위임.

---

## Claude's Discretion

- 경고 이펙트 구체 에셋
- 폭발 AoE 반경 수치
- 텔레포트 VFX 에셋
- 텔레포트 쿨다운 수치
- 폭발 대미지 수치
- 텔레포트 연출 딜레이 정확한 수치

## Deferred Ideas

- 텔레포트 후 추가 패턴 연계 (Phase 4)
- 광폭화 모드 (Phase 4)
- 이속/감속 장판 (Phase 4)
