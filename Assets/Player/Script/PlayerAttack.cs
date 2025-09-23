using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float damage = 10f;
    public GameObject attackBox;
    public float attackDistance = 1.5f;
    public float attackDuration = 0.2f;

    public float teleportDistance = 3f;
    public GameObject slashHitboxPrefab;

    public float radius = 2.5f;
    public GameObject waveEffectPrefab;
    public WaterController waterController;
    public PlayerStats playerStats;


    void Update(){
        //기본공격
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - transform.position).normalized;
            Vector2 spawnPos = (Vector2)transform.position + direction*attackDistance;

            GameObject atkBox = Instantiate(attackBox, spawnPos, Quaternion.identity);
            atkBox.transform.right = direction;

            StartCoroutine(DisableAfterTime(atkBox, attackDuration));
        }
        //섬광참 : 물병 1개 소모
        if (Input.GetKeyDown(KeyCode.E) && waterController.waterCounter() + waterController.corruptedwaterCounter() >= 1)
        {
            waterController.UseBottle(1);

            Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 targetPos = (Vector2)transform.position + direction * teleportDistance;


            GameObject slash = Instantiate(slashHitboxPrefab, transform.position, Quaternion.identity);
            slash.transform.right = direction;
            Destroy(slash, 0.1f);

            transform.position = targetPos;
        }
        //파동참 : 물병 2개 소모
        if (Input.GetKeyDown(KeyCode.R) && waterController.waterCounter() + waterController.corruptedwaterCounter() >= 2)
        {
            waterController.UseBottle(2);

            GameObject wave = Instantiate(waveEffectPrefab, transform.position, Quaternion.identity);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits){
                if (hit.CompareTag("HitBox")){
                    IDamageable target = hit.GetComponentInParent<IDamageable>();
                    if (target != null){
                        target.TakeDamage(damage);
                    }
                }
            }
            Destroy(wave, 1.0f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) && waterController.waterCounter() + waterController.corruptedwaterCounter() >= 1)
        {
            waterController.UseBottle(1);
            playerStats.Heal(1);
            
        }

    }
    
    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("HitBox"))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null){
                enemy.TakeDamage(damage);
            }
        }
    }

    IEnumerator DisableAfterTime(GameObject atkBox, float time){
        yield return new WaitForSeconds(time);
        Destroy(atkBox);
    }

}
