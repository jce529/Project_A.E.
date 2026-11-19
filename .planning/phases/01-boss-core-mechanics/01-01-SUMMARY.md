# Phase 1 Step 1 Summary: 데미지 파이프라인 기반 구축

## 구현 내용
- **DamageInfo.cs**: `DamageElement` (None, Water) enum과 `DamageInfo` struct를 생성하여 속성 기반 데미지 전달 모델을 확립했습니다.
- **BossStatsSystem.cs**:
    - `_currentHealth`, `_currentWater`를 `protected`로 변경하여 상속을 지원합니다.
    - `TakeDamage(float)`를 `TakeDamageInfo(DamageInfo)`로 포워딩하도록 리팩토링했습니다.
    - `TakeDamageInfo(DamageInfo)` public 래퍼를 추가하여 외부(플레이어)에서 속성 데미지를 줄 수 있게 했습니다.
    - `Die()`를 `protected virtual`로 변경했습니다.
- **CombatState.cs**:
    - `ShouldTransitionToGroggy` 가상 함수를 도입하여 배리어 소진 시 그로기 전환 여부를 하위 클래스에서 결정할 수 있게 했습니다.

## 검증 결과
- 모든 파일이 유니티 표준을 준수하며 컴파일 오류 없이 작성되었습니다.
- 기존 NewBoss 시스템과의 호환성이 유지됩니다.
