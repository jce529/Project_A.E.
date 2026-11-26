using System.Collections.Generic;
using UnityEngine;

namespace WaterMonster.Phase2
{
    public class WaterMonsterPuddleSystem : MonoBehaviour
    {
        public static WaterMonsterPuddleSystem Instance { get; private set; }

        [SerializeField] private int _initialSpawnCount = 3;
        [SerializeField] private int _spawnPerPattern = 2;
        [SerializeField] private int _maxPuddles = 6;
        [SerializeField] private float _fuseTime = 10f;
        [SerializeField] private float _edgeMargin = 1f;

        private float _groundY;
        public int SpawnPerPattern => _spawnPerPattern;
        public float FuseTime => _fuseTime;
        public WaterMonsterController BoundBoss { get; private set; }
        public PuddleExplosionController Explosion { get; private set; }
        public bool IsActive { get; private set; }
        public bool CanSpawnMore => IsActive && PuddlePool.Instance != null
            && PuddlePool.Instance.ActiveCount < _maxPuddles;

        public static WaterMonsterPuddleSystem Ensure(WaterMonsterController boss)
        {
            if (Instance != null)
            {
                Instance.Bind(boss);
                return Instance;
            }

            var root = new GameObject("WaterMonsterPuddleSystem");
            if (PuddleStackManager.Instance == null) root.AddComponent<PuddleStackManager>();
            if (PuddlePool.Instance == null) root.AddComponent<PuddlePool>();
            root.AddComponent<PuddleExplosionController>();
            var system = root.AddComponent<WaterMonsterPuddleSystem>();
            system.Bind(boss);
            return system;
        }

        private void Awake()
        {
            Instance = this;
            Explosion = GetComponent<PuddleExplosionController>();
        }

        public void Bind(WaterMonsterController boss)
        {
            if (BoundBoss == boss || boss == null) return;
            if (BoundBoss != null) Unbind(BoundBoss);
            BoundBoss = boss;
            _groundY = boss.transform.position.y;
            boss.PhaseEntered += OnPhaseEntered;
            IsActive = false;
        }

        public void Unbind(WaterMonsterController boss)
        {
            if (BoundBoss != boss) return;
            boss.PhaseEntered -= OnPhaseEntered;
            BoundBoss = null;
            IsActive = false;
            Explosion?.CancelAll();
            if (PuddlePool.Instance != null) PuddlePool.Instance.ReturnAll();
        }

        private void OnDestroy()
        {
            if (BoundBoss != null) BoundBoss.PhaseEntered -= OnPhaseEntered;
            if (Instance == this) Instance = null;
        }

        private void OnPhaseEntered(int phase)
        {
            if (BoundBoss == null) return;
            if (phase >= 2 && !IsActive)
            {
                IsActive = true;
                EnsurePlayerAbsorb();
                WaterMonsterPatterns.TrySetTrigger(BoundBoss.Anim, WaterMonsterPatterns.PuddleSpawn);
                SpawnPuddles(_initialSpawnCount);
            }
        }

        private void EnsurePlayerAbsorb()
        {
            if (BoundBoss == null || BoundBoss.Target == null) return;
            if (BoundBoss.Target.GetComponent<PlayerAbsorb>() == null)
                BoundBoss.Target.gameObject.AddComponent<PlayerAbsorb>();
        }

        public int SpawnPuddles(int count)
        {
            if (BoundBoss == null || PuddlePool.Instance == null) return 0;
            float minX = BoundBoss.transform.position.x - 8f;
            float maxX = BoundBoss.transform.position.x + 8f;
            if (BoundBoss.BattleAreaBounds != null)
            {
                var bounds = BoundBoss.BattleAreaBounds.bounds;
                minX = bounds.min.x + _edgeMargin;
                maxX = bounds.max.x - _edgeMargin;
            }

            int spawned = 0;
            while (spawned < count && PuddlePool.Instance.ActiveCount < _maxPuddles)
            {
                float x = Random.Range(minX, maxX);
                PuddlePool.Instance.Get(new Vector2(x, _groundY));
                spawned++;
            }
            return spawned;
        }

        public List<WaterPuddle> GetAgedPuddles()
        {
            var aged = new List<WaterPuddle>();
            if (PuddlePool.Instance == null) return aged;
            foreach (var puddle in PuddlePool.Instance.GetAllActive())
                if (puddle.Age >= _fuseTime) aged.Add(puddle);
            return aged;
        }

        public bool TrySelectMove(Vector2 bossPos, Vector2 playerPos,
            out WaterPuddle entry, out WaterPuddle exit)
        {
            entry = null;
            exit = null;
            if (BoundBoss == null || PuddlePool.Instance == null) return false;
            var puddles = PuddlePool.Instance.GetAllActive();
            if (puddles.Count < 2) return false;

            float entryDistance = float.PositiveInfinity;
            foreach (var puddle in puddles)
            {
                float distance = ((Vector2)puddle.transform.position - bossPos).sqrMagnitude;
                if (distance < entryDistance)
                {
                    entryDistance = distance;
                    entry = puddle;
                }
            }

            bool close = (bossPos - playerPos).sqrMagnitude <= 9f;
            float exitDistance = close ? float.NegativeInfinity : float.PositiveInfinity;
            foreach (var puddle in puddles)
            {
                if (puddle == entry) continue;
                float distance = ((Vector2)puddle.transform.position - playerPos).sqrMagnitude;
                if ((close && distance > exitDistance) || (!close && distance < exitDistance))
                {
                    exitDistance = distance;
                    exit = puddle;
                }
            }
            return exit != null;
        }
    }
}
