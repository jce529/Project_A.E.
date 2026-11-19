using UnityEngine;

// Put this on a GameObject with a BoxCollider2D (Is Trigger = on) that covers any zone that
// should zoom the camera in/out - a boss arena, a wide vista, a tight corridor, etc. (D-01).
// Renamed from BossZoomTrigger: the zoom target now lives per trigger (targetZoom below) instead
// of a single scene-wide CameraController.bossZoom, so the same component covers any zoom zone,
// not just boss arenas.
// The trigger volumes themselves are placed manually per zone by the designer (D-02) and are
// intentionally NOT created by this script.
public class CameraZoomTrigger : MonoBehaviour
{
    [Header("Zone Zoom")]
    // Orthographic size the camera eases toward while the player is inside this zone.
    // Transition speed still lives on CameraController.zoomSmoothing - shared by every zone.
    public float targetZoom = 7f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetZoomZone(true, targetZoom);
    }

    // Leaving the zone reverts to the normal stage zoom immediately, no boss-death event needed (D-03).
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraController.Instance.SetZoomZone(false, targetZoom);
    }

    // Editor-only zone visualization, no runtime cost in a build. Draws the trigger's own
    // BoxCollider2D volume so the zone is visible without selecting it, mirroring
    // CameraBoundsTrigger's gizmo but in a different color to tell zoom zones apart from
    // bounds zones at a glance when both sit on the same boss arena.
    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Vector3 center = transform.TransformPoint(box.offset);
        Vector3 size = new Vector3(box.size.x * transform.lossyScale.x, box.size.y * transform.lossyScale.y, 0f);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.12f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}
