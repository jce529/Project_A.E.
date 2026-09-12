using UnityEngine;

namespace WaterMonster.Phase2
{
    public class WaterPuddle : MonoBehaviour, IPlayerInteractable
    {
        public bool isDestructible = true;

        [SerializeField] private Color indestructibleColor = new Color(0.3f, 0.3f, 1f, 0.5f);
        
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void SetIndestructible()
        {
            if (!isDestructible) return;
            isDestructible = false;
            if (_sr != null)
                _sr.color = indestructibleColor;
            
            if (PuddleStackManager.Instance != null)
                PuddleStackManager.Instance.RegisterIndestructible(this);
        }

        public bool CanInteract(PlayerInteraction player)
        {
            var absorb = player != null ? player.GetComponent<PlayerAbsorb>() : null;
            return absorb != null && absorb.CanAbsorb(this, true);
        }

        public void Interact(PlayerInteraction player)
        {
            var absorb = player != null ? player.GetComponent<PlayerAbsorb>() : null;
            if (absorb != null) absorb.Absorb(this, true);
        }

        public void OnReturnToPool()
        {
            isDestructible = true;
            if (_sr != null)
                _sr.color = Color.white;

            if (PuddleStackManager.Instance != null)
                PuddleStackManager.Instance.UnregisterIndestructible(this);
            gameObject.SetActive(false);
        }
    }
}
