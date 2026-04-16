#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaterMonster.Phase2;

public static class BuildPhase2Assets
{
    [MenuItem("Tools/Phase2/Build WaterPuddle Prefab")]
    public static void BuildWaterPuddlePrefab()
    {
        // 1. Register "WaterPuddle" tag
        RegisterTag("WaterPuddle");

        // 2. Create WaterPuddle prefab
        const string resourcesPath = "Assets/Enemy/WaterMonster/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Enemy/WaterMonster"))
            {
                AssetDatabase.CreateFolder("Assets/Enemy", "WaterMonster");
            }
            AssetDatabase.CreateFolder("Assets/Enemy/WaterMonster", "Resources");
        }

        GameObject puddleObj = new GameObject("WaterPuddle");
        puddleObj.tag = "WaterPuddle";

        var sr = puddleObj.AddComponent<SpriteRenderer>();
        // Placeholder sprite
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = Color.white;

        var col = puddleObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.5f;

        puddleObj.AddComponent<WaterPuddle>();

        string prefabPath = resourcesPath + "/WaterPuddle.prefab";
        PrefabUtility.SaveAsPrefabAsset(puddleObj, prefabPath);
        Object.DestroyImmediate(puddleObj);

        AssetDatabase.SaveAssets();
        Debug.Log($"[BuildPhase2Assets] WaterPuddle prefab built at {prefabPath} and 'WaterPuddle' tag registered.");
    }

    [MenuItem("Tools/Phase2/Place Phase2 Objects in Scene")]
    public static void PlacePhase2Objects()
    {
        var scene = EditorSceneManager.GetActiveScene();
        
        // Remove existing Phase2_Weather to keep idempotent
        var existing = GameObject.Find("Phase2_Weather");
        if (existing != null) Object.DestroyImmediate(existing);

        // 1. Create "Phase2_Weather" parent
        GameObject phase2Root = new GameObject("Phase2_Weather");

        // 2. Create "RainArea" child
        GameObject rainArea = new GameObject("RainArea");
        rainArea.transform.SetParent(phase2Root.transform);
        var rainCol = rainArea.AddComponent<BoxCollider2D>();
        rainCol.isTrigger = true;
        rainCol.size = new Vector2(30f, 20f);

        // 3. Create "WeatherController" child
        GameObject weatherObj = new GameObject("WeatherController");
        weatherObj.transform.SetParent(phase2Root.transform);
        var weatherCtrl = weatherObj.AddComponent<WeatherController>();

        // Create "RainParticle" child
        GameObject rainParticleObj = new GameObject("RainParticle");
        rainParticleObj.transform.SetParent(weatherObj.transform);
        var ps = rainParticleObj.AddComponent<ParticleSystem>();
        ConfigureRainParticle(ps);
        ps.Stop();

        // 4. Create "PuddleSpawner" child
        GameObject spawnerObj = new GameObject("PuddleSpawner");
        spawnerObj.transform.SetParent(phase2Root.transform);
        var spawner = spawnerObj.AddComponent<PuddleSpawner>();

        // 5. Create "PuddlePool" child
        GameObject poolObj = new GameObject("PuddlePool");
        poolObj.transform.SetParent(phase2Root.transform);
        var pool = poolObj.AddComponent<PuddlePool>();
        GameObject puddlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Enemy/WaterMonster/Resources/WaterPuddle.prefab");
        
        // 6. Create "PuddleStackManager" child
        GameObject stackManagerObj = new GameObject("PuddleStackManager");
        stackManagerObj.transform.SetParent(phase2Root.transform);
        stackManagerObj.AddComponent<PuddleStackManager>();

        // Wiring references
        SerializedObject weatherSO = new SerializedObject(weatherCtrl);
        weatherSO.FindProperty("rainParticle").objectReferenceValue = ps;
        weatherSO.FindProperty("mapBounds").objectReferenceValue = rainCol;
        weatherSO.FindProperty("_puddleSpawner").objectReferenceValue = spawner;
        weatherSO.ApplyModifiedProperties();

        SerializedObject spawnerSO = new SerializedObject(spawner);
        spawnerSO.FindProperty("spawnBounds").objectReferenceValue = rainCol;
        spawnerSO.ApplyModifiedProperties();

        SerializedObject poolSO = new SerializedObject(pool);
        poolSO.FindProperty("puddlePrefab").objectReferenceValue = puddlePrefab;
        poolSO.ApplyModifiedProperties();

        // 8. Wire WaterMonsterController._weatherController
        var boss = Object.FindObjectOfType<WaterMonsterController>();
        if (boss != null)
        {
            SerializedObject bossSO = new SerializedObject(boss);
            bossSO.FindProperty("_weatherController").objectReferenceValue = weatherCtrl;
            bossSO.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[BuildPhase2Assets] WaterMonsterController not found in scene.");
        }

        // 9. Wire PlayerAbsorb on Player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var absorb = player.GetComponent<PlayerAbsorb>();
            if (absorb == null) absorb = player.AddComponent<PlayerAbsorb>();

            var waterCtrl = Object.FindObjectOfType<WaterController>();
            if (waterCtrl != null)
            {
                SerializedObject absorbSO = new SerializedObject(absorb);
                absorbSO.FindProperty("_waterController").objectReferenceValue = waterCtrl;
                absorbSO.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[BuildPhase2Assets] WaterController not found in scene.");
            }
        }
        else
        {
            Debug.LogWarning("[BuildPhase2Assets] Player tag not found in scene.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[BuildPhase2Assets] Phase 2 objects placed and wired successfully.");
    }

    private static void RegisterTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void ConfigureRainParticle(ParticleSystem ps)
    {
        var main = ps.main;
        main.startColor = new Color(0.7f, 0.85f, 1f, 0.6f);
        main.startSpeed = 5f;
        main.startLifetime = 2f;
        main.gravityModifier = 1f;

        var emission = ps.emission;
        emission.rateOverTime = 50f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
    }
}
#endif
