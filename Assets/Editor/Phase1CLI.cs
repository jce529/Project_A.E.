#if UNITY_EDITOR
using UnityEditor;

public static class Phase1CLI
{
    public static void ExecuteAll()
    {
        // 1. Build assets (animator, prefabs)
        BuildWaterMonsterAssets.Build();
        
        // 2. Place monster in scene
        PlaceWaterMonsterInScene.Place();
        
        // 3. Save all changes
        AssetDatabase.SaveAssets();
    }
}
#endif
