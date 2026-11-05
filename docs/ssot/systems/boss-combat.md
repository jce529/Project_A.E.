# 보스 전투

## 책임

`BossController` 계층은 타깃 탐지, 이동, 상태 전환, 공격 전략 실행과 보스별 페이즈를 소유한다. 플레이어 피해 처리와 저장 파일 형식은 각각 플레이어/공통 피해 시스템과 `SaveLoadManager`가 소유한다.

## 런타임 구조

- 공통 상태 계약은 `IBossState.Enter/Execute/Exit`, 공격 계약은 `IAttackStrategy.ExecuteAttack`이다.
- `BossController`는 `IdleState`에서 시작하고 시야/공격 거리, barrier 피해, 물 고갈 이벤트에 따라 chase/combat/counter/groggy 상태로 전환한다.
- `TutorialBossController`는 전용 idle/attack/groggy/dead 상태와 tentacle/root 패턴을 사용하고 `HP.OnDeath`를 구독한다.
- `SpiritController`/`SpiritStats`는 체력 50%에서 stage 2를 한 번 활성화하고, 분신은 피해를 무시한다.
- `WaterMonsterController`/`WaterMonsterStats`는 Phase 23 기획 문서 기준 3페이즈 보스다. 체력 70% = 2페이즈, 50% = 3페이즈, 30% = 3페이즈 강화(`IsEnraged`)다. 임계치를 넘으면 `_pendingPhase`만 기록하고, 실행 중 패턴(`ActivePatternCount`)이 끝난 뒤 `WaterMonsterGroggyState`(그로기 3.0초 → `PhaseTransition` 1.5초)를 거쳐 `CompletePhaseTransition`으로 `CurrentPhase`를 올린다(그로기가 전환보다 먼저). 30% 강화는 기본적으로 그로기 없이 즉시 적용된다(`_groggyOnEmpower = false`).
- WaterMonster 패턴 후보는 `WaterMonsterCombatState.BuildCandidates`가 매 판단마다 누적 구성한다.

  | 페이즈 | 후보(누적) |
  |---|---|
  | 1 | `WaterGeyser`, `WaterBurst`, `WaterPrisonAttack`, `WaterJumpLand`(거리 3.0 이하, 몸이 비었을 때) |
  | 2 | 1페이즈 + `WaterPrisonMapAoe`, `WaterPuddleSpawn`(최대 개수 미만), `WaterPuddleExplode`(생성 10초 지난 웅덩이 있고 폭발 중 아님), `WaterPuddleMove`(몸이 비었고 8초 쿨다운, 이동 대상 2개 이상) |
  | 3 | 2페이즈 + `WaterColorPrison`, `WaterWavePush`(재사용 잠금 45초) |

  1·2페이즈는 패턴 단일 사용(실행 중이면 판단하지 않음), 3페이즈는 겹침 허용(동시 실행 상한 `Phase3MaxConcurrentPatterns` = 2, 겹침 중에는 점프 착지·웅덩이 이동 제외). 쿨다운 배율은 1·2페이즈 1, 3페이즈 0.75, 강화 0.5이며 선택 시 1회만 적용된다. 패턴 이력(`PatternHistory`)은 보스가 보유해 CombatState 재진입 후에도 유지된다. 3페이즈 피해/범위/전조 배율은 x1.2/x1.15/x0.85, 강화는 x1.4/x1.3/x0.7(물 솟아오르기·점프 착지·물 확산 폭발에만 적용).
- 웅덩이 시스템 `WaterMonster.Phase2.WaterMonsterPuddleSystem`은 씬 배치 없이 런타임에 만든다. 보스 `Start`/`OnEnable`이 `WaterMonsterPuddleSystem.Ensure` → `Bind`를 호출하고, 다른 보스로 재바인딩하거나 바인딩된 보스가 비활성/파괴되면 `Unbind`가 폭발 취소(`PuddleExplosionController.CancelAll`)와 웅덩이 회수(`PuddlePool.ReturnAll`)를 한다. 2페이즈 진입(`PhaseEntered`) 시 웅덩이 3개 생성과 플레이어 `PlayerAbsorb` 부착, 최대 6개, 생성 패턴당 2개, 생성 10초 후 폭발 대상(경고 2.0초, 반경 2.5, 피해 50). 플레이어 기본 공격 판정(`PlayerAttackDamager`)이 마시지 않은 웅덩이에 닿으면 제거되고, 마신 웅덩이(`SetIndestructible`)는 제거되지 않는다.
- Animator 트리거는 `WaterMonsterPatterns.AllTriggers` 12종이다: `Attack_Geyser`, `Attack_Jump`, `Attack_Burst`, `Attack_Prison`, `Attack_PrisonAoe`, `Attack_PuddleSpawn`, `Attack_PuddleMove`, `Attack_PuddleExplode`, `Attack_ColorPrison`, `Attack_Wave`, `Groggy`, `PhaseTransition`. `TrySetTrigger`는 컨트롤러에 없는 파라미터를 건너뛴다(현재 `WaterMonster.controller`에는 `Attack_Melee`/`Attack_Ranged`만 있음).
- 비활성(파일 유지): 자가 HP 소모(`WaterMonsterStats.SpendHpCost`/`SetEnraged` 외부 호출처 없음), `WaterMeleeSwipe`·`WaterRangedSpit`(후보 제외), 장판(`SpeedUpZone`/`SlowDownZone`, `SpawnRandomZone` 호출처 없음), `PuddleSpawner`, `WeatherController`(연결된 경우에만 비). 회복 규칙은 그대로다: Normal/WaveSlash 외 피해 타입은 보스를 회복시키며 HP 소모 비용은 최소 1 HP로 clamp된다.
- `BattleArea`(`_battleAreaBounds`)는 `Awake`에서 보스에서 분리돼 플레이어 공격 판정에 끼지 않고, 보스 활성/비활성을 따라간다.

## 흐름과 종료

1. controller가 타깃 거리를 갱신하고 현재 상태의 `Execute`를 호출한다.
2. combat 계열 상태가 후보 공격 전략을 선택해 실행한다.
3. 통계 component가 피해/물 고갈 이벤트를 보내 상태 전환 또는 페이즈 전환을 유발한다.
4. 사망 시 tutorial boss는 `HP.OnDeath` handler에서, water spirit/monster는 stats `Die` override에서 보스 ID를 저장한다.
5. water spirit/monster는 사망 GameObject를 비활성화한다. tutorial boss는 전용 dead state로 진입한다.

## Unity wiring

- `TutorialBossController` GUID는 `Assets/Tutorial Boss.prefab`과 `Assets/Scenes/1 stage.unity`에서 확인된다.
- `WaterMonsterController`는 `Assets/Enemy/WaterMonster/WaterMonster.prefab`에만 있고 스테이지 씬에는 배치되지 않았다. Phase 23 검증은 `Assets/Scenes/TestScene.unity`에서 프리팹을 런타임 Instantiate 해 수행한다(`Assets/Enemy/WaterMonster/Check.md` Phase 23 섹션).
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
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:18`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:45`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:62`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:72`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:115`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:157`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:169`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:210`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs:235`
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs:42`
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs:79`
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs:117`
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterGroggyState.cs:30`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterPatterns.cs:19`
- `Assets/Enemy/WaterMonster/Script/Phase2/WaterMonsterPuddleSystem.cs:25`
- `Assets/Enemy/WaterMonster/Script/Phase2/WaterMonsterPuddleSystem.cs:48`
- `Assets/Enemy/WaterMonster/Script/Phase2/WaterMonsterPuddleSystem.cs:58`
- `Assets/Enemy/WaterMonster/Script/Phase2/WaterMonsterPuddleSystem.cs:74`
- `Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs:27`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs:44`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs:73`
- `Assets/Tutorial Boss.prefab`
- `Assets/Scenes/1 stage.unity`

## 검증

`partial` — 커밋 `4392d3ee4320ad620b323a866734ab1253d0800b`. 상태 계약과 주요 페이즈/사망 경로는 소스로 확인했으나 일부 보스 배치 및 serialized reference는 확인되지 않았다.

