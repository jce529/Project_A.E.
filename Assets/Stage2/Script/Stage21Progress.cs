// Phase 25 - 2-1 progress state machine (static, scene-object free).
// Flags live in SaveLoadManager gimmick state (Phase 23 API: SetGimmick/GetGimmick), keys "s2.a.*".
// Phase 27 (gimmicks) -> this: raise Stage2Events (LanternSolved / AreaCleared / TabletOpened).
// This -> consumers: Can* queries + Stage2Events.FogRemoved / TabletUnlocked.

using System;
using UnityEngine;

public static class Stage21Keys
{
    public const string LanternSolved = "s2.a.lantern.solved";
    public const string FogRemoved = "s2.a.fog.removed";
    public const string TabletUnlocked = "s2.a.tablet.unlocked";
    public const string TabletRead = "s2.a.tablet.read";
    public const int AreaCount = 6;
    public const int CombatArea = 5;
    public const int ExitArea = 6;

    // "s2.a.area3.visited" (areas 1..6)
    public static string AreaVisited(int area) { return "s2.a.area" + area + ".visited"; }
    // "s2.a.area5.cleared"
    public static string AreaCleared(int area) { return "s2.a.area" + area + ".cleared"; }
}

// Event contract between Phase 27 (gimmicks / combat) and Phase 25 (this state machine).
public static class Stage2Events
{
    public static event Action LanternSolved;            // Phase 27 lantern puzzle matched the answer pattern
    public static event Action<int> AreaCleared;         // enemies of 2-1 area N all dead (combat owner raises)
    public static event Action TabletOpened;             // player opened the tablet hint UI (optional)
    public static event Action TabletUnlocked;           // raised by Phase 25 when tablet becomes investigable
    public static event Action FogRemoved;               // raised by Phase 25 when fog is permanently removed

    public static void RaiseLanternSolved() { if (LanternSolved != null) LanternSolved(); }
    public static void RaiseAreaCleared(int area) { if (AreaCleared != null) AreaCleared(area); }
    public static void RaiseTabletOpened() { if (TabletOpened != null) TabletOpened(); }
    internal static void RaiseTabletUnlocked() { if (TabletUnlocked != null) TabletUnlocked(); }
    internal static void RaiseFogRemoved() { if (FogRemoved != null) FogRemoved(); }
}

public static class Stage21Progress
{
    static bool _subscribed;

    // Call from any Stage21* component's Awake/OnEnable; idempotent.
    public static void EnsureSubscribed()
    {
        if (_subscribed) return;
        _subscribed = true;
        Stage2Events.LanternSolved += OnLanternSolved;
        Stage2Events.AreaCleared += OnAreaCleared;
        Stage2Events.TabletOpened += OnTabletOpened;
    }

    static bool Get(string key)
    {
        var m = SaveLoadManager.Instance;
        return m != null && m.GetGimmick(key, false);
    }

    static void Set(string key, bool value)
    {
        var m = SaveLoadManager.Instance;
        if (m != null) m.SetGimmick(key, value);
    }

    // ---- queries ----
    public static bool IsLanternSolved() { return Get(Stage21Keys.LanternSolved); }
    public static bool IsFogRemoved() { return Get(Stage21Keys.FogRemoved); }
    public static bool IsAreaVisited(int area) { return Get(Stage21Keys.AreaVisited(area)); }
    public static bool CanInvestigateTablet() { return Get(Stage21Keys.TabletUnlocked); }
    public static bool CanEnterStage22() { return IsLanternSolved(); }

    // Linear flow 1 -> 2 -> ... -> 6: area N is enterable once N-1 was visited (area 1 always).
    public static bool CanEnterArea(int area)
    {
        if (area < 1 || area > Stage21Keys.AreaCount) return false;
        return area == 1 || IsAreaVisited(area - 1);
    }

    // ---- transitions ----
    // Returns false if the flow forbids it (area skipped).
    public static bool EnterArea(int area)
    {
        if (!CanEnterArea(area)) return false;
        Set(Stage21Keys.AreaVisited(area), true);
        return true;
    }

    // Fog trigger area entered: remove permanently only after the puzzle is solved.
    public static bool TryRemoveFog()
    {
        if (!IsLanternSolved()) return false;
        if (IsFogRemoved()) return true;
        Set(Stage21Keys.FogRemoved, true);
        Stage2Events.RaiseFogRemoved();
        return true;
    }

    static void OnLanternSolved()
    {
        Set(Stage21Keys.LanternSolved, true); // idempotent; phase 27 may already have set it
    }

    static void OnAreaCleared(int area)
    {
        Set(Stage21Keys.AreaCleared(area), true);
        if (area == Stage21Keys.CombatArea && !Get(Stage21Keys.TabletUnlocked))
        {
            Set(Stage21Keys.TabletUnlocked, true);
            Stage2Events.RaiseTabletUnlocked();
        }
    }

    static void OnTabletOpened() { Set(Stage21Keys.TabletRead, true); }
}
