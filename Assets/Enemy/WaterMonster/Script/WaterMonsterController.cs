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
    private bool _phase2Triggered = false;

    [Header("Phase 3 Settings")]
    [SerializeField] private float _teleportCooldown = 8f;
    private float _lastTeleportTime = -999f;
    [SerializeField] [Range(0f,1f)] private float _phase3HpThreshold = 0.50f;
    private bool _phase3Triggered = false;
    public bool IsPhase3 { get; private set; } = false;

    [Header("Phase 4 Settings")]
    [SerializeField] [Range(0f, 1f)] private float _enrageHpThreshold = 0.30f;
    public bool IsEnraged { get; private set; } = false;
    private bool _enrageTriggered = false;

    [Header("Battle Area")]
    [SerializeField] private BoxCollider2D _battleAreaBounds;

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

    protected override void Awake()
    {
        base.Awake();
        WaterStats = Stats as WaterMonsterStats;
        if (WaterStats == null)
        {
            Debug.LogError($"[WaterMonsterController] Stats component must be WaterMonsterStats (found {Stats?.GetType().Name ?? "null"}).", this);
        }

        SetupHitBox();
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
        if (WaterStats != null)
        {
            WaterStats.OnDamageTaken += CheckPhase2Trigger;
            WaterStats.OnDamageTaken += CheckPhase3Trigger;
            WaterStats.OnDamageTaken += CheckEnrageTrigger;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (WaterStats != null)
        {
            WaterStats.OnDamageTaken -= CheckPhase2Trigger;
            WaterStats.OnDamageTaken -= CheckPhase3Trigger;
            WaterStats.OnDamageTaken -= CheckEnrageTrigger;
        }
    }

    private void OnDisable()
    {
        if (_weatherController != null)
            _weatherController.StopRain();

        if (WaterMonster.Phase2.PuddlePool.Instance != null)
            WaterMonster.Phase2.PuddlePool.Instance.ReturnAll();
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

    private void CheckPhase2Trigger()
    {
        if (_phase2Triggered) return;

        if (WaterStats.CurrentHealth / WaterStats.MaxHealth <= _phase2HpThreshold)
        {
            _phase2Triggered = true;
            IsPhase2 = true;
            if (_weatherController != null)
                _weatherController.StartRain();
        }
    }

    private void CheckPhase3Trigger()
    {
        if (_phase3Triggered) return;
        if (WaterStats.CurrentHealth / WaterStats.MaxHealth <= _phase3HpThreshold)
        {
            _phase3Triggered = true;
            IsPhase3 = true;
        }
    }

    private void CheckEnrageTrigger()
    {
        if (_enrageTriggered) return;
        if (WaterStats.CurrentHealth / WaterStats.MaxHealth <= _enrageHpThreshold)
        {
            ActivateEnrage();
        }
    }

    private void ActivateEnrage()
    {
        _enrageTriggered = true;
        IsEnraged = true;
        WaterStats.SetEnraged(true);
        if (CurrentState is WaterMonsterCombatState cs)
            cs.SetEnraged(true);
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

        // Phase 4: Enrage fallback check (tick drain doesn't fire OnDamageTaken)
        if (!_enrageTriggered && WaterStats != null
            && WaterStats.CurrentHealth / WaterStats.MaxHealth <= _enrageHpThreshold)
        {
            ActivateEnrage();
        }
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
