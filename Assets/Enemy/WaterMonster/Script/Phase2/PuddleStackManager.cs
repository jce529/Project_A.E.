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
        private readonly HashSet<WaterPuddle> _registered = new HashSet<WaterPuddle>();
        private List<WaterPuddle> _indestructiblePuddles = new List<WaterPuddle>();

        public event Action OnThresholdReached;

        public int IndestructibleCount => _indestructiblePuddles.Count;
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

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterIndestructible(WaterPuddle puddle)
        {
            if (puddle == null || !_registered.Add(puddle)) return;
            _indestructiblePuddles.Add(puddle);
            if (IndestructibleCount >= explosionThreshold)
            {
                OnThresholdReached?.Invoke();
            }
        }

        public void UnregisterIndestructible(WaterPuddle puddle)
        {
            if (_registered.Remove(puddle)) _indestructiblePuddles.Remove(puddle);
        }

        public void ReturnAllIndestructibleToPool()
        {
            var toReturn = new List<WaterPuddle>(_indestructiblePuddles);
            foreach (var puddle in toReturn)
            {
                PuddlePool.Instance.Return(puddle);
            }

            _registered.Clear();
            _indestructiblePuddles.Clear();
        }

        public void ForceReset()
        {
            _registered.Clear();
            _indestructiblePuddles.Clear();
        }
    }
}
