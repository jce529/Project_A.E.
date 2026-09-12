using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using WaterMonster.Phase2;
using System.Reflection;

public static class PlayerInteractionVerification
{
    [MenuItem("Tools/Verification/Player Interaction")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Run interaction verification in Edit mode.");
        Scene previous = SceneManager.GetActiveScene();
        var stackField = typeof(PuddleStackManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        var previousStack = PuddleStackManager.Instance;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            stackField.SetValue(null, new GameObject("Verification puddle stack").AddComponent<PuddleStackManager>());
            var player = new GameObject("Verification player").AddComponent<PlayerInteraction>();
            player.transform.position = new Vector2(10000, 10000);
            Require(!player.TryInteract(), "empty");
            var far = Target(player, 1.5f);
            var near = Target(player, 0.5f);
            near.gameObject.AddComponent<BoxCollider2D>();
            Physics2D.SyncTransforms();
            Require(player.TryInteract() && near.calls == 1 && far.calls == 0, "nearest and duplicate");
            near.eligible = false;
            Require(player.TryInteract() && far.calls == 1, "ineligible skipped");
            far.enabled = false;
            Require(!player.TryInteract(), "disabled");
            far.enabled = true;
            far.transform.position = player.transform.position + Vector3.right * 3;
            Physics2D.SyncTransforms();
            Require(!player.TryInteract(), "outside radius");
            far.gameObject.SetActive(false);
            Require(!player.TryInteract(), "inactive");
            far.gameObject.SetActive(true);
            near.eligible = true;
            far.transform.position = near.transform.position;
            Physics2D.SyncTransforms();
            int beforeNear = near.calls, beforeFar = far.calls;
            player.TryInteract();
            Require(near.calls - beforeNear == (near.GetInstanceID() < far.GetInstanceID() ? 1 : 0)
                && far.calls - beforeFar == (far.GetInstanceID() < near.GetInstanceID() ? 1 : 0), "tie");
            player.enabled = false;
            Require(!player.TryInteract(), "disabled player");
            player.enabled = true;
            beforeNear = near.calls; beforeFar = far.calls;
            player.TryInteract();
            Require(near.calls + far.calls == beforeNear + beforeFar + 1, "reenable single dispatch");
            player.RefreshTargetAndPrompt();
            Require(player.CurrentTarget != null, "prompt selects nearest");
            near.gameObject.SetActive(false);
            far.gameObject.SetActive(false);
            player.RefreshTargetAndPrompt();
            Require(player.CurrentTarget == null, "prompt clears missing target");
            VerifyPuddles(player);
            using (var action = new InputAction("Interact", binding: "<Keyboard>/f"))
            {
                string original = PlayerInteractionPrompt.GetBindingText(action);
                Require(!string.IsNullOrEmpty(original), "binding label");
                action.ApplyBindingOverride(0, "<Keyboard>/e");
                Require(PlayerInteractionPrompt.GetBindingText(action) != original, "rebound label");
            }
            Debug.Log("PlayerInteractionVerification PASSED");
        }
        finally
        {
            stackField.SetValue(null, previousStack);
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void VerifyPuddles(PlayerInteraction player)
    {
        // These objects belong only to the temporary scene; no save or scene transition actions run.
        var absorb = player.gameObject.AddComponent<PlayerAbsorb>();
        var water = player.gameObject.AddComponent<WaterController>();
        water.bottles.Add(new[] { 0, 0 });
        typeof(PlayerAbsorb).GetField("_waterController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(absorb, water);
        var world = Target(player, 1f);
        var puddle = new GameObject("Verification puddle").AddComponent<WaterPuddle>();
        puddle.transform.position = player.transform.position + Vector3.right * 0.5f;
        puddle.gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
        Physics2D.SyncTransforms();
        Require(player.TryInteract() && !puddle.isDestructible && world.calls == 0, "puddle wins one action");
        Require(water.bottles[0][0] == 1, "water recovery");
        Require(PuddleStackManager.Instance.IndestructibleCount == 1, "stack registration");
        Require(player.TryInteract() && world.calls == 1, "absorbed puddle skipped");
        puddle.OnReturnToPool();
        Require(puddle.isDestructible && !puddle.gameObject.activeSelf, "pool return reset");
        Require(PuddleStackManager.Instance.IndestructibleCount == 0, "pool unregister");
        puddle.gameObject.SetActive(true);
        typeof(PlayerAbsorb).GetField("_inputType", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(absorb, PlayerAbsorb.InputType.BasicAttack);
        Physics2D.SyncTransforms();
        Require(player.TryInteract() && world.calls == 2 && puddle.isDestructible, "other input does not compete");
        typeof(PlayerAbsorb).GetMethod("TryAbsorb", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(absorb, null);
        Require(!puddle.isDestructible, "other input absorption");
        var locked = new GameObject("Locked puzzle").AddComponent<SlidingPuzzleTrigger>();
        locked.isLocked = true;
        locked.transform.position = player.transform.position;
        locked.gameObject.AddComponent<BoxCollider2D>();
        Physics2D.SyncTransforms();
        Require(player.TryInteract() && world.calls == 3, "locked real puzzle skipped");
    }

    private static PlayerInteractionVerificationTarget Target(PlayerInteraction player, float distance)
    {
        var target = new GameObject("Verification target").AddComponent<PlayerInteractionVerificationTarget>();
        target.transform.position = player.transform.position + Vector3.right * distance;
        target.gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
        return target;
    }

    private static void Require(bool condition, string scenario)
    {
        if (!condition) throw new InvalidOperationException("Interaction verification failed: " + scenario);
    }
}

public class PlayerInteractionVerificationTarget : MonoBehaviour, IPlayerInteractable
{
    public bool eligible = true;
    public int calls;
    public bool CanInteract(PlayerInteraction player) => eligible;
    public void Interact(PlayerInteraction player) => calls++;
}
