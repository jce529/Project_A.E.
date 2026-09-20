using UnityEngine;

// Phase 16 (D-01): the project's first ScriptableObject. ItemData implements IItem directly -
// there is no separate runtime item instance class, so the static data and the use logic live
// in one file. Phase 17's inventory will hold ItemData references and call UseEffect on them.
[CreateAssetMenu(menuName = "Items/Item Data", fileName = "New Item")]
public class ItemData : ScriptableObject, IItem
{
    // D-04: exactly two kinds. No SO subclass hierarchy - a single class branches on this enum.
    public enum ItemType
    {
        Consumable,
        Progression
    }

    // D-03: Heal is the only consumable effect in this phase. Future effects are added here.
    public enum ConsumableEffectType
    {
        Heal
    }

    // D-05: manually typed into the Inspector. No auto-generation and no duplicate validation
    // this phase. Kept as a string because Phase 18 will store it in SaveData as ItemSaveEntry.itemId.
    [SerializeField] private string id;

    // D-06: minimal schema. No name/image/blurb fields until a UI layer exists.
    [SerializeField] private ItemType type;

    // Consumable-only parameters. Leave them at their defaults on a Progression item.
    [SerializeField] private ConsumableEffectType effectType;
    [SerializeField] private float amount;

    // The one accessor this phase needs: without it `id` would be a write-only field. Phase 17
    // can add Type / amount accessors when the inventory actually needs them.
    public string Id => id;

    // D-02: fully implemented, not a stub. Consumables apply their effect for real; Progression
    // items are an intentional no-op until Phase 17+ gives them meaning.
    public void UseEffect(PlayerInteraction player)
    {
        if (type != ItemType.Consumable) return;

        switch (effectType)
        {
            case ConsumableEffectType.Heal:
                // D-03: reuse the existing PlayerStats.Heal(float) - no new PlayerStats method.
                // No null guard on GetComponent: that matches the unguarded convention already
                // used by PlayerStats.TakeDamage -> CameraController.Instance.Shake().
                player.GetComponent<PlayerStats>().Heal(amount);
                break;
        }
    }

#if UNITY_EDITOR
    // Phase 16 verification hook. There is no inventory and no world pickup yet (both Phase 17),
    // so this is the only way to invoke UseEffect and prove D-02/D-03 end to end. Enter Play
    // mode, select the asset, then use the Inspector's context menu (three dots / right-click
    // the header) -> "Phase16: Use On Player". Same pattern as the SaveLoadManager ContextMenu
    // hooks added in Phase 11 / Phase 14.
    [ContextMenu("Phase16: Use On Player")]
    private void UseOnPlayerFromInspector()
    {
        UseEffect(FindAnyObjectByType<PlayerInteraction>());
    }
#endif
}
