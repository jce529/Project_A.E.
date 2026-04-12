using UnityEngine;

namespace WaterMonster.Phase2
{
    public class WeatherController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem rainParticle;
        [SerializeField] private BoxCollider2D mapBounds;
        [SerializeField] private PuddleSpawner _puddleSpawner;

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
    }
}
