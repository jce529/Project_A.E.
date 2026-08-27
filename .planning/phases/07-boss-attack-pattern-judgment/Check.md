# Phase 7 — 보스 공격 패턴 판단 로직 리팩토링 검증 상태

**요약:** 07-02-PLAN.md (Play 모드 검증 체크포인트) 미실행. 코드는 07-01 에서 완료됨.

## 무엇이 바뀌었나
`SpiritCombatState` 의 고정 라운드로빈 배열을 `CombatState` 범용 헬퍼(`PatternCandidate` +
`SelectWeightedPattern`)를 이용한 조건부 가중치 랜덤 선택으로 교체 (D-01~D-08, 07-CONTEXT.md).
직전 사용 패턴은 **완전배제**(WaterMonster 의 0.5배 감쇠와 다름), SpiritExhaustion 직후에는
SpiritWakeRepel 강제 실행.

## 실제 체크리스트 위치
- `Assets/Enemy/WaterSpirit/Check.md` — 검증 항목 8개, 전부 미체크
- `Assets/Enemy/Tutorial/TutorialBoss/Check.md` — 검증 항목, 전부 미체크

## 현재 상태
사용자 결정: Phase 8(WaterMonster 마이그레이션) 완료 후 WaterSpirit / TutorialBoss / WaterMonster
**3종을 한 번에 일괄 검증**할 예정. 지금은 검증 보류 중 — 사용자가 직접 요청할 때까지 진행하지 않는다.

## 재개 방법
사용자가 준비되면 위 두 Check.md 를 열어 Unity Play 모드에서 체크리스트를 확인하고,
`.planning/phases/08-watermonster-combatstate/Check.md` 의 WaterMonster 체크리스트와 함께 진행한다.
