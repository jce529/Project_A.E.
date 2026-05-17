#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CombatPrefabGenerator : EditorWindow
{
    private enum Tab { Projectile, Clone }
    private Tab _currentTab = Tab.Projectile;

    // Projectile Fields
    private string _projName = "NewProjectile";
    private float _projSpeed = 8f;
    private float _projDamage = 10f;
    private Sprite _projSprite;

    // Clone Fields
    private string _cloneName = "NewClone";
    private GameObject _baseBossObject;

    [MenuItem("Tools/Combat/Prefab Generator")]
    public static void ShowWindow()
    {
        GetWindow<CombatPrefabGenerator>("Combat Prefab Maker");
    }

    private void OnGUI()
    {
        _currentTab = (Tab)GUILayout.Toolbar((int)_currentTab, new string[] { "Projectile", "Clone" });

        EditorGUILayout.Space(10);

        if (_currentTab == Tab.Projectile)
        {
            DrawProjectileTab();
        }
        else
        {
            DrawCloneTab();
        }
    }

    private void DrawProjectileTab()
    {
        GUILayout.Label("Projectile Prefab Settings", EditorStyles.boldLabel);
        _projName = EditorGUILayout.TextField("Prefab Name", _projName);
        _projSpeed = EditorGUILayout.FloatField("Speed", _projSpeed);
        _projDamage = EditorGUILayout.FloatField("Damage", _projDamage);
        _projSprite = (Sprite)EditorGUILayout.ObjectField("Sprite (Optional)", _projSprite, typeof(Sprite), false);

        if (GUILayout.Button("Generate Projectile Prefab"))
        {
            CreateProjectile();
        }
    }

    private void DrawCloneTab()
    {
        GUILayout.Label("Clone Prefab Settings", EditorStyles.boldLabel);
        _cloneName = EditorGUILayout.TextField("Clone Name", _cloneName);
        _baseBossObject = (GameObject)EditorGUILayout.ObjectField("Base Boss (Prefab/Object)", _baseBossObject, typeof(GameObject), true);

        if (GUILayout.Button("Generate Clone Prefab"))
        {
            CreateClone();
        }
    }

    private void CreateProjectile()
    {
        string path = GetSavePath(_projName);
        if (string.IsNullOrEmpty(path)) return;

        GameObject go = new GameObject(_projName);
        
        // Add Sprite
        if (_projSprite != null)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _projSprite;
        }

        // Add Physics
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        // Add Script
        var script = go.AddComponent<WaterSpitProjectile>();
        // Reflection or public access to set defaults if needed, 
        // but simple enough to let user edit prefab later.
        
        SavePrefab(go, path);
        DestroyImmediate(go);
    }

    private void CreateClone()
    {
        if (_baseBossObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Base Boss object를 할당해주세요.", "OK");
            return;
        }

        string path = GetSavePath(_cloneName);
        if (string.IsNullOrEmpty(path)) return;

        // Instantiate base to modify
        GameObject go = Instantiate(_baseBossObject);
        go.name = _cloneName;

        // Ensure Spirit components exist
        var stats = go.GetComponent<SpiritStats>();
        if (stats == null) stats = go.AddComponent<SpiritStats>();
        stats.IsDummy = true;

        var ctrl = go.GetComponent<SpiritController>();
        if (ctrl == null) ctrl = go.AddComponent<SpiritController>();

        SavePrefab(go, path);
        DestroyImmediate(go);
    }

    private string GetSavePath(string fileName)
    {
        string dir = "Assets/Resources";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        string path = $"{dir}/{fileName}.prefab";
        if (File.Exists(path))
        {
            if (!EditorUtility.DisplayDialog("Overwrite", $"파일이 이미 존재합니다: {path}\n덮어쓰시겠습니까?", "Yes", "No"))
            {
                return null;
            }
        }
        return path;
    }

    private void SavePrefab(GameObject go, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(go, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CombatPrefabGenerator] Prefab saved to: {path}");
        EditorUtility.DisplayDialog("Success", $"프리팹이 생성되었습니다:\n{path}", "OK");
    }
}
#endif
