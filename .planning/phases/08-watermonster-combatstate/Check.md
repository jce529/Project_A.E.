# Phase 8 — WaterMonster CombatState 마이그레이션 검증 상태

**요약:** 08-03-PLAN.md 미실행. 정적 회귀 검사조차 아직 실행되지 않았고, Play 모드 검증도 미착수.
코드 자체(08-01, 08-02)는 완료됨.

## 무엇이 바뀌었나
`WaterMonsterCombatState.SelectAttackStrategy` 의 수작업 `List<IAttackStrategy>` + `Random.Range`
풀-랜덤을 제거하고, `CombatState` 범용 헬퍼 기반 조건부 가중치 랜덤으로 교체 (D-01~D-06,
08-CONTEXT.md). WaterSpirit 과 달리 직전 패턴은 **완전배제가 아니라 가중치 0.5배 감쇠**,
`WaterWavePush` 는 45초(광폭화 시 22.5초) 특수 잠금 유지.

## 실제 체크리스트 위치
- `Assets/Enemy/WaterMonster/Check.md` — 검증 항목 9개, 전부 미체크 (상태: "보류" 로 명시돼 있음)
- Phase 7 의 `Assets/Enemy/WaterSpirit/Check.md`, `Assets/Enemy/Tutorial/TutorialBoss/Check.md` 도
  이 단계에서 함께 일괄 검증하기로 사용자가 결정함 (SpiritCombatState 완전배제 회귀 없음 확인 포함)

## 남은 작업 (08-03-PLAN.md)
1. 정적 회귀 검사 실행 (아직 미실행)
2. WaterSpirit / TutorialBoss / WaterMonster 3종 Play 모드 일괄 검증

## 현재 상태
보류 중 — 사용자가 직접 요청할 때까지 진행하지 않는다.
