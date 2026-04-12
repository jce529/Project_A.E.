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

        private List<WaterPuddle> _pool = new List<WaterPuddle>();

        public int ActiveCount => _pool.Count(p => p.gameObject.activeSelf);

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
            return puddle;
        }

        public void Return(WaterPuddle puddle)
        {
            puddle.OnReturnToPool();
        }
    }
}
