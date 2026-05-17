# Phase 5: 보스 기반 엔티티 및 스테이지 1 공격 패턴 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-30
**Phase:** 05-spirit-boss-stage1-patterns
**Areas discussed:** HP·데미지 파이프라인, 3종 패턴 거리·쿨다운, 돌진 패턴 동작

---

## HP·데미지 파이프라인

| Option | Description | Selected |
|--------|-------------|----------|
| TakeDamage 오버라이드 | SpiritStats : BossStatsSystem. protected override TakeDamage()에서 배리어 로직 제거, _currentHealth -= info.amount 직접 처리 | ✓ |
| BossStatsSystem 직접 사용 | SpiritStats 별도 제작 없이 BossStatsSystem 그대로 붙이고 MaxWater=0으로 설정 | |

**User's choice:** TakeDamage 오버라이드

| Option | Description | Selected |
|--------|-------------|----------|
| return false 고정 | override ShouldTransitionToGroggy() → return false. Spirit은 그로기 없음 | ✓ |
| IsBarrierActive 프로퍼티 재정의 | SpiritStats에서 IsBarrierActive를 new로 override해 상시 true 반환 | |

**User's choice:** return false 고정 (SpiritCombatState에서)

---

## 3종 패턴 거리·쿨다운

| Option | Description | Selected |
|--------|-------------|----------|
| Inspector로만 노출 (Claude 재량) | RepelRange / ChargeRange / ProjectileRange 3개를 SerializeField로 노출. 기본값 없이 플래너가 정함 | ✓ |
| 근접 3유 / 중거리 7유 / 원거리 12유 | 구체적 수치 사전 결정 | |

**User's choice:** Inspector 노출만 (수치 플래너 재량)

| Option | Description | Selected |
|--------|-------------|----------|
| 근접 우선 | 근접 → 튕겨내기, 중거리 → 돌진, 나머지 → 투사체 | ✓ |
| Random 선택 | 조건 만족 패턴 중 랜덤 선택 | |

**User's choice:** 근접 우선

| Option | Description | Selected |
|--------|-------------|----------|
| 패턴별 독립 쿨다운 | 각 IAttackStrategy.Cooldown 필드 사용 | |
| 공통 쿨다운 | 한 패턴 시전 후 모든 패턴에 동일 쿨다운 적용 | ✓ |

**User's choice:** 공통 쿨다운

---

## 돌진 패턴 동작

| Option | Description | Selected |
|--------|-------------|----------|
| 목표 위치 도달 시 종료 | Vector2.Distance < threshold일 때 종료 | |
| 고정 시간 후 종료 | X초 돌진 후 강제 종료 | |
| 준비 끝날 때 목표점 고정 (뒤쪽) + 도달 시 종료 | 준비 종료 시 플레이어 뒤쪽 좌표 고정, 플레이어가 피해도 이동 후 종료 | ✓ |

**User's choice:** 준비 끝날 때 목표점 고정 (플레이어 뒷쪽 좌표), 도달 시 종료
**Notes:** "돌진 준비 끝날 때 플레이어의 뒤쪽의 돌진의 끝좌표를 설정. 플레이어가 피해도 더 이동하다가 종료"

| Option | Description | Selected |
|--------|-------------|----------|
| Inspector 노출 (Claude 재량) | ChargeWindup, ChargeSpeed, OvershotDistance → SerializeField | ✓ |
| 수치 직접 지정 | 준비 0.4초, 속도 12유/초, 초과 거리 2유 등 직접 정함 | |

**User's choice:** Inspector 노출 (수치 플래너 재량)

| Option | Description | Selected |
|--------|-------------|----------|
| 충돌 시 데미지 적용 | 돌진 중 Player 레이어 OnTriggerEnter2D로 데미지 1회 적용 | ✓ |
| 데미지 없음 | 돌진은 위치 이동만, 데미지는 튕겨내기 패턴만 담당 | |

**User's choice:** 충돌 시 데미지 적용

---

## Claude's Discretion

- 파일 위치 (`Assets/Enemy/WaterSpirit/Script/`)
- 사망 처리 방식 (비활성화 vs DeadState)
- S1-02 투사체 세부 수치 및 동작
- S1-03 튕겨내기 knockback force 및 데미지 수치
- 모든 Inspector 기본값

## Deferred Ideas

- 스테이지 2 전환 — Phase 6
- 애니메이션·이펙트 — v3.0+
