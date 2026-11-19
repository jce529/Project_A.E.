# Phase 11: Newtonsoft.Json 기반 싱글톤 세이브/로드 매니저 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-09
**Phase:** 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json
**Areas discussed:** 저장 트리거, 슬롯 구조, 데이터 스키마 범위, UI 범위, 좌표 복원 통합, 새 게임 처리, 플레이어 스탯 범위

---

## 저장 트리거

| Option | Description | Selected |
|--------|-------------|----------|
| 기존 스크립트에 훅 추가 | Checkpoint.cs/보스 OnDeath에서 SaveLoadManager.Save()를 직접 호출. 최소 변경 | |
| 이벤트 기반 완전 분리 | Checkpoint/Boss가 자체 이벤트를 발행하고 SaveLoadManager가 구독 | |
| (자유 응답) 매니저만 저장 기능 보유, 나머지는 전부 호출로 | 매니저가 Save()를 소유하고, Checkpoint/Boss는 그걸 직접 호출만 함 (옵저버 구조 아님) | ✓ |

**User's choice:** "저장기능은 매니저만 보유, 나머지는 전부 호출로" — 사실상 첫 번째 옵션(직접 호출)과 동일한 방향으로 확정.
**Notes:** 별도 이벤트 버스 신설 없이 최소 변경으로 통합.

---

## 슬롯 구조

| Option | Description | Selected |
|--------|-------------|----------|
| 단일 슬롯 (파일 1개) | save.json 하나만 유지. 구현 단순 | ✓ |
| 다중 슬롯 | slot1.json, slot2.json 등. UI에서 슬롯 선택 필요 | |

**User's choice:** 단일 슬롯 (파일 1개)
**Notes:** —

---

## 데이터 스키마 범위 (보스 진행도 / 맵 기믹 Dictionary)

| Option | Description | Selected |
|--------|-------------|----------|
| 최소 스텁만 구현 | Dictionary 틀만 만들고 실제로 채워 넣는 항목은 0~최소 | ✓ |
| 보스 격파 여부까지 지금 연동 | WaterMonster/WaterSpirit/TutorialBoss/WoodBoss 격파 여부를 실제로 기록·로드 | |

**User's choice:** 최소 스텁만 구현
**Notes:** 현재 지속 상태를 가지는 맵 기믹이 프로젝트에 거의 없어 스키마 확장은 후속 페이즈로 미룸.

---

## UI 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 매니저 API만 (UI 제외) | SaveGame()/LoadGame()/HasSaveFile() 공개 API만 구현 | ✓ |
| 이어하기 버튼까지 포함 | MainMenuUI.cs에 이어하기 버튼 추가 + 세이브 존재 여부 분기 | |

**User's choice:** 매니저 API만 (UI 제외)
**Notes:** —

---

## 좌표 복원 통합

| Option | Description | Selected |
|--------|-------------|----------|
| 저장 데이터의 좌표를 직접 transform.position에 적용 | 기존 PlayerSpawner/Portal 경로를 건드리지 않는 독립 새 경로 | |
| PlayerSpawner.targetSpawnPointName 경로 재사용 | 저장된 좌표를 이미 쓰이는 static 필드에 세팅해 기존 ApplySpawn() 흐름 재사용 | ✓ |

**User's choice:** PlayerSpawner.targetSpawnPointName 경로 재사용
**Notes:** GameManager.NextSpawnPointName은 고아 코드로 확인됨 — 건드리지 않음.

---

## 새 게임 처리

| Option | Description | Selected |
|--------|-------------|----------|
| 확인 없이 즉시 덮어쓰기 | SaveLoadManager.NewGame()이 바로 파일을 덮어씀 | |
| 기존 파일 유지, 메모리만 초기화 | 실제 파일 덮어쓰기는 다음 Save() 트리거 시점에만 발생 | ✓ |

**User's choice:** 기존 파일 유지, 새 게임은 메모리만 초기화
**Notes:** —

---

## 플레이어 스탯 범위

| Option | Description | Selected |
|--------|-------------|----------|
| health / maxHealth / maxTotalHealth 3개만 | PlayerStats에 실제 존재하는 필드만 반영 | |
| You decide (Claude 재량) | 연구/계획 단계에서 PlayerStats 구조를 다시 확인해 필요한 필드만 포함 | ✓ |

**User's choice:** You decide (Claude 재량)
**Notes:** —

---

## Claude's Discretion

- 정확한 PlayerStats 저장 필드 구성
- 비동기 씬 로드 중 로딩 화면 UI 유무 (UI 범위 밖 결정에 따라 기본적으로 없음)
- 보스 진행도/맵 기믹 Dictionary 키 네이밍 규칙
- Newtonsoft.Json 직렬화 세부 설정

## Deferred Ideas

- 메인 메뉴 "이어하기" 버튼 UI 연동
- 다중 세이브 슬롯 지원
- GameManager.NextSpawnPointName 고아 코드 정리
