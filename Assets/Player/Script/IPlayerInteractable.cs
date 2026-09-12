public interface IPlayerInteractable
{
    bool CanInteract(PlayerInteraction player);
    void Interact(PlayerInteraction player);
}
