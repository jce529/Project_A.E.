// Phase 16 - AutoSaveNotice (D-10)
// One purpose only: after an interval autosave, hold a short label in the corner for
// HoldSeconds and then fade it out over FadeSeconds, which lands inside D-10's "1 to 2
// seconds". Without it the player has no way of knowing progress was preserved.
//
// Deliberately NOT a general toast system: generalising this to item pickups, quests and the
// like is explicitly out of scope for this phase. It also deliberately does not reuse
// HealPopup / HealPopupSpawner - those are world space combat popups anchored to a character.
//
// The canvas is built in code and parented under the DontDestroyOnLoad "AutoSave" object, so
// no scene and no prefab has to be edited and the label survives every scene load. This
// mirrors SaveSlotDialog.Build(), the project's existing runtime-built UI.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoSaveNotice : MonoBehaviour
{
    public const string Message = "자동 저장됨";
    public const float HoldSeconds = 1.2f;
    public const float FadeSeconds = 0.6f;

    private TMP_Text _label;
    private Coroutine _routine;

    // Called by AutoSaveTimer right after a successful write. Manual saves do not use this:
    // they already have their own feedback in GameSettingsPanel / SaveSlotDialog.
    public void Show()
    {
        if (_label == null) Build();
        if (_routine != null) StopCoroutine(_routine);
        _label.text = Message;
        _routine = StartCoroutine(ShowRoutine());
    }

    private void Build()
    {
        GameObject canvasObject = new GameObject("AutoSave Notice Canvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Above the gameplay HUD, below SaveSlotDialog (30000) so a save dialog still wins.
        canvas.sortingOrder = 20000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // No GraphicRaycaster and no EventSystem on purpose: this label is never clicked, and a
        // raycaster would let it swallow clicks meant for whatever sits underneath.

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(canvasObject.transform, false);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        // No font asset is assigned: AddComponent picks up the TMP Settings default font, which
        // is the only option for a script that no inspector can wire up yet.
        label.fontSize = 32;
        label.alignment = TextAlignmentOptions.BottomRight;
        label.color = Color.white;
        label.outlineWidth = 0.2f;
        label.raycastTarget = false;

        // Bottom right corner, 48 px in from both edges. Top left and top centre are taken by
        // the player health UI and the boss health bar.
        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(420f, 60f);
        rect.anchoredPosition = new Vector2(-48f, 48f);

        _label = label;
    }

    // Unscaled time: if the player opens the pause menu right after a save, Time.timeScale
    // goes to 0 and a scaled fade would freeze the label on screen.
    private IEnumerator ShowRoutine()
    {
        _label.alpha = 1f;

        float held = 0f;
        while (held < HoldSeconds)
        {
            held += Time.unscaledDeltaTime;
            yield return null;
        }

        float faded = 0f;
        while (faded < FadeSeconds)
        {
            faded += Time.unscaledDeltaTime;
            _label.alpha = 1f - Mathf.Clamp01(faded / FadeSeconds);
            yield return null;
        }

        _label.alpha = 0f;
        _routine = null;
    }
}
