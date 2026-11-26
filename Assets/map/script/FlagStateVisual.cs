using System;
using UnityEngine;

// Phase 28: maps gimmick flags (SaveLoadManager.GetGimmick) to one of five art states and applies
// sprite / Animator trigger / object toggles. Generic: no gimmick class is touched; the flags
// are polled, so it works for s1.*, s2.a.*, s2.b.* alike and also re-applies after save/load
// (scene reload -> OnEnable). Leave a state entry empty if its art does not exist yet.
public class FlagStateVisual : MonoBehaviour
{
    public enum VisualState { Default, Active, Disabled, Changed, Interacted }

    [Serializable]
    public class StateArt
    {
        public VisualState state;
        public Sprite sprite;                 // applied to spriteRenderer when not null
        public string animatorTrigger = "";   // SetTrigger when non-empty
        public GameObject[] showOnly;         // shown in this state, hidden in the others
    }

    [Header("Flag keys (empty = unused). Priority: Disabled > Changed > Interacted > Active > Default")]
    [SerializeField] private string disabledKey = "";
    [SerializeField] private string changedKey = "";
    [SerializeField] private string interactedKey = "";
    [SerializeField] private string activeKey = "";

    [Header("Art per state (fill when the artist's assets arrive)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private StateArt[] arts;

    [SerializeField] private float pollInterval = 0.25f;

    public VisualState Current { get; private set; }
    public event Action<VisualState> StateChanged;

    private bool _applied;
    private float _next;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void OnEnable() { _applied = false; Refresh(); }

    private void Update()
    {
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + pollInterval;
        Refresh();
    }

    private static bool Flag(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        var s = SaveLoadManager.Instance;
        return s != null && s.GetGimmick(key, false);
    }

    private VisualState Evaluate()
    {
        if (Flag(disabledKey)) return VisualState.Disabled;
        if (Flag(changedKey)) return VisualState.Changed;
        if (Flag(interactedKey)) return VisualState.Interacted;
        if (Flag(activeKey)) return VisualState.Active;
        return VisualState.Default;
    }

    public void Refresh()
    {
        var next = Evaluate();
        if (_applied && next == Current) return;
        bool first = !_applied;
        _applied = true;
        Current = next;
        Apply(next);
        if (!first) StateChanged?.Invoke(next);
    }

    private void Apply(VisualState state)
    {
        if (arts == null) return;
        // pass 1: hide objects of every other state; pass 2: show this state's objects
        foreach (var a in arts)
            if (a != null && a.state != state && a.showOnly != null)
                foreach (var go in a.showOnly) if (go != null) go.SetActive(false);
        foreach (var a in arts)
        {
            if (a == null || a.state != state) continue;
            if (a.sprite != null && spriteRenderer != null) spriteRenderer.sprite = a.sprite;
            if (!string.IsNullOrEmpty(a.animatorTrigger) && animator != null) animator.SetTrigger(a.animatorTrigger);
            if (a.showOnly != null) foreach (var go in a.showOnly) if (go != null) go.SetActive(true);
        }
    }
}
