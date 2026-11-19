using System.Collections.Generic;
using UnityEngine;
using WaterMonster.Phase2;

/// <summary>Shared Interact uses PlayerInteraction; custom absorption actions retain B's API.</summary>
public class PlayerAbsorb : MonoBehaviour
{
    public enum InputType { Interact, BasicAttack, Skill1, Skill2, Heal }
    [SerializeField] private WaterController _waterController;
    [SerializeField] private float absorbRadius = 2f;
    [SerializeField] private InputType _inputType = InputType.Interact;
    [Tooltip("Standard Interact always requires an empty bottle. Enable this to apply the same rule to custom actions.")]
    [SerializeField] private bool requireEmptyBottleForCustomActions;
    public bool UsesInteraction => _inputType == InputType.Interact;
    public float AbsorbRadius => Mathf.Max(0f, absorbRadius);
    private readonly List<Collider2D> hits = new List<Collider2D>(16);
    private InputHandler subscribedInput;
    private InputType subscribedType;

    private void Awake() { if (_waterController == null) _waterController = GetComponent<WaterController>(); }
    private void OnEnable() => Subscribe();
    private void Start() => Subscribe();
    private void Update() => Subscribe();
    private void Subscribe()
    {
        if (subscribedInput == InputHandler.Instance && subscribedType == _inputType) return;
        OnDisable();
        subscribedInput = InputHandler.Instance;
        subscribedType = _inputType;
        if (subscribedInput == null) return;
        switch (subscribedType)
        {
            case InputType.BasicAttack: subscribedInput.OnBasicAttackEvent += TryAbsorb; break;
            case InputType.Skill1: subscribedInput.OnSkill1Event += TryAbsorb; break;
            case InputType.Skill2: subscribedInput.OnSkill2Event += TryAbsorb; break;
            case InputType.Heal: subscribedInput.OnHealEvent += TryAbsorb; break;
        }
    }
    private void OnDisable()
    {
        if (subscribedInput != null)
        {
            switch (subscribedType)
            {
                case InputType.BasicAttack: subscribedInput.OnBasicAttackEvent -= TryAbsorb; break;
                case InputType.Skill1: subscribedInput.OnSkill1Event -= TryAbsorb; break;
                case InputType.Skill2: subscribedInput.OnSkill2Event -= TryAbsorb; break;
                case InputType.Heal: subscribedInput.OnHealEvent -= TryAbsorb; break;
            }
        }
        subscribedInput = null;
    }
    public bool CanAbsorb(WaterPuddle puddle, bool fromInteraction)
    {
        if (!isActiveAndEnabled || !GameStateManager.CanProcessGameplayInput || puddle == null
            || !puddle.isActiveAndEnabled || !puddle.isDestructible || (fromInteraction && !UsesInteraction)) return false;
        if (fromInteraction || requireEmptyBottleForCustomActions)
            return _waterController != null && _waterController.TotalWater < _waterController.Capacity;
        return true;
    }
    public bool Absorb(WaterPuddle puddle, bool fromInteraction)
    {
        if (!CanAbsorb(puddle, fromInteraction)) return false;
        if (_waterController != null) _waterController.RecoveryWater();
        puddle.SetIndestructible();
        return true;
    }
    private void TryAbsorb()
    {
        if (UsesInteraction || !GameStateManager.CanProcessGameplayInput) return;
        var interaction = GetComponent<PlayerInteraction>();
        if (interaction != null && (!interaction.CanProcessInput || interaction.InteractedThisFrame
            || (interaction.InteractionAction != null && interaction.InteractionAction.WasPressedThisFrame()
                && interaction.FindNearest() != null))) return;
        var filter = new ContactFilter2D { useTriggers = true };
        bool oldQueries = Physics2D.queriesHitTriggers;
        try
        {
            Physics2D.queriesHitTriggers = true;
            Physics2D.OverlapCircle(transform.position, AbsorbRadius, filter, hits);
        }
        finally { Physics2D.queriesHitTriggers = oldQueries; }
        WaterPuddle nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            var puddle = hit.GetComponentInParent<WaterPuddle>();
            if (!CanAbsorb(puddle, false)) continue;
            float distance = ((Vector2)(puddle.transform.position - transform.position)).sqrMagnitude;
            if (distance > AbsorbRadius * AbsorbRadius) continue;
            if (distance < nearestDistance || (distance == nearestDistance
                && (nearest == null || puddle.GetHashCode() < nearest.GetHashCode())))
            {
                nearest = puddle;
                nearestDistance = distance;
            }
        }
        Absorb(nearest, false);
    }
}
