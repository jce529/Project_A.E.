using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Shared manual-save modal, created on demand so every gameplay scene can use it.
public class SaveSlotDialog : MonoBehaviour
{
    private static SaveSlotDialog instance;
    private Action save;
    private Action onSaved;
    private GameStateManager.GameState previousState;
    private TMP_Text heading;
    private readonly Button[] slots = new Button[SaveLoadManager.SlotCount];
    private Button confirm;
    private Button back;
    private int pendingSlot = -1;
    private GameObject previousSelection;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private bool closed;

    public static bool IsOpen => instance != null && !instance.closed;

    public static void Open(Action saveAction, Action savedAction = null)
    {
        if (IsOpen || SaveLoadManager.Instance == null || GameStateManager.Instance == null) return;
        var go = new GameObject("Save Slot Dialog", typeof(RectTransform));
        instance = go.AddComponent<SaveSlotDialog>();
        instance.save = saveAction;
        instance.onSaved = savedAction;
        instance.previousState = GameStateManager.Instance.CurrentState;
        instance.previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        instance.previousCursorLock = Cursor.lockState;
        instance.previousCursorVisible = Cursor.visible;
        GameStateManager.Instance.SetState(GameStateManager.GameState.Paused);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        instance.Build();
    }

    public static bool HandlePause()
    {
        if (!IsOpen) return false;
        instance.Back();
        return true;
    }

    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();
        if (EventSystem.current == null)
        {
            var events = new GameObject("Save Dialog EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
        }
        var shade = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        shade.transform.SetParent(transform, false);
        var rect = (RectTransform)shade.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        shade.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.05f, 0.96f);
        heading = Label(transform, "저장할 슬롯 선택", new Vector2(0, 310), new Vector2(1000, 100));
        for (int i = 0; i < slots.Length; i++)
        {
            int slot = i;
            var manager = SaveLoadManager.Instance;
            var data = manager.PeekSlotData(slot);
            string detail = !manager.HasSaveFile(slot) ? "빈 슬롯" : data == null
                ? "읽을 수 없는 저장 데이터" : data.SceneName + " · 격파 보스 " + (data.BossProgress != null ? data.BossProgress.Count : 0);
            string current = manager.CurrentSlot == slot ? " (현재)" : "";
            slots[i] = MakeButton("슬롯 " + (slot + 1) + current + "\n" + detail,
                new Vector2(0, 175 - i * 135), () => Choose(slot));
        }
        confirm = MakeButton("덮어쓰기", new Vector2(0, -245), Write);
        confirm.gameObject.SetActive(false);
        back = MakeButton("취소 / 뒤로", new Vector2(0, -380), Back);
        SetNavigation(false);
        Focus(slots[SaveLoadManager.Instance.CurrentSlot]);
    }

    private void Choose(int slot)
    {
        pendingSlot = slot;
        if (!SaveLoadManager.Instance.HasSaveFile(slot)) { Write(); return; }
        heading.text = "슬롯 " + (slot + 1) + "의 데이터를 덮어쓰시겠습니까?";
        foreach (var button in slots) button.interactable = false;
        confirm.gameObject.SetActive(true);
        SetNavigation(true);
        Focus(confirm);
    }

    private void Write()
    {
        var manager = SaveLoadManager.Instance;
        if (closed || manager == null || pendingSlot < 0) return;
        int oldSlot = manager.CurrentSlot;
        try
        {
            manager.SelectSlot(pendingSlot);
            save();
        }
        catch (Exception exception)
        {
            manager.SelectSlot(oldSlot);
            heading.text = "저장에 실패했습니다. 다시 시도해주세요.";
            Debug.LogException(exception);
            foreach (var button in slots) button.interactable = true;
            confirm.gameObject.SetActive(false);
            pendingSlot = -1;
            SetNavigation(false);
            Focus(slots[oldSlot]);
            return;
        }
        var callback = onSaved;
        Close();
        callback?.Invoke();
    }

    private void Back()
    {
        if (confirm.gameObject.activeSelf)
        {
            confirm.gameObject.SetActive(false);
            foreach (var button in slots) button.interactable = true;
            heading.text = "저장할 슬롯 선택";
            Focus(slots[pendingSlot]);
            pendingSlot = -1;
            SetNavigation(false);
            return;
        }
        Close();
    }

    private void Close()
    {
        if (closed) return;
        closed = true;
        GameStateManager.Instance?.SetState(previousState);
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(previousSelection);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private static void Focus(Button button)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private void SetNavigation(bool confirming)
    {
        Button[] buttons = confirming ? new[] { confirm, back }
            : new[] { slots[0], slots[1], slots[2], back };
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[(i + buttons.Length - 1) % buttons.Length],
                selectOnDown = buttons[(i + 1) % buttons.Length]
            };
        }
    }

    private Button MakeButton(string text, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(transform, false);
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(900, 110);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(0.18f, 0.25f, 0.33f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        button.onClick.AddListener(action);
        Label(go.transform, text, Vector2.zero, rect.sizeDelta - new Vector2(30, 10));
        return button;
    }

    private static TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 size)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 30;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        label.rectTransform.anchoredPosition = position;
        label.rectTransform.sizeDelta = size;
        return label;
    }
}
