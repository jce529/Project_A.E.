# WaterMonster — 패턴 판단 로직 Play 모드 검증 체크리스트

**상태: 08-03 Play 검증 완료(2026-11-05), 8/9 PASS · 1 수동 확인 대기(D-05 텔레포트)** — Phase 8 마이그레이션 결과이며, WaterSpirit / TutorialBoss / WaterMonster
3종을 `.planning/phases/08-watermonster-combatstate/08-03-PLAN.md` 체크포인트에서
한 번에 일괄 Play 모드 검증할 예정이다.

## 검증 대상 변경사항

`WaterMonsterCombatState.SelectAttackStrategy`의 수작업 `List<IAttackStrategy>` + `Random.Range`
풀-랜덤을 제거하고, `CombatState`의 범용 헬퍼(`PatternCandidate` + `SelectWeightedPattern`)를
이용한 조건부 가중치 랜덤 선택으로 교체했다.

| 패턴 | 가중치 | 거리 조건 | 재사용 잠금 |
|---|---|---|---|
| WaterGeyser | 1.0 | 없음 | 3s |
| WaterWavePush | 1.0 | 없음 | **45s (cooldownOverride)** |
| WaterMeleeSwipe | 1.0 | `dist <= 3.0` | 1.4s |
| WaterJumpLand | 1.0 | `dist <= 3.0` | 4s |
| WaterRangedSpit | 1.0 | `dist >= 3.0` | 2.0s |
| WaterPrisonAttack | 1.0 | 없음 (페이즈 1 전용) | 5s |
| WaterPrisonMapAoe | 1.0 | 없음 (페이즈 2 전용) | 8s |
| WaterColorPrison | 1.0 | 없음 (페이즈 3 전용) | 10s |

추가로: 직전 사용 패턴은 완전배제가 아니라 가중치 0.5배 감쇠(D-01), 광폭화 중 모든 개별
쿨다운 0.5배(D-03, WavePush 45s → 22.5s), 장판 스폰/텔레포트 사전 가드는 변경 없음(D-05).
`CombatState.cs` / `SpiritCombatState.cs` / `Stage2CombatState.cs`는 이 플랜에서 수정하지 않았다.

## 사전 준비

1. Unity 에디터에서 컴파일 에러 0건 확인
2. 물괴물 보스 씬 열기
3. Console Collapse를 끄고 검색창에 `WaterMonsterCombatState` 입력해 필터링

## 정적 회귀 검사 (2026-10-31, 08-03 Task 1 — 소스 grep, Play 모드 아님)

| # | 검사 | 판정 |
|---|---|---|
| A | `SpiritCombatState.cs` 의 2-인자 호출 `SelectWeightedPattern(dist, _candidates);` 유지 | PASS |
| B | `CombatState.cs` 에 구 완전배제 `if (LastUsedPatternType == c.StrategyType) continue;` 0건 | PASS |
| C | `CombatState.cs` 에 `if (lastUsedWeightMultiplier <= 0f) continue;` 존재 | PASS |
| D | `float lastUsedWeightMultiplier = 0f)` 기본값(완전배제) 유지 | PASS |
| E/F | `_lastWaveTime` 이 `CombatState.cs` / `WaterMonsterCombatState.cs` 양쪽에 0건 | PASS |
| G/H | `WaveAttackCooldown = 45f` 및 `cooldownOverride: WaveAttackCooldown` | PASS |
| I | `CanSpawnZone()`(74행)·`CanTeleport()`(84행) 가 `BuildCandidates(wmc)`(95행)보다 위 | PASS |
| J/K | `baseCooldown * GetPatternCooldownMultiplier()` / `_isEnraged ? _enrageCooldownMultiplier : 1f;` | PASS |
| L | Phase 8 커밋(`(08-`)이 바꾼 `.cs` 가 `CombatState.cs`, `WaterMonsterCombatState.cs` 2개뿐 | PASS |
| M | 세 보스 `Check.md` 파일 존재 | PASS |

정적 12/12 PASS. 아래 Play 모드 항목은 **전부 미검증** 상태다.

## 검증 항목

- [x] **D-02 랜덤성**: 30초 이상 교전 → `[WaterMonsterCombatState] 패턴 선택:` 로그가 고정 순서로
      반복되지 않는다. Stop 후 재시작한 두 번째 시퀀스가 첫 번째와 다르다.
- [x] **D-01 가중치 감쇠(완전배제 아님)**: 같은 패턴명이 연속 두 줄로 나오는 경우가 **드물게라도
      발생한다**(0.5배 감쇠이지 금지가 아님). 다만 연속 발생 빈도가 눈에 띄게 낮다.
- [x] **D-04 WaterWavePush 45초 잠금**: `패턴 선택: WaterWavePush`가 찍힌 뒤 최소 45초 동안
      다시 등장하지 않는다 (비광폭화 상태에서 측정).
- [x] **D-03/D-04b 광폭화 배율**: 광폭화 진입 후 `WaterWavePush` 재등장 간격이 약 22.5초로
      줄어들고, 전반적인 `패턴 선택:` 로그 간격도 짧아진다.
- [x] **D-06a 거리 분기**: 플레이어가 보스에 붙어 있을 때(`거리:` 3.0 이하) `WaterRangedSpit`이
      선택되지 않고, 멀리 떨어졌을 때(3.0 초과) `WaterMeleeSwipe`/`WaterJumpLand`가
      선택되지 않는다.
- [x] **D-06b/c 페이즈 전환 반영**: 페이즈 1에서 `WaterPrisonAttack`만 등장 → HP 임계로 페이즈 2
      진입 직후부터 `WaterPrisonMapAoe`로 교체 → 페이즈 3 진입 직후부터 `WaterColorPrison`로 교체.
      전투 상태를 나갔다 들어오지 않아도 즉시 바뀌어야 한다(Enter 캐싱 없음).
- [ ] **D-05 사전 가드 회귀 없음**: 광폭화 중 장판이 기존과 동일하게 스폰되고, 파괴 불가 웅덩이
      2개 이상 + 텔레포트 쿨다운 충족 시 `WaterTeleportState`로 전환된다.
- [x] **후보 고갈 스팸 없음**: `사용 가능한 패턴 후보 없음` 로그가 프레임마다 쏟아지지 않는다.
- [x] **WaterSpirit 회귀 없음**: `SpiritCombatState`의 연속 사용 금지가 여전히 **완전배제**로
      동작한다 (같은 패턴명 연속 두 줄이 단 한 번도 나오지 않는다) — Plan 08-01의 헬퍼 확장이
      기본값으로 기존 동작을 유지하는지 확인하는 회귀 항목.

## 결과 기록

2026-11-05, 08-03 Task 2. Unity CLI(eval) + Play 모드로 실측. WaterMonster 는 어느 씬에도 배치돼 있지 않아
빈 씬에 `WaterMonster.prefab` + `Player.prefab` + 바닥 콜라이더를 런타임에 생성했고, 에디터 샘플러가
프레임마다 (상태, 현재 공격, 거리, 페이즈/광폭화 플래그, HP)를 변화 시점에 기록했다. 플레이어는 보스 x 기준
고정 거리(근접 2.0 / 원거리 3.3)에 매 프레임 두고 HP 를 회복시켰다. 씬·Assets 변경 없음(`dirty=False`, 원래 `1 stage` 씬 복구).
소스에는 `패턴 선택:` 로그가 없어(체크리스트 문구와 다름) 로그 대신 샘플러의 `atk=` 전환을 "패턴 선택"으로 취급했다.
로그 원문: 총괄 노트 폴더 `p8_03_run1_watermonster_log.txt`(근접→원거리, 1페이즈, 244줄),
`p8_03_run2_watermonster_log.txt`(HP 단계 1000→650→450→250, 154줄), `p8_03_run3_enrage_log.txt`(광폭화 단독).

| 항목 | 판정 | 증거 |
|---|---|---|
| D-02 랜덤성 | PASS | 두 런 첫 12패턴이 다르다. Run1 `WavePush,JumpLand,Geyser,JumpLand,Geyser,JumpLand,PrisonAttack,PrisonAttack,MeleeSwipe,JumpLand,MeleeSwipe,JumpLand` / Run2 `Geyser,WavePush,JumpLand,MeleeSwipe,Geyser,JumpLand,PrisonAttack,JumpLand,JumpLand,PrisonAttack,MeleeSwipe,PrisonAttack` |
| D-01 감쇠(완전배제 아님) | PASS | 같은 패턴 연속: Run1 16/132(12%), Run2 12/83(14%), Run3(광폭화) 50/354(14%). 0 이 아니고(금지 아님) 균등 랜덤보다 낮다. 예: `43.90 PrisonAttack → 48.90 PrisonAttack` |
| D-04 WavePush 45초 잠금 | PASS | 비광폭화 재등장 간격(초): 51.6, 46.0, 81.5(Run1; 이후 201.98→247.89 의 45.9 는 광폭화 중이라 22.5 이상이면 정상) / 50.0, 48.1(Run2). 최소 46.0 ≥ 45 |
| D-03/D-04b 광폭화 배율 | PASS | Run3 WavePush 간격 22.50, 22.51, 22.50, 22.50 (2.44→24.94→47.45→69.95, 73.57→96.07). 패턴 시작 평균 간격 0.35초(비광폭화 약 3~5초) |
| D-06a 거리 분기 | PASS | 근접(dist≤3.0) 30회 중 RangedSpit 0회, 원거리(dist 3.32) 103회 중 MeleeSwipe/JumpLand 0회 |
| D-06b/c 페이즈 전환 | PASS | Run2: P2 전환(t=46.76) 직후 PrisonAttack 0회, 51.24 첫 `WaterPrisonMapAoe`. P3 전환(t=83.58) 후 MapAoe 0회, 96.87 `WaterColorPrison`. 전투 상태 이탈 없이 반영 |
| D-05 사전 가드 | 수동 확인 대기 | 광폭화 장판 스폰은 Run3 에서 구분 불가(ColorPrison 존과 이름 혼재). 텔레포트는 `PuddleStackManager` 를 임시 생성해 `_indestructibleCount=2` 로 강제했더니 보스가 패턴 선택을 멈추고 `WaterMonsterCombatState` 에 머물렀다(가드가 매 프레임 작동했으나 실제 웅덩이가 없어 `WaterTeleportState` 정상 이동은 확인 못함). 실제 웅덩이 2개 이상 상황에서 사람이 확인 필요 |
| 후보 고갈 스팸 없음 | PASS | 소스에서 해당 로그가 제거돼 있어 스팸 불가. 정상 런에서 5초 이상 멈춘 구간 없음(Run1/2/3) |
| WaterSpirit 회귀 없음 | PASS(참조) | phase-07-verify(07-03) 측정: 165회 패턴 실행에서 연속 동일 패턴 0건. 이 세션에서 중복 검증하지 않음 |

### 관찰 사항(판정 외)
- **WavePush 잠금은 `CombatState` 재진입 시 초기화된다.** Run3 `69.95 WavePush → ChaseState(73.47) → 73.57 WavePush`(3.6초 만에 재사용). 07-03 의 `PatternHistory` 기본값이 "Enter 마다 새 이력"이라 WaterMonster 는 Chase 를 거치면 45초/22.5초 잠금이 사라진다. 08-02 이전 `_lastWaveTime` 도 상태 인스턴스 필드였으므로 마이그레이션 회귀는 아닌 것으로 보이나, 이전 빌드와 직접 비교는 하지 않았다.
- 광폭화 중 `WaterMeleeSwipe` 가 178/355(50%)로 지배적이다(자가 쿨다운 1.4s×0.5 가 가장 짧아 가장 자주 후보가 됨, 08-02 Pitfall 6 의 자연스러운 결과). 밸런스 이슈 여부는 사람이 판단.
- 광폭화 `WaterColorPrison` 24회 시전 후 `WaterColorPrisonZonePrefab(Clone)` 이 136개 남았다. 원인은 이 테스트 씬에서 `PlayerStats.TakeDamage`(PlayerStats.cs:75)가 NullReference 를 던져 `ColorPrisonSequence` 코루틴이 `Destroy` 정리 전에 중단된 것으로 추정된다(테스트 씬 UI 참조 부재). 실제 보스 씬에서의 누수 여부는 미확인이며, 예외 시 정리 코드가 건너뛰어지는 구조는 별도 이슈 후보.
- `[CombatState] 애니메이션 타임아웃으로 공격 강제 종료` 경고가 다수 찍혔다(테스트 씬의 애니메이터 상태 부재로 추정, 기존 동작).
- 측정 중 자동저장이 `%LocalLow%/DefaultCompany/AQUA ECLIPSE/save.json` 에 세이브를 썼다.

## Phase 23 — 기획 문서 기준 재구현 검증 (TestScene, Unity CLI)

2026-11-05, 23-06 Task 1~3. `Assets/Scenes/TestScene.unity` 에서 Play 진입 후 `WaterMonster.prefab` 을 플레이어(-6, 0.22) 오른쪽 2칸(x=-4)에 런타임 Instantiate 했다.
Unity CLI `run_script`(Roslyn, 리플렉션)로 PlayerLoop `PreLateUpdate` 맨 앞에 샘플러/시험 실행기를 끼워 매 프레임 상태를 읽고 값이 바뀐 시점만 기록했다(로그: `.planning/tmp/p23/P<n>.log`, 판정 요약: `.planning/tmp/p23/results.log`, 스크립트: `.planning/tmp/p23/p23lib.cs.txt`).
플레이어 HP 는 리플렉션으로 1,000,000 으로 두고 피해는 감소량으로 측정했다. 특정 패턴 시험은 보스를 `IdleState`(VisionRange -1)로 묶고 `new <전략>().ExecuteAttack(boss)` 로 강제 호출했다. 시간 허용오차 ±0.06초(그로기 단계는 계획대로 ±0.1초).
코드·프리팹·씬·애니메이터 변경 없음. TestScene 은 저장하지 않았다.

### 판정 규칙
- **필수 자동 항목** S1~S5, P1~P22: 반드시 `PASS`. 하나라도 `FAIL` 이면 그 task 는 미완료다. "수동 확인 대기"로 바꿔 넘기는 것은 금지(자동 측정이 불가능해진 경우에도 `FAIL(측정 불가: 원인)` 으로 기록).
- **수동 항목** V1~V6(Task 4): 시각/조작감처럼 사람만 판단할 수 있는 것만. 이 항목만 "수동 확인 대기"가 허용되고 Task 2/3 완료를 막지 않는다.
- **자연 플레이 표본**(무작위 선택)은 "집합 포함·동시 실행 수·간격"처럼 운에 좌우되지 않는 조건만 판정한다. 특정 패턴의 등장 횟수는 참고값으로만 기록하고, 특정 패턴 동작은 강제 호출로 판정한다.

### Unity 컴파일
`AssetDatabase.Refresh()` 후 `EditorApplication.isCompiling=False`, `EditorUtility.scriptCompilationFailed=False`, `console_status` groundTruth `compilationFailed=false`, `consoleErrors=0`(Editor Console 의 `error CS` 0건). 신규 타입 로드 확인: `WaterMonsterGroggyState`, `WaterMonster.Phase2.WaterMonsterPuddleSystem`, `WaterBurst`, `WaterPuddleMove`. (CLI 버퍼에 남은 `WaterPuddleMove.cs(15,30) error CS1729` 1건은 10:48Z 의 23-04 도중 기록이며 이후 재컴파일로 해소됨.)

### 정적 회귀 (Task 1)

| # | 검사 | 결과 |
|---|---|---|
| S1 PASS | `git diff --exit-code 79e9124 -- CombatState.cs SpiritCombatState.cs Stage2CombatState.cs GroggyState.cs` | 출력 없음, exit 0. Spirit 계열 회귀는 이 소스 무변경으로 판정하고 재실측하지 않음 |
| S2 PASS | `git ls-files --error-unmatch` 6개(`WaterMeleeSwipe.cs`, `WaterRangedSpit.cs`, `Phase4/SpeedUpZone.cs`, `Phase4/SlowDownZone.cs`, `PuddleSpawner.cs`, `WeatherController.cs`) | 6/6 존재(D-04 삭제 금지 준수) |
| S3 PASS | `grep -rn "Anim.SetTrigger" Assets/Enemy/WaterMonster/Script` | `WaterMeleeSwipe.cs:16`, `WaterRangedSpit.cs:14` 2건뿐(후보에서 빠진 파일). 나머지는 `WaterMonsterPatterns.TrySetTrigger` 경유 |
| S4 PASS | `WaterMonsterPatterns.AllTriggers` 12개 ↔ 아트 패턴 1~10 + 그로기/전환 | 1 `Attack_Geyser`, 2 `Attack_Jump`, 3 `Attack_Burst`, 4 `Attack_Prison`, 5 `Attack_PrisonAoe`, 6 `Attack_PuddleSpawn`, 7 `Attack_PuddleMove`, 8 `Attack_PuddleExplode`, 9 `Attack_ColorPrison`, 10 `Attack_Wave`, `Groggy`, `PhaseTransition` — 1:1 |
| S5 PASS | `git log 79e9124..HEAD --name-only` 에 Animations/·`WaterMonster.prefab`·`Assets/Scenes/`·`Assets/Enemy/WaterSpirit/`·`Assets/Player.prefab` | 0건 |

참고: `WaterMonster.controller` 에는 아직 `Attack_Melee`/`Attack_Ranged` 트리거만 있다. 새 트리거 12종은 `TrySetTrigger` 가 파라미터 존재를 확인한 뒤에만 호출하므로 경고 없이 건너뛴다(P15). 애니메이터 연결은 아트 작업 대기.

### 필수 자동 항목 — Task 2 (P1~P15)
- [x] P1 PASS — (D-02/D-03 자연 표본, 1페이즈 60s) 선택 10회 ⊆ {Geyser, Burst, JumpLand, PrisonAttack}, 집합 밖 0, `ActivePatternCount` 최대 1, 연속 선택 최소 간격 5.463s(≥1). 횟수(참고) JumpLand 2, Burst 3, PrisonAttack 4, Geyser 1. 로그 `P1.log`
- [x] P2 PASS — (D-02 결정적) `BuildCandidates` 리플렉션: 1P = {Burst, Geyser, JumpLand, PrisonAttack}; 2P(웅덩이 3, 11s 전 1개, IsActive, CanSpawnMore, CanTeleport, apc 0) = 1P ∪ {PrisonMapAoe, PuddleSpawn, PuddleExplode, PuddleMove}; 3P(apc 0) = 2P ∪ {ColorPrison, WavePush}; 3P ColorPrison 강제 실행 중(apc 1) = JumpLand·PuddleMove 제외 8종. MeleeSwipe/RangedSpit 전 경우 없음. 로그 `P2.log`
- [x] P3 PASS — (D-01/D-05) HP→69%: 그로기 진입 +0.008s(실행 중 패턴 종료 후), 진입 시 `CurrentPhase=1`(그로기가 전환보다 먼저), `PhaseTransition` 단계 +3.005s(3.0), `CurrentPhase==2` +4.503s(4.5), 직후 `PuddlePool.ActiveCount=3`, `PlayerAbsorb` 부착. 두 번 측정(1차 +3.018/+4.503 동일 PASS). 로그 `P3.log`
- [x] P4 PASS — (D-02/D-03 자연 표본, 2페이즈 90s) 선택 7회 ⊆ 2페이즈 집합, `ActivePatternCount` 최대 1, 최소 간격 1.460s. 횟수(참고) PuddleMove 3, Burst 2, Geyser 1, PuddleSpawn 1. 로그 `P4.log` (첫 실행 `P4_contaminated_run1.log` 은 다른 사용자의 Play 조작으로 보스가 피해를 받아 무효 처리, 재측정함)
- [x] P5 PASS — (D-03 결정적 단일/겹침) 전제: `PatternHistory.Clear()` 후 후보 2P 7종·3P 9종, ReadyAt 0건(전부 Time.time 이하). 2P: `WaterPrisonMapAoe` 강제 + `_decisionTimer=0` → 패턴 종료(+3.709s)까지 apc 최대 1. 3P: `WaterColorPrison` 강제 + `_decisionTimer=0` → +0.010s 에 apc==2, 이후 5s 최대 2. 존 개수 +0.000:4 → +0.010:8(두 번째 패턴도 ColorPrison) → +3.502:4 → +3.518:0 (한 패턴 정리가 다른 패턴 존을 먼저 지우지 않음). 로그 `P5.log`
- [x] P6 PASS — (D-06 폭발) 웅덩이 3개 중 1개만 SpawnTime-11s, 플레이어를 그 위에 → `WaterPuddleExplode` 강제 후 +2.003s 에 HP -50, 대상 웅덩이 비활성, 나머지 2개(나이<10s) 유지. 로그 `P6.log`
- [x] P7 PASS — (D-06 이동) `WaterPuddleMove` 강제 → `WaterTeleportState`, +1.124s 종료 후 보스 x=5.841 = `TrySelectMove` exit x 5.841(차 0.000), 보스 HP 1000→1000. 이후 자연 진행 20s `WaterTeleportState` 진입 0회(≤3). 로그 `P7.log`
- [x] P8 PASS — (빈 대상 루프) `PuddlePool.ReturnAll()` 후 `ChangeState(new WaterTeleportState())` → 같은 프레임 `WaterMonsterCombatState`, 이후 10s 진입 0회, 보스 HP 690→690. 로그 `P8.log`
- [x] P9 PASS — (D-01) HP→49%: 그로기 진입 +0.009s(진입 시 phase 2 유지) → `PhaseTransition` +3.001s, `CurrentPhase==3` +4.502s. 로그 `P9.log`
- [x] P10 PASS — (D-01/D-04) HP→29%: 다음 프레임(+0.009s) `IsEnraged=True`, `EnhanceDamageMul=1.4`, `PatternCooldownMultiplier=0.5`, 그로기 진입 0회. 피해 없이 10s 보스 HP 290→290(자가 HP 소모 없음). 로그 `P10.log`
- [x] P11 PASS — (D-12 견고화) `WaterColorPrison` 시전 직후 존 4개, 1.5s 뒤 보스 `SetActive(false)`, 5s 뒤 `ActiveZones.Count=0`, 씬 `WaterColorPrisonZone` 0개. 로그 `P11.log`
- [x] P12 PASS — (BattleArea) parent==null. `attackBox` 인스턴스 `PlayerAttackDamager.OnTriggerEnter2D(BattleArea)` → 보스 HP 1000→1000, `(HitBox)` → 990(공격력 10). 보스 off → BattleArea.activeSelf False, on → True. 로그 `P12.log`
- [x] P13 PASS — (D-06 제거/마시기) (a) 웅덩이 쪽 `OnTriggerEnter2D(attackBox Collider2D)` → 파괴 가능 웅덩이 비활성, `SetIndestructible()` 웅덩이 유지. (b) 실제 2D 물리: attackBox 를 웅덩이 위치에 생성, FixedUpdate 3회 후 비활성 [웅덩이 Rigidbody2D=Kinematic(Awake 추가)·CircleCollider2D trigger, attackBox Rigidbody2D 없음·BoxCollider2D trigger, 레이어 Default/PlayerHitBox 충돌 허용]. (c) `PlayerAbsorb.Absorb(puddle,true)=True` → `isDestructible=False`, `IndestructibleCount` 1→2. 로그 `P13.log`
- [x] P14 PASS — (D-06 수명/재연결) (1) 2P 보스 off → ActiveCount 0, BoundBoss null, IsActive False, IsExploding False. (2) Destroy 후 새 보스 → 다음 프레임 BoundBoss==새 보스, HP 69% → 그로기 후 phase 2, ActiveCount 3. (3) off→on 후 BoundBoss==그 보스. (4) A 폭발 경고 중 +0.5s 에 보스 B 생성 → 다음 프레임 BoundBoss==B, IsExploding False, ActiveCount 0, A.ActivePatternCount 0, 원래 폭발 시각 ±0.5s 플레이어 HP 변화 0. (5 보완) B 2P 웅덩이 3개·폭발 중에 A off → ActiveCount 3·IsExploding True 유지. 로그 `P14.log`
- [x] P15 PASS — 측정 세션 Console 에 `Parameter 'Attack_*'`/`'Groggy'`/`'PhaseTransition'` 경고 0건(재시작 후 에디터 버퍼 23건 전수 확인, 예외는 테스트 씬 기존 `CameraController target` UnassignedReference 뿐). 로그 `P15.log`, `console_after.json`

### 필수 자동 항목 — Task 3 (P16~P22, 1페이즈 기준, t0 = 강제 호출 프레임)
- [x] P16 PASS — (D-07) 거리 4: 프리팹 생성 +0.000s, HP 감소 +0.701s, 감소량 20. 3페이즈 재측정 +0.601s(기대 0.595), 24. 로그 `P16.log`
- [x] P17 PASS — (D-08) (a) 반경 안(x-1.5): 상승 시작 +0.510s(0.50), 정점 y 불변 0.411s(0.40), 하강 시작 t_d=+1.170s, 착지 t_d+0.297s(0.30) HP -30, 착지 x -5.500 = 하강 시작 시 플레이어 x -5.500. (b) 반경 밖: 하강 시작(t_d=+1.165s) 감지 즉시 플레이어 x -6.122→-1.622, 착지 중심 -6.122(이동 전 x 와 차 0.000), 거리 4.5>3, 피해 0 → 착지 x 가 하강 시작 시 고정됨(정상). 로그 `P17.log`
- [x] P18 PASS — (D-09) 거리 3: HP 감소 +1.007s, 25, 시작~폭발 보스 |linearVelocity| 최대 0.000. 거리 5: 피해 0, 속도 0. 로그 `P18.log`
- [x] P19 PASS — (D-10) 투사체 생성 +0.601, +1.202. 피격(+0.860s) 후 `PrisonDebuff` 부착, `IsImprisoned=True`, `PlayerAttack.enabled=False`. 감금 후 3.05s HP -9. `OnMash` 14회 → 유지, 15회째 + 1프레임 → `PrisonDebuff` 제거, `IsImprisoned=False`, `PlayerAttack.enabled=True`. (OnMash 는 리플렉션 직접 호출이라 실제 입력 장치에 의존하지 않음) 로그 `P19.log`
- [x] P20 PASS — (D-11) (a) 감옥 안(발사 직후 `PrisonDebuff` 부착): 발사 +0.600/+1.204, 마지막 발사+2.5 시점 60 피해 0건(도트 3 만). (b) 감옥 밖(투사체 생성 즉시 제거): 60 피해 +3.708s(기대 +3.703, 차 0.005). `WaterPrisonMapAoe.AnimationName=Attack_PrisonAoe`, `WaterPrisonAttack.AnimationName=Attack_Prison`. 로그 `P20.log`
- [x] P21 PASS — (D-12) (a) CoreColor=Red, 같은 색 존으로 이동(IsPlayerInside 관측) → 피해 없음, t0+3.6 존 0개. (b) CoreColor=Blue, 다른 색 존 → +3.504s 에 HP -80, t0+3.6 존 0개. 로그 `P21.log`
- [x] P22 PASS — (D-13) BattleArea min.x=-19, max.x=11, size.x/6=5.000s. 5회 모두 첫 파도 +0.000s x=-19.000 dir +1 v=6, 둘째 +5.001~5.003s x=11.000 dir -1 v=9 → 첫 파도 항상 좌측. (첫 실행은 판정 코드가 생성 후 5초 이동한 위치를 읽은 측정 스크립트 버그로 NG 였고, 생성 시점 x 로 고쳐 재측정. 로그상 값은 두 실행 모두 동일) 로그 `P22.log`

### 관찰 사항(판정 외)
- P14(3): 2페이즈 보스를 `SetActive(false)→(true)` 하면 `BoundBoss` 는 다시 그 보스지만 `IsActive=False`, 웅덩이 0개로 남는다(`Bind` 가 `IsActive=false` 로 초기화하고 2페이즈 진입 이벤트는 다시 오지 않음). 재활성된 2·3페이즈 보스는 웅덩이 패턴(생성/폭발/이동) 후보를 잃는다. 비활성→재활성 경로가 실제 게임에 있는지(체크포인트 리로드 등)에 따라 gap 후보.
- P5/P2: 강제 실행한 패턴은 `CommitSelection` 을 거치지 않아 ReadyAt 이 없으므로, 3페이즈에서 같은 ColorPrison 이 바로 한 번 더 선택될 수 있다(P5 존 4→8). 자연 선택에서는 쿨다운으로 막힌다.
- P4: 2페이즈 90s 동안 선택 7회로 판단 간격이 길다(패턴 쿨다운 + 웅덩이 이동 이후 Chase 복귀). 리듬은 V1 에서 사람 확인.
- 측정 중 자동저장이 `%LocalLow%/DefaultCompany/AQUA ECLIPSE/save.json` 에 기록됐다(11:10, 11:13, 11:3x).

### 수동 항목 (Task 4 — 사용자 확인)
- [ ] V1 수동 확인 대기 — 패턴이 하나씩 끝난 뒤 다음 패턴이 나오는 리듬이 1·2페이즈에서 자연스러운가.
- [ ] V2 수동 확인 대기 — HP 690 이하로 낮췄을 때 그로기 정지(약 3초) → 전환(1.5초) 길이가 적절한가.
- [ ] V3 수동 확인 대기 — 바닥 웅덩이 3개의 위치·높이(보스 시작 y)가 바닥에 붙어 보이는가, 마신 웅덩이 색 변화가 구분되는가.
- [ ] V4 수동 확인 대기 — 물 솟아오르기 경고 0.7초가 플레이어가 피할 수 있는 길이로 보이는가.
- [ ] V5 수동 확인 대기 — 3페이즈 패턴 겹침(색깔 감옥 + 다른 패턴)이 읽을 수 있는 수준인가.
- [ ] V6 수동 확인 대기 — 감옥 탈출 연타(점프 15회) 조작감.

### Open Decisions 결과
사용자 응답 대기(Task 4). 기본값 목록은 `23-06-PLAN.md` Task 4 의 OD-1~OD-9.
- OD-1 웅덩이 제거 수단(기본 공격 판정) — 응답 대기
- OD-2 그로기 3.0s / 피해 배율 1.0 / 전환 1.5s — 응답 대기
- OD-3 30% 강화 진입 그로기 없음 — 응답 대기
- OD-4 강화 수치(3P x1.2/x1.15/x0.85, 30% x1.4/x1.3/x0.7) — 응답 대기
- OD-5 물 감옥 조준 2발 — 응답 대기
- OD-6 웅덩이 수치(3/최대 6/패턴당 2/10초) — 응답 대기
- OD-7 3페이즈 동시 실행 상한 2 — 응답 대기
- OD-8 쿨다운 배율 0.75 / 0.5 — 응답 대기
- OD-9 웅덩이 높이 보스 시작 y — 응답 대기
