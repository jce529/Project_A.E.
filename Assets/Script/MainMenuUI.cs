using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Color loadGameDisabledColor = new Color(0.05f, 0.05f, 0.05f, 1f);
    [SerializeField] private SlotSelectPanel slotSelectPanel;
    [SerializeField] private string newGameSceneName = "Tutorial Map";

    private void Start()
    {
        if (loadGameButton != null)
        {
            SaveLoadManager mgr = SaveLoadManager.Instance;
            bool hasSave = false;
            if (mgr != null)
            {
                for (int slot = 0; slot < SaveLoadManager.SlotCount; slot++)
                {
                    if (mgr.HasSaveFile(slot))
                    {
                        hasSave = true;
                        break;
                    }
                }
            }

            loadGameButton.interactable = hasSave;
            var loadGameText = loadGameButton.GetComponentInChildren<TMP_Text>();
            if (loadGameText != null && !hasSave) loadGameText.color = loadGameDisabledColor;
        }

        if (slotSelectPanel != null) slotSelectPanel.gameObject.SetActive(false);
    }

    public void OnClickStart()
    {
        SaveLoadManager mgr = SaveLoadManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[MainMenuUI] OnClickStart: SaveLoadManager.Instance is null.");
            return;
        }

        for (int slot = 0; slot < SaveLoadManager.SlotCount; slot++)
        {
            if (!mgr.HasSaveFile(slot))
            {
                mgr.NewGameInSlot(slot);
                SceneManager.LoadScene(newGameSceneName);
                return;
            }
        }

        if (slotSelectPanel == null)
        {
            Debug.LogError("[MainMenuUI] OnClickStart: every slot is occupied but slotSelectPanel is not assigned.");
            return;
        }
        slotSelectPanel.OpenForNewGame();
    }

    public void OnClickLoad()
    {
        if (slotSelectPanel == null)
        {
            Debug.LogError("[MainMenuUI] OnClickLoad: slotSelectPanel is not assigned.");
            return;
        }
        slotSelectPanel.OpenForLoad();
    }
}
