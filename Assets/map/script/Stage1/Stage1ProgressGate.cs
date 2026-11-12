using UnityEngine;

// Phase 24: 진행 플래그에 따라 오브젝트를 켜고 끈다 (다음 구간 문/발판/장벽 등).
// 씬 로드 시 저장된 플래그로 즉시 반영하고, 이후 변경도 따라간다.
public class Stage1ProgressGate : MonoBehaviour
{
    public string key;
    [Tooltip("플래그가 true 이면 켤 오브젝트")]
    public GameObject[] activateWhenSet;
    [Tooltip("플래그가 true 이면 끌 오브젝트")]
    public GameObject[] deactivateWhenSet;

    private void OnEnable()
    {
        Stage1Progress.OnFlagChanged += HandleChanged;
        Apply(Stage1Progress.Get(key));
    }

    private void OnDisable() { Stage1Progress.OnFlagChanged -= HandleChanged; }

    private void HandleChanged(string changedKey, bool value)
    {
        if (changedKey == key) Apply(value);
    }

    private void Apply(bool set)
    {
        foreach (var go in activateWhenSet) if (go != null) go.SetActive(set);
        foreach (var go in deactivateWhenSet) if (go != null) go.SetActive(!set);
    }
}
