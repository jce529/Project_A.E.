// Phase 25 - fog object. Hidden at start if the removal flag is already saved (permanent state),
// and hidden when Stage2Events.FogRemoved fires.
using UnityEngine;

public class Stage21FogController : MonoBehaviour
{
    [Tooltip("Object to hide. Defaults to this GameObject.")]
    public GameObject fog;

    void Awake() { if (fog == null) fog = gameObject; Stage21Progress.EnsureSubscribed(); }
    void OnEnable() { Stage2Events.FogRemoved += Hide; Apply(); }
    void OnDisable() { Stage2Events.FogRemoved -= Hide; }
    void Start() { Apply(); } // SaveLoadManager may initialise after OnEnable

    void Apply() { if (Stage21Progress.IsFogRemoved()) Hide(); }
    void Hide() { if (fog != null) fog.SetActive(false); }
}
