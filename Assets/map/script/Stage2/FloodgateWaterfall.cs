using System;
using UnityEngine;

/// <summary>수문 조작으로 지정 폭포를 정지(비활성)시킨다. 한 번 정지하면 유지.</summary>
public class FloodgateWaterfall : MonoBehaviour, IPlayerInteractable
{
    [Tooltip("정지시킬 폭포 오브젝트들")]
    [SerializeField] private GameObject[] waterfalls;
    [SerializeField] private GameObject gateClosedVisual;
    [SerializeField] private GameObject gateOpenVisual;

    public bool Stopped { get; private set; }
    public event Action<bool> Changed;

    private void Start()
    {
        Stopped = Gimmick2Store.Get(Gimmick2Store.WaterfallStopped);
        ApplyVisual();
    }

    public bool CanInteract(PlayerInteraction player) => !Stopped;
    public void Interact(PlayerInteraction player) => Stop();

    public void Stop()
    {
        if (Stopped) return;
        Stopped = true;
        ApplyVisual();
        Gimmick2Store.Set(Gimmick2Store.WaterfallStopped, true);
        Changed?.Invoke(true);
    }

    private void ApplyVisual()
    {
        if (waterfalls != null)
            foreach (var w in waterfalls) if (w != null) w.SetActive(!Stopped);
        if (gateClosedVisual != null) gateClosedVisual.SetActive(!Stopped);
        if (gateOpenVisual != null) gateOpenVisual.SetActive(Stopped);
    }
}
