using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class WhiteboxTools
{
    private const string AssetFolder = "Assets/Tile/Placeholder";
    private const string TexturePath = AssetFolder + "/WhiteTile128.png";
    private const string TilePath = AssetFolder + "/WhiteTile.asset";
    private const string PalettePath = AssetFolder + "/WhitePalette.prefab";
    private static readonly string[] Names = { "Surface", "Platform", "Wall", "Ladder" };

    // Player data expressed in 1.28-unit grid cells.
    private const int JUMP_ROWS = 3;
    private const int JUMP_RUN_COLS = 4;
    private const int WALK_STEP_ROWS = 1;

    public static string CreateAssets()
    {
        try
        {
            if (!AssetDatabase.IsValidFolder("Assets/Tile")) AssetDatabase.CreateFolder("Assets", "Tile");
            if (!AssetDatabase.IsValidFolder(AssetFolder)) AssetDatabase.CreateFolder("Assets/Tile", "Placeholder");

            if (!File.Exists(TexturePath))
            {
                var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                var pixels = Enumerable.Repeat(Color.white, 128 * 128).ToArray();
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(TexturePath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer == null) return "ERROR: TextureImporter unavailable: " + TexturePath;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
            if (sprite == null) return "ERROR: Sprite import failed: " + TexturePath;

            var tile = AssetDatabase.LoadAssetAtPath<Tile>(TilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, TilePath);
            }
            tile.sprite = sprite;
            tile.color = Color.white;
            tile.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(tile);

            var root = new GameObject("WhitePalette");
            try
            {
                var grid = root.AddComponent<Grid>();
                grid.cellSize = new Vector3(1.28f, 1.28f, 0f);
                var layer = new GameObject("Layer1");
                layer.transform.SetParent(root.transform, false);
                var tilemap = layer.AddComponent<Tilemap>();
                layer.AddComponent<TilemapRenderer>();
                tilemap.SetTile(Vector3Int.zero, tile);
                PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
            return "Created/reused " + TexturePath + ", " + TilePath + ", " + PalettePath +
                   "; sprite rect " + sprite.rect.width + "x" + sprite.rect.height + " pixels.";
        }
        catch (Exception ex) { return "ERROR CreateAssets: " + ex.Message; }
    }

    public static string Paint(string jsonPath, string tilePath = TilePath)
    {
        try
        {
            var data = ReadJson(jsonPath);
            var scene = SceneManager.GetActiveScene();
            var maps = FindMaps(scene);
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath);
            if (tile == null) return "ERROR: Tile asset missing: " + tilePath;
            var report = new StringBuilder("Paint " + scene.path + " from " + jsonPath + "\n");
            foreach (var name in StringList(Get(data, "clear")))
            {
                Tilemap map;
                if (maps.TryGetValue(name, out map)) map.ClearAllTiles();
                else report.AppendLine(name + ": missing child, clear skipped");
            }
            var rects = Object(Get(data, "tilemaps"));
            foreach (var entry in rects)
            {
                Tilemap map;
                if (!maps.TryGetValue(entry.Key, out map))
                {
                    report.AppendLine(entry.Key + ": missing child, paint skipped");
                    continue;
                }
                foreach (var item in Array(entry.Value))
                {
                    var r = Ints(item, 4);
                    int w = r[2] - r[0] + 1, h = r[3] - r[1] + 1;
                    if (w <= 0 || h <= 0) throw new FormatException("Invalid rect for " + entry.Key);
                    var block = new TileBase[checked(w * h)];
                    for (int i = 0; i < block.Length; i++) block[i] = tile;
                    map.SetTilesBlock(new BoundsInt(r[0], r[1], 0, w, h, 1), block);
                }
            }
            int tintSet = Number(Get(data, "tintSet"));
            SetTint(maps, "Surface", new Color(1f, 1f, 1f, 1f));
            SetTint(maps, "Wall", new Color(.70f, .70f, .70f, 1f));
            SetTint(maps, "Ladder", new Color(1f, .95f, .70f, 1f));
            SetTint(maps, "Platform", tintSet == 2 ? new Color(.90f, .90f, .95f, 1f) : new Color(.80f, .92f, 1f, 1f));
            Tilemap platform;
            if (maps.TryGetValue("Platform", out platform))
            {
                var collider = platform.GetComponent<TilemapCollider2D>();
                if (collider != null && !collider.usedByEffector) collider.usedByEffector = true;
            }
            foreach (var name in Names)
            {
                Tilemap map;
                if (!maps.TryGetValue(name, out map)) continue;
                map.CompressBounds();
                EditorUtility.SetDirty(map);
                report.AppendLine(name + ": " + Count(map) + " tiles, cellBounds " + Bounds(map.cellBounds));
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) report.AppendLine("WARNING: scene save failed");
            return report.ToString().TrimEnd();
        }
        catch (Exception ex) { return "ERROR Paint: " + ex.Message; }
    }

    public static string Reach(string jsonPath)
    {
        try
        {
            var data = ReadJson(jsonPath);
            var maps = FindMaps(SceneManager.GetActiveScene());
            var solid = new HashSet<long>();
            var floors = new HashSet<long>();
            var ladders = new HashSet<long>();
            AddCells(maps, "Surface", solid);
            floors.UnionWith(solid);
            AddCells(maps, "Platform", floors);
            AddCells(maps, "Wall", solid);
            AddCells(maps, "Ladder", ladders);

            var standing = new HashSet<long>();
            var columns = new Dictionary<int, List<int>>();
            foreach (long cell in floors)
            {
                int x = X(cell), y = Y(cell);
                if (solid.Contains(Key(x, y + 1)) || solid.Contains(Key(x, y + 2))) continue;
                standing.Add(cell);
                List<int> rows;
                if (!columns.TryGetValue(x, out rows)) columns[x] = rows = new List<int>();
                rows.Add(y);
            }
            foreach (var rows in columns.Values) rows.Sort();

            // Each ladder column is a connected component of consecutive ladder cells.
            var ladderGroup = new Dictionary<long, int>();
            var groups = new List<HashSet<long>>();
            foreach (long cell in ladders)
            {
                if (ladderGroup.ContainsKey(cell)) continue;
                int id = groups.Count;
                var group = new HashSet<long>();
                var queue = new Queue<long>();
                queue.Enqueue(cell);
                ladderGroup[cell] = id;
                while (queue.Count > 0)
                {
                    long current = queue.Dequeue();
                    group.Add(current);
                    foreach (int dy in new[] { -1, 1 })
                    {
                        long next = Key(X(current), Y(current) + dy);
                        if (ladders.Contains(next) && !ladderGroup.ContainsKey(next))
                        {
                            ladderGroup[next] = id;
                            queue.Enqueue(next);
                        }
                    }
                }
                groups.Add(group);
            }
            var standingGroups = new Dictionary<long, HashSet<int>>();
            var groupStanding = new List<HashSet<long>>();
            foreach (var ignored in groups) groupStanding.Add(new HashSet<long>());
            foreach (long cell in standing)
            {
                int x = X(cell), y = Y(cell);
                // dx: a floor edge next to a ladder column (ladder in a 1-wide floor hole) also connects.
                foreach (int dx in new[] { -1, 0, 1 })
                foreach (int dy in new[] { 1, 2 })
                {
                    int id;
                    if (!ladderGroup.TryGetValue(Key(x + dx, y + dy), out id)) continue;
                    HashSet<int> ids;
                    if (!standingGroups.TryGetValue(cell, out ids)) standingGroups[cell] = ids = new HashSet<int>();
                    ids.Add(id);
                    groupStanding[id].Add(cell);
                }
            }

            var starts = new List<long>();
            foreach (var item in Array(Get(data, "starts")))
            {
                var p = Ints(item, 2);
                long nearest = Nearest(standing, p[0], p[1], 2);
                if (nearest != long.MinValue) starts.Add(nearest);
            }
            var links = new Dictionary<long, List<long>>();
            foreach (var item in Array(Get(data, "links")))
            {
                var link = Object(item);
                var from = Ints(Get(link, "from"), 2);
                var to = Ints(Get(link, "to"), 2);
                long a = Nearest(standing, from[0], from[1], 2);
                long b = Nearest(standing, to[0], to[1], 2);
                if (a == long.MinValue || b == long.MinValue) continue;
                List<long> destinations;
                if (!links.TryGetValue(a, out destinations)) links[a] = destinations = new List<long>();
                destinations.Add(b);
            }

            var without = Search(starts, standing, columns, solid, ladders, ladderGroup, groups, standingGroups, groupStanding, null);
            var with = Search(starts, standing, columns, solid, ladders, ladderGroup, groups, standingGroups, groupStanding, links);
            var report = new StringBuilder("Reach " + jsonPath + "\n");
            report.AppendLine("Standing cells: " + standing.Count + "; reachable without links: " + without.Count(k => standing.Contains(k)) +
                              "; with links: " + with.Count(k => standing.Contains(k)));
            report.AppendLine("Starts matched: " + starts.Count + "/" + Array(Get(data, "starts")).Count);
            foreach (var target in Object(Get(data, "targets")))
            {
                var p = Ints(target.Value, 2);
                long cell = Key(p[0], p[1]);
                report.AppendLine(target.Key + " (" + p[0] + "," + p[1] + "): A " +
                                  (without.Contains(cell) ? "REACHABLE" : "UNREACHABLE") + ", B " +
                                  (with.Contains(cell) ? "REACHABLE" : "UNREACHABLE"));
            }
            var gaps = GapCandidates(columns, without, standing).Take(15).ToList();
            report.AppendLine("Gap candidates (up to 15): " + gaps.Count);
            foreach (var gap in gaps) report.AppendLine(gap);
            return report.ToString().TrimEnd();
        }
        catch (Exception ex) { return "ERROR Reach: " + ex.Message; }
    }

    public static string Verify(string jsonPath)
    {
        try
        {
            var data = ReadJson(jsonPath);
            var maps = FindMaps(SceneManager.GetActiveScene());
            var report = new StringBuilder("Verify " + jsonPath + "\n");
            foreach (var entry in Object(Get(data, "tilemaps")))
            {
                Tilemap map;
                if (!maps.TryGetValue(entry.Key, out map))
                {
                    report.AppendLine(entry.Key + ": MISSING");
                    continue;
                }
                var expected = new HashSet<long>();
                foreach (var item in Array(entry.Value))
                {
                    var r = Ints(item, 4);
                    for (int x = r[0]; x <= r[2]; x++)
                        for (int y = r[1]; y <= r[3]; y++) expected.Add(Key(x, y));
                }
                int actual = Count(map);
                var go = map.gameObject;
                report.AppendLine(entry.Key + ": " + (actual == expected.Count ? "MATCH" : "MISMATCH") +
                                  " actual=" + actual + " expected=" + expected.Count +
                                  " layer=" + LayerMask.LayerToName(go.layer) + " tag=" + go.tag +
                                  " TilemapCollider2D=" + (go.GetComponent<TilemapCollider2D>() != null) +
                                  " CompositeCollider2D=" + (go.GetComponent<CompositeCollider2D>() != null) +
                                  " PlatformEffector2D=" + (go.GetComponent<PlatformEffector2D>() != null) +
                                  " Rigidbody2D=" + (go.GetComponent<Rigidbody2D>() != null));
            }
            return report.ToString().TrimEnd();
        }
        catch (Exception ex) { return "ERROR Verify: " + ex.Message; }
    }

    private static HashSet<long> Search(List<long> starts, HashSet<long> standing,
        Dictionary<int, List<int>> columns, HashSet<long> solid, HashSet<long> ladders,
        Dictionary<long, int> ladderGroup, List<HashSet<long>> groups,
        Dictionary<long, HashSet<int>> standingGroups, List<HashSet<long>> groupStanding,
        Dictionary<long, List<long>> links)
    {
        var seen = new HashSet<long>();
        var queue = new Queue<long>();
        foreach (long start in starts) if (seen.Add(start)) queue.Enqueue(start);
        var expandedGroups = new HashSet<int>();
        while (queue.Count > 0)
        {
            long cell = queue.Dequeue();
            int x = X(cell), y = Y(cell);
            if (standing.Contains(cell))
            {
                for (int dx = -JUMP_RUN_COLS; dx <= JUMP_RUN_COLS; dx++)
                {
                    if (dx == 0) continue;
                    List<int> rows;
                    if (!columns.TryGetValue(x + dx, out rows)) continue;
                    foreach (int targetY in rows)
                    {
                        if (targetY > y + JUMP_ROWS || targetY < y - 80) continue;
                        bool walk = Math.Abs(dx) == 1 && targetY <= y + WALK_STEP_ROWS && targetY >= y - 1;
                        if (walk || !LineBlocked(x, y + 1, x + dx, targetY + 1, solid))
                            Enqueue(Key(x + dx, targetY), seen, queue);
                    }
                }
                foreach (int dx in new[] { -1, 1 })
                {
                    List<int> rows;
                    if (!columns.TryGetValue(x + dx, out rows)) continue;
                    if (rows.Any(row => row >= y - 1 && row <= y + 1)) continue;
                    // The first landing below the edge is the highest standing tile below it.
                    int landing = int.MinValue;
                    foreach (int row in rows) if (row < y - 1 && row >= y - 120 && row > landing) landing = row;
                    if (landing == int.MinValue) continue;
                    bool blocked = false;
                    for (int fy = y; fy > landing; fy--)
                        if (solid.Contains(Key(x + dx, fy))) { blocked = true; break; }
                    if (!blocked) Enqueue(Key(x + dx, landing), seen, queue);
                }
                HashSet<int> ids;
                if (standingGroups.TryGetValue(cell, out ids))
                    foreach (int id in ids) ExpandLadder(id, expandedGroups, groups, groupStanding, seen, queue);
                List<long> destinations;
                if (links != null && links.TryGetValue(cell, out destinations))
                    foreach (long destination in destinations) Enqueue(destination, seen, queue);
            }
            else if (ladders.Contains(cell))
            {
                int id;
                if (ladderGroup.TryGetValue(cell, out id)) ExpandLadder(id, expandedGroups, groups, groupStanding, seen, queue);
            }
        }
        return seen;
    }

    private static void ExpandLadder(int id, HashSet<int> expanded, List<HashSet<long>> groups,
        List<HashSet<long>> exits, HashSet<long> seen, Queue<long> queue)
    {
        if (!expanded.Add(id)) return;
        foreach (long cell in groups[id]) Enqueue(cell, seen, queue);
        foreach (long cell in exits[id]) Enqueue(cell, seen, queue);
    }

    private static void Enqueue(long cell, HashSet<long> seen, Queue<long> queue)
    {
        if (seen.Add(cell)) queue.Enqueue(cell);
    }

    private static bool LineBlocked(int x0, int y0, int x1, int y1, HashSet<long> solid)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;
        while (true)
        {
            if (solid.Contains(Key(x0, y0))) return true;
            if (x0 == x1 && y0 == y1) return false;
            int twice = 2 * error;
            if (twice >= dy) { error += dy; x0 += sx; }
            if (twice <= dx) { error += dx; y0 += sy; }
        }
    }

    private static IEnumerable<string> GapCandidates(Dictionary<int, List<int>> columns,
        HashSet<long> reachable, HashSet<long> standing)
    {
        var rows = new Dictionary<int, List<int>>();
        foreach (long cell in standing)
        {
            List<int> xs;
            int y = Y(cell);
            if (!rows.TryGetValue(y, out xs)) rows[y] = xs = new List<int>();
            xs.Add(X(cell));
        }
        var gaps = new List<Tuple<int, int, int>>();
        foreach (var entry in rows)
        {
            var xs = entry.Value;
            xs.Sort();
            for (int i = 1; i < xs.Count; i++)
            {
                int width = xs[i] - xs[i - 1] - 1;
                if (width <= JUMP_RUN_COLS) continue;
                bool left = reachable.Contains(Key(xs[i - 1], entry.Key));
                bool right = reachable.Contains(Key(xs[i], entry.Key));
                if (left != right) gaps.Add(Tuple.Create(width, xs[i - 1], entry.Key));
            }
        }
        return gaps.OrderByDescending(g => g.Item1).Take(15)
            .Select(g => "width " + g.Item1 + " at y=" + g.Item3 + ", x=" + g.Item2 + ".." + (g.Item2 + g.Item1 + 1));
    }

    private static long Nearest(HashSet<long> cells, int x, int y, int radius)
    {
        long best = long.MinValue;
        int distance = int.MaxValue;
        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                long key = Key(x + dx, y + dy);
                int d = dx * dx + dy * dy;
                if (d < distance && cells.Contains(key)) { best = key; distance = d; }
            }
        return best;
    }

    private static void AddCells(Dictionary<string, Tilemap> maps, string name, HashSet<long> cells)
    {
        Tilemap map;
        if (!maps.TryGetValue(name, out map)) return;
        foreach (var pos in map.cellBounds.allPositionsWithin)
            if (map.GetTile(pos) != null) cells.Add(Key(pos.x, pos.y));
    }

    private static int Count(Tilemap map)
    {
        int count = 0;
        foreach (var pos in map.cellBounds.allPositionsWithin) if (map.GetTile(pos) != null) count++;
        return count;
    }

    private static string Bounds(BoundsInt b)
    {
        return "(" + b.xMin + "," + b.yMin + ")..(" + (b.xMax - 1) + "," + (b.yMax - 1) + ")";
    }

    private static void SetTint(Dictionary<string, Tilemap> maps, string name, Color color)
    {
        Tilemap map;
        if (maps.TryGetValue(name, out map)) map.color = color;
    }

    private static Dictionary<string, Tilemap> FindMaps(Scene scene)
    {
        var result = new Dictionary<string, Tilemap>(StringComparer.Ordinal);
        GameObject grid = null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Grid") { grid = root; break; }
        if (grid == null) return result;
        foreach (var name in Names)
        {
            var child = grid.transform.Find(name);
            if (child == null) continue;
            var map = child.GetComponent<Tilemap>();
            if (map != null) result[name] = map;
        }
        return result;
    }

    private static long Key(int x, int y) { return ((long)x << 32) | (uint)y; }
    private static int X(long key) { return (int)(key >> 32); }
    private static int Y(long key) { return (int)key; }

    private static Dictionary<string, object> ReadJson(string path)
    {
        return Object(new JsonReader(File.ReadAllText(path)).Read());
    }
    private static object Get(Dictionary<string, object> obj, string name)
    {
        object value;
        return obj.TryGetValue(name, out value) ? value : null;
    }
    private static Dictionary<string, object> Object(object value)
    {
        var result = value as Dictionary<string, object>;
        if (result == null) throw new FormatException("Expected JSON object");
        return result;
    }
    private static List<object> Array(object value)
    {
        var result = value as List<object>;
        if (result == null) throw new FormatException("Expected JSON array");
        return result;
    }
    private static int Number(object value) { return Convert.ToInt32(value, CultureInfo.InvariantCulture); }
    private static int[] Ints(object value, int count)
    {
        var a = Array(value);
        if (a.Count != count) throw new FormatException("Expected " + count + " numbers");
        return a.Select(Number).ToArray();
    }
    private static IEnumerable<string> StringList(object value)
    {
        foreach (var item in Array(value)) yield return (string)item;
    }

    private sealed class JsonReader
    {
        private readonly string source;
        private int position;
        public JsonReader(string source) { this.source = source; }
        public object Read()
        {
            object value = Value();
            Space();
            if (position != source.Length) throw new FormatException("Trailing JSON at " + position);
            return value;
        }
        private object Value()
        {
            Space();
            if (position >= source.Length) throw new FormatException("Unexpected end of JSON");
            char c = source[position];
            if (c == '{') return ReadObject();
            if (c == '[') return ReadArray();
            if (c == '"') return ReadString();
            if (c == 't') { Literal("true"); return true; }
            if (c == 'f') { Literal("false"); return false; }
            if (c == 'n') { Literal("null"); return null; }
            return ReadNumber();
        }
        private Dictionary<string, object> ReadObject()
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            Expect('{');
            Space();
            if (Take('}')) return result;
            while (true)
            {
                Space();
                string key = ReadString();
                Expect(':');
                result[key] = Value();
                if (Take('}')) return result;
                Expect(',');
            }
        }
        private List<object> ReadArray()
        {
            var result = new List<object>();
            Expect('[');
            Space();
            if (Take(']')) return result;
            while (true)
            {
                result.Add(Value());
                if (Take(']')) return result;
                Expect(',');
            }
        }
        private string ReadString()
        {
            Expect('"');
            var result = new StringBuilder();
            while (position < source.Length)
            {
                char c = source[position++];
                if (c == '"') return result.ToString();
                if (c != '\\') { result.Append(c); continue; }
                if (position >= source.Length) throw new FormatException("Bad JSON escape");
                c = source[position++];
                switch (c)
                {
                    case '"': result.Append('"'); break;
                    case '\\': result.Append('\\'); break;
                    case '/': result.Append('/'); break;
                    case 'b': result.Append('\b'); break;
                    case 'f': result.Append('\f'); break;
                    case 'n': result.Append('\n'); break;
                    case 'r': result.Append('\r'); break;
                    case 't': result.Append('\t'); break;
                    case 'u':
                        if (position + 4 > source.Length) throw new FormatException("Bad Unicode escape");
                        result.Append((char)int.Parse(source.Substring(position, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        position += 4;
                        break;
                    default: throw new FormatException("Bad JSON escape: " + c);
                }
            }
            throw new FormatException("Unterminated JSON string");
        }
        private double ReadNumber()
        {
            int start = position;
            while (position < source.Length && "-+0123456789.eE".IndexOf(source[position]) >= 0) position++;
            if (start == position) throw new FormatException("Expected JSON number at " + position);
            return double.Parse(source.Substring(start, position - start), CultureInfo.InvariantCulture);
        }
        private void Literal(string literal)
        {
            if (position + literal.Length > source.Length || source.Substring(position, literal.Length) != literal)
                throw new FormatException("Expected " + literal + " at " + position);
            position += literal.Length;
        }
        private bool Take(char c)
        {
            Space();
            if (position >= source.Length || source[position] != c) return false;
            position++;
            return true;
        }
        private void Expect(char c)
        {
            if (!Take(c)) throw new FormatException("Expected '" + c + "' at " + position);
        }
        private void Space()
        {
            while (position < source.Length && char.IsWhiteSpace(source[position])) position++;
        }
    }
}
