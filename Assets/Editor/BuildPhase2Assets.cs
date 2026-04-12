#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaterMonster.Phase2;

public static class BuildPhase2Assets
{
    [MenuItem("Tools/Phase2/Build WaterPuddle Prefab")]
    public static void BuildPrefab()
    {
        RegisterTag("WaterPuddle");
        CreateWaterPuddlePrefab();
        AssetDatabase.SaveAssets();
        Debug.Log("[BuildPhase2Assets] WaterPuddle prefab and tag built successfully.");
    }

    private static void RegisterTag(string tag)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;

        var tagManager = new SerializedObject(assets[0]);
        var tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void CreateWaterPuddlePrefab()
    {
        const string resourcesPath = "Assets/Enemy/WaterMonster/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets/Enemy/WaterMonster", "Resources");
        }

        var obj = new GameObject("WaterPuddle");
        obj.tag = "WaterPuddle";
        
        var sr = obj.AddComponent<SpriteRenderer>();
        // Sprite is left null for designer assignment in inspector
        
        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.5f;

        obj.AddComponent<WaterPuddle>();

        PrefabUtility.SaveAsPrefabAsset(obj, resourcesPath + "/WaterPuddle.prefab");
        Object.DestroyImmediate(obj);
    }

    [MenuItem("Tools/Phase2/Place Phase2 Objects in Scene")]
    public static void PlaceInScene()
    {
        var scenePath = "Assets/Scenes/InGame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Remove existing to ensure idempotency
        var existing = GameObject.Find("Phase2_Weather");
        if (existing != null) Object.DestroyImmediate(existing);

        var root = new GameObject("Phase2_Weather");

        // RainArea: BoxCollider2D (isTrigger = true, default map coverage size)
        var rainArea = new GameObject("RainArea");
        rainArea.transform.SetParent(root.transform);
        var rainCol = rainArea.AddComponent<BoxCollider2D>();
        rainCol.isTrigger = true;
        rainCol.size = new Vector2(30, 20);

        // WeatherController
        var weatherObj = new GameObject("WeatherController");
        weatherObj.transform.SetParent(root.transform);
        var weatherCtrl = weatherObj.AddComponent<WeatherController>();

        // RainParticle (Child of WeatherController)
        var particleObj = new GameObject("RainParticle");
        particleObj.transform.SetParent(weatherObj.transform);
        particleObj.transform.localPosition = Vector3.zero;
        var ps = particleObj.AddComponent<ParticleSystem>();
        ConfigureRainParticle(ps);
        ps.Stop();

        // PuddleSpawner
        var spawnerObj = new GameObject("PuddleSpawner");
        spawnerObj.transform.SetParent(root.transform);
        var spawner = spawnerObj.AddComponent<PuddleSpawner>();

        // PuddlePool
        var poolObj = new GameObject("PuddlePool");
        poolObj.transform.SetParent(root.transform);
        var pool = poolObj.AddComponent<PuddlePool>();
        var puddlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Enemy/WaterMonster/Resources/WaterPuddle.prefab");

        // PuddleStackManager
        var stackManagerObj = new GameObject("PuddleStackManager");
        stackManagerObj.transform.SetParent(root.transform);
        stackManagerObj.AddComponent<PuddleStackManager>();

        // Wiring via SerializedObject (for private fields)
        var soWeather = new SerializedObject(weatherCtrl);
        soWeather.FindProperty("rainParticle").objectReferenceValue = ps;
        soWeather.FindProperty("mapBounds").objectReferenceValue = rainCol;
        soWeather.FindProperty("_puddleSpawner").objectReferenceValue = spawner;
        soWeather.ApplyModifiedProperties();

        var soSpawner = new SerializedObject(spawner);
        soSpawner.FindProperty("spawnBounds").objectReferenceValue = rainCol;
        soSpawner.ApplyModifiedProperties();

        var soPool = new SerializedObject(pool);
        soPool.FindProperty("puddlePrefab").objectReferenceValue = puddlePrefab;
        soPool.ApplyModifiedProperties();

        // Wire WeatherController to WaterMonsterController
        var boss = Object.FindObjectOfType<WaterMonsterController>();
        if (boss != null)
        {
            var soBoss = new SerializedObject(boss);
            soBoss.FindProperty("_weatherController").objectReferenceValue = weatherCtrl;
            soBoss.ApplyModifiedProperties();
        }

        // Add/Wire PlayerAbsorb to Player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var absorb = player.GetComponent<PlayerAbsorb>();
            if (absorb == null) absorb = player.AddComponent<PlayerAbsorb>();

            var playerWaterCtrl = Object.FindObjectOfType<WaterController>();
            if (playerWaterCtrl != null)
            {
                var soAbsorb = new SerializedObject(absorb);
                soAbsorb.FindProperty("_waterController").objectReferenceValue = playerWaterCtrl;
                soAbsorb.ApplyModifiedProperties();
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BuildPhase2Assets] Phase 2 objects placed and wired in InGame scene successfully.");
    }

    private static void ConfigureRainParticle(ParticleSystem ps)
    {
        var main = ps.main;
        main.startSpeed = 5;
        main.startLifetime = 2;
        main.gravityModifier = 1;
        main.startColor = new Color(0.7f, 0.85f, 1f, 0.6f);
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 50;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30, 1, 1); // Default size, tuned by WeatherController at runtime
    }
}
#endif
