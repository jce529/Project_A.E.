using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInteractionPrompt : MonoBehaviour
{
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [Min(0.001f)] public float worldScale = 0.02f;
    private Canvas _canvas;
    private TMP_Text _text;
    private MonoBehaviour _target;
    private InputAction _action;
    private bool bindingDirty = true;
    private string bindingLabel;

    private void OnEnable() => InputSystem.onActionChange += OnActionChange;
    private void OnActionChange(object source, InputActionChange change)
    {
        if (change == InputActionChange.BoundControlsChanged) bindingDirty = true;
    }

    public static string GetBindingText(InputAction action) =>
        action != null ? action.GetBindingDisplayString() : string.Empty;

    public void Show(MonoBehaviour target, InputAction action)
    {
        _target = target;
        if (_action != action || bindingDirty)
        {
            string key = action != null ? action.GetBindingDisplayString() : "F";
            bindingLabel = string.IsNullOrEmpty(key) ? "[?]" : "[" + key + "]";
            _action = action;
            bindingDirty = false;
        }
        if (_target == null || !_target.isActiveAndEnabled)
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
            return;
        }
        EnsureCanvas();
        _canvas.gameObject.SetActive(true);
        if (_text.text != bindingLabel) _text.text = bindingLabel;
        FollowTarget();
    }

    private void EnsureCanvas()
    {
        if (_canvas != null) return;
        var root = new GameObject("Interaction Prompt", typeof(RectTransform), typeof(Canvas));
        // Owned by the player: destroying/pooling a target must not destroy the shared prompt.
        root.transform.SetParent(transform, false);
        _canvas = root.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.sortingOrder = 1000;
        ((RectTransform)root.transform).sizeDelta = new Vector2(160f, 40f);
        var label = new GameObject("Key", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(root.transform, false);
        var rect = (RectTransform)label.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        _text = label.GetComponent<TextMeshProUGUI>();
        _text.fontSize = 24f;
        _text.alignment = TextAlignmentOptions.Center;
        _text.color = Color.white;
        _text.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (_target == null || !_target.isActiveAndEnabled)
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
            return;
        }
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (_canvas == null || _target == null) return;
        var prompt = _canvas.transform;
        prompt.position = _target.transform.position + worldOffset;
        var camera = Camera.main;
        prompt.rotation = camera != null ? camera.transform.rotation : Quaternion.identity;
        // Counter player flip/scale so the text remains readable.
        Vector3 scale = transform.lossyScale;
        prompt.localScale = new Vector3(SafeScale(scale.x), SafeScale(scale.y), SafeScale(scale.z));
    }

    private float SafeScale(float value) => Mathf.Abs(value) < 0.0001f ? worldScale : worldScale / value;

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
        bindingDirty = true;
        _target = null;
        if (_canvas != null) _canvas.gameObject.SetActive(false);
    }
}
