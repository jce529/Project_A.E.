using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float damage = 10f;
    public GameObject attackBox;
    public float attackDistance = 1.5f;
    public float attackDuration = 0.2f;

    public GameObject slashHitboxPrefab;

    public float radius = 2.5f;
    public GameObject waveEffectPrefab;
    public WaterController waterController;
    public PlayerStats playerStats;
    public FlashSlice FlashSlice;
    public WaveSlice waveSlice;


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
        if (Input.GetKeyDown(KeyCode.E))
        {
            FlashSlice.flashSlice();
        }
        //파동참 : 물병 2개 소모
        if (Input.GetKeyDown(KeyCode.R))
        {
            waveSlice.waveSlice();
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
