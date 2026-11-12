using UnityEngine;

// Phase 18 (D-05): a world-placed pickup. It deliberately adds no detection logic of its own -
// implementing IPlayerInteractable is enough for PlayerInteraction.FindNearest to target it and
// for PlayerInteractionPrompt to show the interact key. Mirrors Checkpoint.cs.
public class WorldItem : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private ItemData item;
    [SerializeField, Min(1)] private int count = 1;

    // Phase 21 (D-13): pickup feedback is published, not pushed into UI. Only this path raises it,
    // so save restore (which calls Inventory.TryAddItem directly) never produces a toast.
    // Args: item, attempted count, whether the add succeeded.
    public static event System.Action<ItemData, int, bool> PickupResolved;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => PickupResolved = null;

    // Checkpoint.cs keeps CanInteract to a bare null check. The extra item guard exists because
    // an unconfigured WorldItem would otherwise show a prompt that can never do anything.
    public bool CanInteract(PlayerInteraction player) => player != null && item != null;

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;

        // No null guard on GetComponent - the project convention (ItemData.UseEffect ->
        // player.GetComponent<PlayerStats>().Heal) treats a missing sibling component as a
        // setup bug, not a runtime case.
        var inventory = player.GetComponent<Inventory>();

        // D-06: destruction is gated on the add actually succeeding. A full inventory must
        // leave this object untouched in the world - no data loss, ever.
        bool added = inventory.TryAddItem(item, count);
        PickupResolved?.Invoke(item, count, added);
        if (added)
        {
            Destroy(gameObject);
        }
    }
}
