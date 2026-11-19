using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour, IPlayerInteractable
{
    public int startingNodeIndex = 0;
    private DialogueManager dialogueManager;

    private void Start() => dialogueManager = FindAnyObjectByType<DialogueManager>();

    public bool CanInteract(PlayerInteraction player)
    {
        if (dialogueManager == null) dialogueManager = FindAnyObjectByType<DialogueManager>();
        return player != null && dialogueManager != null && dialogueManager.isActiveAndEnabled &&
            !dialogueManager.IsDialogueActive;
    }

    public void Interact(PlayerInteraction player)
    {
        if (CanInteract(player)) dialogueManager.StartDialogue(startingNodeIndex);
    }
}
