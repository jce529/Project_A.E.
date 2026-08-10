// Phase 11 - SaveLoadManager
// Decisions: D-01 (manager owns save/load logic; callers call in, no event bus),
//            D-02 (single slot: one save.json under Application.persistentDataPath),
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
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public const string SaveFileName = "save.json";

    // Fallback location used by NewGame(). "1 stage" is the only gameplay scene currently
    // registered in EditorBuildSettings, so it is the only value LoadSceneAsync can resolve.
    [SerializeField] private string defaultSceneName = "1 stage";
    [SerializeField] private string defaultSpawnPointName = "";

    private static readonly JsonSerializerSettings SaveSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Include,
    };

    private SaveData _data = new SaveData();

    // Read-only view for future systems (boss progress / gimmick queries).
    public SaveData Data { get { return _data; } }

    public static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
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
        string json = JsonConvert.SerializeObject(_data, SaveSettings);
        File.WriteAllText(SavePath, json);
        Debug.Log("[SaveLoadManager] Saved to " + SavePath);
    }

    // D-01 integration point A: Checkpoint.cs calls this on S-key activation.
    // The checkpoint's own GameObject name doubles as the PlayerSpawner spawn point name.
    public void SaveAtCheckpoint(string checkpointName)
    {
        _data.SceneName = SceneManager.GetActiveScene().name;
        _data.SpawnPointName = checkpointName;
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

    private void CapturePlayerStats()
    {
        PlayerStats ps = PlayerStats.Instance;
        if (ps == null)
        {
            Debug.LogWarning("[SaveLoadManager] PlayerStats.Instance is null - player stats not captured.");
            return;
        }
        _data.PlayerStats.Health = ps.Health;
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
            loaded = JsonConvert.DeserializeObject<SaveData>(json, SaveSettings);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveLoadManager] Failed to read save file: " + e.Message);
            return;
        }

        if (loaded == null)
        {
            Debug.LogError("[SaveLoadManager] Save file deserialized to null - aborting load.");
            return;
        }

        _data = loaded;
        EnsureCollections();

        if (string.IsNullOrEmpty(_data.SceneName))
        {
            Debug.LogError("[SaveLoadManager] Saved SceneName is empty - aborting load.");
            return;
        }

        StartCoroutine(LoadSceneAndRestoreRoutine(_data.SceneName, _data.SpawnPointName));
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
            Debug.LogError("[SaveLoadManager] LoadSceneAsync returned null for scene '" + sceneName +
                           "'. Is it registered in Build Settings?");
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
            Debug.LogError("[SaveLoadManager] PlayerStats.Instance is null after scene load - stats NOT restored.");
            return;
        }

        ps.RestoreStats(_data.PlayerStats.Health, _data.PlayerStats.MaxHealth, _data.PlayerStats.MaxTotalHealth);
        Debug.Log("[SaveLoadManager] Restored stats: " + ps.Health + "/" + ps.MaxHealth +
                  " (maxTotal " + ps.MaxTotalHealth + "), scene=" + SceneManager.GetActiveScene().name +
                  ", spawnPoint=" + _data.SpawnPointName);
    }
}
