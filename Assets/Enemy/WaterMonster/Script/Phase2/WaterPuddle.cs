using UnityEngine;

namespace WaterMonster.Phase2
{
    public class WaterPuddle : MonoBehaviour
    {
        public bool isDestructible = true;
        public bool playerInRange = false;

        [SerializeField] private Color indestructibleColor = new Color(0.3f, 0.3f, 1f, 0.5f);
        
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void SetIndestructible()
        {
            isDestructible = false;
            if (_sr != null)
                _sr.color = indestructibleColor;
            
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

            PuddleStackManager.Instance.UnregisterIndestructible(this);
            gameObject.SetActive(false);
        }
    }
}
