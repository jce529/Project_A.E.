using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerInteractionVerification
{
    [MenuItem("Tools/Verification/Player Interaction")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Run interaction verification in Edit mode.");
        Scene previous = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
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
            Debug.Log("PlayerInteractionVerification PASSED");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
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
