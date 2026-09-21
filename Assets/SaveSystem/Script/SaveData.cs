// Phase 11 - Save data schema (POCO).
// Decisions: D-02 (single slot), D-03 (dictionary stubs), D-03b (item list stub),
//            D-03c (player stat fields), D-05 (name-based spawn restore, not raw XY).
// Serialized by Newtonsoft.Json - Dictionary<string, bool> round-trips natively,
// which is exactly why UnityEngine.JsonUtility cannot be used here.

using System.Collections.Generic;

public class SaveData
{
    // Schema version for future migration. Bump only when field meaning changes.
    public int SaveVersion = 3;

    // D-05: location is stored as scene name + spawn point GameObject name,
    // NOT raw x/y floats. PlayerSpawner.targetSpawnPointName consumes SpawnPointName
    // and PlayerSpawner.ApplySpawn() resolves it by GameObject.name lookup.
    public string SceneName = "";
    public string SpawnPointName = "";

    // Phase 15 revised health policy: current health is transient and is never persisted.
    // Only maximum-health progression is saved; loading always revives the player at full health.
    public PlayerStatsSaveData PlayerStats = new PlayerStatsSaveData();

    // D-03: stub. Key = boss id string ("TutorialBoss", "WoodBoss", "WaterSpirit",
    // "WaterMonster"). Value = defeated. Entries are added on boss death only.
    public Dictionary<string, bool> BossProgress = new Dictionary<string, bool>();

    // D-03: stub. Key = gimmick id string. Currently never written - the project has
    // no persistent map gimmicks yet. Schema exists so later phases can fill it.
    public Dictionary<string, bool> MapGimmickState = new Dictionary<string, bool>();

    // Phase 19 (D-02): no longer a stub. One entry per non-empty inventory slot at save
    // time, in slot order. Slot index is deliberately NOT stored - restore replays the
    // entries through Inventory.TryAddItem(), which may merge same-item entries into
    // fewer slots. That is accepted behavior, not a bug (no inventory UI exists yet).
    public List<ItemSaveEntry> Items = new List<ItemSaveEntry>();
}

public class PlayerStatsSaveData
{
    public float MaxHealth;
    public float MaxTotalHealth;
}

// Phase 19 (D-02): flat two-field POCO, serialized by default Newtonsoft.Json rules
// exactly like PlayerStatsSaveData - no custom JSON converter, no attributes needed.
// itemId matches ItemData.Id (the hand-typed [SerializeField] string), NOT the asset
// filename - the two already differ for both existing assets.
public class ItemSaveEntry
{
    public string itemId = "";
    public int count;
}
