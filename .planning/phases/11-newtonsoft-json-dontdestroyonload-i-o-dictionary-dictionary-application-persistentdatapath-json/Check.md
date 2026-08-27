# Phase 11 — 세이브/로드 매니저 검증 상태

**요약:** 11-04-PLAN.md 미실행. ContextMenu 검증 훅 코드 자체가 아직 추가되지 않았고, Play 모드
검증(저장/보스격파/로드/새게임 4개 섹션)도 미착수. 코드(11-01~11-03)는 완료됨.

## 무엇이 바뀌었나
`SaveLoadManager` DontDestroyOnLoad 싱글톤(부트스트랩 자동생성), `save.json` 단일슬롯 메모리캐시
I/O, 체크포인트(S키) + 보스 4종(TutorialBoss/WoodBoss/WaterSpirit/WaterMonster) 격파 시점 저장 훅.

## 실제 체크리스트 위치
`Assets/SaveSystem/Check.md`:
- 섹션 1) 정적 회귀 검사 15항목 — **이미 PASS** (2026-08-10, 11-01~03 범위 기준. 11-04 의
  ContextMenu 훅 추가분은 아직 검사되지 않음)
- 섹션 2) Play 모드 — 저장 (D-01/D-02) — 전부 미체크
- 섹션 3) Play 모드 — 보스 격파 자동 저장 (Group A/B 4종) — 전부 미체크
- 섹션 4) Play 모드 — 로드 (D-05, 비동기 씬 로드) — 전부 미체크
- 섹션 5) Play 모드 — 새 게임 (D-06) — 전부 미체크

## 남은 작업 (11-04-PLAN.md)
1. `SaveLoadManager.cs` 에 `[ContextMenu(...)]` 훅 4개 추가 (UI 없이 Save/Load/NewGame 테스트)
2. 위 섹션 2~5 Play 모드 검증

## 현재 상태
보류 중 — 사용자가 직접 요청할 때까지 진행하지 않는다.
