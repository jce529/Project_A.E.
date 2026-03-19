using UnityEngine;

namespace WoodBoss
{
    public class RootSpike : MonoBehaviour
    {
        [Header("Settings")]
        public float Damage = 10f;
        public float LifeTime = 2.0f; // Destroy automatically after 2 seconds

        private void Start()
        {
            // Auto destroy to prevent memory leaks
            Destroy(gameObject, LifeTime);
        }

        // Damage Logic
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                Debug.Log($">> Root Spike Hit Player! Damage: {Damage}");

                // Deal damage to player script
                // var hp = collision.GetComponent<PlayerHealth>();
                // if(hp != null) hp.TakeDamage(Damage);
            }
        }
    }
}