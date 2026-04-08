# Phase 1: 보스 기본 엔티티 및 코어 메커니즘 - Research

**Researched:** 2026-04-08
**Domain:** Unity 6 2D Boss AI (State Pattern, Strategy Pattern, 속성 태그 시스템)
**Confidence:** HIGH (모든 핵심 파일 직접 읽음, 기존 호출자 grep으로 검증)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**속성 태그 시스템**
- **D-01:** 속성 태그는 `enum DamageElement { None, Water }` 로 표현 (Phase 1 범위)
- **D-02:** 데미지 전달은 `struct DamageInfo { float amount; DamageElement element; }` 로 통합
- **D-03:** `DamageElement` enum 과 `DamageInfo` struct 는 `Assets/Script/Combat/DamageInfo.cs` 한 파일에 함께 배치 (신규 폴더 `Assets/Script/Combat/` 생성)
- **D-04:** 필드는 최소 세트만 (`amount`, `element`). YAGNI

**데미지 파이프라인 통합**
- **D-05:** 보스 정통 진입점 = `BossStatsSystem.TakeDamage(DamageInfo)`. 기존 `TakeDamage(float)` 는 내부에서 `TakeDamage(new DamageInfo { amount = damage, element = DamageElement.None })` 으로 forward
- **D-06:** 호출부에서 속성 결정 — 플레이어 스킬(WaveSlice/FlashSlice/PlayerAttack)이 `[SerializeField] DamageElement` 필드를 갖고 DamageInfo 를 만들어 전달
- **D-07:** Phase 1 시점에 WaveSlice = Water, FlashSlice/PlayerAttack = None

**WaterMonsterStats 설계**
- **D-08:** `WaterMonsterStats : BossStatsSystem` 상속
- **D-09:** `MaxWater = 0` 으로 설정 → IsBarrierActive 항상 false → 베리어/물 자연감소 자연 무력화. 다른 보스 영향 없음
- **D-10:** `BossStatsSystem.TakeDamage` 를 `protected virtual` 로 변경 (한 줄 수정 허용됨)
- **D-11:** Override 동작:
  - element == Water → `_currentHealth += amount` (MaxHealth 클램프), 회복 이벤트 발화
  - element != Water → 정상 대미지
  - HP 코스트 사망은 막고, 외부 대미지로 인한 사망은 `Die()` 호출
- **D-12:** OnDamageTaken 이벤트는 그대로. IsBarrierActive == false 라 CounterState 전환은 자연 차단됨
- **D-13:** Update() WaterDecayRate 자연 소모는 IsBarrierActive == false 라 자동 스킵

### Claude's Discretion (이번 RESEARCH가 해소)
1. HP 코스트 저장 방식 — ScriptableObject vs State 필드 vs 인스펙터 dictionary
2. 기존 States/Attacks/ 컨텐츠 카탈로그 및 재사용 결정
3. Phase 1 melee + ranged 패턴 구체 동작
4. 힐 피드백 (플로팅 텍스트 / 파티클 / 사운드)
5. HP.cs 와 BossStatsSystem 공존 시 WaveSlice 호출 분기

### Deferred Ideas (OUT OF SCOPE)
- 기타 속성(Fire, Earth, …) — Phase 1 범위 외
- DamageInfo 확장 필드 (source, isCritical, knockback)
- 비/웅덩이/폭발/순간이동/장판/광폭화 — Phase 2~4
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| REQ-WM-01 | 물 속성 회복 (Elemental Healing Filter) | §3.5 WaterMonsterStats override + §4 통합 맵의 WaveSlice → BossStatsSystem.TakeDamage(DamageInfo) 분기 |
| REQ-WM-02 | 공격 코스트 (Self-HP Attack Cost) | §3.1 HP 코스트 저장 방식 (state field) + WaterMonsterStats.SpendHpCost 헬퍼 (최소 1 HP 보장) |
| REQ-WM-03 | NewBoss 상속 구조 재사용 | §2 자산 카탈로그 — IdleState/ChaseState/CombatState/GroggyState 모두 그대로 사용, CounterState 는 IsBarrierActive=false 로 자연 차단 |
| REQ-WM-P1-01 | 근접/원거리 기본 패턴 (각 HP 코스트 적용) | §3.3 — 근접 = 기존 LightAttack 기반, 원거리 = RangedPokeAttack 기반 (Water 버전으로 신규 작성) |
| REQ-WM-P1-02 | 힐 즉시 체감 피드백 | §3.4 — 미니멀 TMP 플로팅 텍스트 + 파티클 |
| REQ-WM-P1-03 | 1페이즈 클리어 시 플레이어 체력↑ → 2페이즈 유리 | 밸런싱 — Phase 1 에서는 튜닝 포인트만 노출 (인스펙터 [Header("Tuning")]) |
| REQ-WM-X-01 | Layer Damage (Player 레이어에만) | 기존 PlayerAttackDamager / Damager 패턴 그대로 — 보스 공격 hitbox 는 Player 레이어 마스크만 적용 (기존 baseline 유지) |
| REQ-WM-X-02 | 속성 태그 시스템 | §3.5 + DamageInfo.cs 신규 파일 |
</phase_requirements>

## 1. Domain Summary

이 페이즈는 **새 시스템 구축이 아닌 기존 NewBoss 인프라의 상속 확장**이 핵심이다. `BossController` (State 머신), `BossStatsSystem` (HP/Water/이벤트), `IBossState` 인터페이스, `IAttackStrategy` 인터페이스가 모두 이미 존재하고 동작 중이며, `LightAttack` / `HeavyAttack` / `RangedPokeAttack` 세 개의 IAttackStrategy 구현이 이미 `WaterCost` 패턴(`Stats.ConsumeWater(MaxWater * 0.05f)`)을 가지고 있다. Phase 1 의 본질적 작업은 (a) `DamageInfo` 데이터 모델 신설, (b) 기존 `BossStatsSystem.TakeDamage(float)` 시그니처를 `protected virtual` + DamageInfo 오버로드로 확장, (c) `WaterMonsterController` / `WaterMonsterStats` 두 클래스 상속 구현, (d) 기존 IAttackStrategy 의 `ConsumeWater` 호출을 `SpendHpCost` 로 치환한 신규 Water 전용 패턴 클래스 작성, (e) 플레이어 스킬에 `[SerializeField] DamageElement` 필드 추가 + 보스를 향한 호출 분기다. **가장 큰 리스크는 위치적 분리 — 현재 NewBoss 는 `HP` 컴포넌트가 없고 `BossStatsSystem` 만 가지므로, 기존 모든 player damage pipeline (`AttackBox`, `PlayerAttackDamager`, `WaveSlice`)는 `HP` 만 검색해서 보스에게는 데미지가 전혀 들어가지 않는 상태**다. 이는 §4 통합 맵에서 명시적으로 해결한다.

**Primary recommendation:** State 클래스 필드(`private float _hpCostPercent = 0.05f;`)로 HP 코스트를 저장하고, 기존 `LightAttack` / `RangedPokeAttack` 을 베이스로 `States/WaterMonster/Attacks/` 하위에 `WaterMeleeSwipe` + `WaterRangedSpit` 두 IAttackStrategy 신규 작성. ScriptableObject 는 도입하지 않는다 (현 코드베이스에 SO 패턴이 전혀 없음 — YAGNI).

## 2. Existing Assets Catalog

### `Assets/Enemy/NewBoss/Script/`

| File | Lines | Purpose | Phase 1 Reuse |
|------|-------|---------|----------------|
| `BossController.cs` | 172 | State 머신 컨트롤러. `protected virtual Awake`, `ChangeState`, `MoveTo`, `StopMove`, `LookAtTarget`, `StartHeavyAttackCooldown`, `CheckAnimationState`, 이벤트 핸들러 `HandleDamageTaken` / `HandleWaterDepleted`. **`Awake`가 이미 `protected virtual` — 상속 친화적** | **상속**: WaterMonsterController : BossController 하면 거의 그대로 동작 |
| `BossStatesSystem.cs` (파일명 오타: Stats**es**) | 77 | HP/Water/Barrier 상태. `MaxHealth`, `MaxWater`, `WaterDecayRate`, `IsBarrierActive`, `OnDamageTaken`, `OnWaterDepleted`, `TakeDamage(float)` (현재 `public`), `ConsumeWater`, `RestoreWater`, `Die()` (현재 빈 메서드). **`_currentHealth` / `_currentWater` 모두 `private` — override에서 직접 접근 불가, accessor 필요** | **수정 + 상속**: D-10에 따라 (1) `_currentHealth` / `_currentWater` 를 `protected` 로 변경, (2) `TakeDamage(float)` 를 `protected virtual` 로 변경하고 DamageInfo forward 추가, (3) 신규 `protected virtual void TakeDamage(DamageInfo info)` 추가 |

### `Assets/Enemy/NewBoss/Script/States/`

| File | Class | Phase 1 Reuse |
|------|-------|---------------|
| `IBossState.cs` | `interface IBossState` (Enter/Execute/Exit) | **그대로 사용** |
| `IdleState.cs` | `IdleState` — TargetFound 면 ChaseState 전환 | **그대로 사용** |
| `ChaseStates.cs` | `ChaseState` — AttackRange 안이면 CombatState, 아니면 MoveTo | **그대로 사용** |
| `CombatState.cs` | `CombatState` — `_decisionTimer`, `_isAttacking`, `_currentAttack` 로 attack 시퀀싱. `SelectAttackStrategy(boss, dist)` 가 `dist > 8 → RangedPoke`, `CanUseHeavyAttack → Heavy`, else `Light`로 분기. **42행: `if (!boss.Stats.IsBarrierActive) → GroggyState 전환` — WaterMonster는 IsBarrierActive 가 항상 false 라 즉시 그로기로 빠짐 (BUG)** | **반드시 override** — `WaterMonsterCombatState : CombatState` 작성하거나 `CombatState` 의 IsBarrierActive 가드를 가상화 |
| `CounterState.cs` | `CounterState` — Enter()에서 ConsumeWater(MaxWater*0.1) 호출 | **간접 비활성화** — IsBarrierActive=false 라 HandleDamageTaken 에서 진입 자체가 안 됨 (D-12) |
| `GroggyState.cs` | `GroggyState` — 5초 후 `RestoreWater()` 호출 후 CombatState 복귀. WaterMonster에선 RestoreWater 호출돼도 MaxWater=0 이라 의미 없음 | **그대로 — 다만 CombatState 의 IsBarrierActive 가드 문제 때문에 절대 진입하면 안 됨 (위 문제 해결 필수)** |

### `Assets/Enemy/NewBoss/Script/States/Attacks/`

| File | Class | Behavior | Phase 1 Reuse |
|------|-------|----------|----------------|
| `IAttackStrategy.cs` | `interface IAttackStrategy { ExecuteAttack, Cooldown, AnimationName }` (`WaterCost` 주석으로 남아 있음) | **그대로 사용** — 인터페이스 변경 없이 구현 클래스 내부에서 HP 코스트 처리 |
| `LightAttack.cs` | Cooldown 1.5s, AnimationName "Attack_Light", `ConsumeWater(MaxWater*0.05f)` | **베이스 참고**, Phase 1 melee 의 템플릿 |
| `HeavyAttack.cs` | Cooldown 5.0s, "Attack_Heavy", `ConsumeWater(MaxWater*0.10f)` | Phase 1 범위 외 — 추후 사용 가능 |
| `RangedPokeAttack.cs` | Cooldown 2.0s, "Attack_Ranged", `ConsumeWater(MaxWater*0.01f)` | **베이스 참고**, Phase 1 ranged 의 템플릿 |

**중요:** 세 어택 모두 **실제 hitbox/projectile spawn 코드가 없다** — 애니메이션 트리거만 호출하고 `Debug.Log` 만 남긴다. 실제 데미지 판정은 애니메이션 이벤트 또는 hitbox prefab 에서 들어와야 하지만 그 부분이 없다. Phase 1 에서는 **간단한 OverlapCircle / Instantiate(projectile) 을 ExecuteAttack 안에 직접 작성**하는 게 가장 빠르다 (애니메이션 이벤트 시스템 도입은 over-engineering).

### Player Scripts

| File | Relevant for Phase 1 |
|------|-----------------------|
| `Assets/Player/Script/SkillScript/WaveSlice.cs` | **2종 존재** — `Assets/Player/Script/SkillScript/WaveSlice.cs` (위치 1, monobehaviour, `damage=15`, `radius=2.5`, OverlapCircleAll → HitBox 태그 → `HP.TakeDamage`). PlayerAttack.cs 에 `OnSkillR` 도 wave 인스턴스화하지만 데미지 코드 없음. **WaveSlice.cs 가 정전(canonical)** |
| `Assets/Player/Script/SkillScript/FlashSlice.cs` | 텔레포트 + slashHitboxPrefab 인스턴스화. **데미지 코드 없음** — slashHitboxPrefab 의 AttackBox 컴포넌트가 데미지 처리 |
| `Assets/Player/Script/PlayerAttack.cs` | `OnBasicAttack` → `attackBox` 인스턴스화. AttackBox.cs 의 OnTriggerEnter2D 에서 HP 검색 후 TakeDamage |
| `Assets/Player/Script/PlayerAttackBase.cs` | InputHandler 이벤트 구독 추상 클래스. 변경 없음 |
| `Assets/Player/Script/PlayerAttackDamager.cs` | `Damager` 상속, `ApplyDamageEffect(HP targetHP)` override → `targetHP.TakeDamage(playerAttack.damage)`. **HP 만 검색** |
| `Assets/Player/Script/AttackBox.cs` | OnTriggerEnter2D → `HP` → 없으면 `PlayerStats`. **BossStatsSystem 검색 없음** |

### 기존 HP 시스템

| File | Note |
|------|------|
| `Assets/Script/HP.cs` | 일반 적/플레이어용. `virtual TakeDamage(float)`. **NewBoss 프리팹에는 HP 컴포넌트 없음 (`find` 결과 NewBoss 폴더에 .prefab 도 없음 — 씬 직접 배치 필요)** |
| `Assets/Script/TakeDmg.cs` | `GiveDmg.DealtoTarget(GameObject, float)` → `target.GetComponent<HP>().TakeDamage(dmg)`. 사용처 없거나 미미 |

### 다른 보스 (영향 평가)

| Boss | Stats Class | TakeDamage 호출자? |
|------|-------------|---------------------|
| TutorialBoss | (없음 — HP 컴포넌트 사용) | 자체 HP 사용, BossStatsSystem 무관 |
| WoodBoss | `WoodBossStatsSystem` (별개 클래스, BossStatsSystem 상속 X) | 무관 |
| NewBoss (현재) | `BossStatsSystem` | **외부 호출자 0건** (grep 결과: `Stats.TakeDamage` 어디서도 호출 안 됨). D-10 의 `protected virtual` 변경은 100% 안전. |

## 3. Resolved Discretion Items

### 3.1 HP 코스트 저장 방식 — **State 클래스 필드 권장**

**선택:** 각 IAttackStrategy 구현체 안에 `private const float _hpCostPercent = 0.05f;` 와 같은 상수 필드.

**근거:**
1. **YAGNI** — Phase 1 패턴은 2개뿐. ScriptableObject 도입 시 (a) `[CreateAssetMenu]` 정의, (b) 에셋 파일 생성, (c) IAttackStrategy 가 SO 참조 보관, (d) 각 패턴에 SO 1개씩 — 6 step. State 필드는 상수 1줄.
2. **기존 패턴과 일관** — 현 codebase 에 ScriptableObject 단 하나도 없음 (`Assets` 전반 grep 결과). 도입은 패턴 충격.
3. **확장 시 변환 용이** — Phase 4 광폭화에서 코스트 동적 조정이 필요해지면 그때 SO 또는 dictionary 로 리팩터 (decimal phase).
4. **인스펙터 노출 불필요** — 코스트는 디자이너 hot-tweaking 대상이 아니라 코드 상수.

**대안 평가:**
- ScriptableObject `AttackPatternData` — 향후 패턴이 10+ 개 되면 권장. Phase 4 광폭화 진입 시 리팩터.
- BossController 인스펙터 dictionary — Unity 가 dictionary 직렬화 안 함. 비추.

**구현 sketch:**
```csharp
public class WaterMeleeSwipe : IAttackStrategy
{
    public float Cooldown => 1.4f;
    public string AnimationName => "Attack_Melee";
    private const float HpCostPercent = 0.03f; // MaxHealth 의 3%

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);

        // 1. HP 코스트 (최소 1 보장은 WaterMonsterStats.SpendHpCost 안에서 처리)
        if (boss.Stats is WaterMonsterStats wms)
            wms.SpendHpCost(wms.MaxHealth * HpCostPercent);

        // 2. 즉발 hitbox (애니메이션 이벤트 없이 코드 직접)
        var hits = Physics2D.OverlapCircleAll(
            boss.transform.position + boss.transform.right * 1.5f,
            1.2f,
            LayerMask.GetMask("Player"));
        foreach (var hit in hits) { /* 플레이어 PlayerStats.TakeDamage 호출 */ }
    }
}
```

### 3.2 기존 States/Attacks/ 카탈로그 — §2 표 참조

**Phase 1 재사용 결정:**
- **재활용**: `IAttackStrategy` 인터페이스 그대로
- **참조 후 신규 작성**: `LightAttack` → `WaterMeleeSwipe`, `RangedPokeAttack` → `WaterRangedSpit`
- **이번 페이즈 미사용**: `HeavyAttack` (Phase 4 광폭화에서 부활 가능)
- **삭제 금지**: 기존 `LightAttack` / `HeavyAttack` / `RangedPokeAttack` 은 다른 보스(추후)나 디버깅 reference 로 보존

**왜 신규 작성?**
1. 기존 세 어택은 모두 `ConsumeWater` 호출 — WaterMonster에선 의미 없는 호출. WaterMonster 전용 클래스가 `SpendHpCost` 호출하도록 분리.
2. 기존은 hitbox/데미지 코드가 비어 있음. WaterMonster 패턴은 실제 hitbox 가 있어야 §1 의 success criteria #5 ("최소 2종 패턴 동작")가 만족됨.
3. 위치 `States/WaterMonster/Attacks/` 로 격리하면 다른 보스 영향 0.

### 3.3 Phase 1 패턴 구체 동작 — **WaterMeleeSwipe + WaterRangedSpit**

| 패턴 | 거리 조건 | 애니메이션 트리거 | Hitbox | HP 코스트 | Cooldown |
|------|-----------|-------------------|--------|-----------|----------|
| `WaterMeleeSwipe` (근접) | dist ≤ 3.0 | `"Attack_Melee"` | `Physics2D.OverlapCircle(boss + forward*1.5, radius=1.2, Player layer)` 즉발 | MaxHealth × 3% | 1.4s |
| `WaterRangedSpit` (원거리) | dist > 3.0 (CombatState 진입 가능 거리 내) | `"Attack_Ranged"` | `Instantiate(spitProjectilePrefab)` — 직선 등속, 5초 후 자동 파괴, OnTriggerEnter2D 로 PlayerStats 데미지 | MaxHealth × 5% | 2.0s |

**왜 이 동작?**
- **즉발 hitbox**: 애니메이션 이벤트 시스템을 도입하면 Animator Controller 에 이벤트 키프레임을 추가해야 하고, NewBoss 에 그게 셋업돼 있는지 불확실. 현 LightAttack 도 즉발(`Debug.Log` 만 있음). 즉발이 단순.
- **Spit projectile**: TutorialBoss 의 `SeedProjectile.cs` 와 WoodBoss 의 `RootSpike.cs` 가 이미 동일 패턴(직선 이동 → OnTriggerEnter2D → 자동 파괴) 을 가지고 있으므로 그대로 모방. **`SeedProjectile.cs` 를 WaterSpitProjectile 의 베이스 템플릿으로 사용**.
- **`CombatState.SelectAttackStrategy` 의 dist > 8 → Ranged 분기는 그대로 동작** — 다만 dist > 3 에도 ranged 가 발사되도록 임계값 조정이 필요할 수도 있음 (Wave 0 이슈로 분류).

### 3.4 힐 피드백 — **TMP 플로팅 텍스트 (신규 미니멀 컨트롤러) + 색상 플래시 + 파티클 placeholder**

**현황 조사:** 프로젝트에 기존 FloatingText / DamagePopup 시스템 **없음** (grep 결과 0건). TextMesh Pro 패키지는 설치되어 있음 (`Assets/TextMesh Pro/`). HP.cs 의 FlashColor() 코루틴은 SpriteRenderer 색상 깜박임 패턴이 이미 있음 — **재사용 가능**.

**권장 미니멀 구현 (3 파일):**

1. `Assets/Script/Combat/HealPopup.cs` (신규) — TMP_Text 1초 fade-up 코루틴
2. `Assets/Script/Combat/HealPopupSpawner.cs` (신규) — `static SpawnHealPopup(Vector3 worldPos, float amount)` 정적 메서드. 내부에서 `Resources.Load<GameObject>("HealPopup")` 또는 인스펙터 prefab 참조
3. `WaterMonsterStats` 의 Water 분기에서 `HealPopupSpawner.SpawnHealPopup(transform.position + Vector3.up, info.amount);` 호출

**색상 피드백:** WaterMonsterStats 가 `SpriteRenderer` 를 캐싱한 뒤 Water 회복 시 `FlashGreen` 코루틴 (HP.cs 의 FlashColor 패턴 모방). HP.cs 와 별개 — 보스는 HP.cs 를 사용하지 않으므로 자체 SpriteRenderer flash 코루틴 작성.

**파티클:** Wave 0 단계에서는 placeholder GameObject (Inspector slot) 만 노출하고, Phase 1 종료 시 디자이너가 prefab 채워 넣음. Phase 1 코드 책임은 `Instantiate(healVfxPrefab, transform.position, Quaternion.identity)` 호출 한 줄.

**Why TMP not a separate library?** Unity 6 + TMP 가 이미 설치되어 있고 의존성 추가 0. 1 prefab + 30줄 코드.

### 3.5 HP.cs vs BossStatsSystem 공존 + WaveSlice 분기

**현재 (Phase 0):**
```csharp
// WaveSlice.cs:21-27
foreach (var hit in hits)
{
    if (hit.CompareTag("HitBox"))
    {
        HP target = hit.GetComponentInParent<HP>();
        if (target != null) target.TakeDamage(damage);
    }
}
```

**문제:** NewBoss 에는 HP 컴포넌트가 없고 BossStatsSystem 만 있음. 현재 WaveSlice 는 보스를 전혀 때리지 못함.

**Phase 1 후 (권장 패턴):**
```csharp
// WaveSlice.cs (수정)
[SerializeField] private DamageElement element = DamageElement.Water;

// ...
foreach (var hit in hits)
{
    if (!hit.CompareTag("HitBox")) continue;

    // 1. BossStatsSystem 우선 검색 (보스용)
    var bossStats = hit.GetComponentInParent<BossStatsSystem>();
    if (bossStats != null)
    {
        bossStats.TakeDamage(new DamageInfo { amount = damage, element = element });
        continue;
    }

    // 2. 일반 HP 검색 (잡몹용)
    var hpTarget = hit.GetComponentInParent<HP>();
    if (hpTarget != null) hpTarget.TakeDamage(damage);
}
```

**핵심 원칙:**
- **HP 와 BossStatsSystem 은 결코 같은 GameObject 에 동시에 붙지 않는다**. 보스는 BossStatsSystem only, 잡몹/플레이어는 HP only.
- **검색 순서**: BossStatsSystem 우선 → HP 폴백. 보스의 BossStatsSystem 은 부모/조상에 있을 수 있으므로 `GetComponentInParent`.
- **WaveSlice 외 다른 데미지 진입점도 동일 패턴 적용 필수**:
  - `AttackBox.cs:9-26` — OnTriggerEnter2D
  - `PlayerAttackDamager.cs:20-29` — `ApplyDamageEffect(HP)` 시그니처를 깨면 모든 호출자가 영향. **Damager 베이스 클래스 보존, PlayerAttackDamager 에 `OnTriggerEnter2D` 또는 `ApplyDamageEffect` 분기 추가** (구체 방법은 planner 결정)
- **`TakeDamage(float)` forward 는 D-05 에 의해 element=None 으로 forward** — 따라서 분기를 안 한 호출자(현재 사용처 없지만 미래)는 자동으로 비-Water 데미지로 처리됨 (안전 기본값).

**`BossStatsSystem._currentHealth` private 문제:** 현 코드에서 `_currentHealth` / `_currentWater` 가 `private`. WaterMonsterStats 가 override 안에서 직접 수정하려면 `protected` 로 변경 필수. 이건 D-10 의 "한 줄 수정"에 포함되는 것으로 간주(2-3 줄 수정).

## 4. Integration Map

### 신규 파일

```
Assets/
├── Script/Combat/
│   ├── DamageInfo.cs                  ← enum DamageElement + struct DamageInfo (D-03)
│   ├── HealPopup.cs                   ← TMP fade-up 코루틴
│   └── HealPopupSpawner.cs            ← static spawn helper
└── Enemy/NewBoss/Script/
    ├── WaterMonsterController.cs       ← : BossController, override Awake (베이스 호출)
    ├── WaterMonsterStats.cs            ← : BossStatsSystem, override TakeDamage(DamageInfo), SpendHpCost helper
    └── States/WaterMonster/
        ├── WaterMonsterCombatState.cs  ← : CombatState (또는 신규) — IsBarrierActive 가드 우회
        └── Attacks/
            ├── WaterMeleeSwipe.cs      ← : IAttackStrategy
            └── WaterRangedSpit.cs      ← : IAttackStrategy (+ Projectile prefab)

Assets/Resources/ (또는 prefab 폴더)
├── HealPopup.prefab                    ← TMP_Text + HealPopup.cs 컴포넌트
└── WaterSpitProjectile.prefab          ← Rigidbody2D + Collider2D + 이동/데미지 스크립트
```

### 수정 파일 (최소 침습)

| 파일 | 변경 내용 | 라인 추정 |
|------|-----------|-----------|
| `Assets/Enemy/NewBoss/Script/BossStatesSystem.cs` | (a) `_currentHealth` / `_currentWater` 를 `protected`, (b) `public void TakeDamage(float)` → `public virtual void TakeDamage(float)` 로 forward to DamageInfo, (c) `protected virtual void TakeDamage(DamageInfo)` 신규 | +6 / -2 |
| `Assets/Enemy/NewBoss/Script/States/CombatState.cs` | 42행 `if (!boss.Stats.IsBarrierActive) → GroggyState` 가드를 가상 메서드 또는 WaterMonsterCombatState override 로 우회. **선택 1: `protected virtual bool ShouldGroggy()` 도입 (베이스에서 IsBarrierActive 반환)** | +3 / -1 |
| `Assets/Player/Script/SkillScript/WaveSlice.cs` | `[SerializeField] DamageElement element = Water;` 추가 + 보스 분기 (§3.5 코드) | +12 / -3 |
| `Assets/Player/Script/SkillScript/FlashSlice.cs` | `[SerializeField] DamageElement element = None;` 추가 (FlashSlice 자체는 데미지 코드 없음 — slashHitboxPrefab 의 AttackBox 가 처리. **AttackBox.cs 도 동일 분기 추가 필요**) | +1 |
| `Assets/Player/Script/PlayerAttack.cs` | `[SerializeField] DamageElement element = None;` (인스펙터 노출용. PlayerAttack 본체는 직접 데미지 처리 안 함) | +1 |
| `Assets/Player/Script/PlayerAttackDamager.cs` | `ApplyDamageEffect` 분기 추가 — BossStatsSystem 우선 검색 | +6 / -1 |
| `Assets/Player/Script/AttackBox.cs` | `OnTriggerEnter2D` 에 BossStatsSystem 우선 검색 분기 추가 | +6 |

### 호출 흐름 (REQ-WM-01 시나리오: 플레이어가 WaveSlice 로 보스 타격)

```
InputHandler.OnSkill2Event
  → PlayerAttack.OnSkillR()  [PlayerAttack.cs:91]
  → Instantiate(waveEffectPrefab)  [PlayerAttack.cs:103]
  → (별개로) WaveSlice.waveSlice() 컴포넌트 호출 — 또는 PlayerAttack.OnSkillR 안에 직접 OverlapCircleAll
  → Physics2D.OverlapCircleAll  [WaveSlice.cs:18]
  → hit.CompareTag("HitBox")  [WaveSlice.cs:21]
  → bossStats = hit.GetComponentInParent<BossStatsSystem>()  [신규]
  → bossStats.TakeDamage(new DamageInfo { amount = 15, element = Water })
  → (가상 디스패치) WaterMonsterStats.TakeDamage(DamageInfo info)  [신규 override]
  → info.element == Water → _currentHealth += info.amount; Mathf.Min(MaxHealth)
  → HealPopupSpawner.SpawnHealPopup(transform.position, info.amount)
  → SpriteRenderer flash green
```

### 호출 흐름 (REQ-WM-02 시나리오: 보스가 melee 패턴 시전)

```
BossController.Update  [BossController.cs:52]
  → CombatState.Execute  [CombatState.cs:18]
  → (cooldown 0이고 attack 안 하는 중)
  → SelectAttackStrategy(boss, dist)  [CombatState.cs:71]
  → return new WaterMeleeSwipe()  [신규 — WaterMonsterCombatState override]
  → attack.ExecuteAttack(boss)
  → WaterMonsterStats.SpendHpCost(MaxHealth * 0.03)  [신규 helper, 최소 1 HP 보장]
  → boss.Anim.SetTrigger("Attack_Melee")
  → Physics2D.OverlapCircle(player layer mask)
  → 적중 시 PlayerStats.TakeDamage(damage)
```

### 다른 보스 영향 평가

| 보스 | 영향 | 해결 |
|------|------|------|
| TutorialBoss | 0 — BossStatsSystem 미사용 | — |
| WoodBoss | 0 — 별개 WoodBossStatsSystem | — |
| 잡몹 (HP 사용) | 0 — WaveSlice 폴백 분기로 그대로 동작 | — |

## 5. Risks / Open Questions

### High
1. **`CombatState.IsBarrierActive` 가드 (CombatState.cs:42)** — WaterMonster 는 항상 IsBarrierActive=false 라 진입 즉시 GroggyState 로 빠진다. **이 가드를 가상 메서드로 추출하든 WaterMonsterCombatState 신규 작성하든 반드시 처리해야 보스가 동작한다.** 처리 안 하면 success criterion #1 ("Idle/Chase/Combat 상태 전환 동작") 실패.

2. **NewBoss 프리팹 부재** — `find` 결과 `Assets/Enemy/NewBoss/` 아래 `.prefab` 0건. 씬에 직접 GameObject 배치 + 컴포넌트 부착하는 작업이 Wave 0 또는 첫 task 로 명시적으로 들어가야 함. Animator Controller 도 셋업 필요 — `"Attack_Melee"` / `"Attack_Ranged"` / `"Idle"` 트리거/스테이트가 존재하는지 확인 필수.

3. **`BossStatesSystem.cs` 파일명 오타** — 'States'(Stats 가 아님). 새 코드 `WaterMonsterStats : BossStatsSystem` 와 클래스 이름은 일치하지만(클래스명은 `BossStatsSystem`) 파일명만 다름. 그대로 둬도 컴파일은 되지만 혼란 유발. **이번 phase 에서 파일명 변경은 git mv 비용 + meta 충돌 리스크 — 손대지 않는 게 안전.**

### Medium
4. **PlayerAttack.cs 의 `OnSkillR` 가 데미지 처리를 직접 하지 않음** — wave 인스턴스화만. 실제 데미지는 어디서? `Assets/Player/Script/SkillScript/WaveSlice.cs` 의 `waveSlice()` 메서드가 별개 인스턴스에서 호출되는 듯. **PlayerAttack 와 WaveSlice 의 관계를 plan 단계에서 명확히 짚어야 함.** R 키를 누르면 (a) PlayerAttack.OnSkillR 가 wave 이펙트만 띄우고 (b) WaveSlice 컴포넌트의 waveSlice() 가 별도 호출되는 구조인지, 아니면 두 코드가 중복인지 확인 필요.

5. **레이어 정의 가정** — `LayerMask.GetMask("Player")` 를 사용한다는 전제. 프로젝트에 "Player" layer 가 정의돼 있는지 Wave 0 에서 검증.

6. **Animator 상태 이름 가정** — `"Attack_Light"` / `"Attack_Heavy"` / `"Attack_Ranged"` 는 기존 코드에 하드코딩돼 있지만 **실제 AnimatorController 에 그 상태가 있는지는 확인 안 됨**. 신규 `"Attack_Melee"` 트리거도 추가 필요.

### Low
7. **`Die()` 가 빈 메서드** — BossStatesSystem.cs:76. 외부 데미지로 사망 조건이 발생해도 보스가 사라지지 않음. Phase 1 의 직접 책임은 아니지만 success criterion #4 ("HP 코스트로 죽지 않음") 검증 시 외부 데미지 사망도 함께 검증하면 노출됨. **WaterMonsterStats.Die() override 에서 `gameObject.SetActive(false)` 또는 Destroy 호출 권장.**

8. **Resources 폴더 의존성** — `HealPopupSpawner` 가 `Resources.Load` 사용 시 `Assets/Resources/` 폴더 필요. 또는 인스펙터 prefab 참조 패턴(static initializer 못 씀 — singleton 도입 필요). 둘 다 trade-off — planner 결정.

## 6. Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | **None detected** — Unity Test Framework (UTF/NUnit) 미설치, `Assets/Tests/` 폴더 부재, `*.asmdef` 테스트 어셈블리 부재 |
| Config file | none — see Wave 0 |
| Quick run command | `Unity -batchmode -runTests -testPlatform EditMode -projectPath . -testResults results.xml` (Wave 0 에서 UTF 설치 후 사용 가능) |
| Full suite command | 위와 동일 (단일 어셈블리) |

**현실적 노트:** 이 프로젝트는 **자동화 테스트 인프라가 전무**하다. 모든 검증은 (a) Unity Editor Play Mode 에서 수동, (b) 코드 inspection, (c) 최소한의 EditMode 단위 테스트(Wave 0 에서 신설)로 수행. 자동화 비용 대비 가치가 중간이므로 **EditMode 단위 테스트 1-2 개만 핵심 로직에 적용**하고 나머지는 manual playtest 로 처리.

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| REQ-WM-01 | WaveSlice (element=Water) 타격 시 보스 _currentHealth 증가 | EditMode unit | `Unity ... -testFilter WaterMonsterStatsTests.WaterDamage_Heals` | Wave 0 |
| REQ-WM-01 | 비-Water 타격 시 정상 차감 | EditMode unit | `... NonWaterDamage_Reduces` | Wave 0 |
| REQ-WM-01 | Water 회복이 MaxHealth 초과하지 않음 | EditMode unit | `... WaterHeal_ClampsToMax` | Wave 0 |
| REQ-WM-02 | SpendHpCost(amount) 가 _currentHealth 를 amount 만큼 감소시키되 1 미만으로는 안 내려감 | EditMode unit | `... HpCost_NeverKills` | Wave 0 |
| REQ-WM-02 | WaterMeleeSwipe.ExecuteAttack 호출 시 SpendHpCost 가 호출됨 (mock 또는 실측) | manual-only | Play Mode | — |
| REQ-WM-03 | WaterMonsterController : BossController 가 Idle → Chase → Combat 전환 | manual-only | Play Mode (콘솔에 _currentStateName 출력) | — |
| REQ-WM-P1-01 | melee + ranged 두 패턴이 거리에 따라 선택됨 | manual-only | Play Mode (Debug.Log 확인) | — |
| REQ-WM-P1-02 | Water 타격 시 HealPopup 등장 + 색상 플래시 | manual-only | Play Mode (시각 검증) | — |
| REQ-WM-P1-03 | (밸런싱) | manual-only | playtest | — |
| REQ-WM-X-01 | melee/ranged hitbox 가 Player layer 만 영향 | EditMode unit (LayerMask 검증) | `... AttackHitbox_PlayerLayerOnly` | Wave 0 |
| REQ-WM-X-02 | DamageElement enum / DamageInfo struct 컴파일 + 스킬 인스펙터 노출 | compile + manual | (빌드 성공) | — |

### Sampling Rate
- **Per task commit:** Unity Editor 컴파일 성공 (`Editor.log` 확인) + 단위 테스트 1회 — `Unity -batchmode -runTests -testPlatform EditMode -projectPath .`
- **Per wave merge:** 위 + Play Mode 수동 시나리오 (보스 spawn → WaveSlice 타격 → 힐 확인 → 원거리 패턴 트리거 확인)
- **Phase gate:** 모든 EditMode 테스트 통과 + Phase 1 success criteria 5개 manual playtest 통과

### Wave 0 Gaps
- [ ] Unity Test Framework 설치 (`Window → Package Manager → Test Framework`)
- [ ] `Assets/Tests/EditMode/` 폴더 + `Tests.EditMode.asmdef` 생성 (TestAssemblies 옵션 ON)
- [ ] `Assets/Tests/EditMode/WaterMonsterStatsTests.cs` — REQ-WM-01 / REQ-WM-02 단위 테스트 4개
- [ ] NewBoss 씬 GameObject 셋업 (BoxCollider2D, Rigidbody2D, BossController, WaterMonsterStats, Animator) — playtest 가능 baseline
- [ ] AnimatorController 트리거 검증/추가 (`Attack_Melee`, `Attack_Ranged`)
- [ ] "Player" Layer 정의 확인
- [ ] HealPopup.prefab + WaterSpitProjectile.prefab 생성 (placeholder 가능)

## Sources

### Primary (HIGH confidence — 직접 읽음)
- `Assets/Enemy/NewBoss/Script/BossController.cs` (1-172)
- `Assets/Enemy/NewBoss/Script/BossStatesSystem.cs` (1-77)
- `Assets/Enemy/NewBoss/Script/States/{IBossState,IdleState,ChaseStates,CombatState,CounterState,GroggyState}.cs`
- `Assets/Enemy/NewBoss/Script/States/Attacks/{IAttackStrategy,LightAttack,HeavyAttack,RangedPokeAttack}.cs`
- `Assets/Player/Script/{PlayerAttack,PlayerAttackBase,PlayerAttackDamager,AttackBox}.cs`
- `Assets/Player/Script/SkillScript/{WaveSlice,FlashSlice}.cs`
- `Assets/Script/{HP,TakeDmg}.cs`
- Grep: `\.TakeDamage\(` across `Assets/Enemy/` (외부 호출자 0건 검증)
- Grep: `BossStatsSystem|BossStatesSystem` (TutorialBoss/WoodBoss 무관 검증)
- Grep: `FloatingText|DamagePopup|TextMeshPro` (기존 popup 시스템 부재 검증)

### Secondary (MEDIUM)
- 없음 — 모든 결정이 코드베이스 직접 읽기로 뒷받침됨

### Tertiary (LOW)
- 없음

## Metadata

**Confidence breakdown:**
- 자산 카탈로그: HIGH — 모든 파일 직접 읽음
- 통합 맵: HIGH — 호출 사이트 grep 으로 검증
- HP 코스트 저장 방식: HIGH — codebase 에 SO 0건 사실 기반
- 패턴 동작 디자인: MEDIUM — Animator 상태 이름이 실제 Animator 에 있는지 미검증 (Wave 0)
- 힐 피드백: MEDIUM — TMP 설치 확인했으나 prefab 작성은 새 작업
- 테스트 인프라: HIGH — UTF 미설치 사실 기반

**Research date:** 2026-04-08
**Valid until:** 2026-05-08 (30 days — 코드베이스 안정적, 의존성 변동 적음)

## RESEARCH COMPLETE
