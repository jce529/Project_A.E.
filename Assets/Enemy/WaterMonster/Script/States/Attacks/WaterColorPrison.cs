using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterColorPrison : IAttackStrategy
{
    public float Cooldown => 10f;
    public string AnimationName => "Attack_ColorPrison";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);
        boss.StartCoroutine(ColorPrisonSequence(boss));
    }

    private IEnumerator ColorPrisonSequence(BossController boss)
    {
        if (!(boss is WaterMonsterController wmc)) yield break;
        if (wmc.ColorPrisonPrefab == null || wmc.ColorPrisonSpawnBounds == null) yield break;

        PrisonColor coreColor = (Random.value > 0.5f) ? PrisonColor.Red : PrisonColor.Blue;
        wmc.SetCoreColor(coreColor);

        var bossSr = boss.GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (bossSr != null)
        {
            originalColor = bossSr.color;
            bossSr.color = coreColor == PrisonColor.Red
                ? new Color(1f, 0.3f, 0.3f, 1f)
                : new Color(0.3f, 0.5f, 1f, 1f);
        }

        var spawnedZones = new List<GameObject>();
        Bounds bounds = wmc.ColorPrisonSpawnBounds.bounds;
        int count = wmc.ColorPrisonZoneCount;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0f);

            var go = Object.Instantiate(wmc.ColorPrisonPrefab, pos, Quaternion.identity);
            spawnedZones.Add(go);

            var zone = go.GetComponent<WaterColorPrisonZone>();
            if (zone != null)
                zone.SetColor(i % 2 == 0 ? PrisonColor.Red : PrisonColor.Blue);
        }

        yield return new WaitForSeconds(3.5f);

        bool safe = false;
        foreach (var zone in WaterColorPrisonZone.ActiveZones)
        {
            if (zone != null && zone.IsPlayerInside && zone.ZoneColor == coreColor)
            {
                safe = true;
                break;
            }
        }

        if (!safe && PlayerStats.Instance != null)
            PlayerStats.Instance.TakeDamage(80f);

        if (bossSr != null)
            bossSr.color = originalColor;

        foreach (var go in spawnedZones)
        {
            if (go != null) Object.Destroy(go);
        }

        WaterColorPrisonZone.ActiveZones.Clear();
    }
}
