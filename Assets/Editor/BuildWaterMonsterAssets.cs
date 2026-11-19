#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using TMPro;

public static class BuildWaterMonsterAssets
{
    [MenuItem("Tools/Build WaterMonster Assets")]
    public static void Build()
    {
        BuildAnimator();
        BuildPrefabs();
        AssetDatabase.SaveAssets();
        Debug.Log("[BuildWaterMonsterAssets] All assets built successfully.");
    }

    private static void BuildAnimator()
    {
        const string folderPath = "Assets/Enemy/WaterMonster/Animations";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Enemy/WaterMonster"))
            {
                AssetDatabase.CreateFolder("Assets/Enemy", "WaterMonster");
            }
            AssetDatabase.CreateFolder("Assets/Enemy/WaterMonster", "Animations");
        }

        const string path = folderPath + "/WaterMonster.controller";
        var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
        ac.AddParameter("Attack_Melee", AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Attack_Ranged", AnimatorControllerParameterType.Trigger);

        var sm = ac.layers[0].stateMachine;
        var idle = sm.AddState("Idle");
        var melee = sm.AddState("Attack_Melee");
        var ranged = sm.AddState("Attack_Ranged");
        sm.defaultState = idle;

        var t1 = idle.AddTransition(melee);
        t1.AddCondition(AnimatorConditionMode.If, 0, "Attack_Melee");
        t1.hasExitTime = false;
        t1.duration = 0;

        var t2 = melee.AddTransition(idle);
        t2.hasExitTime = true;
        t2.exitTime = 0.9f;
        t2.duration = 0;

        var t3 = idle.AddTransition(ranged);
        t3.AddCondition(AnimatorConditionMode.If, 0, "Attack_Ranged");
        t3.hasExitTime = false;
        t3.duration = 0;

        var t4 = ranged.AddTransition(idle);
        t4.hasExitTime = true;
        t4.exitTime = 0.9f;
        t4.duration = 0;

        Debug.Log($"[BuildWaterMonsterAssets] Animator built at {path}");
    }

    private static void BuildPrefabs()
    {
        // Ensure Resources folder exists
        const string resourcesPath = "Assets/Enemy/WaterMonster/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets/Enemy/WaterMonster", "Resources");
        }

        // 1. HealPopup prefab
        var popup = new GameObject("HealPopup");
        var tmp = popup.AddComponent<TextMeshPro>();
        tmp.text = "+0";
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        popup.AddComponent<HealPopup>();
        PrefabUtility.SaveAsPrefabAsset(popup, resourcesPath + "/HealPopup.prefab");
        Object.DestroyImmediate(popup);

        // 2. WaterSpitProjectile prefab
        var proj = new GameObject("WaterSpitProjectile");
        var rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        var col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;
        proj.AddComponent<WaterSpitProjectile>();
        PrefabUtility.SaveAsPrefabAsset(proj, resourcesPath + "/WaterSpitProjectile.prefab");
        Object.DestroyImmediate(proj);

        Debug.Log($"[BuildWaterMonsterAssets] Prefabs built in {resourcesPath}");
    }
}
#endif
