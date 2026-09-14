// Phase 16 - AutoSaveTimer (the fourth save trigger: time)
// Decisions: D-01 (always writes CurrentSlot, never opens SaveSlotDialog),
//            D-03 + D-03c (fires only while Playing AND a PlayerStats exists),
//            D-04 (180 second interval),
//            D-05 (any disk write restarts the countdown; see SaveLoadManager.Save()),
//            D-07 / D-07b (reuses SaveAnywhere() as-is - no wrapper, no heal),
//            D-11 (always on; no setting is exposed).
//
// D-03b: every guard lives HERE, in the caller, and NOT inside SaveLoadManager. The pause
// menu saves while CurrentState is Paused, so a Playing gate inside SaveAnywhere() would
// silently kill manual saving. This file is the only place that decides "may we write now".
//
// Why a separate component instead of a timer inside SaveLoadManager: P11 D-01 gives the
// manager the save logic and leaves the decision to call it to the caller. This timer IS a
// caller.
//
// Why it bootstraps itself instead of sitting on the PersistentManagers prefab: a brand new
// script cannot be referenced from prefab or scene YAML before the Unity editor has imported
// it, and hand-editing that YAML is how scenes get corrupted. Creating the object in code
// mirrors SaveLoadManager.Bootstrap() and needs no editor wiring at all.

using System.Collections;
using UnityEngine;

public class AutoSaveTimer : MonoBehaviour
{
    // D-04: three minutes of actual playable time.
    public const float IntervalSeconds = 180f;

    // The countdown is sampled once a second instead of accumulated every frame, because
    // PlayerStats.Instance falls back to FindAnyObjectByType whenever its cached instance is
    // gone - which is every single call while the main menu is up.
    public const float TickSeconds = 1f;

    private static AutoSaveTimer _instance;

    // Playable time accumulated since the last disk write, in seconds.
    private float _elapsedSeconds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;
        GameObject go = new GameObject("AutoSave");
        go.AddComponent<AutoSaveTimer>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    private void Start()
    {
        StartCoroutine(AutoSaveRoutine());
    }

    // D-05: called from SaveLoadManager.Save(), the single disk write that every save trigger
    // (checkpoint, boss defeat, pause menu and this timer itself) funnels through. Static and
    // null-tolerant on purpose, so the manager never has to care whether a timer exists.
    public static void NotifySaveWritten()
    {
        if (_instance != null) _instance._elapsedSeconds = 0f;
    }

    // Coroutine based (IEnumerator / StartCoroutine) to match the codebase, which uses no
    // C# Task-based keywords anywhere.
    //
    // WaitForSeconds runs on scaled time, so while Time.timeScale is 0 - pause, inventory,
    // puzzle, game clear, and the save slot dialog - this loop does not advance at all. Paused
    // time therefore cannot trigger a write and does not count towards the interval either.
    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(TickSeconds);

            // Time spent outside actual gameplay (main menu, loading, a dialog) is not counted,
            // so the player never walks into a gameplay scene and gets an instant autosave.
            if (!CanAutoSave()) continue;

            _elapsedSeconds += TickSeconds;
            if (_elapsedSeconds < IntervalSeconds) continue;

            TryAutoSave();
        }
    }

    // Returns false when the gate blocked the write. Split out of the routine so the
    // ContextMenu hook below drives the exact same path during manual verification.
    private bool TryAutoSave()
    {
        if (!CanAutoSave()) return false;

        // Reset before the write so a throwing File.WriteAllText cannot spin this every tick.
        // SaveLoadManager.Save() also calls NotifySaveWritten(), which lands on the same 0.
        _elapsedSeconds = 0f;

        // D-07 / D-07b: SaveAnywhere() already records the current scene and clears
        // SpawnPointName when the scene changed, and it is the only existing method that
        // refreshes SceneName. Deliberately NOT SaveAtCheckpoint() - that one heals the player
        // to full and would hand out a free heal every three minutes. Deliberately NOT Save()
        // either - it never touches SceneName, so it would rewrite an identical file (D-07c).
        // D-01: no slot dialog is involved. SaveSlotDialog lives in the manual callers, not in
        // SaveAnywhere(), so this writes CurrentSlot silently.
        SaveLoadManager.Instance.SaveAnywhere();
        return true;
    }

    // D-03 + D-03c. Both halves are mandatory:
    //  - CurrentState == Playing keeps the write out of pause / inventory / puzzle / clear.
    //  - PlayerStats.Instance != null is what actually keeps the main menu out.
    //    GameStateManager.HandleSceneLoaded() forces CurrentState back to Playing on EVERY
    //    scene load, the main menu included. Without this second check the timer would write
    //    SceneName = "MainMenu" into the current slot; loading that slot later would try to
    //    open the main menu as a gameplay scene, find no PlayerStats and abort, leaving the
    //    slot permanently unusable.
    private static bool CanAutoSave()
    {
        if (SaveLoadManager.Instance == null) return false;

        GameStateManager state = GameStateManager.Instance;
        if (state == null) return false;
        if (state.CurrentState != GameStateManager.GameState.Playing) return false;

        return PlayerStats.Instance != null;
    }

    // ---- Verification hooks (no automated test harness exists in this project) ----
    // Select the runtime "AutoSave" GameObject in the Hierarchy during Play mode and use the
    // component gear menu, exactly like the Phase11 / Phase14 hooks on SaveLoadManager.

    [ContextMenu("Phase16/1. Force AutoSave Now")]
    private void DebugForceAutoSave()
    {
        if (!TryAutoSave())
            Debug.LogWarning("[AutoSaveTimer] Gate blocked the write - see Phase16/2 for the reason.");
    }

    [ContextMenu("Phase16/2. Log Timer State")]
    private void DebugLogTimerState()
    {
        Debug.Log("[AutoSaveTimer] elapsed=" + _elapsedSeconds + "/" + IntervalSeconds +
                  " canAutoSave=" + CanAutoSave() +
                  " state=" + (GameStateManager.Instance != null
                      ? GameStateManager.Instance.CurrentState.ToString() : "no GameStateManager") +
                  " hasPlayerStats=" + (PlayerStats.Instance != null) +
                  " slot=" + (SaveLoadManager.Instance != null ? SaveLoadManager.Instance.CurrentSlot : -1));
    }
}
