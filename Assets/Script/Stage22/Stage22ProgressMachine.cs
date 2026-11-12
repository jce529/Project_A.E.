// Phase 26 - holds the player's current 2-2 zone and validates zone transitions.
using System;
using UnityEngine;

public class Stage22ProgressMachine : MonoBehaviour
{
    public static Stage22ProgressMachine Instance { get; private set; }

    public Stage22Zone Current { get; private set; } = Stage22Zone.None;

    public event Action<Stage22Zone, Stage22Zone> OnZoneChanged;                 // from, to
    public event Action<Stage22Zone, Stage22Zone, Stage22Block> OnMoveBlocked;   // from, attempted, reason

    void Awake() { if (Instance == null) Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    // First call (Current == None, e.g. after load) accepts any zone.
    public bool TryEnter(Stage22Zone to)
    {
        if (to == Current) return true;
        var block = Stage22Progress.Check(Current, to);
        if (block != Stage22Block.None)
        {
            OnMoveBlocked?.Invoke(Current, to, block);
            return false;
        }
        var from = Current;
        Current = to;
        if (from == Stage22Zone.Z9 && to == Stage22Zone.Z1 && SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetGimmick(Stage22Progress.KeyLoopCompleted, true);
        OnZoneChanged?.Invoke(from, to);
        return true;
    }
}
