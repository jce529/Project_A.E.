using UnityEngine;

namespace WaterMonster.Phase2
{
    public class WeatherController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem rainParticle;
        [SerializeField] private BoxCollider2D mapBounds;
        [SerializeField] private PuddleSpawner _puddleSpawner;

        private void Awake()
        {
            // 게임 시작 시 비를 확실히 멈춤
            StopRain();
        }

        public void StartRain()
        {
            if (rainParticle == null || mapBounds == null || _puddleSpawner == null)
            {
                Debug.LogError("WeatherController: Missing references!");
                return;
            }

            var shape = rainParticle.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(mapBounds.bounds.size.x, 1f, 1f);

            rainParticle.transform.position = mapBounds.bounds.center + Vector3.up * 10f;
            rainParticle.Play();

            _puddleSpawner.StartSpawning();
        }

        public void StopRain()
        {
            if (rainParticle != null)
                rainParticle.Stop();

            if (_puddleSpawner != null)
                _puddleSpawner.StopSpawning();
        }

        private void OnDrawGizmos()
        {
            if (mapBounds != null)
            {
                // 비 내리는 영역 시각화 (반투명 파란색)
                Gizmos.color = new Color(0, 0.5f, 1f, 0.2f);
                Gizmos.DrawCube(mapBounds.bounds.center, mapBounds.bounds.size);

                // 테두리선 시각화 (하늘색)
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(mapBounds.bounds.center, mapBounds.bounds.size);
            }
        }
    }
}
