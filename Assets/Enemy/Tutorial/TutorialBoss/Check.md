# TutorialBoss — 패턴 판단 로직 Play 모드 검증 체크리스트

**상태: 보류** — WaterMonster 보스가 CombatState 기반 패턴 판단 로직으로 전환된 뒤,
전체 보스(WaterSpirit 포함)를 한 번에 일괄 Play 모드 검증할 예정이다.
이 보스는 아직 한 번도 Play 모드 검증을 거치지 않은 채 패턴 판단 로직이 교체되었다.

## 검증 대상 변경사항

기존 "짝수/홀수 카운터로 두 패턴을 무조건 교대"하던 `TutorialAttackState`의
고정 로직을 제거하고, `TutorialBossController.SelectPattern()`이 거리·정지시간·
쿨다운·연속사용금지 조건을 평가해 다음 패턴을 결정하는 방식으로 교체했다.
그로기 진입 조건도 "TentaclePierce 완료 시 무조건 예약"에서 "HP 70%/30% 임계치
통과 시, 실행 중이던 패턴이 끝난 뒤 예약"으로 바뀌었다.

| 패턴 | 발동 조건 | 쿨다운 | Attack 상태 점유 시간 |
|---|---|---|---|
| TentacleStab (촉수 찌르기) | 거리 ≤ `PlayerWidth`×3 | 8초 | 2.8초 |
| GroundTentacle (바닥 촉수) | 거리 ≥ `PlayerWidth`×3 **또는** 플레이어 2초 이상 정지(오차 0.1 이내) | 5초 | 1.5초 |

공통 조건: 직전 패턴과 동일하면 후보에서 제외(연속 사용 금지), 공격 종료 후
`PostAttackIdleTime`(2초) 동안 Idle 대기 후 재판단, 두 조건 모두 불충족 시
Idle 유지 후 다음 프레임에 재시도.

그로기: HP가 70% 또는 30%를 처음 통과하는 순간 각각 1회씩만 예약되며, 현재
실행 중인 패턴이 끝난 뒤에 `GroggyState`로 전환된다 (공격 도중 강제 캔슬 없음).

## 사전 준비

1. Unity 에디터에서 컴파일 에러 0건 확인
2. 튜토리얼 보스 씬 열기
3. Console 검색창에 `[TutorialBoss]` 입력해 필터링
   (주의: 기존 로그가 `SpiritCombatState`만큼 상세하지 않다 — `──→ Attack 상태
   ({pattern})` / `──→ Idle 상태` 두 종류뿐이므로, 거리·정지시간·쿨다운 조건은
   Scene 뷰에서 직접 위치를 보거나 Inspector의 `LastUsedPattern`/`IsGroggy`
   값을 확인해서 판단해야 한다.)

## 검증 항목

- [x] **근접 판정**: `PlayerWidth`×3 이내로 접근 후 대기 → `TentacleStab`이
      선택된다 (로그: `──→ Attack 상태 (TentacleStab)`). — 근접(거리 2.1~2.6)에서 TentacleStab 선택
- [x] **원거리 판정**: `PlayerWidth`×3 밖으로 이동 → `GroundTentacle`이
      선택된다. — 원거리(거리 7.8~8.4)에서 GroundTentacle 선택
- [x] **정지 판정**: 근접 범위 안에서 2초 이상 가만히 서 있어도(오차 0.1 이내)
      `GroundTentacle`이 선택된다 (거리 조건과 별개로 정지 조건이 단독으로도
      트리거되는지 확인). — 근접(거리 2.1)에서 정지 2.07초 시점에 GroundTentacle 선택
- [x] **연속 사용 금지**: 같은 패턴이 두 번 연속 나오지 않는다 (조건을 계속
      만족해도 직전과 다른 패턴이거나 Idle로 대기한다). — 20회 교대 중 같은 패턴 연속 0회, 직전과 같은 패턴 조건만 충족하면 Idle 유지
- [x] **쿨다운**: TentacleStab 사용 직후 8초 이내, GroundTentacle 사용 직후
      5초 이내에는 해당 패턴이 다시 선택되지 않는다. — 타이머 감소(7.9→0 / 4.9→0)는 확인. 교대 주기(약 8.7초)가 쿨다운보다 길어 쿨다운이 단독으로 막는 경우는 관찰되지 않음(간접 확인)
- [x] **PostAttackIdleTime**: 공격 종료 후 곧바로 다음 패턴으로 전환되지 않고
      약 2초간 Idle 상태를 유지한다. — 공격 종료 후 다음 공격까지 2.06~2.14초
- [x] **후보 없음 유지**: 조건을 만족하는 패턴이 하나도 없을 때 보스가 멈추거나
      에러를 내지 않고 Idle을 유지하다가 조건이 충족되면 자연스럽게 전환된다. — 25초 이상 Idle 유지, 에러 없음, 조건 충족 시 즉시 전환
- [x] **그로기 HP 70%**: HP가 70%를 처음 통과하는 순간, 실행 중이던 패턴이
      끝난 직후 그로기로 전환된다 (공격 도중 강제 캔슬되지 않는다). — 공격 시작 후 타격해도 공격은 끝까지 진행(GroundTentacle 1.59초), 종료 직후 그로기
- [x] **그로기 HP 30%**: HP가 30%를 처음 통과할 때도 동일하게 1회 더 그로기로
      전환된다 (70%/30% 각각 정확히 1번씩만 — 반복 트리거되지 않는다). — 공격 시작 0.04초 후 타격(HP 28%)해도 TentacleStab 2.87초 완주 후 그로기, 70%/30% 각 1회, 이후 추가 타격에도 재발동 없음
- [x] **스프라이트 콜라이더 동기화** (`SpriteColliderSync.cs`): 애니메이션 프레임이
      바뀔 때마다 `PolygonCollider2D`가 해당 프레임의 Physics Shape로 즉시
      갱신된다 (TentacleStab/GroundTentacle 전환, 그로기 전환 포함 모든 애니메이션
      구간에서 히트박스가 스프라이트 모양과 어긋나지 않는다 — 콘솔 에러/널 참조 없음). — Spike 프리팹의 Tentacle 스프라이트 22종에서 물리 모양 수·점 수 전부 일치(불일치 0프레임). 보스 본체에는 이 컴포넌트가 없음(Spike.prefab에만 있음)

## 결과 기록

2026-09-29, Unity CLI(eval) + Play 모드(Tutorial Map)로 실측. 상태 전환·LastUsedPattern·쿨다운 타이머를 프레임 단위로
기록했고, 측정 중 플레이어는 매 프레임 회복시켰다(사망 리로드 방지). 씬·Assets 변경 없음(`dirty=False`).
전 항목 PASS(쿨다운은 간접 확인). 공격 지속시간은 TentacleStab 2.84~2.93초(기대 2.8), GroundTentacle 1.55~1.64초(기대 1.5).

**참고:** 측정 중 자동저장(Phase 16)이 `%LocalLow%/DefaultCompany/AQUA ECLIPSE/save.json`에 기본값 세이브를 썼다.
