#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class MergeVerification
{
    private const string Pending = "ProjectAE.MergeVerification.Pending";
    private static readonly List<string> Results = new List<string>();
    private static readonly HashSet<string> Errors = new HashSet<string>();
    private static readonly HashSet<string> EditorEnvironment = new HashSet<string>();
    private static string[] scenes;
    private static int index = -1, frames, lastFrame, failures;
    private static double deadline;
    private static Keyboard keyboard;
    static MergeVerification() => EditorApplication.playModeStateChanged += OnState;

    [MenuItem("Tools/Integration/Prepare and Verify Merged Project")]
    public static void Run()
    {
        Directory.CreateDirectory("Integration");
        PrepareScenes();
        PlayerInteractionVerification.Run();
        File.WriteAllText("Integration/interaction-verification.txt", "PASS nearest/eligible targets, duplicate colliders, same-frame dispatch, radius, inactive targets, pooled puddles, custom absorption, puzzle locks and rebound key labels.\n");
        VerifyStorage();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    private static void PrepareScenes()
    {
        var report = new List<string>();
        string[] paths = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)
            .Concat(new[] { "Assets/Scenes/IntegrationVariants/1 stage - Partner A.unity" }).ToArray();
        var animator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Player/Animation/PlayerAnim.controller");
        var healthBar = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/에셋/PlayerHealthBar.prefab");
        var waterBar = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/에셋/WaterBar.prefab");
        var bossAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Enemy/Tutorial/TutorialBoss/Resource/Anim/TutorialBoss.controller");
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (asset == null || asset.GetComponentInChildren<TutorialBoss.TutorialBossController>(true) == null) continue;
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach (var boss in prefab.GetComponentsInChildren<TutorialBoss.TutorialBossController>(true))
                    EnsureTutorialAnimator(boss, bossAnimator);
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }
        foreach (string path in paths)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int players = 0, missing = 0, managers = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                foreach (InputHandler service in root.GetComponentsInChildren<InputHandler>(true))
                    if (service.GetComponentInParent<PersistentManagers>() == null) { Object.DestroyImmediate(service); managers++; }
                foreach (GameStateManager service in root.GetComponentsInChildren<GameStateManager>(true))
                    if (service.GetComponentInParent<PersistentManagers>() == null) { Object.DestroyImmediate(service); managers++; }
                foreach (PlayerController player in root.GetComponentsInChildren<PlayerController>(true))
                {
                    players++;
                    if (player.GetComponent<PlayerInteraction>() == null) player.gameObject.AddComponent<PlayerInteraction>();
                    if (player.GetComponent<PlayerInteractionPrompt>() == null) player.gameObject.AddComponent<PlayerInteractionPrompt>();
                    Animator playerAnimator = player.GetComponent<Animator>();
                    if (playerAnimator != null && animator != null) playerAnimator.runtimeAnimatorController = animator;
                    var attack = player.GetComponent<PlayerAttack>();
                    if (attack != null)
                    {
                        attack.waterController = player.GetComponent<WaterController>();
                        attack.playerStats = player.GetComponent<PlayerStats>();
                        attack.playerAnimator = player.GetComponent<PlayerAnimator>();
                    }
                }
                foreach (var boss in root.GetComponentsInChildren<TutorialBoss.TutorialBossController>(true))
                    EnsureTutorialAnimator(boss, bossAnimator);
            }
            var canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(c => c.renderMode != RenderMode.WorldSpace);
            foreach (var ui in Object.FindObjectsByType<PlayerHealthUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (ui.healthBarPrefab == null && ui.heartsParent == null) ui.healthBarPrefab = healthBar;
                if (ui.canvasParent == null && canvas != null) ui.canvasParent = canvas.transform;
            }
            foreach (var ui in Object.FindObjectsByType<PlayerWaterUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (ui.waterBarPrefab == null && ui.bottlesParent == null) ui.waterBarPrefab = waterBar;
                if (ui.canvasParent == null && canvas != null) ui.canvasParent = canvas.transform;
                if (ui.waterController == null) ui.waterController = Object.FindAnyObjectByType<WaterController>();
            }
            foreach (var ui in Object.FindObjectsByType<WaterUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (ui.bottleSlot == null) ui.bottleSlot = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Player/Resourcess/BottleSlot.prefab");
            report.Add(path + ": players=" + players + ", missingScripts=" + missing + ", centralizedManagers=" + managers);
            if (missing > 0) throw new InvalidOperationException("Missing scripts in " + path + ": " + missing);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Integration/scene-migration.txt", report);
    }

    private static void EnsureTutorialAnimator(TutorialBoss.TutorialBossController boss, RuntimeAnimatorController controller)
    {
        var animator = boss.GetComponent<Animator>();
        if (animator == null) animator = boss.gameObject.AddComponent<Animator>();
        if (animator.runtimeAnimatorController == null) animator.runtimeAnimatorController = controller;
    }

    private static void VerifyStorage()
    {
        string folder = Path.GetFullPath("Integration/VerificationTemp");
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "atomic-save.json");
        AtomicFile.WriteAllText(path, "{\"version\":1}");
        AtomicFile.WriteAllText(path, "{\"version\":2}");
        if (File.ReadAllText(path) != "{\"version\":2}" || !File.ReadAllText(path + ".bak").Contains(":1"))
            throw new InvalidOperationException("Atomic save replacement/backup failed");
        File.Delete(path);
        File.Delete(path + ".bak");
        File.WriteAllText("Integration/storage-verification.txt", "PASS atomic replacement and previous-file backup; isolated test path; player saves untouched\n");
    }

    private static void OnState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
        SessionState.SetBool(Pending, false);
        // Editor Search can throw in unrelated startup delay callbacks. Use the
        // next editor update so that failure cannot consume our one-shot runner.
        EditorApplication.update += BeginOnNextUpdate;
    }

    private static void BeginOnNextUpdate()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        EditorApplication.update -= BeginOnNextUpdate;
        Begin();
    }

    private static void Begin()
    {
        try { VerifyIntegratedServices(); }
        catch (Exception e)
        {
            File.WriteAllText("Integration/service-verification.txt", "FAIL " + e);
            EditorApplication.Exit(1);
            return;
        }
        scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)
            .Concat(new[] { "Assets/Scenes/IntegrationVariants/1 stage - Partner A.unity" }).ToArray();
        var settings = Object.Instantiate(InputSystem.settings);
        settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings = settings;
        keyboard = InputSystem.AddDevice<Keyboard>();
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
        NextScene();
    }

    private static void VerifyIntegratedServices()
    {
        var rows = new List<string>();
        var go = new GameObject("Integration water verification");
        var water = go.AddComponent<WaterController>();
        water.AddBottle(5);
        water.UseBottle(100);
        if (water.TotalWater != 0 || water.Capacity != 5) throw new Exception("Water consumption bounds");
        water.RecoveryCorruptedWater();
        water.UseBottle(1);
        water.RecoveryWater();
        if (water.corruptedwaterCounter() != 0 || water.waterCounter() != 1) throw new Exception("Pure-water refill retained contamination");
        var random = new System.Random(42);
        for (int i = 0; i < 500; i++)
        {
            int action = random.Next(3);
            if (action == 0) water.UseBottle(random.Next(8));
            else if (action == 1) water.RecoveryWater();
            else water.RecoveryCorruptedWater();
            int count = water.bottles.Count(b => b[0] == 1);
            if (water.TotalWater != count) throw new Exception("Water cache mismatch at " + i);
        }
        Object.DestroyImmediate(go);
        rows.Add("PASS water bounds, clean refill and cache consistency across 500 mutations");

        var dialogueObject = new GameObject("Integration dialogue verification");
        dialogueObject.SetActive(false);
        var dialogue = dialogueObject.AddComponent<DialogueManager>();
        var json = new TextAsset("{\"dialogues\":[{\"speakerName\":\"Test\",\"dialogueText\":\"Test\",\"isEnd\":true,\"jumpToIndex\":-1}]}");
        dialogue.dialogueJsonData = json;
        dialogueObject.SetActive(true);
        dialogue.StartDialogue(0);
        if (!DialogueManager.AnyDialogueActive || GameStateManager.CanProcessGameplayInput) throw new Exception("Dialogue input gate");
        GameStateManager.Instance.SetState(GameStateManager.GameState.Paused);
        dialogue.EndDialogue();
        if (Time.timeScale != 0f) throw new Exception("Dialogue end unpaused menu");
        GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        if (!GameStateManager.CanProcessGameplayInput) throw new Exception("Gameplay did not resume");
        Object.DestroyImmediate(dialogueObject);
        Object.DestroyImmediate(json);
        rows.Add("PASS dialogue/menu overlap and input resume");

        var data = new SaveData { SceneName = "Tutorial Map", SpawnPointName = "Checkpoint" };
        data.BossProgress["TutorialBoss"] = true;
        var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(Newtonsoft.Json.JsonConvert.SerializeObject(data));
        if (restored.SceneName != data.SceneName || !restored.BossProgress["TutorialBoss"] || restored.SaveVersion != data.SaveVersion)
            throw new Exception("Save schema round trip");
        rows.Add("PASS original save schema, checkpoint and boss-progress JSON round trip");

        InputHandler input = InputHandler.Instance;
        if (input == null || input.inputActions == null) throw new Exception("Persistent input bootstrap");
        var actionToRebind = input.inputActions.FindAction("Player/Interact");
        if (actionToRebind == null) throw new Exception("Interaction action missing");
        string original = input.inputActions.SaveBindingOverridesAsJson();
        string previousSettings = SaveLoadManager.CurrentSettings.InputBindingsJson;
        actionToRebind.ApplyBindingOverride(0, "<Keyboard>/j");
        input.SaveBindingOverrides();
        input.inputActions.RemoveAllBindingOverrides();
        input.LoadBindingOverrides();
        if (!actionToRebind.bindings[0].effectivePath.EndsWith("/j")) throw new Exception("Settings binding restoration");
        input.inputActions.RemoveAllBindingOverrides();
        input.inputActions.LoadBindingOverridesFromJson(original);
        SaveLoadManager.CurrentSettings.InputBindingsJson = previousSettings;
        rows.Add("PASS shared-input rebinding and settings restoration without disk writes");
        File.WriteAllLines("Integration/service-verification.txt", rows);
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        // This installed editor has a Search index startup bug, outside the
        // player assemblies. Record its exact stack separately; retain every
        // gameplay error in the scene failure count.
        if (type == LogType.Exception && stack.Contains("UnityEditor.Search.SearchDatabase")
            && stack.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup"))
        {
            EditorEnvironment.Add(message + "\n" + stack);
            return;
        }
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) Errors.Add(message + "\n" + stack);
    }
    private static void NextScene()
    {
        index++;
        if (index >= scenes.Length) { Finish(); return; }
        Errors.Clear(); frames = 0; lastFrame = -1;
        deadline = EditorApplication.timeSinceStartup + 90;
        Time.timeScale = 1f;
        try { EditorSceneManager.LoadSceneInPlayMode(scenes[index], new LoadSceneParameters(LoadSceneMode.Single)); }
        catch (Exception e) { Errors.Add(e.ToString()); CompleteScene(); }
    }
    private static void Tick()
    {
        if (index < 0 || index >= scenes.Length) return;
        if (EditorApplication.timeSinceStartup > deadline) { Errors.Add("Scene timeout"); CompleteScene(); return; }
        if (Time.frameCount == lastFrame) return;
        lastFrame = Time.frameCount;
        frames++;
        Key[] keys = frames >= 10 && frames < 30 ? new[] { Key.D } : frames == 35 ? new[] { Key.Space } : Array.Empty<Key>();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.Update();
        if (frames >= 120) CompleteScene();
    }
    private static void CompleteScene()
    {
        bool isMenu = scenes[index].EndsWith("MainMenu.unity");
        if (InputHandler.Instance == null) Errors.Add("Shared InputHandler missing");
        if (GameStateManager.Instance == null) Errors.Add("Shared GameStateManager missing");
        if (SaveLoadManager.Instance == null) Errors.Add("SaveLoadManager missing");
        if (!isMenu)
        {
            if (Object.FindAnyObjectByType<PlayerController>() == null) Errors.Add("PlayerController missing");
            if (Object.FindAnyObjectByType<PlayerAttack>() == null) Errors.Add("PlayerAttack missing");
            if (Object.FindAnyObjectByType<WaterController>() == null) Errors.Add("WaterController missing");
            if (Object.FindAnyObjectByType<PlayerInteraction>() == null) Errors.Add("PlayerInteraction missing");
        }
        if (Object.FindObjectsByType<InputHandler>(FindObjectsSortMode.None).Length != 1) Errors.Add("InputHandler is not unique");
        if (Object.FindObjectsByType<GameStateManager>(FindObjectsSortMode.None).Length != 1) Errors.Add("GameStateManager is not unique");
        if (Errors.Count > 0) failures++;
        string row = (Errors.Count == 0 ? "PASS " : "FAIL ") + scenes[index] + ": " + frames + " frames; " + Errors.Count + " errors";
        Results.Add(row); Results.AddRange(Errors);
        Debug.Log("MERGE_SCENE " + row);
        NextScene();
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        string report = string.Join("\n", Results) + "\nFailures: " + failures;
        File.WriteAllText("Integration/play-verification.txt", report);
        File.WriteAllText("Integration/editor-environment.txt", EditorEnvironment.Count == 0
            ? "No editor environment exceptions captured.\n" : string.Join("\n", EditorEnvironment));
        Debug.Log("MERGE_VERIFICATION_COMPLETE\n" + report);
        EditorApplication.Exit(failures == 0 ? 0 : 1);
    }
}
#endif
