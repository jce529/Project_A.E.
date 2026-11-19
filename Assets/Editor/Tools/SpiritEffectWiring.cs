using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// WaterSpirit/Resource/이펙트 시트를 SpiritEffects 컴포넌트에 연결한다. (프리팹 에셋 대상)
public static class SpiritEffectWiring
{
    private const string BossPrefabPath = "Assets/Enemy/WaterSpirit/WaterSpirit.prefab";
    private const string DummyPrefabGuid = "146b2fee94d4cf541b68d860c547fe7b";

    [MenuItem("Tools/Spirit Boss/Wire Water Spirit Effects")]
    public static void Wire()
    {
        var entries = BuildEntries();
        string[] paths = { BossPrefabPath, AssetDatabase.GUIDToAssetPath(DummyPrefabGuid) };
        foreach (var path in paths)
        {
            if (string.IsNullOrEmpty(path)) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            var fx = root.GetComponent<SpiritEffects>() ?? root.AddComponent<SpiritEffects>();
            fx.Entries = entries.Select(e => new SpiritEffects.Entry
            {
                Id = e.Id, Stage1 = e.Stage1, Stage2 = e.Stage2, Fps = e.Fps, Scale = e.Scale
            }).ToList();
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"[SpiritEffectWiring] {path} ← {fx.Entries.Count}개 이펙트 연결");
        }
    }

    // 이펙트 시트는 자동 슬라이스(알파 기준)가 아니라 BuildEntries 표의 열x행 균등 격자로 자른다.
#pragma warning disable CS0618
    private static void SliceGrid(string path, int cols, int rows)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        imp.GetSourceTextureWidthAndHeight(out int w, out int h);
        float cw = (float)w / cols, ch = (float)h / rows;
        string name = System.IO.Path.GetFileNameWithoutExtension(path);

        var metas = new SpriteMetaData[cols * rows];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                int i = r * cols + c;
                metas[i] = new SpriteMetaData
                {
                    name = $"{name}_{i}",
                    rect = new Rect(c * cw, h - (r + 1) * ch, cw, ch),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.maxTextureSize = 8192; // 2048 이면 가로로 긴 시트(그로기·사망)가 뭉개진다
        imp.spritesheet = metas;
        imp.SaveAndReimport();
    }
#pragma warning restore CS0618

    private static string N(string s) => s.Normalize(NormalizationForm.FormC);

    // 시트 이름 규칙: 1단계 = 이름, 2단계 = 이름 + "2". 물방울은 번호로 이어진다
    // (물방울/물방울4 = 시작, 물방울3/물방울6 = 명중, 물방울2/물방울5 는 쓰지 않음). 사망은 양쪽 공용.
    // 격자(열x행)는 파일명이 아니라 이 표가 정한다.
    private static SpiritEffects.Entry[] BuildEntries()
    {
        var map = new (SpiritFx id, string stage1, string stage2, int cols, int rows, float fps)[]
        {
            (SpiritFx.Windup, "물모으기", "물모으기2", 6, 1, 12f),
            (SpiritFx.Charge, "돌진", "돌진2", 1, 5, 15f),
            (SpiritFx.Move, "이동", "이동2", 2, 5, 15f),
            (SpiritFx.Repel, "밀치기", "밀치기2", 4, 2, 16f),
            (SpiritFx.Flash, "섬광", "섬광2", 2, 1, 8f),
            (SpiritFx.Hit, "피격", "피격2", 4, 1, 14f),
            (SpiritFx.Death, "사망", "사망", 7, 2, 12f),
            (SpiritFx.Groggy, "그로기", "그로기2", 9, 2, 10f),
            (SpiritFx.DropStart, "물방울", "물방울4", 3, 1, 12f),
            (SpiritFx.DropHit, "물방울3", "물방울6", 5, 1, 14f),
        };

        var tex = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Enemy/WaterSpirit/Resource" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => N(p).Contains("이펙트/"))
            .ToArray();

        Sprite[] Load(string name, int cols, int rows)
        {
            var p = tex.FirstOrDefault(t => N(System.IO.Path.GetFileNameWithoutExtension(t)) == N(name));
            if (p == null) { Debug.LogWarning($"[SpiritEffectWiring] 시트 없음: {name}"); return null; }

            // 이미 격자대로 잘려 있으면 다시 자르지 않는다(스프라이트 참조 유지)
            if (AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>().Count() != cols * rows) SliceGrid(p, cols, rows);

            // 읽는 순서: 위 → 아래, 왼쪽 → 오른쪽
            return AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>()
                .OrderByDescending(s => Mathf.RoundToInt(s.rect.y / 10f)).ThenBy(s => s.rect.x).ToArray();
        }

        var result = new System.Collections.Generic.List<SpiritEffects.Entry>();
        foreach (var m in map)
        {
            var e = new SpiritEffects.Entry
            {
                Id = m.id, Fps = m.fps,
                Stage1 = Load(m.stage1, m.cols, m.rows),
                Stage2 = Load(m.stage2, m.cols, m.rows)
            };
            if (e.Stage1 != null || e.Stage2 != null) result.Add(e);
        }
        return result.ToArray();
    }
}
