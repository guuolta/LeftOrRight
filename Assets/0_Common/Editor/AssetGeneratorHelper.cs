using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace Common.Editor
{
    /// <summary>
    /// フォントアセットとスプライトテクスチャを生成するエディタヘルパー。
    /// </summary>
    public static class AssetGeneratorHelper
    {
        // ========== フォント ==========

        [MenuItem("Tools/InGame Setup/Set TMP Default Font")]
        public static void SetTMPDefaultFont()
        {
            const string fontAssetPath = "Assets/0_Common/Fonts/Zen_Kaku_Gothic_Antique/ZenKakuGothicAntique-Medium SDF.asset";
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            if (fontAsset == null)
            {
                Debug.LogError($"[AssetGeneratorHelper] フォントアセットが見つかりません: {fontAssetPath}");
                return;
            }

            // TMP Settings のデフォルトフォントを設定
            var settings = TMP_Settings.instance;
            if (settings == null)
            {
                Debug.LogError("[AssetGeneratorHelper] TMP_Settings が見つかりません");
                return;
            }

            var so = new SerializedObject(settings);
            so.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssetGeneratorHelper] TMP デフォルトフォントを設定しました: {fontAssetPath}");
        }

        [MenuItem("Tools/InGame Setup/Set TMP Default Sprite Asset")]
        public static void SetTMPDefaultSpriteAsset()
        {
            // TMP 付属の EmojiOne をデフォルトスプライトアセットに設定する
            // → フォントにない絵文字 Unicode 文字を自動的にスプライトで代替表示する
            const string spriteAssetPath = "Assets/TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset";
            var spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(spriteAssetPath);
            if (spriteAsset == null)
            {
                Debug.LogError($"[AssetGeneratorHelper] スプライトアセットが見つかりません: {spriteAssetPath}");
                return;
            }

            var settings = TMP_Settings.instance;
            if (settings == null)
            {
                Debug.LogError("[AssetGeneratorHelper] TMP_Settings が見つかりません");
                return;
            }

            var so = new SerializedObject(settings);
            so.FindProperty("m_defaultSpriteAsset").objectReferenceValue = spriteAsset;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssetGeneratorHelper] TMP デフォルトスプライトアセットを設定しました: {spriteAssetPath}");
        }

        [MenuItem("Tools/InGame Setup/Create Japanese Font Asset")]
        public static void CreateJapaneseFontAsset()
        {
            const string fontPath  = "Assets/0_Common/Fonts/Zen_Kaku_Gothic_Antique/ZenKakuGothicAntique-Medium.ttf";
            const string assetPath = "Assets/0_Common/Fonts/Zen_Kaku_Gothic_Antique/ZenKakuGothicAntique-Medium SDF.asset";

            // 既存アセットを削除して再作成
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }

            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogError($"[AssetGeneratorHelper] フォントが見つかりません: {fontPath}");
                return;
            }

            // Dynamic SDF フォントアセットを API で生成
            // GlyphRenderMode.SDFAA：アンチエイリアス付き SDF（標準設定）
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                samplingPointSize: 90,
                atlasPadding: 9,
                renderMode: GlyphRenderMode.SDFAA,
                atlasWidth: 1024,
                atlasHeight: 1024,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true
            );
            fontAsset.name = "ZenKakuGothicAntique-Medium SDF";

            // メインアセットとして保存
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            // アトラステクスチャをサブアセットとして登録
            foreach (var tex in fontAsset.atlasTextures)
            {
                if (tex != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex)))
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
            }

            // マテリアルをサブアセットとして登録
            if (fontAsset.material != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(fontAsset.material)))
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AssetGeneratorHelper] フォントアセット作成完了: {assetPath}");
        }

        [MenuItem("Tools/InGame Setup/Create Japanese Font Asset (OLD)")]
        private static void CreateJapaneseFontAssetOld()
        {
            const string fontPath  = "Assets/0_Common/Fonts/HiraginoSans-W4.ttc";
            const string assetPath = "Assets/0_Common/Fonts/HiraginoSans-W4 SDF.asset";
            const string assetName = "HiraginoSans-W4 SDF";

            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogError($"[AssetGeneratorHelper] フォントが見つかりません: {fontPath}");
                return;
            }

            // 既存アセットを削除して再作成
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            // FontEngineで初期化
            FontEngine.InitializeFontEngine();
            if (FontEngine.LoadFontFace(font, 90) != FontEngineError.Success)
            {
                Debug.LogError($"[AssetGeneratorHelper] フォントフェースの読み込みに失敗: {fontPath}");
                return;
            }

            // FontAssetを生成してアセットとして保存
            var fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            // SerializedObjectで内部フィールドを設定
            var so = new SerializedObject(fontAsset);
            so.FindProperty("m_Version").stringValue                    = "1.1.0";
            so.FindProperty("m_SourceFontFileGUID").stringValue         = AssetDatabase.AssetPathToGUID(fontPath);
            so.FindProperty("m_SourceFontFile_EditorRef").objectReferenceValue = font;
            so.FindProperty("m_AtlasPopulationMode").intValue           = (int)AtlasPopulationMode.Dynamic;
            so.FindProperty("m_AtlasWidth").intValue                    = 1024;
            so.FindProperty("m_AtlasHeight").intValue                   = 1024;
            so.FindProperty("m_AtlasPadding").intValue                  = 9;
            so.FindProperty("m_AtlasRenderMode").intValue               = (int)GlyphRenderMode.SDFAA;

            // FaceInfo設定
            var fi = FontEngine.GetFaceInfo();
            var fip = so.FindProperty("m_FaceInfo");
            fip.FindPropertyRelative("m_FamilyName").stringValue = fi.familyName;
            fip.FindPropertyRelative("m_StyleName").stringValue  = fi.styleName;
            fip.FindPropertyRelative("m_PointSize").intValue     = (int)fi.pointSize;
            fip.FindPropertyRelative("m_Scale").floatValue       = fi.scale;
            fip.FindPropertyRelative("m_LineHeight").floatValue  = fi.lineHeight;
            fip.FindPropertyRelative("m_AscentLine").floatValue  = fi.ascentLine;
            fip.FindPropertyRelative("m_DescentLine").floatValue = fi.descentLine;
            so.ApplyModifiedPropertiesWithoutUndo();

            // アトラステクスチャをサブアセットとして追加
            var texture = new Texture2D(1, 1, TextureFormat.Alpha8, false) { name = assetName + " Atlas" };
            AssetDatabase.AddObjectToAsset(texture, fontAsset);
            var atlasArr = so.FindProperty("m_AtlasTextures");
            atlasArr.arraySize = 1;
            atlasArr.GetArrayElementAtIndex(0).objectReferenceValue = texture;
            so.ApplyModifiedPropertiesWithoutUndo();

            // マテリアルをサブアセットとして追加
            var shader = Shader.Find("TextMeshPro/Distance Field");
            var mat = new Material(shader) { name = assetName + " Atlas Material" };
            mat.SetFloat(ShaderUtilities.ID_GradientScale, 10f);
            mat.SetTexture(ShaderUtilities.ID_MainTex, texture);
            mat.SetFloat(ShaderUtilities.ID_TextureWidth, 1024f);
            mat.SetFloat(ShaderUtilities.ID_TextureHeight, 1024f);
            AssetDatabase.AddObjectToAsset(mat, fontAsset);
            so.FindProperty("m_Material").objectReferenceValue = mat;

            // freeGlyphRects 初期化
            var freeRects = so.FindProperty("m_FreeGlyphRects");
            freeRects.arraySize = 1;
            var r = freeRects.GetArrayElementAtIndex(0);
            r.FindPropertyRelative("x").intValue      = 0;
            r.FindPropertyRelative("y").intValue      = 0;
            r.FindPropertyRelative("width").intValue  = 1023;
            r.FindPropertyRelative("height").intValue = 1023;
            so.FindProperty("m_UsedGlyphRects").arraySize = 0;
            so.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AssetGeneratorHelper] フォントアセット作成完了（Texture・Material付き）: {assetPath}");
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
