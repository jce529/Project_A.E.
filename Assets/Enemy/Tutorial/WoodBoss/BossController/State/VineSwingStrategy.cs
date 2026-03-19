using System.Collections;
using UnityEngine;

namespace WoodBoss
{
    public class VineSwingStrategy : IAttackStrategy
    {
        public float Cooldown => 4.0f;
        public string AnimationName => "Attack_VineSwing";

        // 전조 시간
        private float _telegraphTime = 0.6f;

        // 공격 반경
        private float _attackRadius = 4.0f;

        // 데미지
        private float _damage = 15.0f;

        public void ExecuteAttack(BossController boss)
        {
            boss.StartCoroutine(AttackRoutine(boss));
        }

        private IEnumerator AttackRoutine(BossController boss)
        {
            // ── 차징 ──
            Debug.Log($"[VineSwing] 차징 시작 ({_telegraphTime}s)");
            boss.StopMove();

            // boss.ShowWarningCircle(_attackRadius);
            // boss.Anim.SetTrigger("Charge");

            yield return new WaitForSeconds(_telegraphTime);

            // ── 즉발 원형 판정 ──
            Debug.Log("[VineSwing] >> 즉발 타격!");
            // boss.Anim.SetTrigger(AnimationName);

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                boss.transform.position, _attackRadius
            );

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    Debug.Log($"[VineSwing] >> Player Hit! Damage: {_damage}");
                    var hp = hit.GetComponent<HP>();
                    if (hp != null)
                        hp.TakeDamage(_damage);
                }
            }

            // boss.HideWarningCircle();
        }

#if UNITY_EDITOR
        public void DrawGizmos(Vector3 bossPos)
        {
            UnityEditor.Handles.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            UnityEditor.Handles.DrawSolidDisc(bossPos, Vector3.forward, _attackRadius);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(bossPos, Vector3.forward, _attackRadius);
        }
#endif
    }
}