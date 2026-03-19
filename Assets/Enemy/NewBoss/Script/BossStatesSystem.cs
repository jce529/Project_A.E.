using UnityEngine;
using System;

    public class BossStatsSystem : MonoBehaviour
    {
        [Header("Settings")]
        public float MaxHealth = 1000f;
        public float MaxWater = 100f;
        public float WaterDecayRate = 1.0f; // 초당 자연 소모

        private float _currentHealth;
        private float _currentWater;

        // [에러 해결 부분] 외부에서 값을 읽을 수 있게 '프로퍼티' 추가
        public float CurrentHealth => _currentHealth;
        public float CurrentWater => _currentWater;

        public bool IsBarrierActive => _currentWater > 0;

        // 이벤트: 물이 다 떨어지면 그로기(딜타임) 발생
        public event Action OnWaterDepleted;
        public event Action OnDamageTaken;

        void Start()
        {
            _currentHealth = MaxHealth;
            _currentWater = MaxWater;
        }

        void Update()
        {
            // 베리어가 켜져있을 때만 자연 소모
            if (IsBarrierActive)
            {
                ConsumeWater(WaterDecayRate * Time.deltaTime);
            }
        }

        public void ConsumeWater(float amount)
        {
            if (_currentWater <= 0) return;

            _currentWater -= amount;
            if (_currentWater <= 0)
            {
                _currentWater = 0;
                OnWaterDepleted?.Invoke(); // 물 고갈 이벤트 발생
            }
            // UI 업데이트 로직 호출
        }

        public void RestoreWater()
        {
            _currentWater = MaxWater;
            // 컷씬이나 그로기 종료 후 호출
        }

        public void TakeDamage(float damage)
        {
            if (IsBarrierActive)
            {
                // 베리어 상태: 체력 피해는 줄이고, 물을 대폭 깎음 (기획: 20%)
                ConsumeWater(MaxWater * 0.2f);
                // 반격 로직 트리거를 위해 컨트롤러에 알림
                OnDamageTaken?.Invoke();
            }
            else
            {
                // 그로기 상태: 체력 직접 피해
                _currentHealth -= damage;
            }

            if (_currentHealth <= 0) Die();
        }

        private void Die() { /* 사망 처리 */ }
    }
