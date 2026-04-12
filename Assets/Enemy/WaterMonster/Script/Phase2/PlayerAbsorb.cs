using UnityEngine;
using WaterMonster.Phase2;

/// <summary>
/// Attach to the Player GameObject. Listens for InputHandler.OnInteractEvent (F key)
/// and absorbs the nearest in-range WaterPuddle.
/// Per D-17: absorb = RecoveryWater() + SetIndestructible().
/// </summary>
public class PlayerAbsorb : MonoBehaviour
{
    [SerializeField] private WaterController _waterController;
    [SerializeField] private float absorbRadius = 2f;

    private void OnEnable()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnInteractEvent += TryAbsorb;
    }

    private void OnDisable()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnInteractEvent -= TryAbsorb;
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
