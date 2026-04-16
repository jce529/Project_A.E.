// Phase 1 — WaterMonsterController
// Decisions: D-08 (inherit BossController), REQ-WM-03 (reuse hierarchy)
// NOTE: Because ChaseState instantiates `new CombatState()` directly, this
// controller intercepts state changes in Update() and swaps a plain
// CombatState for WaterMonsterCombatState on the next frame.
// The cleaner long-term fix is a virtual CreateCombatState() factory on
// BossController, but that is out of scope for Phase 1 (HIGH-RISK #4).

using UnityEngine;
using WaterMonster.Phase2;

[RequireComponent(typeof(WaterMonsterStats))]
public class WaterMonsterController : BossController
{
    [Header("Phase 2 Settings")]
    [SerializeField] private WeatherController _weatherController;
    [SerializeField] [Range(0f, 1f)] private float _phase2HpThreshold = 0.70f;
    private bool _phase2Triggered = false;

    [Header("Phase 3 Settings")]
    [SerializeField] private float _teleportCooldown = 8f; // D-14: Inspector tuning
    private float _lastTeleportTime = -999f;

    public WaterMonsterStats WaterStats { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        WaterStats = Stats as WaterMonsterStats;
        if (WaterStats == null)
        {
            Debug.LogError($"[WaterMonsterController] Stats component must be WaterMonsterStats (found {Stats?.GetType().Name ?? "null"}).", this);
        }
    }

    protected override void Start()
    {
        base.Start();
        if (WaterStats != null)
            WaterStats.OnDamageTaken += CheckPhase2Trigger;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (WaterStats != null)
            WaterStats.OnDamageTaken -= CheckPhase2Trigger;
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
            if (_weatherController != null)
                _weatherController.StartRain();
        }
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
    }

    /// <summary>
    /// Convenience: directly enter WaterMonsterCombatState. Used by test harnesses.
    /// </summary>
    public void EnterWaterCombat()
    {
        ChangeState(new WaterMonsterCombatState());
    }
}
