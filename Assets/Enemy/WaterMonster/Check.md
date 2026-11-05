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
