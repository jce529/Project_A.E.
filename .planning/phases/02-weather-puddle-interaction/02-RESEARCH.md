# Phase 2: 날씨 시스템 및 물 웅덩이 상호작용 - Research

**Researched:** 2026-04-10
**Domain:** Unity 2D — WeatherController (ParticleSystem), WaterPuddle 상태 관리, Object Pool, Unity New Input System 확장, 싱글턴 매니저 패턴
**Confidence:** HIGH (전부 기존 프로젝트 소스 직접 확인)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Phase 2 진입 트리거**
- D-01: 보스 HP <= 70% 도달 시 WeatherController 활성화. 임계치는 인스펙터에서 튜닝 가능, 기본값 70%.
- D-02: 페이즈 전환 전용 연출(화면 플래시, 보스 대사 등) 없음 — 비 ParticleSystem이 즉시 켜지는 것으로 Phase 2 진입 알림.
- D-03: `WaterMonsterController`가 `WaterMonsterStats.OnDamageTaken` 이벤트를 구독하고, HP 임계치 도달 여부를 체크해 `WeatherController.StartRain()` 호출. 한 번만 트리거 (bool 가드).

**WeatherController & 비 이펙트**
- D-04: `WeatherController`는 보스와 분리된 독립 컴포넌트 (씬에 별도 GameObject).
- D-05: 비 이펙트는 간단한 Unity ParticleSystem 기반 — 복잡한 셰이더/동적 이펙트 없음.
- D-06: 맵 커버리지는 씬에 BoxCollider2D (isTrigger)를 인스펙터에서 지정. WeatherController가 bounds를 읽어 ParticleSystem Shape 영역으로 사용.
- D-07: WeatherController는 StartRain() / StopRain() 두 메서드만 필요.

**WaterPuddle 스포너**
- D-08: PuddleSpawner가 맵 랜덤 위치에 WaterPuddle 프리팹을 주기적으로 Instantiate. 스폰 간격/최대 개수는 인스펙터 튜닝 가능.
- D-09: 스폰 위치는 WeatherController의 BoxCollider2D bounds 내 랜덤 좌표.

**WaterPuddle 상태 구조**
- D-10: WaterPuddle 컴포넌트는 `bool isDestructible` 필드로 상태 구분 (true=Destructible, false=Indestructible). enum 불필요.
- D-11: Indestructible 전환 시 시각 구분은 SpriteRenderer.color 변경 또는 별도 Sprite 전환. 구체 색상/스프라이트는 Claude's Discretion.
- D-12: WaterPuddle에 "WaterPuddle" 태그 부착. WaveSlice의 OverlapCircleAll 루프에서 CompareTag("WaterPuddle") 체크.
- D-13: WaveSlice WaterPuddle 파괴 처리 순서:
  1. CompareTag("WaterPuddle")으로 감지
  2. WaterPuddle.isDestructible 확인 — false면 무시
  3. true면 PuddlePool.Return(puddle) (Disable) + 파괴 VFX/사운드

**Object Pool**
- D-14: WaterPuddle은 Destroy 대신 Object Pool 방식 (Disable → 재사용). PuddlePool 또는 PuddleSpawner에 풀 로직 내장.

**흡수 상호작용**
- D-15: InputHandler에 신규 `OnInteractEvent (Action)` 추가. `.inputactions` 파일의 "Interact" 액션 신규 추가. 키 바인딩 권장: E 키.
- D-16: WaterPuddle에 CircleCollider2D (isTrigger) 부착. 플레이어가 범위 진입 시 `playerInRange = true`. OnInteractEvent 발생 시 흡수.
- D-17: 흡수 결과:
  1. WaterController.AddWater(amount) 호출 (플레이어 수분 회복)
  2. isDestructible = false (Indestructible 전환)
  3. PuddleStackManager.RegisterIndestructible(puddle) 호출

**PuddleStackManager**
- D-18: 싱글턴 또는 씬 GameObject. Indestructible 웅덩이 개수를 `int _indestructibleCount`로 중앙 카운팅.
- D-19: 임계치 도달 시 `OnThresholdReached` 이벤트 발화. 실제 연쇄 AoE 폭발 구현은 Phase 3 소관.
- D-20: Destructible 파괴 시 카운트 영향 없음 (Indestructible만 카운팅).

### Claude's Discretion
- WaterPuddle Indestructible 시각 구분 구체 색상/스프라이트 (기존 에셋 확인 후)
- Interact 키 바인딩 구체 키 값 (.inputactions 미사용 키 확인 후)
- PuddlePool 구현 위치 (PuddleSpawner 내장 vs 별도 PuddlePool MonoBehaviour)
- WaterPuddle CircleCollider2D 흡수 반경 수치 (인스펙터 튜닝 위임)
- WaveSlice 파괴 VFX 구체 에셋 (기존 이펙트 프리팹 재사용 or 신규)

### Deferred Ideas (OUT OF SCOPE)
- 연쇄 AoE 폭발 구현 — Phase 3 소관
- 보스 순간이동 패턴 — Phase 3 소관
- 광폭화 모드 — Phase 4 소관
- WaterPuddle 흡수 애니메이션/사운드 — Claude's Discretion (플래너 단계)
- 비 사운드 (Rain ambience) — Claude's Discretion (플래너 단계)
- 임계치 도달 시 경고 UI — Phase 3 연계 시 고려
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| REQ-WM-P2-01 | 페이즈 2 진입 시 맵 전체에 비 파티클/이펙트 발생. WeatherController는 보스와 분리된 독립 컴포넌트 | WeatherController 신규 작성, ParticleSystem.Play()/Stop() 패턴 확인 |
| REQ-WM-P2-02 | 비 시작과 함께 맵 랜덤 위치에 WaterPuddle 오브젝트 주기적 생성. 위치/주기 인스펙터 튜닝 가능 | PuddleSpawner 신규 작성, InvokeRepeating/Coroutine 패턴 확인 |
| REQ-WM-P2-03 | WaveSlice 스킬이 Destructible 웅덩이를 완전히 파괴할 수 있어야 함 | WaveSlice.cs OverlapCircleAll 루프 확장 패턴 직접 확인 |
| REQ-WM-P2-04 | 흡수 상호작용으로 수분 회복, 웅덩이 Indestructible 전환, 시각 구분 | WaterController.RecoveryWater() 확인, InputHandler 확장 패턴 확인 |
| REQ-WM-P2-05 | PuddleStackManager가 Indestructible 개수 카운팅, 임계치 초과 시 이벤트 | 싱글턴 패턴, C# Action 이벤트 패턴 확인 |
| REQ-WM-X-01 | 보스 공격/폭발은 Player 레이어에만 대미지. Layer Damage 원칙 유지 | WaterPuddle 흡수 범위 Trigger는 플레이어만 감지하도록 레이어 마스크 적용 필요 |
</phase_requirements>

---

## Summary

Phase 2는 5개의 신규 스크립트와 1개의 기존 스크립트 수정(WaveSlice), 1개 확장(InputHandler), 그리고 씬/프리팹 셋업으로 구성된다. 모든 코어 메커니즘은 기존 코드베이스 패턴(이벤트 구독, PhysicsOverlap, OverlapCircleAll)을 그대로 따른다.

**핵심 발견 사항:**
1. `PlayerInputActions.inputactions` 파일에 이미 "Action" 액션이 존재하며 F 키에 바인딩되어 있다. D-15의 Interact 신규 추가는 이 기존 "Action" 액션을 재명명/재사용하거나, 실제로 InputHandler에 `OnInteractEvent`를 연결하는 방식으로 구현 가능하다. E 키는 이미 Skill_1에 사용 중 — **Interact 키는 F 키 (기존 "Action" 액션)로 결정해야 한다.**
2. `WaterController`에는 `RecoveryWater()` (순수 물 한 병 채움)와 `RecoveryCorruptedWater()` (오염된 물 채움)가 있다. 흡수 상호작용의 "수분 회복"은 `RecoveryWater()`를 호출하는 것이 적합하다. `AddWater(amount)` 메서드는 존재하지 않는다 — D-17의 `WaterController.AddWater(amount)` 호출은 `RecoveryWater()` 또는 신규 메서드로 해석해야 한다.
3. `BossStatsSystem.OnDamageTaken`은 `event Action` (인수 없음) 타입이다. D-03의 트리거 연결은 `Action` 구독으로 구현한다.
4. Phase 2 스크립트(WeatherController, WaterPuddle, PuddleSpawner, PuddlePool, PuddleStackManager)는 전부 미존재 — 모두 신규 작성이다.

**Primary recommendation:** 5개 신규 스크립트를 `Assets/Enemy/WaterMonster/Script/Phase2/` 하위에 생성하고, WaterPuddle 프리팹을 `Assets/Enemy/WaterMonster/Resources/WaterPuddle.prefab`에 배치한다.

---

## Standard Stack

### Core (모두 기존 프로젝트 스택, 신규 패키지 없음)

| Component | Version | Purpose | Why Standard |
|-----------|---------|---------|--------------|
| Unity 6 ParticleSystem | 내장 | 비 이펙트 (WeatherController) | D-05 결정, 프로젝트 내 WaterSpitProjectile 등 기존 이펙트와 동일 방식 |
| Unity 6 Physics2D.OverlapCircleAll | 내장 | WaveSlice WaterPuddle 감지 | WaveSlice.cs 기존 코드가 이미 이 패턴 사용 |
| Unity 6 New Input System | com.unity.inputsystem@57d0e36f | Interact 입력 등록 | 프로젝트 전체가 이미 사용 중, InputHandler 싱글턴 존재 |
| C# event Action | 언어 내장 | OnThresholdReached 이벤트, OnDamageTaken 구독 | BossStatsSystem 등 기존 이벤트 패턴과 동일 |
| Unity Object Pool (수동) | 내장 | WaterPuddle 재사용 | D-14 결정, Disable/Enable 기반 풀 패턴 |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| 수동 Object Pool | UnityEngine.Pool.ObjectPool<T> (Unity 2021.1+) | Unity 6에서 공식 지원되나 기존 코드가 수동 풀 패턴 사용 — 일관성 위해 수동 Pool 유지 |
| InvokeRepeating | Coroutine (WaitForSeconds) | 둘 다 동일 효과; Coroutine이 정지/재개 제어 더 쉬움 — 권장 |

---

## Architecture Patterns

### 권장 디렉토리 구조

```
Assets/Enemy/WaterMonster/
├── Script/
│   ├── WaterMonsterController.cs      (기존 — D-03 트리거 추가)
│   ├── WaterMonsterStats.cs           (기존)
│   ├── States/
│   │   └── WaterMonsterCombatState.cs (기존)
│   └── Phase2/                        (신규 폴더)
│       ├── WeatherController.cs       (D-04, D-05, D-06, D-07)
│       ├── PuddleSpawner.cs           (D-08, D-09)
│       ├── WaterPuddle.cs             (D-10, D-11, D-12, D-16)
│       ├── PuddlePool.cs              (D-14)
│       └── PuddleStackManager.cs      (D-18, D-19, D-20)
├── Resources/
│   ├── HealPopup.prefab               (기존)
│   ├── WaterSpitProjectile.prefab     (기존)
│   └── WaterPuddle.prefab             (신규 — PuddlePool이 Resources.Load로 로드)
Assets/Player/Script/
├── SkillScript/
│   └── WaveSlice.cs                   (기존 — WaterPuddle 감지 로직 추가)
└── InputHandler.cs                    (기존 — OnInteractEvent + interactAction 추가)
```

### Pattern 1: WeatherController — ParticleSystem Shape 기반 맵 커버

**What:** WeatherController는 씬에 별도 GameObject로 배치. BoxCollider2D (isTrigger)의 bounds를 ParticleSystem의 Shape 모듈에 반영하여 맵 전체 범위에 비를 내린다.

**When to use:** D-04 결정에 따라 보스와 독립된 컴포넌트. WeatherController.StartRain() / StopRain() 으로 외부에서 제어.

**Example:**
```csharp
// WeatherController.cs (신규)
public class WeatherController : MonoBehaviour
{
    [SerializeField] private ParticleSystem rainParticle;
    [SerializeField] private BoxCollider2D mapBounds; // 씬 인스펙터에서 지정

    public void StartRain()
    {
        // ParticleSystem Shape 모듈에 bounds 적용
        var shape = rainParticle.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(mapBounds.bounds.size.x, 1f, 1f);
        rainParticle.transform.position = mapBounds.bounds.center + Vector3.up * 10f;
        rainParticle.Play();
        // PuddleSpawner 활성화
    }

    public void StopRain()
    {
        rainParticle.Stop();
        // PuddleSpawner 비활성화
    }
}
```

### Pattern 2: WaterMonsterController Phase 2 트리거 (D-03)

**What:** WaterMonsterController가 Awake/Start에서 WaterStats.OnDamageTaken을 구독하고, HP 임계치 도달 시 WeatherController.StartRain() 호출. bool 가드로 한 번만 트리거.

**Critical:** `BossStatsSystem.OnDamageTaken`은 `event Action` (인수 없음)이다. 서명을 맞춰야 한다.

**Example:**
```csharp
// WaterMonsterController.cs 추가 부분
[SerializeField] private WeatherController _weatherController;
[SerializeField] [Range(0f, 1f)] private float _phase2HpThreshold = 0.70f;
private bool _phase2Triggered = false;

protected override void Start()
{
    base.Start();
    WaterStats.OnDamageTaken += CheckPhase2Trigger;
}

private void CheckPhase2Trigger()
{
    if (_phase2Triggered) return;
    if (WaterStats.CurrentHealth / WaterStats.MaxHealth <= _phase2HpThreshold)
    {
        _phase2Triggered = true;
        _weatherController.StartRain();
    }
}

protected override void OnDestroy()
{
    base.OnDestroy(); // BossController.OnDestroy 호출 유지
    if (WaterStats != null) WaterStats.OnDamageTaken -= CheckPhase2Trigger;
}
```

**주의:** `BossController.OnDestroy()`는 `private void OnDestroy()`로 선언되어 있어 override가 불가능하다. WaterMonsterController에서 이벤트를 별도로 해제하려면 BossController.OnDestroy를 `protected virtual void OnDestroy()`로 변경해야 한다. 이것이 Phase 2의 기존 파일 수정 포인트 중 하나다.

### Pattern 3: WaterPuddle 상태 + Trigger 범위 감지

**What:** WaterPuddle MonoBehaviour는 isDestructible bool, CircleCollider2D (isTrigger), SpriteRenderer를 가진다. 플레이어가 Trigger 범위에 진입/이탈 시 playerInRange 플래그를 토글.

**Example:**
```csharp
// WaterPuddle.cs (신규)
public class WaterPuddle : MonoBehaviour
{
    public bool isDestructible = true;
    public bool playerInRange = false;

    [SerializeField] private Color indestructibleColor = new Color(0.3f, 0.3f, 1f, 0.5f);
    private SpriteRenderer _sr;

    private void Awake() { _sr = GetComponent<SpriteRenderer>(); }

    public void SetIndestructible()
    {
        isDestructible = false;
        _sr.color = indestructibleColor;
        PuddleStackManager.Instance.RegisterIndestructible(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    public void OnReturnToPool()
    {
        isDestructible = true;
        playerInRange = false;
        _sr.color = Color.white;
        gameObject.SetActive(false);
    }
}
```

### Pattern 4: WaveSlice WaterPuddle 파괴 추가 (D-12, D-13)

**What:** WaveSlice.waveSlice() 의 OverlapCircleAll 루프에 WaterPuddle 검사 추가. HitBox 검사와 별도로 WaterPuddle 태그를 체크한다.

**현재 WaveSlice 코드 구조 (실제 코드 기반):**
```csharp
foreach (var hit in hits)
{
    if (!hit.CompareTag("HitBox")) continue;  // ← 기존: HitBox만 처리
    // 보스/일반적 처리
}
```

**변경 후 추가 로직:**
```csharp
foreach (var hit in hits)
{
    // WaterPuddle 파괴 처리 (HitBox 체크 이전에 분기)
    if (hit.CompareTag("WaterPuddle"))
    {
        var puddle = hit.GetComponent<WaterPuddle>();
        if (puddle != null && puddle.isDestructible)
        {
            PuddlePool.Instance.Return(puddle);
            // 파괴 VFX (기존 waveEffectPrefab 재사용 또는 신규)
        }
        continue;
    }

    if (!hit.CompareTag("HitBox")) continue;
    // 기존 보스/적 처리 (변경 없음)
}
```

### Pattern 5: InputHandler OnInteractEvent 추가 (D-15)

**What:** InputHandler에 `OnInteractEvent (event Action)` 추가. `interactAction` InputAction 필드 추가. OnEnable/OnDisable에서 구독/해제.

**중요 발견:** `.inputactions` 파일 ("PlayerInputActions.inputactions")에 이미 "Action" 액션이 존재하고 F 키에 바인딩되어 있다. 새 "Interact" 액션을 추가하거나 기존 "Action" 액션 이름을 "Interact"로 변경하는 방식 중 선택 필요.

**권장 접근법 (Claude's Discretion 반영):** 기존 "Action" 액션을 InputHandler에서 `interactAction`으로 연결하고 `OnInteractEvent` 이벤트를 발행. 키는 F로 유지. .inputactions 파일 자체는 수정하지 않거나 "Action" 이름 유지.

**Example:**
```csharp
// InputHandler.cs 추가 내용
public event Action OnInteractEvent;
private InputAction interactAction;

// Awake() 내 추가:
interactAction = playerMap.FindAction("Action"); // 기존 "Action" 액션 재사용

// OnEnable() 내 추가:
if (interactAction != null) interactAction.performed += ctx => OnInteractEvent?.Invoke();

// OnDisable() 내 추가 (이미 inputActions.Disable()이 전체 비활성화하므로 람다 분리가 필요할 수 있음)
```

**주의:** 현재 InputHandler.OnDisable()은 `inputActions.Disable()`만 호출하고 개별 이벤트를 해제하지 않는다. 람다로 구독하면 해제 불가 — 기존 패턴(heal 등)과 동일하게 람다 방식으로 추가하면 일관성 유지된다.

### Pattern 6: PuddleSpawner — Coroutine 기반 주기적 스폰

**What:** PuddleSpawner는 WeatherController에 의해 활성화/비활성화되거나 StartRain/StopRain에서 직접 Coroutine을 제어한다.

**Example:**
```csharp
// PuddleSpawner.cs (신규) — WeatherController 또는 독립 GameObject
public class PuddleSpawner : MonoBehaviour
{
    [SerializeField] private BoxCollider2D spawnBounds;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxPuddles = 10;

    private Coroutine _spawnCoroutine;

    public void StartSpawning()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (PuddlePool.Instance.ActiveCount < maxPuddles)
            {
                Vector2 pos = GetRandomPosition();
                PuddlePool.Instance.Get(pos);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector2 GetRandomPosition()
    {
        Bounds b = spawnBounds.bounds;
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
    }
}
```

### Pattern 7: PuddlePool (Disable/Enable 방식)

**What:** 간단한 수동 Object Pool. List 기반 풀. PuddleSpawner와 분리된 별도 MonoBehaviour 싱글턴으로 구현 권장 (Claude's Discretion).

**Example:**
```csharp
// PuddlePool.cs (신규)
public class PuddlePool : MonoBehaviour
{
    public static PuddlePool Instance { get; private set; }
    [SerializeField] private GameObject puddlePrefab;
    [SerializeField] private int initialSize = 15;

    private List<WaterPuddle> _pool = new List<WaterPuddle>();
    public int ActiveCount => _pool.Count(p => p.gameObject.activeSelf);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        for (int i = 0; i < initialSize; i++) CreateNew();
    }

    private WaterPuddle CreateNew()
    {
        var go = Instantiate(puddlePrefab, transform);
        var p = go.GetComponent<WaterPuddle>();
        go.SetActive(false);
        _pool.Add(p);
        return p;
    }

    public WaterPuddle Get(Vector2 position)
    {
        var puddle = _pool.FirstOrDefault(p => !p.gameObject.activeSelf)
                     ?? CreateNew();
        puddle.transform.position = position;
        puddle.isDestructible = true;
        puddle.gameObject.SetActive(true);
        return puddle;
    }

    public void Return(WaterPuddle puddle)
    {
        puddle.OnReturnToPool();
    }
}
```

### Anti-Patterns to Avoid

- **WaterPuddle마다 Destroy() 호출:** 웅덩이 스폰/파괴가 빈번하면 GC 스파이크 발생 — D-14 결정대로 반드시 Object Pool 사용.
- **Phase 2 트리거를 Update()에서 매 프레임 HP 체크:** `OnDamageTaken` 이벤트 구독으로 체크 — Update polling 금지.
- **PuddleStackManager를 static 클래스로 구현:** MonoBehaviour 싱글턴으로 구현해야 씬 라이프사이클에 맞게 정리됨.
- **WaterPuddle CircleCollider2D가 레이어 필터 없이 모든 Collider 감지:** OnTriggerEnter2D 내에서 `CompareTag("Player")` 체크 필수 (REQ-WM-X-01 레이어 분리).
- **WaveSlice의 WaterPuddle 체크를 HitBox 루프 안에 넣는 실수:** WaterPuddle은 HitBox 태그가 없으므로 `if (!hit.CompareTag("HitBox")) continue;` 앞에서 WaterPuddle을 먼저 분기해야 한다.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 주기적 스폰 타이밍 | 커스텀 타이머 클래스 | Coroutine + WaitForSeconds | Unity 내장, 이미 프로젝트 전반에서 사용 중 |
| 맵 랜덤 위치 | 커스텀 spatial distribution | Random.Range + BoxCollider2D.bounds | D-09 결정, bounds 읽기가 인스펙터 친화적 |
| 비 이펙트 렌더링 | 커스텀 셰이더 기반 비 | ParticleSystem | D-05 결정, 복잡도 불필요 |
| Object Pool | 범용 제네릭 풀 프레임워크 | 수동 List<WaterPuddle> 풀 | Phase 2 WaterPuddle 전용이므로 YAGNI |

**Key insight:** Phase 2의 모든 기술적 문제는 Unity 내장 기능으로 해결 가능하다. 외부 패키지 추가 없음.

---

## Critical Code Integration Notes

### 1. WaterController.AddWater() 미존재 문제 (HIGH)

**D-17에서 `WaterController.AddWater(amount)` 호출을 지시하고 있으나, 실제 WaterController에는 해당 메서드가 없다.**

실제 존재하는 메서드:
- `RecoveryWater()` — 빈 병 하나를 순수 물로 채움
- `RecoveryCorruptedWater()` — 빈 병 하나를 오염된 물로 채움
- `UseBottle(int amount)` — 채워진 병 N개를 소비

**해결 방안 (Claude's Discretion):**
- 옵션 A: WaterController에 `AddWater(int amount)` 신규 추가 — `RecoveryWater()`를 amount번 반복 호출
- 옵션 B: 흡수 시 `RecoveryWater()`를 1회 호출 (1회 흡수 = 1병 회복)

플래너는 옵션 B를 권장한다: 흡수 1회 = 물 1병 회복이 가장 단순하며, WaterController 수정이 불필요하다.

### 2. BossController.OnDestroy() private 문제 (MEDIUM)

BossController의 `OnDestroy()`는 `private void OnDestroy()`로 선언되어 override 불가. WaterMonsterController에서 Phase 2 이벤트 구독(`OnDamageTaken -= CheckPhase2Trigger`)을 해제하려면:

**해결 방안:** BossController.cs의 `private void OnDestroy()` → `protected virtual void OnDestroy()`로 변경 (1줄 수정). Phase 1에서 이미 BossStatsSystem 수정을 위한 기존 파일 수정이 있었으므로 허용 범위 내.

### 3. Interact 키 바인딩 확인 결과 (HIGH)

`.inputactions` ("PlayerInputActions.inputactions") 파일 분석 결과:
- 현재 사용 중인 키: W/A/S/D (이동), Space (점프), LShift (달리기), Escape (일시정지), LMB (기본공격), E (Skill_1), R (Skill_2), 1 (힐), F (Action)
- **E 키는 Skill_1에 이미 할당되어 있다** — D-15 "권장: E 키"는 불가
- **F 키가 "Action" 액션에 이미 존재함** — F 키를 Interact로 사용하는 것이 가장 자연스럽다
- 대안 미사용 키: Q, G, X 등

**플래너 결정 권고:** "Action" 액션을 `interactAction`으로 InputHandler에 연결하고 `OnInteractEvent`를 발행. 키는 F. .inputactions 파일에서 "Action" 액션 이름을 "Interact"로 변경하거나 유지.

### 4. BossStatsSystem.OnDamageTaken 서명 확인 (HIGH)

실제 코드:
```csharp
public event Action OnDamageTaken;  // 인수 없음 (Action, not Action<float>)
```

D-03의 트리거 구현 시 `void CheckPhase2Trigger()` (인수 없음)으로 구현해야 한다.

또한 `BossStatsSystem.TakeDamage(DamageInfo)` 현재 구현에서:
- `IsBarrierActive` (즉 `_currentWater > 0`)가 true인 경우에만 `OnDamageTaken`을 발화한다.
- `WaterMonsterStats`의 경우 MaxWater=0이므로 IsBarrierActive는 항상 false.
- **따라서 기존 BossStatsSystem.TakeDamage(DamageInfo)에서 OnDamageTaken이 발화되지 않는다.**
- WaterMonsterStats.TakeDamage(DamageInfo) override에서 base.TakeDamage(info)를 호출하는 Non-water 경로에서도 OnDamageTaken이 발화되지 않는다.

**결론:** D-03 트리거를 위해 WaterMonsterStats.TakeDamage override에서 직접 OnDamageTaken을 발화하거나, WaterMonsterStats에 별도 `OnWaterMonsterDamageTaken` 이벤트를 추가해야 한다.

**플래너 권고:** WaterMonsterStats.TakeDamage override 끝에서 `OnDamageTaken?.Invoke()`를 명시적으로 호출하도록 수정 (가장 단순).

---

## Common Pitfalls

### Pitfall 1: WaveSlice의 WaterPuddle HitBox 태그 미처리

**What goes wrong:** WaterPuddle에 "WaterPuddle" 태그를 붙여도, WaveSlice의 OverlapCircleAll 루프가 `if (!hit.CompareTag("HitBox")) continue;`로 HitBox가 아닌 콜라이더를 전부 스킵한다.
**Why it happens:** WaterPuddle의 Collider2D는 "HitBox" 태그가 없어 루프에서 즉시 스킵.
**How to avoid:** WaterPuddle 처리를 `if (!hit.CompareTag("HitBox")) continue;` 라인 **이전**에 배치. 아래 순서:
  1. `if (hit.CompareTag("WaterPuddle"))` → 처리 후 continue
  2. `if (!hit.CompareTag("HitBox")) continue;`
  3. 기존 보스/적 처리

### Pitfall 2: Phase 2 트리거가 발화되지 않는 문제

**What goes wrong:** `WaterMonsterController`가 `WaterStats.OnDamageTaken`을 구독해도 이벤트가 발화되지 않아 비가 시작되지 않음.
**Why it happens:** `BossStatsSystem.TakeDamage(DamageInfo)`의 현재 구현은 `IsBarrierActive == true`일 때만 `OnDamageTaken`을 발화한다. WaterMonster는 MaxWater=0이므로 IsBarrierActive는 항상 false → 이벤트 미발화.
**How to avoid:** WaterMonsterStats.TakeDamage(DamageInfo) override에서 Non-water 경로의 base.TakeDamage(info) 호출 후 (또는 base 내부) `OnDamageTaken?.Invoke()`를 명시적으로 발화하도록 수정.

### Pitfall 3: Object Pool Reset 누락

**What goes wrong:** 파괴된 WaterPuddle이 풀에 반환될 때 isDestructible/playerInRange/색상이 리셋되지 않아 다음에 스폰될 때 Indestructible 상태로 등장.
**Why it happens:** Return 시 상태 초기화를 누락.
**How to avoid:** `WaterPuddle.OnReturnToPool()` 메서드에서 반드시 isDestructible=true, playerInRange=false, SpriteRenderer.color=Color.white 초기화.

### Pitfall 4: PuddleStackManager 씬 전환 시 카운트 오염

**What goes wrong:** PuddleStackManager가 DontDestroyOnLoad로 설정되면 씬 전환 후에도 이전 카운트가 남음.
**Why it happens:** InputHandler는 DontDestroyOnLoad로 싱글턴화. PuddleStackManager도 동일하게 하면 문제.
**How to avoid:** PuddleStackManager는 DontDestroyOnLoad를 사용하지 않는다. 씬 내 일반 GameObject로 배치.

### Pitfall 5: WaterPuddle CircleCollider2D가 보스와도 충돌

**What goes wrong:** CircleCollider2D isTrigger가 보스의 Collider2D와도 OnTriggerEnter2D를 발화해 playerInRange가 잘못 설정.
**Why it happens:** 태그 체크 없이 playerInRange=true 설정.
**How to avoid:** OnTriggerEnter/Exit에서 `other.CompareTag("Player")` 체크 필수.

---

## Code Examples

### WaveSlice 확장 전체 패턴

```csharp
// Source: 기존 WaveSlice.cs 코드 기반 확장
// Assets/Player/Script/SkillScript/WaveSlice.cs
public void waveSlice()
{
    if (waterController.waterCounter() + waterController.corruptedwaterCounter() >= 2)
    {
        waterController.UseBottle(2);
        GameObject wave = Instantiate(waveEffectPrefab, transform.position, Quaternion.identity);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            // [추가] WaterPuddle 파괴 처리 — HitBox 체크 이전에 분기
            if (hit.CompareTag("WaterPuddle"))
            {
                var puddle = hit.GetComponent<WaterPuddle>();
                if (puddle != null && puddle.isDestructible)
                    PuddlePool.Instance.Return(puddle);
                continue;
            }

            if (!hit.CompareTag("HitBox")) continue;

            // [기존 유지] 보스 우선 검색
            var bossStats = hit.GetComponentInParent<BossStatsSystem>();
            if (bossStats != null)
            {
                bossStats.TakeDamageInfo(new DamageInfo { amount = damage, element = element });
                continue;
            }
            // [기존 유지] 일반 적
            HP target = hit.GetComponentInParent<HP>();
            if (target != null) target.TakeDamage(damage);
        }
        Destroy(wave, 1.0f);
    }
}
```

### PuddleStackManager 싱글턴

```csharp
// Assets/Enemy/WaterMonster/Script/Phase2/PuddleStackManager.cs
using System;
using UnityEngine;

public class PuddleStackManager : MonoBehaviour
{
    public static PuddleStackManager Instance { get; private set; }

    [SerializeField] private int _threshold = 5;
    private int _indestructibleCount = 0;

    public event Action OnThresholdReached; // Phase 3가 구독

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterIndestructible(WaterPuddle puddle)
    {
        _indestructibleCount++;
        if (_indestructibleCount >= _threshold)
            OnThresholdReached?.Invoke();
    }

    // Phase 3 연계: Indestructible 해제 시 카운트 감소 (선택적)
    public void UnregisterIndestructible()
    {
        _indestructibleCount = Mathf.Max(0, _indestructibleCount - 1);
    }

    public int Count => _indestructibleCount;
}
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Destroy() 기반 스폰/파괴 | Object Pool (Disable/Enable) | Phase 2 설계 시 | GC 최적화 |
| Update() HP polling | event Action OnDamageTaken 구독 | Phase 1 결정 | 효율적 트리거 |
| 고정 하드코딩 임계치 | SerializeField 인스펙터 튜닝 | Phase 설계 원칙 | 밸런싱 편의 |

---

## Open Questions

1. **WaterMonsterStats.TakeDamage override에서 OnDamageTaken 발화 누락**
   - What we know: 현재 BossStatsSystem은 IsBarrierActive=true일 때만 OnDamageTaken 발화. WaterMonster는 항상 false.
   - What's unclear: Phase 1 구현 시 이를 의도적으로 수정했는지 여부 (Phase 1 SUMMARY 미확인).
   - Recommendation: 플래너가 WaterMonsterStats 수정 태스크에서 OnDamageTaken 발화 코드 추가를 명시.

2. **WaterPuddle 스프라이트/색상 에셋 부재**
   - What we know: Assets/Sprites/ 에는 Project.psb와 Unit.prefab만 존재. WaterPuddle용 전용 스프라이트 없음.
   - What's unclear: 기존 SpriteRenderer에 임시 색상(파란 사각형 등)으로 대체 가능한지.
   - Recommendation: 플래너가 "씬 셋업" 태스크에서 WaterPuddle 프리팹을 단색 원형 스프라이트(GameObject > 2D Object > Sprite) 또는 색상 오버라이드로 임시 처리 지시.

3. **BossController.OnDestroy() 접근성**
   - What we know: 현재 `private void OnDestroy()`.
   - What's unclear: Phase 1 계획들이 이를 변경했는지 여부.
   - Recommendation: 플래너가 Phase 2 트리거 태스크에서 BossController.OnDestroy를 `protected virtual`로 변경하는 1줄 수정을 명시.

---

## Environment Availability

Phase 2는 외부 도구/서비스 의존 없음 — 모두 Unity 에디터 내 순수 C# 코드 작성 및 씬 컴포넌트 배치 작업.

Step 2.6: SKIPPED (no external dependencies beyond Unity Editor itself)

---

## Project Constraints (from CLAUDE.md)

CLAUDE.md 파일이 프로젝트 루트에 존재하지 않음. 대신 PROJECT.md에서 추출한 프로젝트 공통 원칙:

- **상속 기반 확장:** 기존 BossController/BossStatsSystem을 상속. 기존 스크립트 수정 최소화 (필요한 최소한만).
- **네임스페이스 없음:** 전체 프로젝트가 global namespace 사용. 신규 스크립트도 namespace 금지.
- **Unity 6 API:** `Rigidbody2D.linearVelocity` 등 Unity 6 전용 API 사용. `velocity` 대신 `linearVelocity`.
- **레이어 분리:** 보스 공격/효과는 Player 레이어에만 영향 (REQ-WM-X-01). WaterPuddle CircleCollider2D의 Trigger도 Player 태그/레이어 체크 필수.
- **State Pattern:** 보스 AI는 IBossState 기반 State Pattern 유지. Phase 2는 보스 State 추가 없음 (트리거만 WaterMonsterController에 추가).
- **인스펙터 튜닝 가능:** 수치 하드코딩 금지, `[SerializeField]` 사용.

---

## Validation Architecture

`workflow.nyquist_validation` = false (`.planning/config.json`에서 확인). 이 섹션은 생략한다.

---

## Sources

### Primary (HIGH confidence)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — 직접 읽음: Phase 2 트리거 추가 대상 구조 확인
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — 직접 읽음: TakeDamage override 구조, OnDamageTaken 미발화 이슈 확인
- `Assets/Enemy/NewBoss/Script/BossStatesSystem.cs` — 직접 읽음: OnDamageTaken은 event Action (인수 없음), IsBarrierActive 조건 확인
- `Assets/Enemy/NewBoss/Script/BossController.cs` — 직접 읽음: OnDestroy private 문제 확인
- `Assets/Player/Script/SkillScript/WaveSlice.cs` — 직접 읽음: OverlapCircleAll 루프 패턴, HitBox 태그 의존 구조 확인
- `Assets/Player/Script/InputHandler.cs` — 직접 읽음: 이벤트 구조, 기존 액션 목록 확인
- `Assets/Player/Script/PlayerInputActions.inputactions` — 직접 읽음: E키=Skill_1, F키=Action(미사용), 키 할당 현황 전수 확인
- `Assets/Player/Script/WaterController.cs` — 직접 읽음: AddWater() 미존재, RecoveryWater() 구조 확인
- `Assets/Player/Script/PlayerAttackBase.cs` — 직접 읽음: InputHandler 이벤트 구독 패턴 확인
- `Assets/Script/Combat/DamageInfo.cs` — 직접 읽음: DamageElement enum 구조 확인
- `.planning/phases/02-weather-puddle-interaction/02-CONTEXT.md` — 결정사항 D-01~D-20 전수 확인

### Secondary (MEDIUM confidence)
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — 직접 읽음: Phase 1 결과물 패턴 확인
- `.planning/phases/01-boss-core-mechanics/01-01-PLAN.md` — 직접 읽음: 플랜 파일 포맷 및 태스크 구조 참조

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 모든 컴포넌트를 실제 소스에서 직접 확인
- Architecture: HIGH — 기존 코드 패턴과 결정사항 D-01~D-20 전수 검토 완료
- Pitfalls: HIGH — 실제 코드 분석으로 발견된 구체적 문제점 (OnDamageTaken 미발화, HitBox 태그 스킵, WaterController.AddWater 미존재)

**Research date:** 2026-04-10
**Valid until:** 2026-05-10 (안정적 Unity 내장 스택 — 30일 유효)
