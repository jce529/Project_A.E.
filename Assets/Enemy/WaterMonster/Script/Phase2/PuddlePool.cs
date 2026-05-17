using System;
using System.Collections.Generic;
using System.Linq;
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

        private List<WaterPuddle> _pool = new List<WaterPuddle>();

        public int ActiveCount => _pool.Count(p => p.gameObject.activeSelf);
        public event Action OnTotalThresholdReached;

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

            for (int i = 0; i < initialSize; i++)
            {
                CreateNew();
            }
        }

        private WaterPuddle CreateNew()
        {
            GameObject obj = Instantiate(puddlePrefab, transform);
            WaterPuddle puddle = obj.GetComponent<WaterPuddle>();
            obj.SetActive(false);
            _pool.Add(puddle);
            return puddle;
        }

        public WaterPuddle Get(Vector2 position)
        {
            WaterPuddle puddle = _pool.FirstOrDefault(p => !p.gameObject.activeSelf);
            if (puddle == null)
            {
                puddle = CreateNew();
            }

            puddle.transform.position = position;
            puddle.gameObject.SetActive(true);

            if (ActiveCount >= totalExplosionThreshold)
                OnTotalThresholdReached?.Invoke();

            return puddle;
        }

        public List<WaterPuddle> GetAllActive()
        {
            return _pool.Where(p => p != null && p.gameObject.activeSelf).ToList();
        }

        public void Return(WaterPuddle puddle)
        {
            puddle.OnReturnToPool();
        }

        public void ReturnAll()
        {
            foreach (var puddle in _pool)
            {
                if (puddle != null && puddle.gameObject.activeSelf)
                    puddle.OnReturnToPool();
            }
            // OnReturnToPool sets isDestructible=true before calling UnregisterIndestructible,
            // so the unregister check fails — force-reset the stack manager here.
            if (PuddleStackManager.Instance != null)
                PuddleStackManager.Instance.ForceReset();
        }
    }
}
