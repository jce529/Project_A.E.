using UnityEngine;

namespace WaterMonster.Phase2
{
    public class WaterPuddle : MonoBehaviour, IPlayerInteractable
    {
        public bool isDestructible = true;
        public bool playerInRange = false;

        [SerializeField] private Color indestructibleColor = new Color(0.3f, 0.3f, 1f, 0.5f);
        
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public bool CanInteract(PlayerInteraction player)
        {
            if (player == null || !isDestructible) return false;
            var water = player.WaterController;
            var absorb = player.GetComponent<PlayerAbsorb>();
            // Respect existing non-interaction configurations and custom absorption radius.
            if (absorb != null && absorb.isActiveAndEnabled && !absorb.UsesInteraction) return false;
            if (absorb != null && absorb.isActiveAndEnabled &&
                ((Vector2)(player.transform.position - transform.position)).sqrMagnitude > absorb.AbsorbRadius * absorb.AbsorbRadius) return false;
            return water != null && water.waterCounter() + water.corruptedwaterCounter() < water.bottles.Count;
        }

        public void Interact(PlayerInteraction player)
        {
            if (!CanInteract(player)) return;
            player.WaterController.RecoveryWater();
            SetIndestructible();
        }

        public void SetIndestructible()
        {
            isDestructible = false;
            if (_sr != null)
                _sr.color = indestructibleColor;
            
            if (PuddleStackManager.Instance != null)
                PuddleStackManager.Instance.RegisterIndestructible(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }

        public void OnReturnToPool()
        {
            isDestructible = true;
            playerInRange = false;
            if (_sr != null)
                _sr.color = Color.white;

            if (PuddleStackManager.Instance != null)
                PuddleStackManager.Instance.UnregisterIndestructible(this);
            gameObject.SetActive(false);
        }
    }
}
