---
phase: 02
plan: 01
subsystem: water-monster
tags: [infrastructure, weather, puddles, phase-trigger]
dependency-graph:
  requires: []
  provides: [weather-system, puddle-lifecycle]
  affects: [boss-stats, boss-controller]
tech-stack:
  added: [PuddlePool, WeatherController, PuddleSpawner, PuddleStackManager]
  patterns: [Object Pooling, Scene-scoped Singleton, Event-driven Phase Trigger]
key-files:
  created:
    - Assets/Enemy/WaterMonster/Script/Phase2/WeatherController.cs
    - Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs
    - Assets/Enemy/WaterMonster/Script/Phase2/PuddlePool.cs
    - Assets/Enemy/WaterMonster/Script/Phase2/PuddleSpawner.cs
    - Assets/Enemy/WaterMonster/Script/Phase2/PuddleStackManager.cs
  modified:
    - Assets/Enemy/NewBoss/Script/BossController.cs
    - Assets/Enemy/NewBoss/Script/BossStatesSystem.cs
    - Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs
    - Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs
decisions:
  - "Use scene-scoped singletons for PuddlePool and PuddleStackManager to avoid cross-scene state issues"
  - "Trigger Phase 2 via OnDamageTaken event in WaterMonsterController for clean decoupling"
metrics:
  duration: 15m
  completed_date: 2026-04-12
---

# Phase 02 Plan 01: Weather & Puddle Infrastructure Summary

Completed all infrastructure scripts for Phase 2, including the weather controller, puddle pool, spawner, and stack manager. Wired the phase transition so rain starts when WaterMonster HP drops below 70%.

## Key Features

### Weather & Puddle Lifecycle
- **WeatherController**: Manages rain particle system and coordinates with PuddleSpawner.
- **PuddlePool**: Efficient object pooling for WaterPuddle objects to prevent runtime allocation jitter.
- **PuddleSpawner**: Spawns puddles at random positions within map bounds during rain.
- **PuddleStackManager**: Tracks indestructible puddles and provides events for Phase 3 integration.

### Integration Fixes
- **BossController**: Changed `OnDestroy` to `protected virtual` to allow proper clean-up in derived classes.
- **BossStatsSystem**: Added `InvokeOnDamageTaken` helper to allow derived classes to fire the damage event even when the barrier is inactive (critical for WaterMonster).
- **WaterMonsterStats**: Now correctly fires `OnDamageTaken` on every non-water hit.
- **WaterMonsterController**: Subscribes to damage events and triggers the rain system at 70% HP.

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED
- All 5 new scripts exist and follow the spec.
- Integration fixes in existing scripts verified and committed.
- Event-driven trigger chain verified via code review.
