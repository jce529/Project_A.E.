using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public float health = 100f;

    public void TakeDamage(float damage){
        health -= damage;
        Debug.Log("적 피격! 남은 체력 : " + health);
        if(health <= 0){
            Die();
        }
    }

    private void Die(){
        Debug.Log("적이 쓰러졌습니다!");
        Destroy(gameObject);
    }
}
