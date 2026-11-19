# Phase 11 — 세이브/로드 매니저 검증 상태

**요약:** ContextMenu 훅을 포함한 코드는 현재 존재한다. 2026-09-10 Unity MCP로 비보스
저장/로드/새게임 경로를 실측했고, 씬·스폰·스탯 복원과 memory-only 새게임 계약을 통과했다.

## 무엇이 바뀌었나
`SaveLoadManager` DontDestroyOnLoad 싱글톤(부트스트랩 자동생성), `save.json` 단일슬롯 메모리캐시
I/O, 체크포인트(S키) + 보스 4종(TutorialBoss/WoodBoss/WaterSpirit/WaterMonster) 격파 시점 저장 훅.

## 실제 체크리스트 위치
`Assets/SaveSystem/Check.md`:
- 섹션 1) 정적 회귀 검사 15항목 — **이미 PASS** (2026-08-10, 11-01~03 범위 기준. 11-04 의
  ContextMenu 훅 추가분은 아직 검사되지 않음)
- 섹션 2) Play 모드 — 저장 (D-01/D-02) — 통과
- 섹션 3) Play 모드 — 보스 격파 자동 저장 (Group A/B 4종) — 전부 미체크
- 섹션 4) Play 모드 — 로드 (D-05, 비동기 씬 로드) — 핵심 복원 통과, UI/파일없음 경고 미확인
- 섹션 5) Play 모드 — 새 게임 (D-06) — 통과

## 2026-09-10 비보스 실측 결과

- MainMenu 슬롯 카드로 실제 로드: `Tutorial Map`, `check_slot1`, 체력 `100/100` 복원.
- 기존 슬롯 2 실제 로드: `Tutorial Map`, `check`, 체력 `180/400` 복원.
- `NewGameInSlot` 직후 파일 무변경, 이후 `SaveAtCheckpoint`에서만 선택 슬롯 기록.
- 원본 세이브 3개는 검증 종료 후 원래 크기와 수정 시각으로 복구.

## 남은 작업 (11-04-PLAN.md)

1. 복원된 체력 UI 표시 확인.
2. 저장 파일이 없을 때 ContextMenu Load 경고 확인.
3. 보스 격파 자동 저장은 사용자 요청으로 보류.

## 현재 상태
부분 통과 — 비보스 핵심 I/O와 복원 코드는 구현 및 실측 완료.
