using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;

    public void Clear()
    {
        item = null;
        count = 0;
    }
}

[DisallowMultipleComponent]
public class Inventory : MonoBehaviour
{
    public const int SlotCount = 20;
    public const int MaxStack = 99;

    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>(
        SlotCount);
    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        playerInteraction = GetComponent<PlayerInteraction>();
        slots.Clear();
        for (int i = 0; i < SlotCount; i++) slots.Add(new InventorySlot());
    }

    public int SlotCountTotal => slots.Count;
    public InventorySlot GetSlot(int index) =>
        (index < 0 || index >= slots.Count) ? null : slots[index];

    private int AvailableCapacityFor(ItemData item)
    {
        int capacity = 0;
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) capacity += MaxStack;
            else if (slot.item == item && slot.count < MaxStack) capacity += MaxStack - slot.count;
        }
        return capacity;
    }

    public bool TryAddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if (AvailableCapacityFor(item) < amount) return false;

        int remaining = amount;
        // Pass 1: top up existing partial stacks of the same item first.
        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.IsEmpty || slot.item != item || slot.count >= MaxStack) continue;
            int add = Mathf.Min(MaxStack - slot.count, remaining);
            slot.count += add;
            remaining -= add;
        }
        // Pass 2: open empty slots for the leftover.
        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (!slot.IsEmpty) continue;
            int add = Mathf.Min(MaxStack, remaining);
            slot.item = item;
            slot.count = add;
            remaining -= add;
        }
        return true;
    }

    public bool RemoveItem(int slotIndex, int amount)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count || amount <= 0) return false;
        var slot = slots[slotIndex];
        if (slot.IsEmpty || slot.count < amount) return false;
        slot.count -= amount;
        if (slot.count <= 0) slot.Clear();
        return true;
    }

    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return false;
        var slot = slots[slotIndex];
        if (slot.IsEmpty) return false;

        slot.item.UseEffect(playerInteraction);
        slot.count--;
        if (slot.count <= 0) slot.Clear();
        return true;
    }

#if UNITY_EDITOR
    // Phase 18 verification hooks (D-07). No inventory UI exists this phase, so Play mode
    // Inspector context menu is the only way to exercise the API. Same pattern as
    // ItemData.UseOnPlayerFromInspector (Phase 17).
    [ContextMenu("Phase18: Use Slot 0")]
    private void UseSlot0FromInspector() => UseItem(0);

    [ContextMenu("Phase18: Use Slot 1")]
    private void UseSlot1FromInspector() => UseItem(1);

    [ContextMenu("Phase18: Log Slots")]
    private void LogSlotsFromInspector()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot.IsEmpty) continue;
            Debug.LogWarning($"[Phase18] slot {i}: {slot.item.Id} x{slot.count}");
        }
        Debug.LogWarning($"[Phase18] slot count = {slots.Count}");
    }

    [ContextMenu("Phase18: Fill All Slots (D-06 test)")]
    private void FillAllSlotsFromInspector()
    {
        var filler = GetSlot(0) != null && !GetSlot(0).IsEmpty ? GetSlot(0).item : null;
        if (filler == null) { Debug.LogWarning("[Phase18] put an item in slot 0 first"); return; }
        foreach (var slot in slots) { slot.item = filler; slot.count = MaxStack; }
        Debug.LogWarning("[Phase18] all slots filled to MaxStack");
    }
#endif
}
