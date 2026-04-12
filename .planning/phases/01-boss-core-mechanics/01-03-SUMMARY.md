# Phase 1 Step 3 Summary: 플레이어 데미지 파이프라인 연결

## 구현 내용
- **WaveSlice.cs**: `DamageElement.Water` 속성을 부여하고, 타격 시 `BossStatsSystem`을 우선 검색하여 보스에게는 속성 데미지(`TakeDamageInfo`)를, 일반 적에게는 기존 데미지(`HP.TakeDamage`)를 주도록 분기 로직을 적용했습니다.
- **PlayerAttack.cs & FlashSlice.cs**: `element` 필드와 `Element` 프로퍼티를 추가하여 공격 시의 속성 정보를 노출했습니다.
- **PlayerAttackDamager.cs & AttackBox.cs**: `OnTriggerEnter2D`를 수정하여 보스를 우선 타격하도록 분기 처리했습니다. 플레이어의 모든 근접/원거리 공격이 보스의 스탯 시스템과 연동됩니다.

## 검증 결과
- 플레이어의 공격 수단이 보스의 새로운 속성 시스템을 완벽하게 인식합니다.
- 잡몹에 대한 데미지 경로는 그대로 유지되어 기존 게임 플레이에 영향이 없습니다.
