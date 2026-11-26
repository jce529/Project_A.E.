using UnityEngine;

public abstract class PlayerInputHandler : MonoBehaviour
{
    protected virtual void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentState != GameStateManager.GameState.Playing) return;
        var controller = GetComponent<PlayerController>();
        if (controller != null && (controller.movementLocked || controller.IsImprisoned)) return;

        var interaction = GetComponent<PlayerInteraction>();
        // Rebinding Interact to a skill/attack key must not invoke both systems.
        if (interaction != null && interaction.isActiveAndEnabled &&
            (interaction.InteractedThisFrame || (interaction.InteractionAction != null &&
             interaction.InteractionAction.WasPressedThisFrame() && interaction.FindNearest() != null))) return;
        bool interactionOwnsF = interaction != null && interaction.isActiveAndEnabled &&
            (interaction.InteractedThisFrame || interaction.FindNearest() != null);
        if (Input.GetMouseButtonDown(0) || (Input.GetKeyDown(KeyCode.F) && !interactionOwnsF)) OnBasicAttack();
        if (Input.GetKeyDown(KeyCode.E)) OnSkillE();
        if (Input.GetKeyDown(KeyCode.R)) OnSkillR();
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnHeal();
        if (Input.GetKeyDown(KeyCode.Q)) OnSkillQ();
    }

    protected abstract void OnBasicAttack();
    protected abstract void OnSkillE();
    protected abstract void OnSkillR();
    protected abstract void OnHeal();
    protected abstract void OnSkillQ();
}
