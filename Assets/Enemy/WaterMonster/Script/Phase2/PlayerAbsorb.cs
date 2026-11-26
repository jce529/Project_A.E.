using UnityEngine;
using WaterMonster.Phase2;

/// <summary>
/// Legacy adapter for custom absorption actions. Standard interactions are
/// dispatched exclusively by PlayerInteraction to WaterPuddle.
/// Per D-17: absorb = RecoveryWater() + SetIndestructible().
/// </summary>
public class PlayerAbsorb : MonoBehaviour
{
    public enum InputType { Interact, BasicAttack, Skill1, Skill2, Heal }

    [SerializeField] private WaterController _waterController;
    [SerializeField] private float absorbRadius = 2f;
    [SerializeField] private InputType _inputType = InputType.Interact;

    public bool UsesInteraction => _inputType == InputType.Interact;
    public float AbsorbRadius => absorbRadius;
    private InputHandler _subscribedInput;

    private void Update()
    {
        if (_subscribedInput != InputHandler.Instance)
        {
            SubscribeInput(false);
            SubscribeInput(true);
        }
    }

    private void OnEnable()
    {
        SubscribeInput(true);
    }

    private void OnDisable()
    {
        SubscribeInput(false);
    }

    private void SubscribeInput(bool subscribe)
    {
        // Interact is exclusively dispatched by PlayerInteraction.
        if (UsesInteraction) return;
        var input = subscribe ? InputHandler.Instance : _subscribedInput;
        if (input == null) return;
        _subscribedInput = subscribe ? input : null;

        switch (_inputType)
        {
            case InputType.Interact:
                break;
            case InputType.BasicAttack:
                if (subscribe) input.OnBasicAttackEvent += TryAbsorb;
                else input.OnBasicAttackEvent -= TryAbsorb;
                break;
            case InputType.Skill1:
                if (subscribe) input.OnSkill1Event += TryAbsorb;
                else input.OnSkill1Event -= TryAbsorb;
                break;
            case InputType.Skill2:
                if (subscribe) input.OnSkill2Event += TryAbsorb;
                else input.OnSkill2Event -= TryAbsorb;
                break;
            case InputType.Heal:
                if (subscribe) input.OnHealEvent += TryAbsorb;
                else input.OnHealEvent -= TryAbsorb;
                break;
        }
    }

    private void TryAbsorb()
    {
        if (Time.timeScale <= 0f || (_waterController == null)) return;
        if (_waterController.waterCounter() + _waterController.corruptedwaterCounter() >= _waterController.bottles.Count) return;
        var interaction = GetComponent<PlayerInteraction>();
        if (interaction != null && !interaction.CanProcessInput) return;
        // Legacy custom action configurations remain supported.
        // Find all WaterPuddle colliders in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, absorbRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("WaterPuddle")) continue;

            var puddle = hit.GetComponent<WaterPuddle>();
            if (puddle == null) continue;
            if (!puddle.isDestructible) continue;   // already absorbed — skip
            if (!puddle.playerInRange) continue;     // player not in puddle's trigger zone

            // Absorb: recover water + make indestructible (per D-17)
            if (_waterController != null)
            {
                _waterController.RecoveryWater();        // fills one empty bottle (RESEARCH Note #1)
            }
            puddle.SetIndestructible();              // changes color + registers with PuddleStackManager
            return; // absorb one puddle per interaction
        }
    }
}
