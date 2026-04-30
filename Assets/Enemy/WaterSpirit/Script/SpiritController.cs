using UnityEngine;

[RequireComponent(typeof(SpiritStats))]
public class SpiritController : BossController
{
    [Header("Spirit Combat Ranges")]
    [SerializeField] public float RepelRange = 1.5f;     // D-02a
    [SerializeField] public float ChargeRange = 5.0f;    // D-02a
    // ProjectileRange is implicitly anything beyond ChargeRange (D-03a)

    [Header("Charge Settings (S1-01)")]
    [SerializeField] public float ChargeWindup = 0.5f;    // D-04e
    [SerializeField] public float ChargeSpeed = 12f;      // D-04e
    [SerializeField] public float OvershotDistance = 2.0f; // D-04e
    [SerializeField] public float ChargeDamage = 15f;

    [Header("Repel Settings (S1-03)")]
    [SerializeField] public float RepelDamage = 10f;
    [SerializeField] public float RepelForce = 8f;

    [Header("Projectile (S1-02)")]
    [SerializeField] public GameObject ProjectilePrefab;
    [SerializeField] public float ProjectileDamage = 12f;

    public bool IsCharging { get; private set; }
    private bool _hasHitPlayerThisCharge;

    public bool IsDummy => (Stats as SpiritStats)?.IsDummy ?? false;

    public void SetCharging(bool value)
    {
        IsCharging = value;
        if (value) _hasHitPlayerThisCharge = false;
    }

    public void SetVelocity(Vector2 vel)
    {
        // Unity 6 Physics 2D: use linearVelocity instead of velocity
        _rb.linearVelocity = vel;
    }

    protected override void Update()
    {
        base.Update();

        // CORE-01: Intercept CombatState and swap with SpiritCombatState
        if (CurrentState != null && CurrentState.GetType() == typeof(CombatState))
        {
            ChangeState(new SpiritCombatState());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // D-04d: Apply damage only once per charge
        if (!IsCharging || _hasHitPlayerThisCharge) return;

        // Filter for Player layer
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Player")) == 0) return;

        var ps = other.GetComponentInParent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(ChargeDamage);
            _hasHitPlayerThisCharge = true;
            Debug.Log($"[SpiritController] Charge Hit! Damage: {ChargeDamage}");
        }
    }
}
