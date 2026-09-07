# 보스 전투

## 책임

`BossController` 계층은 타깃 탐지, 이동, 상태 전환, 공격 전략 실행과 보스별 페이즈를 소유한다. 플레이어 피해 처리와 저장 파일 형식은 각각 플레이어/공통 피해 시스템과 `SaveLoadManager`가 소유한다.

## 런타임 구조

- 공통 상태 계약은 `IBossState.Enter/Execute/Exit`, 공격 계약은 `IAttackStrategy.ExecuteAttack`이다.
- `BossController`는 `IdleState`에서 시작하고 시야/공격 거리, barrier 피해, 물 고갈 이벤트에 따라 chase/combat/counter/groggy 상태로 전환한다.
- `TutorialBossController`는 전용 idle/attack/groggy/dead 상태와 tentacle/root 패턴을 사용하고 `HP.OnDeath`를 구독한다.
- `SpiritController`/`SpiritStats`는 체력 50%에서 stage 2를 한 번 활성화하고, 분신은 피해를 무시한다.
- `WaterMonsterController`/`WaterMonsterStats`는 70%/50%/30% 체력 임계치로 비, teleport, enrage 기능을 활성화한다. Normal/WaveSlash 외 피해 타입은 보스를 회복시키며 공격 비용은 최소 1 HP로 clamp된다.

## 흐름과 종료

1. controller가 타깃 거리를 갱신하고 현재 상태의 `Execute`를 호출한다.
2. combat 계열 상태가 후보 공격 전략을 선택해 실행한다.
3. 통계 component가 피해/물 고갈 이벤트를 보내 상태 전환 또는 페이즈 전환을 유발한다.
4. 사망 시 tutorial boss는 `HP.OnDeath` handler에서, water spirit/monster는 stats `Die` override에서 보스 ID를 저장한다.
5. water spirit/monster는 사망 GameObject를 비활성화한다. tutorial boss는 전용 dead state로 진입한다.

## Unity wiring

- `TutorialBossController` GUID는 `Assets/Tutorial Boss.prefab`과 `Assets/Scenes/1 stage.unity`에서 확인된다.
- `WaterMonsterController` GUID를 참조하는 `.unity`/`.prefab`은 현재 검색에서 발견되지 않아 배치 상태는 `editor-verification-required`이다.
- 동적 prefab, Animator state name, battle bounds 및 보스별 serialized reference의 완전성은 Play Mode 검증 대상이다.

## 근거

- `Assets/Enemy/NewBoss/Script/BossController.cs:31`
- `Assets/Enemy/NewBoss/Script/BossController.cs:46`
- `Assets/Enemy/NewBoss/Script/BossController.cs:72`
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs`
- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs:134`
- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs:302`
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs:14`
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs:54`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:140`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs:30`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs:72`
- `Assets/Tutorial Boss.prefab`
- `Assets/Scenes/1 stage.unity`

## 검증

`partial` — 커밋 `4392d3ee4320ad620b323a866734ab1253d0800b`. 상태 계약과 주요 페이즈/사망 경로는 소스로 확인했으나 일부 보스 배치 및 serialized reference는 확인되지 않았다.

