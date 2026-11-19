using System.Collections;
using UnityEngine;

public class WaterJumpLand : IAttackStrategy
{
    public float Cooldown => 4f;
    public string AnimationName => "Attack_Jump";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);
        boss.StartCoroutine(JumpLandSequence(boss));
    }

    private IEnumerator JumpLandSequence(BossController boss)
    {
        boss.StopMove();
        yield return new WaitForSeconds(0.5f);

        var rb = boss.GetComponent<Rigidbody2D>();
        float savedGravity = 0f;
        if (rb != null)
        {
            savedGravity = rb.gravityScale;
            rb.gravityScale = 0f;
        }

        Vector3 origin = boss.transform.position;
        Vector3 apex = origin + Vector3.up * 3f;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            boss.transform.position = Vector3.Lerp(origin, apex, elapsed / 0.25f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        boss.transform.position = apex;

        yield return new WaitForSeconds(0.4f);

        Vector3 landPos = new Vector3(boss.Target.position.x, origin.y, origin.z);
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            boss.transform.position = Vector3.Lerp(apex, landPos, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        boss.transform.position = landPos;

        if (rb != null)
            rb.gravityScale = savedGravity;

        var hits = Physics2D.OverlapCircleAll(landPos, 3f, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            var ps = hit.GetComponentInParent<PlayerStats>();
            if (ps != null) ps.TakeDamage(30f);

            var pc = hit.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)landPos).normalized;
                pc.ApplyKnockback(dir, 10f);
            }
        }
    }
}
