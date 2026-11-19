# Phase 1 Step 4 Summary: 보스 공격 패턴 및 힐 피드백 구현

## 구현 내용
- **공격 패턴 구현**:
    - **WaterMeleeSwipe.cs**: 근접 휘두르기 패턴 (MaxHP 3% 소모, 10 데미지).
    - **WaterRangedSpit.cs**: 원거리 물탄 발사 패턴 (MaxHP 5% 소모, 투사체 소환).
    - **WaterSpitProjectile.cs**: 직선 이동 투사체. 플레이어만 타격하며 5초 후 소멸합니다.
- **상태 로직 연결**: `WaterMonsterCombatState`에서 `SelectAttackStrategy`를 오버라이드하여 거리가 3.0 이하일 때 근접, 그 이상일 때 원거리 공격을 수행하도록 설정했습니다.
- **힐 피드백 시스템**:
    - **HealPopup.cs & HealPopupSpawner.cs**: 힐링 발생 시 팝업을 표시하는 유틸리티를 구축했습니다.
    - **WaterMonsterStats.cs**: `OnHealed` 오버라이드를 통해 힐 발생 시 팝업 스포너를 호출하도록 연결했습니다.

## 검증 결과
- 보스의 공격 및 자가 HP 소모 로직이 완성되었습니다.
- 힐링 발생 시 플레이어가 명확하게 수치를 확인할 수 있는 피드백 체계가 마련되었습니다.
