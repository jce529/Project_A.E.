using UnityEngine;

// Put this on a GameObject with a BoxCollider2D (Is Trigger = on) that covers one room / zone
// (260805-m41). Entering the zone hands CameraController the X limits that room should clamp to,
// so a single scene can frame room by room instead of sharing one pair of scene-wide bounds.
// By default the collider's own world bounds ARE the zone (min.x / max.x) - tiling rooms with
// triggers needs no numbers typed in, and there is nothing left to fall out of sync with the box.
// useCustomBounds opts a zone out of that for the one case where the trigger volume must stay
// smaller than the area the camera should reveal, e.g. a narrow doorway that opens onto a much
// wider hidden room (easter egg areas).
//
// A zone is a SCOPED OVERRIDE, not a permanent handoff. On exit the camera is always sent back to
// CameraController's own minX / maxX - the fixed stage base bounds - instead of to whatever was
// active right before entering (Q2-06, supersedes MX-05). Nothing is remembered per trigger, so
// overlapping or out-of-order zones can no longer hand back a logically stale pair, and a player
// who SPAWNS inside a zone and walks out simply lands on the base bounds with no guard needed.
//
// The swap is instant: CameraController writes this pair straight into its live clamp bounds,
// so crossing a zone edge snaps the wall immediately instead of sliding it (supersedes Q2-02's
// boundsSmoothing Lerp, per user request - the slide made the zone edge feel soft).
//
// Because a zone only ever hands off to another zone or falls back to the base bounds, rooms are
// meant to be TILED - put a trigger on every stretch that has its own walls. Level design guide:
// Assets/Camera/Check.md, section "quick task 260805-q2u".
//
// The trigger volumes themselves are placed manually per zone by the designer, exactly like
// CameraZoomTrigger: this script never creates or edits scene objects.
public class CameraBoundsTrigger : MonoBehaviour
{
    [Header("Zone X Bounds")]
    // Off by default: the BoxCollider2D's own world bounds are used, so a zone left untouched
    // just works and can never disagree with where the box is actually placed.
    public bool useCustomBounds = false;
    // Only read when useCustomBounds is true. Ignored otherwise (the collider bounds win).
    // Keep the zone at least as wide as the camera view, or the clamp collapses onto a single X.
    public float zoneMinX = -1000f;
    public float zoneMaxX = 1000f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        float min, max, minY, maxY;
        GetEffectiveBounds(out min, out max, out minY, out maxY);
        CameraController.Instance.SetXBounds(min, max);
        CameraController.Instance.SetYBounds(minY, maxY);
    }

    // Leaving the zone falls back to the controller's fixed base bounds (Q2-06) - UNLESS the
    // player is still standing inside another CameraBoundsTrigger zone right now. Tiling two
    // zones edge-to-edge (the documented setup) means this Exit and the neighbour's Enter can
    // both fire in the same physics step, and Unity does not guarantee which callback runs
    // first. Without this guard, an Exit that happens to run after the neighbour's Enter would
    // stomp the neighbour's just-applied bounds back to the wide fallback (user-reported: walking
    // straight from one tiled zone into the next left the camera unbounded). Collider2D.IsTouching
    // reads the physics engine's current overlap state directly, so it is correct regardless of
    // callback order.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var allZones = FindObjectsOfType<CameraBoundsTrigger>();
        foreach (var zone in allZones)
        {
            if (zone == this) continue;
            var zoneCollider = zone.GetComponent<Collider2D>();
            if (zoneCollider != null && zoneCollider.IsTouching(other)) return;
        }
        CameraController.Instance.SetXBounds(CameraController.Instance.minX, CameraController.Instance.maxX);
        CameraController.Instance.SetYBounds(CameraController.Instance.minY, CameraController.Instance.maxY);
    }

    // Single source of truth for "what range does this zone apply". X honors useCustomBounds
    // (the doorway -> wide-room pattern described above); Y always comes straight from the
    // collider - there is no custom-Y equivalent, nothing has asked for one yet.
    private void GetEffectiveBounds(out float min, out float max, out float minY, out float maxY)
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        minY = b.min.y;
        maxY = b.max.y;
        if (useCustomBounds)
        {
            min = zoneMinX;
            max = zoneMaxX;
            return;
        }
        min = b.min.x;
        max = b.max.x;
    }

    // Editor-only zone visualization, no runtime cost in a build. Always draws the trigger's own
    // BoxCollider2D volume. The zoneMinX / zoneMaxX lines are only drawn when useCustomBounds is
    // on - in the default case the cyan box already IS the applied X range, so overlapping green
    // lines on top of its own edges would be pure clutter. Green is used deliberately to read
    // apart from CameraController's own red minX/maxX fallback lines when both are visible.
    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Vector3 center = transform.TransformPoint(box.offset);
        Vector3 size = new Vector3(box.size.x * transform.lossyScale.x, box.size.y * transform.lossyScale.y, 0f);
        Gizmos.color = new Color(0f, 1f, 1f, 0.12f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);

        if (!useCustomBounds) return;
        Gizmos.color = Color.green;
        const float lineHeight = 20f;
        float halfLine = lineHeight / 2f;
        Gizmos.DrawLine(new Vector3(zoneMinX, transform.position.y - halfLine, 0f), new Vector3(zoneMinX, transform.position.y + halfLine, 0f));
        Gizmos.DrawLine(new Vector3(zoneMaxX, transform.position.y - halfLine, 0f), new Vector3(zoneMaxX, transform.position.y + halfLine, 0f));
    }
}
