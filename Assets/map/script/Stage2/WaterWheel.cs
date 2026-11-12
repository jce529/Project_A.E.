using System;
using UnityEngine;

/// <summary>물레방아. 상호작용(또는 외부 Set 호출)으로 가동/정지. 가동 중 회전하며 연결된 RisingLift 를 올린다.</summary>
public class WaterWheel : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private Transform wheelVisual;
    [SerializeField] private float degreesPerSecond = 90f;
    [Tooltip("가동 시 올라갈 상승 장치(선택)")]
    [SerializeField] private RisingLift lift;

    public bool Running { get; private set; }
    public event Action<bool> Changed;

    private void Start()
    {
        Running = Gimmick2Store.Get(Gimmick2Store.WaterwheelRunning);
        if (lift != null) lift.SetRaised(Running, false);
    }

    private void Update()
    {
        if (Running && wheelVisual != null)
            wheelVisual.Rotate(0f, 0f, -degreesPerSecond * Time.deltaTime);
    }

    public bool CanInteract(PlayerInteraction player) => true;
    public void Interact(PlayerInteraction player) => Set(!Running);

    public void Set(bool running)
    {
        if (Running == running) return;
        Running = running;
        Gimmick2Store.Set(Gimmick2Store.WaterwheelRunning, running);
        if (lift != null) lift.SetRaised(running, true);
        Changed?.Invoke(running);
    }
}
