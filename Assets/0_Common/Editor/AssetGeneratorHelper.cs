using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;

namespace Common.Editor
{
    /// <summary>
    /// フォントアセットとスプライトテクスチャを生成するエディタヘルパー。
    /// </summary>
    public static class AssetGeneratorHelper
    {
        // ========== フォント ==========

        [MenuItem("Tools/InGame Setup/Create Japanese Font Asset")]
        public static void CreateJapaneseFontAsset()
        {
            const string fontPath    = "Assets/0_Common/Fonts/HiraginoSans-W4.ttc";
            const string assetPath   = "Assets/0_Common/Fonts/HiraginoSans-W4 SDF.asset";

            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogError($"[AssetGeneratorHelper] フォントが見つかりません: {fontPath}");
                return;
            }

            // 既存チェック
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
            {
                Debug.Log("[AssetGeneratorHelper] フォントアセットは既に存在します");
                return;
            }

            // Dynamic SDF フォントアセットを作成（文字は実行時にロード）
            var fontAsset = TMP_FontAsset.CreateFontAsset(font);

            if (fontAsset == null)
            {
                Debug.LogError("[AssetGeneratorHelper] フォントアセットの作成に失敗しました");
                return;
            }

            AssetDatabase.CreateAsset(fontAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AssetGeneratorHelper] フォントアセット作成完了: {assetPath}");
        }

        // ========== スプライト ==========

        [MenuItem("Tools/InGame Setup/Create All Sprites")]
        public static void CreateAllSprites()
        {
            // 角丸矩形 (UIパネル用)
            CreateRoundedRectSprite("RoundedRect",     256, 256, 32, Color.white);
            // 楕円 (吹き出しエリア用)
            CreateEllipseSprite("Ellipse",             256, 180, Color.white);
            // 円 (投稿ネタ用)
            CreateCircleSprite("Circle",               128, Color.white);
            // スマホ枠
            CreatePhoneFrameSprite("PhoneFrame",       160, 280, 20, Color.white);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetGeneratorHelper] 全スプライト作成完了");
        }

        private static void CreateRoundedRectSprite(string name, int w, int h, int radius, Color color)
        {
            var path = $"Assets/0_Common/Sprites/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "../", path)))
            {
                Debug.Log($"[AssetGeneratorHelper] {name} は既に存在します");
                return;
            }

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    pixels[y * w + x] = IsInsideRoundedRect(x, y, w, h, radius) ? color : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            SaveTexture(tex, path);
            Object.DestroyImmediate(tex);
        }

        private static void CreateEllipseSprite(string name, int w, int h, Color color)
        {
            var path = $"Assets/0_Common/Sprites/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "../", path)))
            {
                Debug.Log($"[AssetGeneratorHelper] {name} は既に存在します");
                return;
            }

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];
            var cx = w * 0.5f;
            var cy = h * 0.5f;

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var dx = (x - cx) / cx;
                    var dy = (y - cy) / cy;
                    pixels[y * w + x] = (dx * dx + dy * dy <= 1f) ? color : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            SaveTexture(tex, path);
            Object.DestroyImmediate(tex);
        }

        private static void CreateCircleSprite(string name, int size, Color color)
        {
            var path = $"Assets/0_Common/Sprites/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "../", path)))
            {
                Debug.Log($"[AssetGeneratorHelper] {name} は既に存在します");
                return;
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            var c = size * 0.5f;
            var r = c - 1f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - c;
                    var dy = y - c;
                    pixels[y * size + x] = (dx * dx + dy * dy <= r * r) ? color : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            SaveTexture(tex, path);
            Object.DestroyImmediate(tex);
        }

        private static void CreatePhoneFrameSprite(string name, int w, int h, int cornerRadius, Color color)
        {
            var path = $"Assets/0_Common/Sprites/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "../", path)))
            {
                Debug.Log($"[AssetGeneratorHelper] {name} は既に存在します");
                return;
            }

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    pixels[y * w + x] = IsInsideRoundedRect(x, y, w, h, cornerRadius) ? color : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            SaveTexture(tex, path);
            Object.DestroyImmediate(tex);
        }

        private static bool IsInsideRoundedRect(int x, int y, int w, int h, int r)
        {
            // コーナーの角丸判定
            var inX = x >= r && x < w - r;
            var inY = y >= r && y < h - r;
            if (inX || inY) return true;

            var corners = new[]
            {
                new Vector2Int(r, r),
                new Vector2Int(w - r - 1, r),
                new Vector2Int(r, h - r - 1),
                new Vector2Int(w - r - 1, h - r - 1),
            };

            foreach (var corner in corners)
            {
                var dx = x - corner.x;
                var dy = y - corner.y;
                if (dx * dx + dy * dy <= r * r) return true;
            }

            return false;
        }

        private static void SaveTexture(Texture2D tex, string assetPath)
        {
            var bytes = tex.EncodeToPNG();
            var fullPath = Path.Combine(Application.dataPath, "../", assetPath);
            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.ImportAsset(assetPath);

            // Spriteとしてインポート設定
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.filterMode          = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            Debug.Log($"[AssetGeneratorHelper] スプライト作成: {assetPath}");
        }
    }
}
