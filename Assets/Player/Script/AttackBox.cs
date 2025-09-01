using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public float damage = 10f;
    public string targetTag = "HitBox";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            // 부모에서 IDamageable 찾기 (플레이어나 적 본체)
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target != null){
                target.TakeDamage(damage);
                Debug.Log("공격 성공! 데미지: " + damage);
            }
        }
    }
}
