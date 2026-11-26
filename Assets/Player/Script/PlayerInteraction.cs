using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Owns target selection and dispatches one interaction per input.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInteractionPrompt))]
public class PlayerInteraction : MonoBehaviour
{
    [Min(0f)] public float interactionRadius = 2f;
    public LayerMask interactionLayers = ~0;

    public IPlayerInteractable CurrentTarget { get; private set; }
    public InputAction InteractionAction { get; private set; }
    public WaterController WaterController => GetComponent<WaterController>();
    public PlayerRespawn Respawn => GetComponent<PlayerRespawn>();
    public bool InteractedThisFrame => _lastInteractionFrame == Time.frameCount;

    private readonly List<Collider2D> _hits = new List<Collider2D>();
    private readonly HashSet<MonoBehaviour> _visited = new HashSet<MonoBehaviour>();
    private InputHandler _input;
    private InputAction _directAction;
    private InputAction _fallbackAction;
    private PlayerInteractionPrompt _prompt;
    private PlayerController _controller;
    private PlayerInput _playerInput;
    private int _lastInteractionFrame = -1;
    private bool _enabledDirectAction;

    public bool CanProcessInput
    {
        get
        {
            if (!isActiveAndEnabled || Time.timeScale <= 0f) return false;
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState != GameStateManager.GameState.Playing) return false;
            return _controller == null || (!_controller.movementLocked && !_controller.IsImprisoned);
        }
    }

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _playerInput = GetComponent<PlayerInput>();
        _prompt = GetComponent<PlayerInteractionPrompt>();
    }

    private void OnEnable() => RefreshInput();
    private void Update() => RefreshInput();
    private void LateUpdate() => RefreshTargetAndPrompt();

    private void OnDisable()
    {
        UnsubscribeInput();
        CurrentTarget = null;
        if (_prompt != null) _prompt.Show(null, null);
    }

    private void OnDestroy() => _fallbackAction?.Dispose();

    private static InputAction FindAction(InputActionAsset actions)
    {
        var map = actions != null ? actions.FindActionMap("Player") : null;
        return map?.FindAction("Interact") ?? map?.FindAction("Action");
    }

    // Scenes without an InputHandler still use their PlayerInput bindings.
    // The standalone F action is only used when neither asset contains an interaction action.
    private void RefreshInput()
    {
        InputHandler next = InputHandler.Instance;
        if (next != null && (!next.isActiveAndEnabled || FindAction(next.inputActions) == null)) next = null;
        InputAction action = next != null ? FindAction(next.inputActions) : FindAction(_playerInput != null ? _playerInput.actions : null);
        if (action == null)
        {
            if (_fallbackAction == null) _fallbackAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");
            action = _fallbackAction;
        }
        if (_input == next && ReferenceEquals(InteractionAction, action)) return;
        UnsubscribeInput();
        _input = next;
        InteractionAction = action;
        if (_input != null) _input.OnInteractEvent += TryInteract;
        else
        {
            _directAction = action;
            _directAction.performed += OnDirectInteract;
            _enabledDirectAction = !_directAction.enabled;
            if (_enabledDirectAction) _directAction.Enable();
        }
    }

    private void UnsubscribeInput()
    {
        if (_input != null) _input.OnInteractEvent -= TryInteract;
        if (_directAction != null)
        {
            _directAction.performed -= OnDirectInteract;
            if (_enabledDirectAction) _directAction.Disable();
        }
        _input = null;
        _directAction = null;
        InteractionAction = null;
        _enabledDirectAction = false;
    }

    private void OnDirectInteract(InputAction.CallbackContext context) => TryInteract();

    public void TryInteract()
    {
        if (!CanProcessInput || InteractedThisFrame) return;
        CurrentTarget = FindNearest();
        if (CurrentTarget is MonoBehaviour target && target != null && target.isActiveAndEnabled && CurrentTarget.CanInteract(this))
        {
            _lastInteractionFrame = Time.frameCount;
            CurrentTarget.Interact(this);
        }
        if (this != null && isActiveAndEnabled) RefreshTargetAndPrompt();
    }

    public void RefreshTargetAndPrompt()
    {
        CurrentTarget = CanProcessInput ? FindNearest() : null;
        if (_prompt != null) _prompt.Show(CurrentTarget as MonoBehaviour, InteractionAction);
    }

    public IPlayerInteractable FindNearest()
    {
        _hits.Clear();
        _visited.Clear();
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(interactionLayers);
        // Some engine versions also apply the global query switch to list queries.
        // Include triggers for this query, then restore the project's setting immediately.
        bool queriedTriggers = Physics2D.queriesHitTriggers;
        try
        {
            if (!queriedTriggers) Physics2D.queriesHitTriggers = true;
            Physics2D.OverlapCircle(transform.position, interactionRadius, filter, _hits);
        }
        finally
        {
            if (!queriedTriggers) Physics2D.queriesHitTriggers = false;
        }
        IPlayerInteractable nearest = null;
        float nearestDistance = float.PositiveInfinity;
        int nearestId = int.MaxValue;
        foreach (var hit in _hits)
        {
            if (hit == null) continue;
            foreach (var component in hit.GetComponentsInParent<MonoBehaviour>(false))
            {
                if (component == null || !component.isActiveAndEnabled || !_visited.Add(component) ||
                    !(component is IPlayerInteractable candidate)) continue;
                float distance = ((Vector2)(component.transform.position - transform.position)).sqrMagnitude;
                if (distance > interactionRadius * interactionRadius || !candidate.CanInteract(this)) continue;
                // UnityEngine.Object hashes use the object's identity, including on newer editors.
                int id = component.GetHashCode();
                if (distance < nearestDistance || (distance == nearestDistance && id < nearestId))
                {
                    nearest = candidate;
                    nearestDistance = distance;
                    nearestId = id;
                }
            }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
