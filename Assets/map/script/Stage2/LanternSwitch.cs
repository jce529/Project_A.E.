using System;
using UnityEngine;

/// <summary>대형 석등 1개. 상호작용으로 ON/OFF. 값은 LanternPuzzle 이 평가한다.</summary>
public class LanternSwitch : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private GameObject onVisual;
    [SerializeField] private GameObject offVisual;

    public bool IsOn { get; private set; }
    public bool Locked { get; set; }
    public event Action<bool> Toggled;

    public bool CanInteract(PlayerInteraction player) => !Locked;
    public void Interact(PlayerInteraction player) => SetOn(!IsOn);

    public void SetOn(bool on)
    {
        if (IsOn == on) { ApplyVisual(); return; }
        IsOn = on;
        ApplyVisual();
        Toggled?.Invoke(on);
    }

    /// <summary>이벤트 없이 상태만 적용(로드/고정용).</summary>
    public void SetOnSilently(bool on) { IsOn = on; ApplyVisual(); }

    private void Awake() => ApplyVisual();

    private void ApplyVisual()
    {
        if (onVisual != null) onVisual.SetActive(IsOn);
        if (offVisual != null) offVisual.SetActive(!IsOn);
    }
}
