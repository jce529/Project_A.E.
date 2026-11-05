using UnityEngine;

namespace WaterMonster.Phase2
{
    public class WaterPuddle : MonoBehaviour, IPlayerInteractable
    {
        public bool isDestructible = true;
        public float SpawnTime { get; private set; }
        public float Age => Time.time - SpawnTime;

        [SerializeField] private Color indestructibleColor = new Color(0.3f, 0.3f, 1f, 0.5f);
        [SerializeField] private bool _removableByPlayerAttack = true;
        
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            var body = GetComponent<Rigidbody2D>();
            if (body == null) body = gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
        }

        private void OnEnable() => SpawnTime = Time.time;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_removableByPlayerAttack || !isDestructible
                || other.GetComponentInParent<PlayerAttackDamager>() == null) return;
            if (PuddlePool.Instance != null) PuddlePool.Instance.Return(this);
            else OnReturnToPool();
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
