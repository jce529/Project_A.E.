using UnityEngine;

/// <summary>2스테이지 기믹 영구 상태 키와 저장 래퍼. SaveLoadManager.SetGimmick/GetGimmick(Phase 23)만 호출한다.</summary>
public static class Gimmick2Store
{
    public const string LanternSolved = "s2.a.lantern.solved";
    public const string TabletUnlocked = "s2.a.tablet.unlocked";
    public const string TelescopeViewed = "s2.a.telescope.viewed";
    public const string TreeTransformed = "s2.a.tree.transformed";
    public const string WaterwayChanged = "s2.b.waterway.changed";
    public const string WaterfallStopped = "s2.b.waterfall.stopped";
    public const string WaterwheelRunning = "s2.b.waterwheel.running";
    public const string LiftRaised = "s2.b.lift.raised";

    public static bool Get(string key, bool defaultValue = false)
    {
        var s = SaveLoadManager.Instance;
        return s != null ? s.GetGimmick(key, defaultValue) : defaultValue;
    }

    public static void Set(string key, bool value)
    {
        var s = SaveLoadManager.Instance;
        if (s != null) s.SetGimmick(key, value);
    }

    /// <summary>퍼즐 UI 열림/닫힘 중 입력 상태 전환(GameStateManager 가 있을 때만).</summary>
    public static void SetPuzzleInput(bool puzzleOpen)
    {
        var g = GameStateManager.Instance;
        if (g != null) g.SetState(puzzleOpen ? GameStateManager.GameState.Puzzle : GameStateManager.GameState.Playing);
    }
}
