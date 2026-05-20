using UnityEngine;

namespace WoodBoss
{
    public class RootSpike : MonoBehaviour
    {
        [Header("Settings")]
        public float LifeTime = 2.0f;

        private void Start()
        {
            Destroy(gameObject, LifeTime);
        }
    }
}