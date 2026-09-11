# Phase 15 Bug Index

Phase: `15-load-timing-and-load-scope`
Purpose: Phase 15에서 발견된 결함과 수정·재검증 이력을 한곳에서 추적한다.

| ID | 제목 | 심각도 | 상태 | 발견일 |
|---|---|---:|---|---|
| [BUG-001](BUG-001-player-attackbox-null.md) | Player 기본 공격 AttackBox 참조 누락 | 낮음 | 수정됨 — 재검증 필요 | 2026-09-10 |
| [BUG-002](BUG-002-player-health-bounds-inverted.md) | Player 최대 체력과 성장 상한 역전 | 중간 | 해결됨 | 2026-09-10 |
| [BUG-003](BUG-003-watermonster-not-wired.md) | WaterMonsterController 씬·프리팹 배치 없음 | 높음 | 확인됨 — 미해결 | 2026-09-10 |
| [BUG-004](BUG-004-mainmenuui-duplicate-component.md) | MainMenuUI 중복 컴포넌트 및 빈 참조 | 낮음 | 수정됨 — 재검증 필요 | 2026-09-10 |
| [BUG-005](BUG-005-camera-shake-continues-while-paused.md) | 일시정지 중 카메라 흔들림 지속 | 중간 | 해결됨 | 2026-09-10 |
| [BUG-006](BUG-006-tutorialboss-wall-reference-missing.md) | TutorialBoss 격파 상태 로드 시 보스방 벽이 해제되지 않음 | 중간 | 해결됨 | 2026-09-11 |
| [BUG-007](BUG-007-inputhandler-lost-on-scene-transition.md) | 씬 전환 후 InputHandler 액션 에셋 유실로 플레이어 입력 불가 | 높음 | 확인됨 — 미해결 | 2026-09-11 |

## 최신 집계 (2026-09-11)

- 해결됨: 3건 — BUG-002, BUG-005, BUG-006
- 수정됨 — 재검증 필요: 2건 — BUG-001, BUG-004
- 확인됨 — 미해결: 2건 — BUG-003, BUG-007
- 최우선 조치: 플레이 진행을 차단하는 BUG-007 수정 및 Tutorial Map → 1 stage 실제 전환 재검증

## 운영 규칙

- 새 결함은 이 폴더에 `BUG-<global-id>-<slug>.md`로 추가한다.
- 버그 상태가 바뀌면 이 표와 개별 문서를 함께 갱신한다.
- 해결 시 수정 커밋과 Unity Play 모드 또는 자동 검증 결과를 개별 문서에 기록한다.
- Phase 15 UAT 및 검증 문서에서 관련 버그 문서로 연결한다.

## 이전 인덱스

기존 루트 `bug/README.md`는 원문 보존을 위해 [LEGACY-README.md](LEGACY-README.md)로 이동했다. 현재 상태 확인에는 위 표를 사용한다.
