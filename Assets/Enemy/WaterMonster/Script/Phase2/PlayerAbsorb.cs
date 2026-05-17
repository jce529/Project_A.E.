using UnityEngine;
using WaterMonster.Phase2;

/// <summary>
/// Attach to the Player GameObject. Listens for InputHandler.OnInteractEvent (F key)
/// and absorbs the nearest in-range WaterPuddle.
/// Per D-17: absorb = RecoveryWater() + SetIndestructible().
/// </summary>
public class PlayerAbsorb : MonoBehaviour
{
    public enum InputType { Interact, BasicAttack, Skill1, Skill2, Heal }

    [SerializeField] private WaterController _waterController;
    [SerializeField] private float absorbRadius = 2f;
    [SerializeField] private InputType _inputType = InputType.Interact;

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
        if (InputHandler.Instance == null) return;

        switch (_inputType)
        {
            case InputType.Interact:
                if (subscribe) InputHandler.Instance.OnInteractEvent += TryAbsorb;
                else InputHandler.Instance.OnInteractEvent -= TryAbsorb;
                break;
            case InputType.BasicAttack:
                if (subscribe) InputHandler.Instance.OnBasicAttackEvent += TryAbsorb;
                else InputHandler.Instance.OnBasicAttackEvent -= TryAbsorb;
                break;
            case InputType.Skill1:
                if (subscribe) InputHandler.Instance.OnSkill1Event += TryAbsorb;
                else InputHandler.Instance.OnSkill1Event -= TryAbsorb;
                break;
            case InputType.Skill2:
                if (subscribe) InputHandler.Instance.OnSkill2Event += TryAbsorb;
                else InputHandler.Instance.OnSkill2Event -= TryAbsorb;
                break;
            case InputType.Heal:
                if (subscribe) InputHandler.Instance.OnHealEvent += TryAbsorb;
                else InputHandler.Instance.OnHealEvent -= TryAbsorb;
                break;
        }
    }

    private void TryAbsorb()
    {
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
