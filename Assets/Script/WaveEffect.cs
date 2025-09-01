using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEffect : MonoBehaviour
{
    public float duration = 1.5f;
    public float maxScale = 3.5f;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = Vector3.one * 0.1f; 
        transform.localScale = initialScale;
        StartCoroutine(ExpandAndDestroy());
    }

    IEnumerator ExpandAndDestroy()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one * maxScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
