using UnityEngine;

namespace WoodBoss
{
    public class SeedProjectile : MonoBehaviour
    {
        [Header("Settings")]
        public float Speed = 8.0f;
        public float Damage = 10.0f;
        public float LifeTime = 5.0f; // 5초 뒤 자동 삭제

        private Vector2 _direction;
        private bool _isLaunched = false;

        public void Launch(Vector2 dir)
        {
            _direction = dir.normalized;
            _isLaunched = true;

            // 발사 후 일정 시간 지나면 자동 삭제 (메모리 관리)
            Destroy(gameObject, LifeTime);
        }

        void Update()
        {
            if (!_isLaunched) return;

            // 방향대로 이동
            transform.Translate(_direction * Speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 플레이어 태그 확인 (Player 태그가 맞는지 확인하세요)
            if (collision.CompareTag("Player"))
            {
                Debug.Log("플레이어 피격!");

                // 플레이어 스크립트 찾아서 데미지 주기 (예시)
                // var playerStats = collision.GetComponent<PlayerStats>();
                // if (playerStats != null) playerStats.TakeDamage(Damage);

                // 명중 후 투사체 삭제
                Destroy(gameObject);
            }
            // 땅이나 벽에 닿으면 삭제 (Layer 체크 등)
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Destroy(gameObject);
            }
        }
    }
}