using UnityEngine;

public class WaveSlice : MonoBehaviour
{
    public float damage = 15f;
    public float radius = 2.5f;
    public GameObject waveEffectPrefab;
    public WaterController waterController;
    public PlayerStats playerStats;

    public void waveSlice()
    {
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() >= 2)
        {
            waterController.UseBottle(2);

            GameObject wave = Instantiate(waveEffectPrefab, transform.position, Quaternion.identity);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("HitBox"))
                {
                    IDamageable target = hit.GetComponentInParent<IDamageable>();
                    if (target != null)
                    {
                        target.TakeDamage(damage);
                    }
                }

            }
            Destroy(wave, 1.0f);
        }
        else {Debug.Log("물이 부족합니다"); }
    }
}
