using UnityEngine;

public class InteractableWall : MonoBehaviour
{
    public GameObject wallTilemap;

    [Header("Save Data Unlock")]
    [Tooltip("Unlock condition key, for example BossProgress.TutorialBoss")]
    [SerializeField] private string unlockDataKey = "";

    private bool isPlayerInRange;
    private bool isSubscribedToInteract;

    private void Start()
    {
        SubscribeToInteract();
    }

    private void OnEnable()
    {
        SubscribeToInteract();
    }

    private void OnDisable()
    {
        UnsubscribeFromInteract();
        isPlayerInRange = false;
    }

    private void SubscribeToInteract()
    {
        if (isSubscribedToInteract || InputHandler.Instance == null) return;

        InputHandler.Instance.OnInteractEvent += HandleInteractInput;
        isSubscribedToInteract = true;
    }

    private void UnsubscribeFromInteract()
    {
        if (!isSubscribedToInteract) return;

        if (InputHandler.Instance != null)
            InputHandler.Instance.OnInteractEvent -= HandleInteractInput;

        isSubscribedToInteract = false;
    }

    private void HandleInteractInput()
    {
        if (isPlayerInRange)
            TryUnlockFromSaveData();
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) isPlayerInRange = false;
    }
}
