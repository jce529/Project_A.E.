using UnityEngine;
using System;

namespace WoodBoss
{
    public class WoodBossStatsSystem : MonoBehaviour
    {
        [Header("Settings")]
        public float MaxHealth = 1000f;

        public float CurrentHealth { get; private set; }

        public event Action OnDeath;
        public event Action OnDamageTaken;


        void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth -= damage;
            OnDamageTaken?.Invoke();

            Debug.Log($"보스체력 : {CurrentHealth}/{MaxHealth}");

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        private void Die() 
        {
            Debug.Log("보스 사망");
            OnDeath?.Invoke();
        }
    }
}