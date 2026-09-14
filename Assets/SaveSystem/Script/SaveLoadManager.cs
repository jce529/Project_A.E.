// Phase 11 - SaveLoadManager
// Decisions: D-01 (manager owns save/load logic; callers call in, no event bus),
//            D-02 (one JSON file per save under Application.persistentDataPath; Phase 14
//                  extended this to 3 slots - save.json = slot 0, save_1.json, save_2.json),
//            D-04 (public API only - no menu UI, no loading screen),
//            D-05 (restore position via PlayerSpawner.targetSpawnPointName, not raw XY),
//            D-06 (NewGame resets memory only; the file is overwritten on the next Save()).
//
// In-play behaviour: everything mutates _data in memory. File I/O happens ONLY inside
// Save() (checkpoint interaction / boss defeat) and LoadGame() (continue / checkpoint revive).

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    #region Singleton
    public static SaveLoadManager Instance { get; private set; }

    // The manager must exist in every scene without the designer placing it by hand,
    // because Checkpoint.cs and the four boss death sites call SaveLoadManager.Instance
    // directly. Creating it before the first scene loads removes both the manual editor
    // step and the null-reference risk when entering Play mode from an arbitrary scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("SaveLoadManager");
        go.AddComponent<SaveLoadManager>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public const string SaveFileName = "save.json";

    // Phase 15 (D-04): the single place that names the main menu scene. It is index 0 in
    // EditorBuildSettings (Assets/Scenes/MainMenu.unity) and is the only screen that can
    // recover from a broken save, so every load failure funnels back here.
    public const string MainMenuSceneName = "MainMenu";

    // ---- Phase 14: save slots (D-06 file per slot, D-07 naming) -----------------
    // Slot 0 deliberately keeps the original "save.json" filename, so a player's
    // existing Phase 11 file simply IS slot 0. Nothing is renamed, copied or migrated,
    // which is the structural answer to D-07's absolute floor ("the existing save must
    // never disappear") - there is no migration routine that could fail halfway.
    public const int SlotCount = 3;

    // The active slot. Gameplay save triggers (Checkpoint.cs and the boss death sites)
    // never pass a slot: by the time gameplay runs, the main menu already picked one.
    // Default 0 preserves the exact Phase 11 behaviour for anything that runs before
    // slot selection exists (entering Play mode directly in a gameplay scene, the
    // ContextMenu debug hooks below).
    public int CurrentSlot { get; private set; }

    private static string GetSlotFileName(int slot)
    {
        return slot == 0 ? SaveFileName : "save_" + slot + ".json";
    }

    public string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, GetSlotFileName(slot));
    }

    public void SelectSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            Debug.LogError("[SaveLoadManager] SelectSlot: slot index out of range: " + slot);
            return;
        }
        CurrentSlot = slot;
    }

    public bool HasSaveFile(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return false;
        return File.Exists(GetSavePath(slot));
    }

    // Fallback location used by NewGame(). "1 stage" is the only gameplay scene currently
    // registered in EditorBuildSettings, so it is the only value LoadSceneAsync can resolve.
    [SerializeField] private string defaultSceneName = "1 stage";
    [SerializeField] private string defaultSpawnPointName = "";

    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Include,
    };

    private SaveData _data = new SaveData();

    // Read-only view for future systems (boss progress / gimmick queries).
    public SaveData Data { get { return _data; } }

    // Now slot-aware. Verified before this change: no code outside this file references
    // SaveLoadManager.SavePath, so dropping "static" breaks no caller.
    public string SavePath
    {
        get { return GetSavePath(CurrentSlot); }
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    // D-06: memory-only reset. The existing file on disk is intentionally left untouched;
    // it gets overwritten on the next Save() trigger (checkpoint or boss defeat).
    public void NewGame()
    {
        _data = new SaveData();
        _data.SceneName = defaultSceneName;
        _data.SpawnPointName = defaultSpawnPointName;
        Debug.Log("[SaveLoadManager] NewGame - memory reset only, file untouched.");
    }

    // Core write. Captures live player stats into the memory cache, then serializes.
    public void Save()
    {
        CapturePlayerStats();
        string json = JsonConvert.SerializeObject(_data, JsonSettings);
        File.WriteAllText(SavePath, json);
        Debug.Log("[SaveLoadManager] Saved to " + SavePath);

        // Phase 16 (D-05): every save trigger - checkpoint, boss defeat, pause menu and the
        // interval autosave itself - funnels through this one method, so restarting the
        // autosave countdown here covers all of them with a single line. Placed after the
        // write so a throwing File.WriteAllText does not reset anything. This is a
        // notification, NOT a gate: D-03b forbids any state check inside this file, and none
        // is added - AutoSaveTimer still owns the entire Playing / PlayerStats decision.
        AutoSaveTimer.NotifySaveWritten();
    }

    // D-01 integration point A: Checkpoint.cs calls this on S-key activation.
    // The checkpoint's own GameObject name doubles as the PlayerSpawner spawn point name.
    public void SaveAtCheckpoint(string checkpointName)
    {
        _data.SceneName = SceneManager.GetActiveScene().name;
        _data.SpawnPointName = checkpointName;

        // Checkpoints are recovery points as well as save triggers. Heal only here so the
        // pause-menu save-anywhere action cannot be used as an unlimited combat heal.
        PlayerStats ps = PlayerStats.Instance;
        if (ps != null) ps.ResetHealthToMax();

        Save();
    }

    // Phase 14: "save anywhere" trigger, used by the pause menu Game tab. Records the CURRENT
    // scene but deliberately does NOT invent a new spawn point - D-05 still forbids raw XY, so
    // the last activated checkpoint stays the respawn anchor.
    public void SaveAnywhere()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // The stored spawn point belongs to whichever scene the last checkpoint was in. Once the
        // player has moved to a different scene that name cannot resolve there, so drop it and
        // let PlayerSpawner fall back to the scene's own default start position.
        if (_data.SceneName != currentScene) _data.SpawnPointName = "";

        _data.SceneName = currentScene;
        Save();
    }

    // D-01 integration point B: the four boss death sites call this.
    // RESEARCH Open Question 1 resolution: a boss defeat does NOT introduce a new spawn
    // point. It records the boss id and reuses whatever scene/spawn point the last
    // checkpoint activation stored, so respawn semantics stay checkpoint-based.
    public void SaveOnBossDefeated(string bossId)
    {
        if (!string.IsNullOrEmpty(bossId))
        {
            _data.BossProgress[bossId] = true;
        }
        Save();
    }

    public bool IsBossDefeated(string bossId)
    {
        bool defeated;
        return _data.BossProgress.TryGetValue(bossId, out defeated) && defeated;
    }

    // Returns one value from the live save-data cache as text. The key uses a JSON path,
    // for example "BossProgress.TutorialBoss" or "PlayerStats.MaxHealth". Callers own
    // the conversion to their required type so this API does not need a method per field.
    // A missing/invalid key returns null, which remains distinguishable from values such
    // as false, 0, or an empty string.
    public string LoadData(string dataKey)
    {
        if (string.IsNullOrWhiteSpace(dataKey))
        {
            Debug.LogWarning("[SaveLoadManager] LoadData requires a non-empty data key.");
            return null;
        }

        EnsureCollections();

        JToken token;
        try
        {
            token = JObject.FromObject(_data).SelectToken(dataKey, false);
        }
        catch (JsonException e)
        {
            Debug.LogWarning("[SaveLoadManager] LoadData received an invalid data key '" +
                             dataKey + "': " + e.Message);
            return null;
        }

        if (token == null || token.Type == JTokenType.Null)
        {
            Debug.LogWarning("[SaveLoadManager] LoadData could not find '" + dataKey + "'.");
            return null;
        }

        return token.Type == JTokenType.String
            ? token.Value<string>()
            : token.ToString(Formatting.None);
    }

    // ---- Phase 14: slot-select screen API ---------------------------------------

    // Read-only peek used by the slot select screen, which must show progress for all
    // 3 slots at once. This deliberately leaves the live memory cache and CurrentSlot
    // alone - that is the whole difference from LoadGame(), which replaces the cache
    // and kicks off a scene load. Never call LoadGame() just to display a slot card.
    public SaveData PeekSlotData(int slot)
    {
        if (!HasSaveFile(slot)) return null;
        try
        {
            string json = File.ReadAllText(GetSavePath(slot));
            return JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveLoadManager] PeekSlotData failed for slot " + slot + ": " + e.Message);
            return null;
        }
    }

    public void NewGameInSlot(int slot)
    {
        SelectSlot(slot);
        NewGame();
    }

    public void LoadSlot(int slot)
    {
        SelectSlot(slot);
        LoadGame();
    }

    // ---- Settings (setting.json) ------------------------------------------------
    // Separate file, separate model, separate API from save.json / SaveData.
    // Panels mutate CurrentSettings in memory during play; the only disk write is
    // SaveSettings(), triggered by the settings save button.

    public const string SettingsFileName = "setting.json";

    public static string SettingsPath
    {
        get { return Path.Combine(Application.persistentDataPath, SettingsFileName); }
    }

    // Used when Instance is not up yet (edit-mode inspectors, early static callers).
    // Keeps every caller free of null checks.
    private static readonly SettingsData _fallbackSettings = new SettingsData();

    private SettingsData _settings = new SettingsData();

    public SettingsData Settings { get { return _settings; } }

    public static SettingsData CurrentSettings
    {
        get { return Instance != null ? Instance._settings : _fallbackSettings; }
    }

    public bool HasSettingsFile()
    {
        return File.Exists(SettingsPath);
    }

    // The one and only settings disk write.
    public void SaveSettings()
    {
        string json = JsonConvert.SerializeObject(_settings, JsonSettings);
        File.WriteAllText(SettingsPath, json);
        Debug.Log("[SaveLoadManager] Settings saved to " + SettingsPath);
    }

    // Called once from Awake(). A missing or unreadable file leaves the defaults intact.
    public void LoadSettings()
    {
        if (!HasSettingsFile())
        {
            Debug.Log("[SaveLoadManager] No settings file - using defaults.");
            return;
        }

        SettingsData loaded = null;
        try
        {
            string json = File.ReadAllText(SettingsPath);
            loaded = JsonConvert.DeserializeObject<SettingsData>(json, JsonSettings);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveLoadManager] Failed to read settings file: " + e.Message);
            return;
        }

        if (loaded == null)
        {
            Debug.LogError("[SaveLoadManager] Settings file deserialized to null - keeping defaults.");
            return;
        }

        if (loaded.InputBindingsJson == null) loaded.InputBindingsJson = "";
        _settings = loaded;
    }

    private void CapturePlayerStats()
    {
        PlayerStats ps = PlayerStats.Instance;
        if (ps == null)
        {
            Debug.LogWarning("[SaveLoadManager] PlayerStats.Instance is null - player stats not captured.");
            return;
        }
        _data.PlayerStats.MaxHealth = ps.MaxHealth;
        _data.PlayerStats.MaxTotalHealth = ps.MaxTotalHealth;
    }

    // ---- Load flow -------------------------------------------------------------
    // Entry point for BOTH "continue game" and "checkpoint revive". There is no separate
    // revive method: reviving at a checkpoint is exactly "load the last saved state".
    public void LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveLoadManager] LoadGame aborted - no save file at " + SavePath);
            return;
        }

        SaveData loaded = null;
        try
        {
            string json = File.ReadAllText(SavePath);
            loaded = JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
        }
        catch (System.Exception e)
        {
            AbortLoadToMainMenu("save file unreadable or malformed: " + e.Message);
            return;
        }

        if (loaded == null)
        {
            AbortLoadToMainMenu("save file deserialized to null");
            return;
        }

        _data = loaded;
        EnsureCollections();

        if (string.IsNullOrEmpty(_data.SceneName))
        {
            AbortLoadToMainMenu("saved SceneName is empty");
            return;
        }

        StartCoroutine(LoadSceneAndRestoreRoutine(_data.SceneName, _data.SpawnPointName));
    }

    // Phase 15 (D-04): every load failure ends here instead of a silent return, so the player
    // always gets a visible outcome instead of a frozen menu or a half-restored scene.
    // D-05: this performs NO file write and NO delete - the possibly corrupt save file is left
    // exactly as it is so it can still be recovered by hand.
    private void AbortLoadToMainMenu(string reason)
    {
        Debug.LogError("[SaveLoadManager] Load failed (" + reason + ") - returning to " +
                       MainMenuSceneName + ". Save file left untouched.");
        SceneManager.LoadScene(MainMenuSceneName);
    }

    // A hand-edited or older save file can contain nulls where the schema expects
    // collections. Re-materialize them so callers never have to null-check.
    private void EnsureCollections()
    {
        if (_data.PlayerStats == null) _data.PlayerStats = new PlayerStatsSaveData();
        if (_data.BossProgress == null) _data.BossProgress = new Dictionary<string, bool>();
        if (_data.MapGimmickState == null) _data.MapGimmickState = new Dictionary<string, bool>();
        if (_data.Items == null) _data.Items = new List<string>();
    }

    // Coroutine-based (IEnumerator/StartCoroutine) - the codebase does not use C# Task-based keywords anywhere.
    private IEnumerator LoadSceneAndRestoreRoutine(string sceneName, string spawnPointName)
    {
        // D-05: set the static field BEFORE the load, exactly like SignpostPortal does.
        // The freshly instantiated Player in the target scene consumes it in its own
        // PlayerSpawner.Start() -> ApplySpawn(), so no explicit "move the player" call is
        // needed here and no second respawn code path is invented.
        PlayerSpawner.targetSpawnPointName = spawnPointName;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            AbortLoadToMainMenu("LoadSceneAsync returned null for scene '" + sceneName +
                                "' - not registered in Build Settings?");
            yield break;
        }

        yield return op;

        // By the time the yield resumes, the new scene is active and its Awake/Start have
        // run - including HP.Awake() which forces health = maxHealth. Stats must therefore
        // be restored AFTER this point, not before.
        ApplyPlayerStatsFromSave();
    }

    private void ApplyPlayerStatsFromSave()
    {
        PlayerStats ps = PlayerStats.Instance;
        if (ps == null)
        {
            AbortLoadToMainMenu("PlayerStats.Instance is null after scene load");
            return;
        }

        ps.RestoreStats(_data.PlayerStats.MaxHealth, _data.PlayerStats.MaxTotalHealth);
        Debug.Log("[SaveLoadManager] Restored stats: " + ps.Health + "/" + ps.MaxHealth +
                  " (maxTotal " + ps.MaxTotalHealth + "), scene=" + SceneManager.GetActiveScene().name +
                  ", spawnPoint=" + _data.SpawnPointName);
    }

    // ---- Verification hooks (D-04: no menu UI exists in this phase) -------------
    // Select the runtime "SaveLoadManager" GameObject in the Hierarchy during Play mode
    // and use the component's gear menu to drive these. See Assets/SaveSystem/Check.md.

    [ContextMenu("Phase11/1. Save Now")]
    private void DebugSaveNow()
    {
        Save();
    }

    [ContextMenu("Phase11/2. Load Game")]
    private void DebugLoadGame()
    {
        LoadGame();
    }

    [ContextMenu("Phase11/3. New Game (memory only)")]
    private void DebugNewGame()
    {
        NewGame();
    }

    [ContextMenu("Phase11/4. Log State")]
    private void DebugLogState()
    {
        Debug.Log("[SaveLoadManager] slot=" + CurrentSlot +
                  " path=" + SavePath +
                  " exists=" + HasSaveFile() +
                  " scene=" + _data.SceneName +
                  " spawnPoint=" + _data.SpawnPointName +
                  " reviveHp=" + _data.PlayerStats.MaxHealth + "/" + _data.PlayerStats.MaxHealth +
                  " maxTotal=" + _data.PlayerStats.MaxTotalHealth +
                  " bossProgress=" + _data.BossProgress.Count +
                  " gimmicks=" + _data.MapGimmickState.Count +
                  " items=" + _data.Items.Count);
    }

    [ContextMenu("Settings/1. Save Settings")]
    private void DebugSaveSettings()
    {
        SaveSettings();
    }

    [ContextMenu("Settings/2. Load Settings")]
    private void DebugLoadSettings()
    {
        LoadSettings();
    }

    [ContextMenu("Settings/3. Log Settings")]
    private void DebugLogSettings()
    {
        Debug.Log("[SaveLoadManager] settingsPath=" + SettingsPath +
                  " exists=" + HasSettingsFile() +
                  " lang=" + _settings.Language +
                  " shake=" + _settings.ScreenShake +
                  " hint=" + _settings.TutorialHint +
                  " screenMode=" + _settings.ScreenMode +
                  " bgm=" + _settings.BgmVolume +
                  " sfx=" + _settings.SfxVolume +
                  " bindingsLen=" + (_settings.InputBindingsJson != null ? _settings.InputBindingsJson.Length : 0));
    }

    [ContextMenu("Phase14/1. Log All Slots")]
    private void DebugLogAllSlots()
    {
        for (int slot = 0; slot < SlotCount; slot++)
        {
            SaveData d = PeekSlotData(slot);
            Debug.Log("[SaveLoadManager] slot " + slot +
                      " path=" + GetSavePath(slot) +
                      " exists=" + HasSaveFile(slot) +
                      " scene=" + (d != null ? d.SceneName : "-") +
                      " bossProgress=" + (d != null && d.BossProgress != null ? d.BossProgress.Count : 0));
        }
        Debug.Log("[SaveLoadManager] CurrentSlot=" + CurrentSlot);
    }

    [ContextMenu("Phase14/2. Select Slot 0")]
    private void DebugSelectSlot0() { SelectSlot(0); Debug.Log("[SaveLoadManager] CurrentSlot=" + CurrentSlot); }

    [ContextMenu("Phase14/3. Select Slot 1")]
    private void DebugSelectSlot1() { SelectSlot(1); Debug.Log("[SaveLoadManager] CurrentSlot=" + CurrentSlot); }

    [ContextMenu("Phase14/4. Select Slot 2")]
    private void DebugSelectSlot2() { SelectSlot(2); Debug.Log("[SaveLoadManager] CurrentSlot=" + CurrentSlot); }
}
