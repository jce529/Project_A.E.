using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SlotSelectPanel : MonoBehaviour
{
    public enum Intent { Load, NewGame }

    public const string TitleCopy = "슬롯 선택";
    public const string EmptyBodyCopy = "빈 슬롯";
    public const string CorruptHeadCopy = "세이브 데이터를 읽을 수 없습니다";
    public const string CorruptBodyCopy = "파일이 손상되었을 수 있습니다. 이 슬롯에 새 게임을 시작해주세요.";
    public const string NewGameCtaCopy = "새 게임 시작";
    public const string ContinueCtaCopy = "이어하기";
    public const string BossProgressLabel = "격파 보스 ";
    public const int TotalBossCount = 3;

    [Header("Panel")]
    public TMP_Text titleText;
    [Header("Slot cards - array index IS the slot index (size 3)")]
    public Button[] slotButtons = new Button[3];
    public TMP_Text[] slotLabelTexts = new TMP_Text[3];
    public TMP_Text[] slotBodyTexts = new TMP_Text[3];
    public TMP_Text[] slotCtaTexts = new TMP_Text[3];
    [Header("Overwrite confirm (D-04)")]
    public OverwriteConfirmPanel overwriteConfirmPanel;
    [Header("New game target scene")]
    [SerializeField] private string newGameSceneName = "Tutorial Map";

    private Intent _intent = Intent.Load;

    public void OpenForLoad()
    {
        _intent = Intent.Load;
        gameObject.SetActive(true);
        RefreshAllCards();
    }

    public void OpenForNewGame()
    {
        _intent = Intent.NewGame;
        gameObject.SetActive(true);
        RefreshAllCards();
    }

    public void OnClickBack() { gameObject.SetActive(false); }

    private void OnEnable() { RefreshAllCards(); }

    private void RefreshAllCards()
    {
        if (titleText != null) titleText.text = TitleCopy;
        for (int slot = 0; slot < SaveLoadManager.SlotCount; slot++) RefreshCard(slot);
    }

    private void RefreshCard(int slot)
    {
        SaveLoadManager mgr = SaveLoadManager.Instance;
        bool fileExists = mgr != null && mgr.HasSaveFile(slot);
        SaveData data = fileExists ? mgr.PeekSlotData(slot) : null;
        SetText(slotLabelTexts, slot, "슬롯 " + (slot + 1));

        string body;
        string cta;
        if (!fileExists)
        {
            body = EmptyBodyCopy;
            cta = NewGameCtaCopy;
        }
        else if (data == null)
        {
            body = CorruptHeadCopy + "\n" + CorruptBodyCopy;
            cta = NewGameCtaCopy;
        }
        else
        {
            int bossCount = data.BossProgress != null ? data.BossProgress.Count : 0;
            body = data.SceneName + "\n" + BossProgressLabel + bossCount + "/" + TotalBossCount;
            cta = _intent == Intent.Load ? ContinueCtaCopy : NewGameCtaCopy;
        }

        SetText(slotBodyTexts, slot, body);
        SetText(slotCtaTexts, slot, cta);
        bool interactable = _intent == Intent.NewGame || data != null;
        if (slot < slotButtons.Length && slotButtons[slot] != null) slotButtons[slot].interactable = interactable;
    }

    private static void SetText(TMP_Text[] arr, int index, string value)
    {
        if (arr != null && index < arr.Length && arr[index] != null) arr[index].text = value;
    }

    public void OnClickSlot(int slot)
    {
        SaveLoadManager mgr = SaveLoadManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[SlotSelectPanel] SaveLoadManager.Instance is null.");
            return;
        }
        if (_intent == Intent.Load)
        {
            if (!mgr.HasSaveFile(slot)) return;
            gameObject.SetActive(false);
            mgr.LoadSlot(slot);
            return;
        }
        if (mgr.HasSaveFile(slot))
        {
            if (overwriteConfirmPanel == null)
            {
                Debug.LogError("[SlotSelectPanel] overwriteConfirmPanel is not assigned - refusing to overwrite slot " + slot + ".");
                return;
            }
            overwriteConfirmPanel.Open(slot, StartNewGameInSlot);
            return;
        }
        StartNewGameInSlot(slot);
    }

    private void StartNewGameInSlot(int slot)
    {
        SaveLoadManager mgr = SaveLoadManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[SlotSelectPanel] SaveLoadManager.Instance is null.");
            return;
        }
        gameObject.SetActive(false);
        mgr.NewGameInSlot(slot);
        SceneManager.LoadScene(newGameSceneName);
    }
}
