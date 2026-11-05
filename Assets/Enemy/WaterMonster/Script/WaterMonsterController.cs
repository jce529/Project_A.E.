// Phase 1 — WaterMonsterController
// Decisions: D-08 (inherit BossController), REQ-WM-03 (reuse hierarchy)
// NOTE: Because ChaseState instantiates `new CombatState()` directly, this
// controller intercepts state changes in Update() and swaps a plain
// CombatState for WaterMonsterCombatState on the next frame.
// The cleaner long-term fix is a virtual CreateCombatState() factory on
// BossController, but that is out of scope for Phase 1 (HIGH-RISK #4).

using UnityEngine;
using System.Collections;
using WaterMonster.Phase2;

[RequireComponent(typeof(WaterMonsterStats))]
public class WaterMonsterController : BossController
{
    [Header("Phase 2 Settings")]
    [SerializeField] private WeatherController _weatherController;
    [SerializeField] [Range(0f, 1f)] private float _phase2HpThreshold = 0.70f;

    [Header("Phase 3 Settings")]
    [SerializeField] private float _teleportCooldown = 8f;
    private float _lastTeleportTime = -999f;
    [SerializeField] [Range(0f,1f)] private float _phase3HpThreshold = 0.50f;
    public bool IsPhase3 { get; private set; } = false;

    [Header("Phase 3 Empowerment")]
    [SerializeField] [Range(0f, 1f)] private float _enrageHpThreshold = 0.30f;
    // HP 30% phase 3 enhancement; no self-inflicted HP cost.
    public bool IsEnraged { get; private set; } = false;
    private bool _enrageTriggered = false;

    [Header("Groggy / Phase Transition")]
    [SerializeField] private float _groggyDuration = 3f;
    [SerializeField] private float _phaseTransitionDuration = 1.5f;
    [SerializeField] private float _groggyDamageMultiplier = 1f;
    [SerializeField] private bool _groggyOnEmpower = false;

    public float GroggyDuration => _groggyDuration;
    public float PhaseTransitionDuration => _phaseTransitionDuration;
    public float GroggyDamageMultiplier => _groggyDamageMultiplier;
    public int CurrentPhase { get; private set; } = 1;
    public event System.Action<int> PhaseEntered;
    private int _pendingPhase;
    private bool _pendingEmpower;
    public bool HasPendingPhaseTransition => _pendingPhase > CurrentPhase || _pendingEmpower;

    [Header("Battle Area")]
    [SerializeField] private BoxCollider2D _battleAreaBounds;
    private bool _battleAreaDetached;

    [Header("Phase 3 Enhancement")]
    [SerializeField] private float _phase3DamageMul = 1.2f;
    [SerializeField] private float _phase3RadiusMul = 1.15f;
    [SerializeField] private float _phase3WindupMul = 0.85f;
    [SerializeField] private float _empoweredDamageMul = 1.4f;
    [SerializeField] private float _empoweredRadiusMul = 1.3f;
    [SerializeField] private float _empoweredWindupMul = 0.7f;
    [SerializeField] private float _phase3CooldownMultiplier = 0.75f;
    [SerializeField] private float _empoweredCooldownMultiplier = 0.5f;
    [SerializeField] private int _phase3MaxConcurrentPatterns = 2;

    public float EnhanceDamageMul => IsEnraged ? _empoweredDamageMul : IsPhase3 ? _phase3DamageMul : 1f;
    public float EnhanceRadiusMul => IsEnraged ? _empoweredRadiusMul : IsPhase3 ? _phase3RadiusMul : 1f;
    public float EnhanceWindupMul => IsEnraged ? _empoweredWindupMul : IsPhase3 ? _phase3WindupMul : 1f;
    public float PatternCooldownMultiplier => IsEnraged ? _empoweredCooldownMultiplier : IsPhase3 ? _phase3CooldownMultiplier : 1f;
    public int Phase3MaxConcurrentPatterns => _phase3MaxConcurrentPatterns;

    public int ActivePatternCount { get; private set; }
    public bool IsPatternRunning => ActivePatternCount > 0;
    public PatternHistory PatternHistory { get; } = new PatternHistory();

    public Coroutine RunPattern(IEnumerator routine)
    {
        return StartCoroutine(TrackPattern(routine));
    }

    private IEnumerator TrackPattern(IEnumerator routine)
    {
        ActivePatternCount++;
        try
        {
            yield return StartCoroutine(routine);
        }
        finally
        {
            ActivePatternCount = Mathf.Max(0, ActivePatternCount - 1);
        }
    }

    [Header("Phase 4 Zone Settings")]
    [SerializeField] private GameObject _speedUpZonePrefab;
    [SerializeField] private GameObject _slowDownZonePrefab;
    [SerializeField] private float _zoneCooldown = 5f;
    [SerializeField] private float _zoneDuration = 8f;
    [SerializeField] private int _maxActiveZones = 4;

    private float _lastZoneTime = -999f;
    private int _activeZoneCount = 0;

    public bool IsPhase2 { get; private set; } = false;

    public WaterMonsterStats WaterStats { get; private set; }
    private bool _started;

    protected override void Awake()
    {
        base.Awake();
        WaterStats = Stats as WaterMonsterStats;
        if (WaterStats == null)
        {
            Debug.LogError($"[WaterMonsterController] Stats component must be WaterMonsterStats (found {Stats?.GetType().Name ?? "null"}).", this);
        }

        SetupHitBox();
        if (_battleAreaBounds != null && _battleAreaBounds.transform.IsChildOf(transform))
        {
            _battleAreaBounds.transform.SetParent(null, true);
            _battleAreaDetached = true;
        }
    }

    private void SetupHitBox()
    {
        // 자식 중에서 "HitBox" 태그를 가진 오브젝트 찾기
        foreach (Transform child in transform)
        {
            if (child.CompareTag("HitBox"))
            {
                // 1. Z축 고정 (충돌 방해 요소 제거)
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, 0);

                // 2. 레이어 설정 (Enemy 레이어가 있다면 사용, 없으면 Default)
                int enemyLayer = LayerMask.NameToLayer("Enemy");
                if (enemyLayer != -1) child.gameObject.layer = enemyLayer;

                // 3. Collider 설정 보강
                var col = child.GetComponent<Collider2D>();
                if (col != null)
                {
                    col.isTrigger = true; // Trigger로 변경하여 감지 확률 높임
                }

                // 4. EnemyHitBox 컴포넌트 추가
                if (child.GetComponent<EnemyHitBox>() == null)
                {
                    child.gameObject.AddComponent<EnemyHitBox>();
                }
                return;
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        _started = true;
        WaterMonsterPuddleSystem.Ensure(this);
    }

    protected override void OnDestroy()
    {
        if (WaterMonsterPuddleSystem.Instance != null)
            WaterMonsterPuddleSystem.Instance.Unbind(this);
        base.OnDestroy();
        if (_battleAreaDetached && _battleAreaBounds != null)
            Destroy(_battleAreaBounds.gameObject);
    }

    private void OnDisable()
    {
        if (_weatherController != null)
            _weatherController.StopRain();

        if (WaterMonsterPuddleSystem.Instance != null
            && WaterMonsterPuddleSystem.Instance.BoundBoss == this)
        {
            WaterMonsterPuddleSystem.Instance.Unbind(this);
            if (PuddlePool.Instance != null)
                PuddlePool.Instance.ReturnAll();
        }

        ActivePatternCount = 0;
        if (_battleAreaDetached && _battleAreaBounds != null)
            _battleAreaBounds.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_battleAreaDetached && _battleAreaBounds != null)
            _battleAreaBounds.gameObject.SetActive(true);
        if (_started) WaterMonsterPuddleSystem.Ensure(this);
    }

    /// <summary>
    /// Checks if teleport cooldown has elapsed.
    /// </summary>
    public bool CanTeleport()
    {
        return Time.time - _lastTeleportTime >= _teleportCooldown;
    }

    /// <summary>
    /// Records the time of teleportation. Called by WaterTeleportState.Enter.
    /// </summary>
    public void RecordTeleportTime()
    {
        _lastTeleportTime = Time.time;
    }

    private void EvaluatePhaseThresholds()
    {
        if (WaterStats == null) return;

        float ratio = WaterStats.CurrentHealth / WaterStats.MaxHealth;
        int target = ratio <= _phase3HpThreshold ? 3 : ratio <= _phase2HpThreshold ? 2 : 1;
        if (target > CurrentPhase)
            _pendingPhase = Mathf.Max(_pendingPhase, target);

        if (CurrentPhase >= 3 && !_enrageTriggered && !_pendingEmpower && ratio <= _enrageHpThreshold)
        {
            if (_groggyOnEmpower) _pendingEmpower = true;
            else ActivateEnrage();
        }

        if (HasPendingPhaseTransition && !IsPatternRunning
            && !(CurrentState is WaterMonsterGroggyState)
            && !(CurrentState is WaterTeleportState))
        {
            ChangeState(new WaterMonsterGroggyState(
                _pendingPhase > CurrentPhase ? _pendingPhase : CurrentPhase,
                _pendingPhase <= CurrentPhase && _pendingEmpower));
        }
    }

    public void CompletePhaseTransition(int phase)
    {
        CurrentPhase = Mathf.Max(CurrentPhase, phase);
        IsPhase2 = CurrentPhase >= 2;
        IsPhase3 = CurrentPhase >= 3;
        if (_pendingPhase <= CurrentPhase) _pendingPhase = 0;
        if (CurrentPhase >= 2 && _weatherController != null)
            _weatherController.StartRain();
        PhaseEntered?.Invoke(CurrentPhase);
    }

    public void ActivateEnrage()
    {
        _enrageTriggered = true;
        IsEnraged = true;
        _pendingEmpower = false;
    }

    protected override void Update()
    {
        base.Update();

        // Swap any plain CombatState for WaterMonsterCombatState so the
        // IsBarrierActive guard override (HIGH-RISK #1) takes effect.
        // NOTE: WaterTeleportState does not inherit from CombatState, so it is naturally ignored.
        if (CurrentState != null
            && CurrentState.GetType() == typeof(CombatState))
        {
            ChangeState(new WaterMonsterCombatState());
        }

        EvaluatePhaseThresholds();
    }

    public bool CanSpawnZone()
    {
        return Time.time - _lastZoneTime >= _zoneCooldown
               && _activeZoneCount < _maxActiveZones;
    }

    public void RecordZoneTime()
    {
        _lastZoneTime = Time.time;
    }

    public void SpawnRandomZone()
    {
        if (_battleAreaBounds == null) return;

        GameObject prefab = Random.value > 0.5f ? _speedUpZonePrefab : _slowDownZonePrefab;
        if (prefab == null) return;

        Bounds b = _battleAreaBounds.bounds;
        Vector2 pos = new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y));

        var zone = Instantiate(prefab, pos, Quaternion.identity);
        _activeZoneCount++;
        StartCoroutine(DestroyZoneAfter(zone, _zoneDuration));
    }

    private IEnumerator DestroyZoneAfter(GameObject zone, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (zone != null)
        {
            Destroy(zone);
            _activeZoneCount--;
        }
    }

    [Header("Pattern Prefabs")]
    [SerializeField] private GameObject _geyserEffectPrefab;
    [SerializeField] private GameObject _prisonProjectilePrefab;
    [SerializeField] private GameObject _colorPrisonPrefab;
    [SerializeField] private GameObject _wavePrefab;
    [SerializeField] private int _colorPrisonZoneCount = 4;

    public GameObject GeyserEffectPrefab => _geyserEffectPrefab;
    public GameObject PrisonProjectilePrefab => _prisonProjectilePrefab;
    public GameObject ColorPrisonPrefab => _colorPrisonPrefab;
    public GameObject WavePrefab => _wavePrefab;
    public BoxCollider2D BattleAreaBounds => _battleAreaBounds;
    public BoxCollider2D ColorPrisonSpawnBounds => _battleAreaBounds;
    public int ColorPrisonZoneCount => _colorPrisonZoneCount;

    public PrisonColor CoreColor { get; private set; }
    public void SetCoreColor(PrisonColor color) { CoreColor = color; }
}
