using UnityEngine;

public class FlashSlice : MonoBehaviour
{
    PlayerAttack playerAttack;
    public float teleportDistance = 3f;

    public WaterController waterController;
    public PlayerStats playerStats;
    public GameObject slashHitboxPrefab;

    //flashSlice 호출 스킬 

    public void flashSlice()
    {
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() >= 1)
        {
            waterController.UseBottle(1);

            Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 targetPos = (Vector2)transform.position + direction * teleportDistance;


            GameObject slash = Instantiate(slashHitboxPrefab, transform.position, Quaternion.identity);
            slash.transform.right = direction;
            Destroy(slash, 0.1f);

            transform.position = targetPos;
        }
        else
        {
        }
    }
}
