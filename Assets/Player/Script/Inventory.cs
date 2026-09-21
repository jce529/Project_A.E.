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
}
