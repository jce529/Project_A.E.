// Phase 26 - 2-2 progress rules (pure logic, no scene references).
// Flow: 1-2-3-4-5-6-7-6-5-8-9-1. Gimmick flags (SetGimmick/GetGimmick, Phase 23) are written by
// Phase 27 gimmicks; this class only reads them.
public enum Stage22Zone { None = 0, Z1 = 1, Z2, Z3, Z4, Z5, Z6, Z7, Z8, Z9 }

public enum Stage22Block { None, NotAdjacent, WaterwayNotChanged, TabletNotRead }

public static class Stage22Progress
{
    public const string KeyWaterwayChanged = "s2.b.waterway.changed";
    public const string KeyWaterfallStopped = "s2.b.waterfall.stopped";
    public const string KeyTabletRead = "s2.b.tablet.read";
    public const string KeyLoopCompleted = "s2.b.loop.completed";

    static bool Get(string key)
    {
        var s = SaveLoadManager.Instance;
        return s != null && s.GetGimmick(key, false);
    }

    public static bool WaterwayChanged => Get(KeyWaterwayChanged);
    public static bool WaterfallStopped => Get(KeyWaterfallStopped);
    public static bool TabletRead => Get(KeyTabletRead);

    // Tablet can be inspected only after the waterfall has stopped.
    public static bool CanAccessTablet() => WaterfallStopped;

    static bool Adjacent(Stage22Zone a, Stage22Zone b)
    {
        int x = (int)a, y = (int)b;
        if (x > y) { int t = x; x = y; y = t; }
        // chain 1-2-3-4-5-6-7, branch 5-8, 8-9. 7-8 and 6-8 are NOT adjacent.
        if (y - x == 1 && y <= 7) return true;
        return (x == 5 && y == 8) || (x == 8 && y == 9);
    }

    public static Stage22Block Check(Stage22Zone from, Stage22Zone to)
    {
        if (from == Stage22Zone.None || from == to) return Stage22Block.None;
        if (from == Stage22Zone.Z9 && to == Stage22Zone.Z1) return Stage22Block.None; // return trip
        if (!Adjacent(from, to)) return Stage22Block.NotAdjacent;
        if (to == Stage22Zone.Z8 && from == Stage22Zone.Z5 && !WaterwayChanged) return Stage22Block.WaterwayNotChanged;
        if (to == Stage22Zone.Z9 && from == Stage22Zone.Z8 && !TabletRead) return Stage22Block.TabletNotRead;
        return Stage22Block.None;
    }

    public static bool CanMove(Stage22Zone from, Stage22Zone to) => Check(from, to) == Stage22Block.None;
}
