#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlaceWaterMonsterInScene
{
    [MenuItem("Tools/Place WaterMonster in InGame Scene")]
    public static void Place()
    {
        var scenePath = "Assets/Scenes/InGame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Remove any existing WaterMonster to keep idempotent
        var existing = GameObject.Find("WaterMonster");
        if (existing != null) Object.DestroyImmediate(existing);

        var boss = new GameObject("WaterMonster");
        boss.transform.position = new Vector3(5f, 0f, 0f);

        var sr = boss.AddComponent<SpriteRenderer>();
        // Placeholder: assign Unity's built-in Square sprite
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = new Color(0.3f, 0.6f, 0.9f, 1f);

        var rb = boss.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var col = boss.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);

        var anim = boss.AddComponent<Animator>();
        var ac = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(
            "Assets/Enemy/WaterMonster/Animations/WaterMonster.controller");
        anim.runtimeAnimatorController = ac;

        // RequireComponent on WaterMonsterController forces WaterMonsterStats
        var stats = boss.AddComponent<WaterMonsterStats>();
        stats.MaxHealth = 500f;
        stats.MaxWater = 0f;
        stats.WaterDecayRate = 0f;

        boss.AddComponent<WaterMonsterController>();

        // Child HitBox (WaveSlice searches by tag)
        var hitbox = new GameObject("HitBox");
        hitbox.tag = "HitBox";
        hitbox.transform.SetParent(boss.transform, false);
        var hitCol = hitbox.AddComponent<BoxCollider2D>();
        hitCol.size = new Vector2(1.2f, 1.2f);
        hitCol.isTrigger = false;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[PlaceWaterMonsterInScene] Added WaterMonster to InGame.unity");
    }
}
#endif
