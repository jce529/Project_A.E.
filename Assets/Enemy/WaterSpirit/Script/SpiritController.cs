using UnityEngine;

[RequireComponent(typeof(SpiritStats))]
public class SpiritController : BossController
{
    [Header("Spirit Combat Ranges")]
    public float RepelRange = 1.5f;

    [Header("Charge Settings")]
    public float ChargeWindup = 0.5f;
    public float ChargeSpeed = 20f;
    public float OvershotDistance = 3.0f;
    public float ChargeDamage = 15f;

    [Header("Repel Settings")]
    public float RepelDamage = 10f;
    public float RepelForce = 8f;

    [Header("Projectile Settings")]
    public GameObject ProjectilePrefab;
    public float ProjectileDamage = 12f;

    [Header("Stage 2 Settings")]
    public GameObject DummyPrefab;
    [Range(0.1f, 3f)] public float StealthDuration = 0.5f;
    [Range(1f, 10f)] public float MinTeleportRadius = 3f;
    [Range(1f, 15f)] public float MaxTeleportRadius = 6f;

    [Header("Detection")]
    public LayerMask PlayerLayer; // 에디터에서 설정 가능하도록 노출

    public bool IsCharging { get; private set; }
    private bool _hasHitPlayerThisCharge;

    public bool IsStage2 { get; private set; } = false;
    public bool IsDummy => (Stats as SpiritStats)?.IsDummy ?? false;
    public bool IsHeavyComboInProgress { get; private set; }

    // NOTE: 물리 설정(Kinematic, Trigger)은 에디터의 프리팹 설정에서 직접 변경하는 것을 권장합니다.

    public void SetCharging(bool value)
    {
        IsCharging = value;
        if (value) _hasHitPlayerThisCharge = false;
    }

    public void SetVelocity(Vector2 vel) => _rb.linearVelocity = vel;

    public void OnStage2Trigger()
    {
        if (IsStage2) return;
        IsStage2 = true;
        ChangeState(new Stage2CombatState());
    }

    public void TriggerHeavyCombo()
    {
        StopAllCoroutines();
        StartCoroutine(HeavyComboRoutine());
    }

    private System.Collections.IEnumerator HeavyComboRoutine()
    {
        IsHeavyComboInProgress = true;
        yield return new SpiritStealth().StealthRoutine(this); 
        yield return new WaitForSeconds(0.1f);
        
        new SpiritCharge().ExecuteAttack(this);
        yield return new WaitForSeconds(ChargeWindup + 2.0f);
        
        IsHeavyComboInProgress = false;
    }

    protected override void Update()
    {
        base.Update();
        // 상태 인터셉트 로직 유지
        if (CurrentState is CombatState && !(CurrentState is SpiritCombatState) && !(CurrentState is Stage2CombatState))
        {
            ChangeState(IsStage2 && !IsDummy ? (IBossState)new Stage2CombatState() : new SpiritCombatState());
        }
    }

    public void CleanupClones()
    {
        if (IsDummy) return;
        var allSpirits = Object.FindObjectsByType<SpiritController>(FindObjectsSortMode.None);
        foreach (var spirit in allSpirits)
        {
            if (spirit != null && spirit.IsDummy) Object.Destroy(spirit.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => HandleChargeImpact(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => HandleChargeImpact(collision.gameObject);

    private void HandleChargeImpact(GameObject other)
    {
        if (!IsCharging || _hasHitPlayerThisCharge) return;
        if (((1 << other.layer) & PlayerLayer) == 0) return;

        var hp = other.GetComponentInParent<HP>();
        if (hp != null)
        {
            _hasHitPlayerThisCharge = true;
            hp.TakeDamage(ChargeDamage);
        }
    }
}