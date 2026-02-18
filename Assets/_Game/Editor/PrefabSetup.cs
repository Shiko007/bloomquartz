using UnityEngine;
using UnityEditor;
using System.IO;

namespace Bloomquartz.Editor
{
    public static class PrefabSetup
    {
        private static readonly Color[] GemColors = new Color[]
        {
            new Color(1f,   0.15f, 0.15f), // Ruby
            new Color(0.2f, 0.45f, 1f),    // Sapphire
            new Color(0.1f, 0.85f, 0.3f),  // Emerald
            new Color(0.75f,0.2f,  1f),    // Amethyst
            new Color(1f,   0.78f, 0.1f),  // Topaz
            new Color(0.85f,0.95f, 1f),    // Diamond
        };

        [MenuItem("Bloomquartz/Create Tile Prefab")]
        public static void CreateTilePrefab()
        {
            EnsureFolder("Assets/_Game/Sprites/Gems");
            EnsureFolder("Assets/_Game/Prefabs/Tiles");

            // Create gem sprites (colored circles via texture)
            Sprite[] gemSprites = new Sprite[GemColors.Length];
            for (int i = 0; i < GemColors.Length; i++)
                gemSprites[i] = CreateGemSprite(i, GemColors[i]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Build Tile prefab
            var tileGO = new GameObject("Tile");

            // Background quad
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(tileGO.transform);
            var bgSr = bgGO.AddComponent<SpriteRenderer>();
            bgSr.sprite = CreateRoundedSquareSprite();
            bgSr.color = new Color(0.15f, 0.08f, 0.25f, 0.8f);
            bgSr.sortingOrder = 0;

            // Gem sprite
            var gemGO = new GameObject("Gem");
            gemGO.transform.SetParent(tileGO.transform);
            gemGO.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            var gemSr = gemGO.AddComponent<SpriteRenderer>();
            gemSr.sprite = gemSprites[0];
            gemSr.sortingOrder = 1;

            // Selection ring
            var selGO = new GameObject("SelectionRing");
            selGO.transform.SetParent(tileGO.transform);
            selGO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            var selSr = selGO.AddComponent<SpriteRenderer>();
            selSr.sprite = CreateRingSprite();
            selSr.color = new Color(1f, 0.9f, 0.2f);
            selSr.sortingOrder = 2;

            // BoxCollider for click detection
            var col = tileGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.95f);

            // Tile script
            var tileScript = tileGO.AddComponent<Bloomquartz.Puzzle.Tile>();

            // Wire serialized fields via SerializedObject
            var prefabPath = "Assets/_Game/Prefabs/Tiles/Tile.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(tileGO, prefabPath);

            // Set sprite arrays on the prefab
            var so = new SerializedObject(prefab.GetComponent<Bloomquartz.Puzzle.Tile>());
            so.FindProperty("gemRenderer").objectReferenceValue = prefab.transform.Find("Gem").GetComponent<SpriteRenderer>();
            so.FindProperty("selectionRenderer").objectReferenceValue = prefab.transform.Find("SelectionRing").GetComponent<SpriteRenderer>();

            var spritesArr = so.FindProperty("gemSprites");
            spritesArr.arraySize = gemSprites.Length;
            for (int i = 0; i < gemSprites.Length; i++)
                spritesArr.GetArrayElementAtIndex(i).objectReferenceValue = gemSprites[i];

            so.ApplyModifiedProperties();
            PrefabUtility.SavePrefabAsset(prefab);

            Object.DestroyImmediate(tileGO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Bloomquartz] Tile prefab created at " + prefabPath);
            EditorUtility.DisplayDialog("Bloomquartz Setup", "Tile prefab created!\n\nNext: Open PuzzleBoard scene and assign the Tile prefab to the Board component.", "OK");
        }

        // ── Texture helpers ────────────────────────────────────────

        private static Sprite CreateGemSprite(int index, Color color)
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float cx = size / 2f, cy = size / 2f, r = size / 2f - 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= r)
                    {
                        // Gem shine effect
                        float shine = Mathf.Clamp01(1f - dist / r);
                        shine = Mathf.Pow(shine, 1.5f);
                        Color c = Color.Lerp(color, Color.white, shine * 0.35f);

                        // Highlight spot
                        float hlDx = x - (cx - r * 0.25f), hlDy = y - (cy + r * 0.25f);
                        float hlDist = Mathf.Sqrt(hlDx * hlDx + hlDy * hlDy);
                        if (hlDist < r * 0.25f)
                            c = Color.Lerp(c, Color.white, 0.7f * (1f - hlDist / (r * 0.25f)));

                        // Edge feather
                        float alpha = dist > r - 1.5f ? Mathf.Clamp01((r - dist) / 1.5f) : 1f;
                        tex.SetPixel(x, y, new Color(c.r, c.g, c.b, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();

            string path = $"Assets/_Game/Sprites/Gems/Gem_{index}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Bilinear;
            AssetDatabase.ImportAsset(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateRoundedSquareSprite()
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float r = 10f;
            float cx = size / 2f, cy = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - cx) - (cx - r);
                    float ny = Mathf.Abs(y - cy) - (cy - r);
                    float dist = Mathf.Sqrt(Mathf.Max(nx, 0) * Mathf.Max(nx, 0) + Mathf.Max(ny, 0) * Mathf.Max(ny, 0));
                    float alpha = dist <= r ? 1f : dist <= r + 1.5f ? Mathf.Clamp01((r + 1.5f - dist) / 1.5f) : 0f;
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            }
            tex.Apply();

            string path = "Assets/_Game/Sprites/Gems/TileBG.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            AssetDatabase.ImportAsset(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateRingSprite()
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float cx = size / 2f, cy = size / 2f;
            float outerR = size / 2f - 1f, innerR = outerR - 5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    bool inRing = dist <= outerR && dist >= innerR;
                    float alpha = inRing ? 1f : 0f;
                    if (dist > outerR - 1.5f && dist <= outerR)
                        alpha = Mathf.Clamp01((outerR - dist) / 1.5f);
                    if (dist >= innerR && dist < innerR + 1.5f)
                        alpha = Mathf.Clamp01((dist - innerR) / 1.5f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            }
            tex.Apply();

            string path = "Assets/_Game/Sprites/Gems/SelectionRing.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            AssetDatabase.ImportAsset(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("Bloomquartz/Create Star Sprite")]
        public static Sprite CreateStarSprite()
        {
            EnsureFolder("Assets/_Game/Sprites/UI");
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // Clear to transparent
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, Color.clear);

            // Draw a 5-pointed star
            float cx = size / 2f, cy = size / 2f;
            float outerR = size / 2f - 2f, innerR = outerR * 0.42f;
            int points = 5;

            // Build star polygon vertices
            var verts = new Vector2[points * 2];
            for (int i = 0; i < points * 2; i++)
            {
                float angle = Mathf.PI / 2f + i * Mathf.PI / points;
                float r = (i % 2 == 0) ? outerR : innerR;
                verts[i] = new Vector2(cx + r * Mathf.Cos(angle), cy + r * Mathf.Sin(angle));
            }

            // Fill star using point-in-polygon per pixel
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (PointInPolygon(new Vector2(x, y), verts))
                        tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();

            string path = "Assets/_Game/Sprites/UI/Star.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType      = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            AssetDatabase.ImportAsset(path);

            Debug.Log("[Bloomquartz] Star sprite created at " + path);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            int n = poly.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                    inside = !inside;
            }
            return inside;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
