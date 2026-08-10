// Phase 11 - Save data schema (POCO).
// Decisions: D-02 (single slot), D-03 (dictionary stubs), D-03b (item list stub),
//            D-03c (player stat fields), D-05 (name-based spawn restore, not raw XY).
// Serialized by Newtonsoft.Json - Dictionary<string, bool> round-trips natively,
// which is exactly why UnityEngine.JsonUtility cannot be used here.

using System.Collections.Generic;

public class SaveData
{
    // Schema version for future migration. Bump only when field meaning changes.
    public int SaveVersion = 1;

    // D-05: location is stored as scene name + spawn point GameObject name,
    // NOT raw x/y floats. PlayerSpawner.targetSpawnPointName consumes SpawnPointName
    // and PlayerSpawner.ApplySpawn() resolves it by GameObject.name lookup.
    public string SceneName = "";
    public string SpawnPointName = "";

    // D-03c: mirrors PlayerStats/HP public getters (Health, MaxHealth, MaxTotalHealth).
    public PlayerStatsSaveData PlayerStats = new PlayerStatsSaveData();

    // D-03: stub. Key = boss id string ("TutorialBoss", "WoodBoss", "WaterSpirit",
    // "WaterMonster"). Value = defeated. Entries are added on boss death only.
    public Dictionary<string, bool> BossProgress = new Dictionary<string, bool>();

    // D-03: stub. Key = gimmick id string. Currently never written - the project has
    // no persistent map gimmicks yet. Schema exists so later phases can fill it.
    public Dictionary<string, bool> MapGimmickState = new Dictionary<string, bool>();

    // D-03b: stub. Empty list - no item/inventory system exists in the project yet.
    public List<string> Items = new List<string>();
}

public class PlayerStatsSaveData
{
    public float Health;
    public float MaxHealth;
    public float MaxTotalHealth;
}
