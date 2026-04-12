using System;
using UnityEngine;

namespace WaterMonster.Phase2
{
    public class PuddleStackManager : MonoBehaviour
    {
        public static PuddleStackManager Instance { get; private set; }

        [SerializeField] private int explosionThreshold = 5;
        private int _indestructibleCount = 0;

        public event Action OnThresholdReached;

        public int IndestructibleCount => _indestructibleCount;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void RegisterIndestructible(WaterPuddle puddle)
        {
            _indestructibleCount++;
            if (_indestructibleCount >= explosionThreshold)
            {
                OnThresholdReached?.Invoke();
            }
        }

        public void UnregisterIndestructible(WaterPuddle puddle)
        {
            if (!puddle.isDestructible)
            {
                _indestructibleCount = Mathf.Max(0, _indestructibleCount - 1);
            }
        }
    }
}
