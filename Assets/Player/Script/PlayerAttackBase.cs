using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerAttackBase : MonoBehaviour
{
    private InputHandler input;
    private PlayerController controller;
    private PlayerInteraction interaction;
    private int pendingActions;
    protected virtual void Start() => Subscribe();
    protected virtual void OnEnable() => Subscribe();
    protected virtual void OnDisable() => Unsubscribe();
    private void Subscribe()
    {
        if (input == InputHandler.Instance && input != null) return;
        Unsubscribe();
        controller = GetComponent<PlayerController>();
        interaction = GetComponent<PlayerInteraction>();
        input = InputHandler.Instance;
        if (input == null) return;
        input.OnBasicAttackEvent += Basic;
        input.OnSkill1Event += Skill1;
        input.OnSkill2Event += Skill2;
        input.OnSkillQEvent += SkillQ;
        input.OnHealEvent += Heal;
    }
    private void Unsubscribe()
    {
        pendingActions = 0;
        if (input != null)
        {
            input.OnBasicAttackEvent -= Basic;
            input.OnSkill1Event -= Skill1;
            input.OnSkill2Event -= Skill2;
            input.OnSkillQEvent -= SkillQ;
            input.OnHealEvent -= Heal;
        }
        input = null;
    }
    private bool CanAct(InputAction action = null)
    {
        if (!GameStateManager.CanProcessGameplayInput || !isActiveAndEnabled) return false;
        if (controller != null && (controller.movementLocked || controller.IsImprisoned)) return false;
        if (interaction == null || !interaction.isActiveAndEnabled) return true;
        if (interaction.InteractedThisFrame) return false;
        var interactAction = interaction.InteractionAction;
        if (interactAction == null || !interactAction.WasPressedThisFrame()) return true;
        return interaction.FindNearest() == null;
    }
    // Dispatch after all Input System callbacks, so an Interact binding shared
    // with an attack key wins regardless of action callback order.
    private void Basic() => pendingActions |= 1;
    private void Skill1() => pendingActions |= 2;
    private void Skill2() => pendingActions |= 4;
    private void SkillQ() => pendingActions |= 8;
    private void Heal() => pendingActions |= 16;
    protected virtual void Update()
    {
        if (input != InputHandler.Instance) Subscribe();
        int actions = pendingActions;
        pendingActions = 0;
        if (!CanAct()) return;
        if (input != null)
        {
            if ((actions & 1) != 0) OnBasicAttack();
            if ((actions & 2) != 0 && CanAct()) OnSkillE();
            if ((actions & 4) != 0 && CanAct()) OnSkillR();
            if ((actions & 8) != 0 && CanAct()) OnSkillQ();
            if ((actions & 16) != 0 && CanAct()) OnHeal();
            return;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)) OnBasicAttack();
        if (Input.GetKeyDown(KeyCode.E)) OnSkillE();
        if (Input.GetKeyDown(KeyCode.R)) OnSkillR();
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnHeal();
        if (Input.GetKeyDown(KeyCode.Q)) OnSkillQ();
    }
    protected abstract void OnBasicAttack();
    protected abstract void OnSkillE();
    protected abstract void OnSkillR();
    protected abstract void OnHeal();
    protected virtual void OnSkillQ() { }
}
