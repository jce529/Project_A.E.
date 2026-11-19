// Phase 1 — HealPopup
// TMP floating text that fades upward over 1 second, then destroys itself.
using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class HealPopup : MonoBehaviour
{
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private float riseDistance = 0.8f;

    private TMP_Text _text;

    public void Initialize(float amount)
    {
        _text = GetComponent<TMP_Text>();
        _text.text = $"+{Mathf.RoundToInt(amount)}";
        _text.color = new Color(0.4f, 0.9f, 1.0f, 1.0f); // light cyan
        StartCoroutine(FadeAndRise());
    }

    private IEnumerator FadeAndRise()
    {
        float t = 0f;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * riseDistance;
        Color startColor = _text.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(start, end, u);
            _text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - u);
            yield return null;
        }
        Destroy(gameObject);
    }
}
