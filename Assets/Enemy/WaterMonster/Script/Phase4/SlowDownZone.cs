using UnityEngine;

namespace WaterMonster.Phase4
{
    public class SlowDownZone : MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 0.5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // REQ-WM-X-01: Player 레이어에만 적용
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null) pc.currentSpeedModifier = speedMultiplier;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null) pc.currentSpeedModifier = 0f;
        }
    }
}
