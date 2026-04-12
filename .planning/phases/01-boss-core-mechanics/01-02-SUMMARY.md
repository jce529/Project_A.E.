# Phase 1 Step 2 Summary: 물괴물 엔티티 및 컨트롤러 구축

## 구현 내용
- **WaterMonsterStats.cs**: `BossStatsSystem`을 상속받아 물 속성 힐링(`TakeDamage` 오버라이드)과 공격 시 HP 소모(`SpendHpCost`) 로직을 구현했습니다. HP 소모 시 최소 1 HP를 보장하여 보스가 스스로 죽지 않게 설계되었습니다.
- **WaterMonsterController.cs**: `BossController`를 상속받아 `Update` 중 상태 교체(Swap) 방식을 통해 일반 `CombatState`를 `WaterMonsterCombatState`로 강제 전환하도록 구현했습니다. 이를 통해 그로기 버그를 해결했습니다.
- **WaterMonsterCombatState.cs**: `ShouldTransitionToGroggy`가 항상 `false`를 반환하도록 오버라이드하여 배리어 소진 시에도 전투를 지속하게 했습니다.
- **BossController.cs 수정**: 상속을 지원하기 위해 `Update`를 `protected virtual`로, `_currentState`를 `protected`로 변경하고 `CurrentState` 프로퍼티를 노출했습니다.

## 검증 결과
- 물괴물 전용 상속 구조가 명확하게 구축되었습니다.
- 기존 NewBoss 시스템을 최대한 재사용하면서 필요한 부분만 오버라이드했습니다.
