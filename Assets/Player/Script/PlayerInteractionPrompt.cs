using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInteractionPrompt : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField, Min(0.001f)] private float worldScale = 0.02f;
    private GameObject canvasObject;
    private TextMeshProUGUI label;
    private InputAction cachedAction;
    private string bindingText;
    private string formattedText;
    private bool bindingDirty = true;

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }
    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
        Hide();
    }
    private void OnDestroy()
    {
        // The canvas may belong to a target rather than the player hierarchy.
        if (canvasObject != null) Destroy(canvasObject);
    }
    private void OnActionChange(object changed, InputActionChange change)
    {
        if (change == InputActionChange.BoundControlsChanged) bindingDirty = true;
    }

    public void Show(MonoBehaviour target, InputHandler input)
    {
        var action = input != null && input.inputActions != null
            ? input.inputActions.FindAction("Player/Interact") : null;
        if (action != cachedAction || bindingDirty)
        {
            cachedAction = action;
            bindingText = GetBindingText(action);
            formattedText = "[" + bindingText + "]";
            bindingDirty = false;
        }
        if (!isActiveAndEnabled || target == null || !target.isActiveAndEnabled
            || string.IsNullOrEmpty(bindingText))
        {
            Hide();
            return;
        }
        if (canvasObject == null)
        {
            canvasObject = new GameObject("Interaction Key Prompt", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            ((RectTransform)canvas.transform).sizeDelta = new Vector2(240f, 60f);
            var textObject = new GameObject("Key", typeof(RectTransform));
            textObject.transform.SetParent(canvasObject.transform, false);
            label = textObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 28;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.outlineWidth = 0.2f;
            label.raycastTarget = false;
            label.rectTransform.sizeDelta = new Vector2(240f, 60f);
        }
        // World-space geometry follows the target and camera naturally, including zoom.
        Transform canvasTransform = canvasObject.transform;
        if (canvasTransform.parent != target.transform)
            canvasTransform.SetParent(target.transform, false);
        canvasTransform.localPosition = worldOffset;
        canvasTransform.localRotation = Quaternion.identity;
        canvasTransform.localScale = Vector3.one * Mathf.Max(0.001f, worldScale);
        canvasObject.SetActive(true);
        if (label.text != formattedText) label.text = formattedText;
    }

    public static string GetBindingText(InputAction action)
    {
        return action == null ? string.Empty : action.GetBindingDisplayString();
    }

    public void Hide()
    {
        if (canvasObject == null) return;
        canvasObject.SetActive(false);
        // Keep the hidden canvas available when the previous target is removed or pooled.
        canvasObject.transform.SetParent(transform, false);
    }
}
