using UnityEngine;

public class InteractableWall : MonoBehaviour, IPlayerInteractable
{
    public GameObject wallTilemap;

    [Header("Save Data Unlock")]
    [Tooltip("Unlock condition key, for example BossProgress.TutorialBoss")]
    [SerializeField] private string unlockDataKey = "";

    public bool CanInteract(PlayerInteraction player)
    {
        if (SaveLoadManager.Instance == null || string.IsNullOrWhiteSpace(unlockDataKey))
            return false;
        return bool.TryParse(SaveLoadManager.Instance.LoadData(unlockDataKey), out bool unlocked) && unlocked;
    }

    public void Interact(PlayerInteraction player) => TryUnlockFromSaveData();

    public bool TryUnlockFromSaveData()
    {
        if (SaveLoadManager.Instance == null || string.IsNullOrWhiteSpace(unlockDataKey))
            return false;

        string rawValue = SaveLoadManager.Instance.LoadData(unlockDataKey);
        bool shouldUnlock;
        if (!bool.TryParse(rawValue, out shouldUnlock) || !shouldUnlock)
            return false;

        UnlockWall();
        return true;
    }

    public void UnlockWall()
    {
        if (wallTilemap != null)
        {
            wallTilemap.SetActive(false);
        }

        gameObject.SetActive(false);
    }

}
