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
    public RuntimeAnimatorController Stage2AnimController;

    [Header("Detection")]
    public LayerMask PlayerLayer; // 에디터에서 설정 가능하도록 노출

    public bool IsCharging { get; private set; }
    private bool _hasHitPlayerThisCharge;
    private bool _dying;

    public bool IsStage2 { get; private set; } = false;
    public PatternHistory PatternHistory { get; } = new PatternHistory(); // Phase 7-03
    public bool IsDummy => (Stats as SpiritStats)?.IsDummy ?? false;
    public bool IsHeavyComboInProgress { get; private set; }

    // NOTE: 물리 설정(Kinematic, Trigger)은 에디터의 프리팹 설정에서 직접 변경하는 것을 권장합니다.

    public void SetCharging(bool value)
    {
        IsCharging = value;
        if (value) _hasHitPlayerThisCharge = false;
    }

    public void SetVelocity(Vector2 vel) => _rb.linearVelocity = vel;

    private bool CanAnimate => Anim != null && Anim.runtimeAnimatorController != null;

    public void PlayAnim(string trigger)
    {
        if (CanAnimate) Anim.SetTrigger(trigger);
    }

    // 현재 컨트롤러(오버라이드 포함)에서 이름이 prefix 로 시작하는 클립 길이. 없으면 0.
    public float GetClipLength(string clipNamePrefix)
    {
        if (!CanAnimate) return 0f;
        foreach (var clip in Anim.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name.StartsWith(clipNamePrefix)) return clip.length;
        }
        return 0f;
    }

    private SpiritEffects _fx;
    public SpiritEffects Fx => _fx != null ? _fx : (_fx = GetComponent<SpiritEffects>());
    private GameObject _groggyFx;

    public GameObject PlayFx(SpiritFx id, Vector3? pos = null, float rotZ = 0f, float sizeMul = 1f, bool loop = false, Transform follow = null)
        => Fx != null ? Fx.Play(id, pos ?? transform.position, rotZ, sizeMul, loop, follow) : null;

    // 몸통 스프라이트 범위의 가운데 70% 안에서 무작위 지점 — 투명 여백에서 이펙트가 나오지 않게 한다.
    private Vector3 RandomBodyPoint()
    {
        var body = GetComponentInChildren<SpriteRenderer>();
        if (body == null) return transform.position;
        Bounds b = body.bounds;
        return new Vector3(
            b.center.x + Random.Range(-b.extents.x, b.extents.x) * 0.7f,
            b.center.y + Random.Range(-b.extents.y, b.extents.y) * 0.7f,
            transform.position.z);
    }

    private void PlayHitAnim()
    {
        PlayAnim("Hit");
        PlayFx(SpiritFx.Hit, RandomBodyPoint());
    }

    protected override void Start()
    {
        base.Start();
        if (Stats != null) Stats.OnDamageTaken += PlayHitAnim;
    }

    protected override void OnDestroy()
    {
        if (Stats != null) Stats.OnDamageTaken -= PlayHitAnim;
        base.OnDestroy();
    }

    public void OnStage2Trigger()
    {
        if (IsStage2) return;
        IsStage2 = true;
        PatternHistory.Clear();
        if (Anim != null && Stage2AnimController != null) Anim.runtimeAnimatorController = Stage2AnimController;
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

    public void BeginDeath()
    {
        if (_dying) return;
        _dying = true;
        StopAllCoroutines();
        IsHeavyComboInProgress = false;
        SetCharging(false);
        StopMove();
        if (_groggyFx != null) Destroy(_groggyFx);
        PlayFx(SpiritFx.Death);

        if (!CanAnimate)
        {
            gameObject.SetActive(false);
            return;
        }

        float length = GetClipLength("WaterSpirit_Death");

        if (length <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        PlayAnim("Death");
        StartCoroutine(DeactivateAfter(length));
    }

    private System.Collections.IEnumerator DeactivateAfter(float seconds)
    {
        // P12: 대기 중이던 Hit 트리거가 먼저 소비되면 Death 상태 진입이 늦어진다.
        //      Death 상태가 끝까지 재생될 때까지 기다리고, 진입하지 못하는 경우를 위해 길이 + 0.5초 상한을 둔다.
        float deadline = Time.time + seconds + 0.5f;
        while (Time.time < deadline)
        {
            var info = Anim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Death") && info.normalizedTime >= 1f) break;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        if (_dying) return;
        base.Update();
        if (CanAnimate)
        {
            Anim.SetBool("Move", _rb.linearVelocity.sqrMagnitude > 0.01f);
            Anim.SetBool("Groggy", CurrentState is GroggyState);
        }
        bool groggy = CurrentState is GroggyState;
        if (groggy && _groggyFx == null) _groggyFx = PlayFx(SpiritFx.Groggy, null, 0f, 1f, true, transform);
        else if (!groggy && _groggyFx != null) Destroy(_groggyFx);
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