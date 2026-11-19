---
phase: 05-spirit-boss-stage1-patterns
plan: 02
subsystem: Enemy (Spirit Boss Attacks)
tags: [attack-patterns, coroutine, projectiles, area-of-effect]
requires: [SpiritController, IAttackStrategy, PlayerController, PlayerStats]
provides: [SpiritCharge, SpiritProjectileAttack, SpiritProjectile, SpiritRepel]
affects: [Spirit Boss Combat Gameplay]
tech-stack:
  added: [Unity 6 linearVelocity]
  patterns: [Coroutine-based Action, Instantiate-Init Pattern, Area Trigger Damage]
key-files:
  created:
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritCharge.cs
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectileAttack.cs
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectile.cs
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritRepel.cs
key-decisions:
  - "SpiritCharge는 Windup(대기)과 Dash(이동)로 구성된 2단계 코루틴으로 구현하여 행동의 리듬감 부여"
  - "SpiritProjectileAttack은 발사 시점의 플레이어 방향을 고정하여 정밀한 원거리 공격 구현"
  - "SpiritRepel은 OverlapCircleAll을 사용하여 보스 주변의 플레이어를 즉시 감지하고 Knockback 적용"
  - "IAttackStrategy의 Cooldown을 각각 3.0s(돌진), 2.5s(투사체), 1.5s(넉백)로 설정하여 패턴 순환 밸런스 조정"
requirements-completed: [S1-01, S1-02, S1-03]
duration: 20 min
completed: 2026-04-30
---

# Phase 05 Plan 02: Spirit Boss Stage 1 Patterns Summary

## Substantive Changes
정령 보스의 1페이즈 핵심 공격 패턴 3종을 `IAttackStrategy` 기반으로 완전하게 구현했습니다.

- **SpiritCharge (S1-01)**: 2단계 코루틴(Windup -> Dash)을 통해 플레이어 위치를 지나치는 돌진 공격을 구현했습니다. `SetCharging`을 통해 충돌 데미지 판정을 제어합니다.
- **SpiritProjectile (S1-02)**: 플레이어 방향으로 발사되는 투사체 시스템을 구축했습니다. `SpiritProjectileAttack`에서 생성 및 방향 초기화를 담당하며, `SpiritProjectile`에서 물리 비행 및 충돌 데미지를 처리합니다.
- **SpiritRepel (S1-03)**: 근접한 플레이어를 밀쳐내는 방어적 공격 패턴입니다. `OverlapCircleAll`로 범위를 판정하고 데미지와 함께 `PlayerController.ApplyKnockback`을 호출하여 넉백 효과를 줍니다.

## Deviations from Plan
- **Stub Cleanup**: Plan 01에서 생성했던 `SpiritAttackStubs.cs`를 삭제하고 실제 구현체로 대체했습니다.

## Verification Results
- `grep_search`를 통해 모든 패턴이 인터페이스 사양을 준수하고 필수 물리 로직(`linearVelocity`, `TakeDamage`, `ApplyKnockback` 등)을 포함하고 있음을 확인했습니다.
- 모든 파일이 지정된 경로에 정상적으로 생성되었습니다.

## Self-Check: PASSED
- [x] 모든 공격 패턴 태스크 완료
- [x] 개별 태스크 커밋 수행
- [x] SUMMARY.md 작성 완료
- [x] STATE.md / ROADMAP.md 업데이트 완료
