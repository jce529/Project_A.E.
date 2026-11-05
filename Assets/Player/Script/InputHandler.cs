using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Converts the shared Player input action map into C# events used by gameplay code.
/// Its lifetime is owned by the PersistentManagers root.
/// </summary>
public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    [Header("Input Settings")]
    [Tooltip("Assign Assets/InputSystem_Actions.inputactions on PersistentManagers.prefab.")]
    public InputActionAsset inputActions;

    public event Action<Vector2> OnMoveEvent;
    public event Action OnJumpEvent;
    public event Action OnPauseEvent;
    public event Action<bool> OnRunEvent;
    public event Action OnDashEvent;
    public event Action OnBasicAttackEvent;
    public event Action OnSkill1Event;
    public event Action OnSkill2Event;
    public event Action OnHealEvent;
    public event Action OnInteractEvent;
    public event Action OnSkillQEvent;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction pauseAction;
    private InputAction runAction;
    private InputAction dashAction;
    private InputAction basicAttack;
    private InputAction skill1Action;
    private InputAction skill2Action;
    private InputAction healAction;
    private InputAction interactAction;
    private InputAction skillQAction;

    private bool initialized;
    private bool callbacksBound;
    private bool ownsRuntimeActions;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inputActions == null)
        {
            Debug.LogError(
                "InputHandler: InputSystem_Actions is not assigned on PersistentManagers.prefab.");
            return;
        }

        // A private runtime copy prevents another component from enabling/disabling the
        // imported asset shared by the project or from accumulating callback state on it.
        inputActions = Instantiate(inputActions);
        inputActions.name = "InputSystem_Actions (Runtime)";
        ownsRuntimeActions = true;

        InputActionMap playerMap = inputActions.FindActionMap("Player", false);
        if (playerMap == null)
        {
            Debug.LogError("InputHandler: the 'Player' action map could not be found.");
            return;
        }

        moveAction = playerMap.FindAction("Move", false);
        jumpAction = playerMap.FindAction("Jump", false);
        pauseAction = playerMap.FindAction("Pause", false);
        runAction = playerMap.FindAction("Run", false);
        dashAction = playerMap.FindAction("Dash", false);
        basicAttack = playerMap.FindAction("BasicAttack", false);
        skill1Action = playerMap.FindAction("Skill_1", false);
        skill2Action = playerMap.FindAction("Skill_2", false);
        healAction = playerMap.FindAction("Heal", false);
        interactAction = playerMap.FindAction("Interact", false);
        skillQAction = playerMap.FindAction("Action", false);

        LoadBindingOverrides();
        initialized = true;
    }

    private void OnEnable()
    {
        if (Instance != this || !initialized)
            return;

        BindCallbacks();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (Instance != this)
            return;

        if (inputActions != null)
            inputActions.Disable();

        UnbindCallbacks();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        UnbindCallbacks();

        if (inputActions != null)
            inputActions.Disable();

        if (ownsRuntimeActions && inputActions != null)
            Destroy(inputActions);

        Instance = null;
    }

    private void BindCallbacks()
    {
        if (callbacksBound)
            return;

        if (moveAction != null)
        {
            moveAction.performed += HandleMovePerformed;
            moveAction.canceled += HandleMoveCanceled;
        }

        if (jumpAction != null) jumpAction.performed += HandleJumpPerformed;
        if (pauseAction != null) pauseAction.performed += HandlePausePerformed;
        if (runAction != null)
        {
            runAction.performed += HandleRunPerformed;
            runAction.canceled += HandleRunCanceled;
        }

        if (dashAction != null) dashAction.performed += HandleDashPerformed;
        if (basicAttack != null) basicAttack.performed += HandleBasicAttackPerformed;
        if (skill1Action != null) skill1Action.performed += HandleSkill1Performed;
        if (skill2Action != null) skill2Action.performed += HandleSkill2Performed;
        if (healAction != null) healAction.performed += HandleHealPerformed;
        if (interactAction != null) interactAction.performed += HandleInteractPerformed;
        if (skillQAction != null) skillQAction.performed += HandleSkillQPerformed;

        callbacksBound = true;
    }

    private void UnbindCallbacks()
    {
        if (!callbacksBound)
            return;

        if (moveAction != null)
        {
            moveAction.performed -= HandleMovePerformed;
            moveAction.canceled -= HandleMoveCanceled;
        }

        if (jumpAction != null) jumpAction.performed -= HandleJumpPerformed;
        if (pauseAction != null) pauseAction.performed -= HandlePausePerformed;
        if (runAction != null)
        {
            runAction.performed -= HandleRunPerformed;
            runAction.canceled -= HandleRunCanceled;
        }

        if (dashAction != null) dashAction.performed -= HandleDashPerformed;
        if (basicAttack != null) basicAttack.performed -= HandleBasicAttackPerformed;
        if (skill1Action != null) skill1Action.performed -= HandleSkill1Performed;
        if (skill2Action != null) skill2Action.performed -= HandleSkill2Performed;
        if (healAction != null) healAction.performed -= HandleHealPerformed;
        if (interactAction != null) interactAction.performed -= HandleInteractPerformed;
        if (skillQAction != null) skillQAction.performed -= HandleSkillQPerformed;

        callbacksBound = false;
    }

    private void HandleMovePerformed(InputAction.CallbackContext context) =>
        OnMoveEvent?.Invoke(context.ReadValue<Vector2>());

    private void HandleMoveCanceled(InputAction.CallbackContext context) =>
        OnMoveEvent?.Invoke(Vector2.zero);

    private void HandleJumpPerformed(InputAction.CallbackContext context) => OnJumpEvent?.Invoke();
    private void HandlePausePerformed(InputAction.CallbackContext context) => OnPauseEvent?.Invoke();
    private void HandleRunPerformed(InputAction.CallbackContext context) => OnRunEvent?.Invoke(true);
    private void HandleRunCanceled(InputAction.CallbackContext context) => OnRunEvent?.Invoke(false);
    private void HandleDashPerformed(InputAction.CallbackContext context) => OnDashEvent?.Invoke();
    private void HandleBasicAttackPerformed(InputAction.CallbackContext context) => OnBasicAttackEvent?.Invoke();
    private void HandleSkill1Performed(InputAction.CallbackContext context) => OnSkill1Event?.Invoke();
    private void HandleSkill2Performed(InputAction.CallbackContext context) => OnSkill2Event?.Invoke();
    private void HandleHealPerformed(InputAction.CallbackContext context) => OnHealEvent?.Invoke();
    private void HandleInteractPerformed(InputAction.CallbackContext context) => OnInteractEvent?.Invoke();
    private void HandleSkillQPerformed(InputAction.CallbackContext context) => OnSkillQEvent?.Invoke();

    public void SaveBindingOverrides()
    {
        if (inputActions == null)
            return;

        SaveLoadManager.CurrentSettings.InputBindingsJson =
            inputActions.SaveBindingOverridesAsJson();
    }

    public void LoadBindingOverrides()
    {
        if (inputActions == null)
            return;

        string json = SaveLoadManager.CurrentSettings.InputBindingsJson;
        if (!string.IsNullOrEmpty(json))
            inputActions.LoadBindingOverridesFromJson(json);
    }

    public InputAction GetAction(string actionName)
    {
        if (inputActions == null)
        {
            Debug.LogError("InputHandler: the runtime Input Action Asset is unavailable.");
            return null;
        }

        InputAction foundAction = inputActions.FindAction(actionName, false);
        if (foundAction == null)
            Debug.LogError($"InputHandler: action '{actionName}' could not be found.");

        return foundAction;
    }
}
