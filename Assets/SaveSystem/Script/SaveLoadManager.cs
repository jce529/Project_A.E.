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
}
