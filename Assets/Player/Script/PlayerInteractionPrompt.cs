using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInteractionPrompt : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    private GameObject canvasObject;
    private TextMeshProUGUI label;
    private InputAction cachedAction;
    private string bindingText;
    private bool bindingDirty = true;

    private void OnEnable() => InputSystem.onActionChange += OnActionChange;
    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
        Hide();
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
            bindingDirty = false;
        }
        Camera camera = Camera.main;
        if (!isActiveAndEnabled || target == null || !target.isActiveAndEnabled
            || camera == null || string.IsNullOrEmpty(bindingText))
        {
            Hide();
            return;
        }
        Vector3 screenPosition = camera.WorldToScreenPoint(target.transform.position + worldOffset);
        if (screenPosition.z <= 0f) { Hide(); return; }
        if (canvasObject == null)
        {
            canvasObject = new GameObject("Interaction Key Prompt", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
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
        canvasObject.SetActive(true);
        label.text = "[" + bindingText + "]";
        label.rectTransform.position = screenPosition;
    }

    public static string GetBindingText(InputAction action)
    {
        return action == null ? string.Empty : action.GetBindingDisplayString();
    }

    public void Hide()
    {
        if (canvasObject != null) canvasObject.SetActive(false);
    }
}
