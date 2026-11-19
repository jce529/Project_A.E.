using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterMonster.Phase2
{
    public class PuddlePool : MonoBehaviour
    {
        public static PuddlePool Instance { get; private set; }
        [SerializeField] private GameObject puddlePrefab;
        [SerializeField] private int initialSize = 15;
        private int totalExplosionThreshold = 8;
        public int TotalExplosionThreshold { set => totalExplosionThreshold = value; }
        private readonly Stack<WaterPuddle> _available = new Stack<WaterPuddle>();
        private readonly HashSet<WaterPuddle> _active = new HashSet<WaterPuddle>();
        public int ActiveCount => _active.Count;
        public event Action OnTotalThresholdReached;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (puddlePrefab == null) { Debug.LogError("PuddlePool requires a puddle prefab.", this); return; }
            for (int i = 0; i < initialSize; i++) _available.Push(CreateNew());
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private WaterPuddle CreateNew()
        {
            var obj = Instantiate(puddlePrefab, transform);
            var puddle = obj.GetComponent<WaterPuddle>();
            obj.SetActive(false);
            return puddle;
        }

        public WaterPuddle Get(Vector2 position)
        {
            WaterPuddle puddle = null;
            while (_available.Count > 0 && puddle == null) puddle = _available.Pop();
            if (puddle == null)
            {
                if (puddlePrefab == null) return null;
                puddle = CreateNew();
            }
            if (puddle == null) return null;
            _active.Add(puddle);
            puddle.transform.position = position;
            puddle.gameObject.SetActive(true);
            if (_active.Count >= totalExplosionThreshold) OnTotalThresholdReached?.Invoke();
            return puddle;
        }

        public List<WaterPuddle> GetAllActive() => new List<WaterPuddle>(_active);

        public void Return(WaterPuddle puddle)
        {
            if (puddle == null || !_active.Remove(puddle)) return;
            puddle.OnReturnToPool();
            _available.Push(puddle);
        }

        public void ReturnAll()
        {
            foreach (var puddle in _active)
            {
                if (puddle == null) continue;
                puddle.OnReturnToPool();
                _available.Push(puddle);
            }
            _active.Clear();
            if (PuddleStackManager.Instance != null) PuddleStackManager.Instance.ForceReset();
        }
    }
}
