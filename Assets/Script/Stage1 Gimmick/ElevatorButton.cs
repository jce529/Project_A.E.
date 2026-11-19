using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ElevatorButton : MonoBehaviour, IPlayerInteractable
{
    public UnityEvent onButtonPressed;
    public bool isOneTimeUse = true;
    private bool hasBeenPressed;

    public bool CanInteract(PlayerInteraction player) => player != null && (!isOneTimeUse || !hasBeenPressed);

    public void Interact(PlayerInteraction player)
    {
        if (CanInteract(player)) PressButton();
    }

    private void PressButton()
    {
        hasBeenPressed = true;
        onButtonPressed?.Invoke();
    }
}
