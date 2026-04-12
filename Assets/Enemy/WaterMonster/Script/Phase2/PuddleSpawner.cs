using System.Collections;
using UnityEngine;

namespace WaterMonster.Phase2
{
    public class PuddleSpawner : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D spawnBounds;
        [SerializeField] private float spawnInterval = 3f;
        [SerializeField] private int maxPuddles = 10;

        private Coroutine _spawnCoroutine;

        public void StartSpawning()
        {
            StopSpawning();
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        public void StopSpawning()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (PuddlePool.Instance.ActiveCount < maxPuddles)
                {
                    PuddlePool.Instance.Get(GetRandomPosition());
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private Vector2 GetRandomPosition()
        {
            Bounds bounds = spawnBounds.bounds;
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            return new Vector2(x, y);
        }
    }
}
