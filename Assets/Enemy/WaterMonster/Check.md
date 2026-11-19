# WaterMonster — 패턴 판단 로직 Play 모드 검증 체크리스트

**상태: 보류** — Phase 8 마이그레이션 결과이며, WaterSpirit / TutorialBoss / WaterMonster
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

## 검증 항목

- [ ] **D-02 랜덤성**: 30초 이상 교전 → `[WaterMonsterCombatState] 패턴 선택:` 로그가 고정 순서로
      반복되지 않는다. Stop 후 재시작한 두 번째 시퀀스가 첫 번째와 다르다.
- [ ] **D-01 가중치 감쇠(완전배제 아님)**: 같은 패턴명이 연속 두 줄로 나오는 경우가 **드물게라도
      발생한다**(0.5배 감쇠이지 금지가 아님). 다만 연속 발생 빈도가 눈에 띄게 낮다.
- [ ] **D-04 WaterWavePush 45초 잠금**: `패턴 선택: WaterWavePush`가 찍힌 뒤 최소 45초 동안
      다시 등장하지 않는다 (비광폭화 상태에서 측정).
- [ ] **D-03/D-04b 광폭화 배율**: 광폭화 진입 후 `WaterWavePush` 재등장 간격이 약 22.5초로
      줄어들고, 전반적인 `패턴 선택:` 로그 간격도 짧아진다.
- [ ] **D-06a 거리 분기**: 플레이어가 보스에 붙어 있을 때(`거리:` 3.0 이하) `WaterRangedSpit`이
      선택되지 않고, 멀리 떨어졌을 때(3.0 초과) `WaterMeleeSwipe`/`WaterJumpLand`가
      선택되지 않는다.
- [ ] **D-06b/c 페이즈 전환 반영**: 페이즈 1에서 `WaterPrisonAttack`만 등장 → HP 임계로 페이즈 2
      진입 직후부터 `WaterPrisonMapAoe`로 교체 → 페이즈 3 진입 직후부터 `WaterColorPrison`로 교체.
      전투 상태를 나갔다 들어오지 않아도 즉시 바뀌어야 한다(Enter 캐싱 없음).
- [ ] **D-05 사전 가드 회귀 없음**: 광폭화 중 장판이 기존과 동일하게 스폰되고, 파괴 불가 웅덩이
      2개 이상 + 텔레포트 쿨다운 충족 시 `WaterTeleportState`로 전환된다.
- [ ] **후보 고갈 스팸 없음**: `사용 가능한 패턴 후보 없음` 로그가 프레임마다 쏟아지지 않는다.
- [ ] **WaterSpirit 회귀 없음**: `SpiritCombatState`의 연속 사용 금지가 여전히 **완전배제**로
      동작한다 (같은 패턴명 연속 두 줄이 단 한 번도 나오지 않는다) — Plan 08-01의 헬퍼 확장이
      기본값으로 기존 동작을 유지하는지 확인하는 회귀 항목.

## 결과 기록

(검증 완료 시 여기에 관찰된 패턴 시퀀스 로그 원문과 각 항목 PASS/FAIL을 기록한다.)
