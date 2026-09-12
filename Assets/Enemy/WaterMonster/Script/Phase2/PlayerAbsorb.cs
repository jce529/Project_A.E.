using System.Collections.Generic;
using UnityEngine;
using WaterMonster.Phase2;

/// <summary>Player-owned puddle absorption. Interact is dispatched by PlayerInteraction.</summary>
public class PlayerAbsorb : MonoBehaviour
{
    public enum InputType { Interact, BasicAttack, Skill1, Skill2, Heal }
    [SerializeField] private WaterController _waterController;
    [SerializeField] private float absorbRadius = 2f;
    [SerializeField] private InputType _inputType = InputType.Interact;
    private readonly List<Collider2D> hits = new List<Collider2D>(16);
    private InputHandler subscribedInput;
    private InputType subscribedType;

    private void OnEnable() => Subscribe();
    private void Start() => Subscribe();
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
        return isActiveAndEnabled && puddle != null && puddle.isActiveAndEnabled
            && puddle.isDestructible && (!fromInteraction || _inputType == InputType.Interact);
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
        if (!isActiveAndEnabled || _inputType == InputType.Interact) return;
        var filter = new ContactFilter2D { useTriggers = true };
        float radius = Mathf.Max(0f, absorbRadius);
        Physics2D.OverlapCircle(transform.position, radius, filter, hits);
        WaterPuddle nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (var hit in hits)
        {
            var puddle = hit.GetComponentInParent<WaterPuddle>();
            if (!CanAbsorb(puddle, false)) continue;
            float distance = ((Vector2)(puddle.transform.position - transform.position)).sqrMagnitude;
            if (distance > radius * radius) continue;
            if (distance < nearestDistance || (distance == nearestDistance
                && (nearest == null || puddle.GetInstanceID() < nearest.GetInstanceID())))
            {
                nearest = puddle;
                nearestDistance = distance;
            }
        }
        Absorb(nearest, false);
    }
}
