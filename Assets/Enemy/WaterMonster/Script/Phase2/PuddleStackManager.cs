using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterMonster.Phase2
{
    public class PuddleStackManager : MonoBehaviour
    {
        public static PuddleStackManager Instance { get; private set; }

        private int explosionThreshold = 5;
        public int ExplosionThreshold { set => explosionThreshold = value; }
        private int _indestructibleCount = 0;
        private List<WaterPuddle> _indestructiblePuddles = new List<WaterPuddle>();

        public event Action OnThresholdReached;

        public int IndestructibleCount => _indestructibleCount;
        public IReadOnlyList<WaterPuddle> IndestructiblePuddles => _indestructiblePuddles;

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
            _indestructiblePuddles.Add(puddle);
            if (_indestructibleCount >= explosionThreshold)
            {
                OnThresholdReached?.Invoke();
            }
        }

        public void UnregisterIndestructible(WaterPuddle puddle)
        {
            if (_indestructiblePuddles.Remove(puddle))
                _indestructibleCount = Mathf.Max(0, _indestructibleCount - 1);
        }

        public void ReturnAllIndestructibleToPool()
        {
            var toReturn = new List<WaterPuddle>(_indestructiblePuddles);
            foreach (var puddle in toReturn)
            {
                PuddlePool.Instance.Return(puddle);
            }

            _indestructibleCount = 0;
            _indestructiblePuddles.Clear();
        }

        public void ForceReset()
        {
            _indestructibleCount = 0;
            _indestructiblePuddles.Clear();
        }
    }
}
