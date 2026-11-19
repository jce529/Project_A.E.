using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    // Keep the serialized field and legacy methods for existing scenes and UnityEvents.
    public List<int[]> bottles = new List<int[]>();
    public bool PureWaterMod;
    public bool CorruptedWaterMod;
    public event Action OnWaterChanged;

    private int _pureCount;
    private int _corruptedCount;
    private int _knownCapacity = -1;
    public int Capacity => bottles.Count;
    public int TotalWater { get { EnsureCounts(); return _pureCount + _corruptedCount; } }

    private void Awake() => RefreshCounts();
    private void Start() => AddBottle(5);
    private void OnValidate() => RefreshCounts();

    private void EnsureCounts()
    {
        if (_knownCapacity != bottles.Count) RefreshCounts();
    }

    private void RefreshCounts()
    {
        _pureCount = 0;
        _corruptedCount = 0;
        for (int i = 0; i < bottles.Count; i++)
        {
            int[] bottle = bottles[i];
            if (bottle == null || bottle.Length < 2 || bottle[0] != 1) continue;
            if (bottle[1] == 0) _pureCount++;
            else _corruptedCount++;
        }
        _knownCapacity = bottles.Count;
        PureWaterMod = _corruptedCount == 0;
        CorruptedWaterMod = _pureCount == 0;
    }

    private void NotifyChanged()
    {
        RefreshCounts();
        OnWaterChanged?.Invoke();
    }

    public void AddBottle(int amount)
    {
        if (amount <= 0) return;
        for (int i = 0; i < amount; i++) bottles.Add(new[] { 1, 0 });
        NotifyChanged();
    }

    public void UseBottle(int amount)
    {
        if (amount <= 0) return;
        int remaining = Math.Min(amount, TotalWater);
        if (remaining == 0) return;
        for (int i = bottles.Count - 1; i >= 0 && remaining > 0; i--)
        {
            int[] bottle = bottles[i];
            if (bottle == null || bottle.Length < 2 || bottle[0] != 1) continue;
            bottle[0] = 0;
            remaining--;
        }
        NotifyChanged();
    }

    public void RecoveryWater() => FillEmptyBottle(false);
    public void RecoveryCorruptedWater() => FillEmptyBottle(true);

    private void FillEmptyBottle(bool corrupted)
    {
        for (int i = 0; i < bottles.Count; i++)
        {
            int[] bottle = bottles[i];
            if (bottle == null || bottle.Length < 2 || bottle[0] != 0) continue;
            bottle[0] = 1;
            bottle[1] = corrupted ? 1 : 0;
            NotifyChanged();
            return;
        }
    }

    public int waterCounter() { EnsureCounts(); return _pureCount; }
    public int corruptedwaterCounter() { EnsureCounts(); return _corruptedCount; }
}
