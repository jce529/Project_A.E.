using System;
using UnityEngine;

/// <summary>레버로 물길 A/B 전환. IsChanged=false → A 활성, true → B 활성.</summary>
public class WaterwayLever : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private GameObject waterwayA;
    [SerializeField] private GameObject waterwayB;
    [SerializeField] private GameObject leverDefaultVisual;
    [SerializeField] private GameObject leverChangedVisual;

    public bool IsChanged { get; private set; }
    public event Action<bool> Changed;

    private void Start()
    {
        IsChanged = Gimmick2Store.Get(Gimmick2Store.WaterwayChanged);
        ApplyVisual();
    }

    public bool CanInteract(PlayerInteraction player) => true;
    public void Interact(PlayerInteraction player) => Set(!IsChanged);

    public void Set(bool changed)
    {
        if (IsChanged == changed) return;
        IsChanged = changed;
        ApplyVisual();
        Gimmick2Store.Set(Gimmick2Store.WaterwayChanged, changed);
        Changed?.Invoke(changed);
    }

    private void ApplyVisual()
    {
        if (waterwayA != null) waterwayA.SetActive(!IsChanged);
        if (waterwayB != null) waterwayB.SetActive(IsChanged);
        if (leverDefaultVisual != null) leverDefaultVisual.SetActive(!IsChanged);
        if (leverChangedVisual != null) leverChangedVisual.SetActive(IsChanged);
    }
}
