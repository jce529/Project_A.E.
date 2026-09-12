using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField, Min(0f)] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    private readonly List<Collider2D> hits = new List<Collider2D>(32);
    private readonly List<MonoBehaviour> components = new List<MonoBehaviour>(8);
    private readonly HashSet<int> visited = new HashSet<int>();
    private InputHandler subscribedInput;
    private PlayerInteractionPrompt prompt;
    public MonoBehaviour CurrentTarget { get; private set; }

    private void Awake()
    {
        prompt = GetComponent<PlayerInteractionPrompt>();
        if (prompt == null) prompt = gameObject.AddComponent<PlayerInteractionPrompt>();
    }
    private void LateUpdate()
    {
        Subscribe();
        RefreshTargetAndPrompt();
    }
    public void RefreshTargetAndPrompt()
    {
        CurrentTarget = isActiveAndEnabled ? FindNearest() : null;
        if (prompt != null) prompt.Show(CurrentTarget, subscribedInput);
    }

    private void OnEnable() => Subscribe();
    private void Start() => Subscribe();
    private void Subscribe()
    {
        if (subscribedInput == InputHandler.Instance) return;
        OnDisable();
        subscribedInput = InputHandler.Instance;
        if (subscribedInput != null) subscribedInput.OnInteractEvent += HandleInteract;
    }
    private void OnDisable()
    {
        if (subscribedInput != null) subscribedInput.OnInteractEvent -= HandleInteract;
        subscribedInput = null;
        CurrentTarget = null;
        if (prompt != null) prompt.Hide();
    }
    private void HandleInteract() => TryInteract();

    public bool TryInteract()
    {
        if (!isActiveAndEnabled) return false;
        var nearest = FindNearest();
        if (nearest == null || !nearest.isActiveAndEnabled) return false;
        var selected = (IPlayerInteractable)nearest;
        if (!selected.CanInteract(this)) return false;
        selected.Interact(this);
        if (this != null && isActiveAndEnabled) RefreshTargetAndPrompt();
        return true;
    }

    private MonoBehaviour FindNearest()
    {
        Vector2 origin = transform.position;
        float radius = Mathf.Max(0f, interactionRadius);
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(interactionLayers);
        Physics2D.OverlapCircle(origin, radius, filter, hits);
        visited.Clear();
        MonoBehaviour nearest = null;
        float nearestDistance = float.PositiveInfinity;
        int nearestId = int.MaxValue;
        foreach (var hit in hits)
        {
            for (Transform current = hit.transform; current != null; current = current.parent)
            {
                current.GetComponents(components);
                foreach (var component in components)
                {
                    if (component == null || !(component is IPlayerInteractable candidate)) continue;
                    int id = component.GetInstanceID();
                    if (!visited.Add(id) || !component.isActiveAndEnabled) continue;
                    float distance = ((Vector2)component.transform.position - origin).sqrMagnitude;
                    if (distance > radius * radius || !candidate.CanInteract(this)) continue;
                    if (distance < nearestDistance || (distance == nearestDistance && id < nearestId))
                    {
                        nearest = component;
                        nearestDistance = distance;
                        nearestId = id;
                    }
                }
            }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0f, interactionRadius));
    }
}
