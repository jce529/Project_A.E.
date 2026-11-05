using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Phase 21 (D-10): the inventory panel is built entirely at runtime - no scene or prefab
// authoring, same family as PlayerInteractionPrompt. Inventory.Awake attaches this component,
// so it lives and dies with the Player in every scene.
[DisallowMultipleComponent]
public class InventoryUI : MonoBehaviour
{
    private const int Columns = 5;
    private const float CellSize = 96f;
    private const float CellSpacing = 8f;
    private const float ToastDuration = 2f;
    private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.8f);
    internal static readonly Color SlotColor = new Color(1f, 1f, 1f, 0.12f);
    internal static readonly Color SelectedColor = new Color(1f, 0.85f, 0.3f, 0.55f);

    private Inventory inventory;
    private GameObject canvasObject;
    private GameObject panelRoot;
    private TextMeshProUGUI detailLabel;
    private InventorySlotView[] slotViews;
    private int selectedIndex = -1;
    private bool isOpen;
    private bool dirty = true;
    private bool pendingClose;
    private TextMeshProUGUI toastLabel;
    private float toastRemaining;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        inventory.OnChanged += MarkDirty;
        BuildUI();
        WorldItem.PickupResolved += HandlePickupResolved;
    }

    private void Start()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnInventoryEvent += HandleToggle;
            InputHandler.Instance.OnPauseEvent += HandlePause;
        }
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnGameStateChange += HandleStateChange;
    }

    private void OnDestroy()
    {
        WorldItem.PickupResolved -= HandlePickupResolved;
        if (inventory != null) inventory.OnChanged -= MarkDirty;
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnInventoryEvent -= HandleToggle;
            InputHandler.Instance.OnPauseEvent -= HandlePause;
        }
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnGameStateChange -= HandleStateChange;
    }

    // D-01: the same key opens and closes. D-05: opening is only allowed from Playing.
    private void HandleToggle()
    {
        if (isOpen) Close();
        else if (GameStateManager.Instance != null
                 && GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
            Open();
    }

    // D-05: Esc only raises a flag. PauseMenu.OnPauseInput may run after this handler for the
    // same key press; closing synchronously would restore Playing first and the pause menu would
    // open as well. LateUpdate closes after every input callback of the frame has run.
    private void HandlePause()
    {
        if (isOpen) pendingClose = true;
    }

    // Scene loads or death can move the state away from Inventory while the panel is open:
    // hide the panel only, never push the state back.
    private void HandleStateChange(GameStateManager.GameState state)
    {
        if (isOpen && state != GameStateManager.GameState.Inventory) HidePanel();
    }

    // D-12: success and full-inventory messages. Only WorldItem raises this (D-13).
    private void HandlePickupResolved(ItemData item, int count, bool added)
    {
        ShowToast(added ? $"{DisplayNameOf(item)} x{count} 획득" : "인벤토리가 가득 찼습니다");
    }

    // D-14: replace instead of queue - the newest message overwrites the old one and restarts
    // the timer, so toasts never stack up over the screen.
    private void ShowToast(string message)
    {
        toastLabel.text = message;
        toastLabel.gameObject.SetActive(true);
        toastRemaining = ToastDuration;
    }

    private void Update()
    {
        if (toastRemaining <= 0f) return;
        // D-14: unscaled, so the countdown keeps running while timeScale is 0.
        toastRemaining -= Time.unscaledDeltaTime;
        if (toastRemaining <= 0f) toastLabel.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (pendingClose)
        {
            pendingClose = false;
            if (isOpen) Close();
        }
        if (isOpen && dirty) Refresh();
    }

    // D-04: GameStateManager already sets timeScale 0 and InGameHUDController hides the HUD.
    private void Open()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.Inventory);
        isOpen = true;
        panelRoot.SetActive(true);
        Refresh();
    }

    private void Close()
    {
        HidePanel();
        if (GameStateManager.Instance != null
            && GameStateManager.Instance.CurrentState == GameStateManager.GameState.Inventory)
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
    }

    private void HidePanel()
    {
        isOpen = false;
        pendingClose = false;
        panelRoot.SetActive(false);
    }

    // D-11: no polling. Changes only mark dirty; the panel redraws while visible. Save restore
    // raises OnChanged once per entry, which costs nothing while the panel is closed.
    private void MarkDirty() => dirty = true;

    // D-08: a left click selects a non-empty slot. Empty slots do nothing.
    internal void Select(int index)
    {
        var slot = inventory.GetSlot(index);
        if (slot == null || slot.IsEmpty) return;
        selectedIndex = index;
        dirty = true;
    }

    // D-08: a right click on the selected slot uses it; on any other slot it only selects.
    internal void Use(int index)
    {
        if (index != selectedIndex)
        {
            Select(index);
            return;
        }
        var slot = inventory.GetSlot(index);
        if (slot == null || slot.IsEmpty) return;
        // D-09: Progression items (keys) are never consumed from the UI. The guard lives here, not
        // in Inventory.UseItem, so the Phase 18 contract and its ContextMenu hooks stay unchanged.
        if (slot.item.Type != ItemData.ItemType.Consumable) return;
        inventory.UseItem(index);
        dirty = true;
    }

    internal static string DisplayNameOf(ItemData item) =>
        string.IsNullOrEmpty(item.DisplayName) ? item.Id : item.DisplayName;

    private void Refresh()
    {
        dirty = false;
        var selected = inventory.GetSlot(selectedIndex);
        if (selected == null || selected.IsEmpty)
        {
            selectedIndex = -1;
            selected = null;
        }
        for (int i = 0; i < slotViews.Length; i++)
            slotViews[i].Show(inventory.GetSlot(i), i == selectedIndex);
        detailLabel.text = selected == null
            ? string.Empty
            : $"{DisplayNameOf(selected.item)} x{selected.count}";
    }

    private void BuildUI()
    {
        canvasObject = new GameObject("Inventory UI Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        // Same scaler settings as the scenes' "UI" canvas (1920x1080, match width).
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        int rows = Inventory.SlotCount / Columns;
        float gridWidth = Columns * CellSize + (Columns - 1) * CellSpacing;
        float gridHeight = rows * CellSize + (rows - 1) * CellSpacing;

        panelRoot = CreateChild("Inventory Panel", canvasObject.transform);
        ((RectTransform)panelRoot.transform).sizeDelta = new Vector2(gridWidth + 48f, gridHeight + 112f);
        // raycastTarget stays true so clicks on the panel background do not fall through.
        panelRoot.AddComponent<Image>().color = PanelColor;

        var grid = CreateChild("Slots", panelRoot.transform);
        var gridRect = (RectTransform)grid.transform;
        gridRect.anchorMin = gridRect.anchorMax = new Vector2(0.5f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.anchoredPosition = new Vector2(0f, -24f);
        gridRect.sizeDelta = new Vector2(gridWidth, gridHeight);
        var layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(CellSize, CellSize);
        layout.spacing = new Vector2(CellSpacing, CellSpacing);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = Columns;

        slotViews = new InventorySlotView[Inventory.SlotCount];
        for (int i = 0; i < slotViews.Length; i++)
        {
            slotViews[i] = CreateChild("Slot " + i, grid.transform).AddComponent<InventorySlotView>();
            slotViews[i].Build(this, i);
        }

        detailLabel = CreateText("Selected", panelRoot.transform, 26f, TextAlignmentOptions.Center);
        var detailRect = detailLabel.rectTransform;
        detailRect.anchorMin = detailRect.anchorMax = new Vector2(0.5f, 0f);
        detailRect.pivot = new Vector2(0.5f, 0f);
        detailRect.anchoredPosition = new Vector2(0f, 24f);
        detailRect.sizeDelta = new Vector2(gridWidth, 48f);

        // The toast sits directly on the canvas, not under panelRoot, so it shows with the panel closed.
        toastLabel = CreateText("Pickup Toast", canvasObject.transform, 32f, TextAlignmentOptions.Center);
        var toastRect = toastLabel.rectTransform;
        toastRect.anchorMin = toastRect.anchorMax = new Vector2(0.5f, 1f);
        toastRect.pivot = new Vector2(0.5f, 1f);
        toastRect.anchoredPosition = new Vector2(0f, -140f);
        toastRect.sizeDelta = new Vector2(900f, 60f);
        toastLabel.outlineWidth = 0.2f;
        toastLabel.gameObject.SetActive(false);

        panelRoot.SetActive(false);
    }

    internal static GameObject CreateChild(string name, Transform parent)
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        return child;
    }

    internal static TextMeshProUGUI CreateText(string name, Transform parent, float size,
        TextAlignmentOptions alignment)
    {
        var text = CreateChild(name, parent).AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    internal static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }
}

// One grid cell. IPointerClickHandler is used instead of Button because Button ignores right clicks.
public class InventorySlotView : MonoBehaviour, IPointerClickHandler
{
    private InventoryUI owner;
    private int index;
    private Image background;
    private Image icon;
    private TextMeshProUGUI nameLabel;
    private TextMeshProUGUI countLabel;

    internal void Build(InventoryUI owner, int index)
    {
        this.owner = owner;
        this.index = index;
        background = gameObject.AddComponent<Image>();   // raycast target: receives the clicks
        icon = InventoryUI.CreateChild("Icon", transform).AddComponent<Image>();
        InventoryUI.Stretch(icon.rectTransform, 10f);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        nameLabel = InventoryUI.CreateText("Name", transform, 18f, TextAlignmentOptions.Center);
        InventoryUI.Stretch(nameLabel.rectTransform, 4f);
        countLabel = InventoryUI.CreateText("Count", transform, 22f, TextAlignmentOptions.BottomRight);
        InventoryUI.Stretch(countLabel.rectTransform, 6f);
    }

    internal void Show(InventorySlot slot, bool selected)
    {
        background.color = selected ? InventoryUI.SelectedColor : InventoryUI.SlotColor;
        bool empty = slot == null || slot.IsEmpty;
        Sprite sprite = empty ? null : slot.item.Icon;
        icon.sprite = sprite;
        icon.enabled = sprite != null;
        // D-06: no icon -> fall back to the display name text.
        nameLabel.text = empty || sprite != null ? string.Empty : InventoryUI.DisplayNameOf(slot.item);
        countLabel.text = empty ? string.Empty : slot.count.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) owner.Select(index);
        else if (eventData.button == PointerEventData.InputButton.Right) owner.Use(index);
    }
}
