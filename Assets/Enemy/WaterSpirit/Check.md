# WaterSpirit — 패턴 판단 로직 Play 모드 검증 체크리스트

**상태: 보류** — WaterMonster 보스가 동일한 CombatState 기반 패턴 판단 로직으로
전환된 뒤, 전체 보스를 한 번에 일괄 Play 모드 검증할 예정이다.
(출처: `.planning/phases/07-boss-attack-pattern-judgment/07-02-PLAN.md`)

## 검증 대상 변경사항

`SpiritCombatState`의 고정 라운드로빈(`_pattern[_patternIndex++ % 5]`)을 제거하고,
`CombatState`의 범용 헬퍼(`PatternCandidate` + `SelectWeightedPattern` + `ForceSelectPattern`)를
이용한 조건부 가중치 랜덤 선택으로 교체했다.

| 패턴 | 가중치 | 거리 조건 |
|---|---|---|
| SpiritCharge | 1.0 | 없음 |
| SpiritExhaustion | 0.6 | 없음 |
| SpiritWakeRepel | 1.0 | `RepelRange`(1.5) 이내만 |
| SpiritFarProjectile | 1.0 | 없음 |

추가로 직전 패턴 연속 사용 금지, 패턴별 쿨다운, `SpiritExhaustion` 직후
`SpiritWakeRepel` 강제 체인이 적용되었다. `Stage2CombatState.cs` /
`WaterMonsterCombatState.cs` / `SpiritController.cs`는 수정하지 않았다.

## 사전 준비

1. Unity 에디터에서 컴파일 에러 0건 확인
2. 물의 정령 보스 씬 열기
3. Console Collapse를 끄고 검색창에 `SpiritCombatState` 입력해 필터링

## 검증 항목

- [ ] **D-03 랜덤성**: Play로 30초 이상 교전 → `[SpiritCombatState] 패턴 선택:` 로그가
      매번 같은 순서로 반복되지 않는다. Stop 후 재시작해 두 번째 시퀀스도 기록
      (첫 번째와 달라야 함).
- [ ] **D-05 연속 사용 금지**: 로그에서 같은 패턴명이 연속 두 줄로 나오지 않는다.
- [ ] **D-04 강제 체인**: `패턴 선택: SpiritExhaustion` 직후 반드시
      `체인 발동: SpiritExhaustion → SpiritWakeRepel (강제)` →
      `[CombatState] 새로운 공격 시작: SpiritWakeRepel` 순서로 찍히고,
      그 다음엔 다시 일반 `패턴 선택:`으로 복귀한다.
- [ ] **후보 고갈 스팸 없음**: `사용 가능한 패턴 후보 없음` 로그가 프레임마다
      쏟아지지 않는다 (나와도 1줄만, 곧 공격 재개).
- [ ] **D-07 Stage 2 사이클**: HP 50% 이하로 Stage 2 진입 → `일반 패턴 실행 #1/3~#3/3`
      → `헤비콤보 단계 진입` → 헤비콤보 종료 → 분신 삭제+그로기 전환이 Phase 6과
      동일하게 동작하고, 분신 2 + 진짜 보스 1 = 총 3개가 동시 존재한다.
- [ ] **D-08b 물괴물 보스 회귀 없음**: WaterMonster 마이그레이션 완료 후,
      `WaterMonsterCombatState`가 이번 변경 이전과 동일하게 동작한다.

## 결과 기록

(검증 완료 시 여기에 관찰된 패턴 시퀀스 로그 원문과 각 항목 PASS/FAIL을 기록한다.)
