using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using InGame.Spawner;
using InGame.Post;
using InGame.Input;
using InGame.SortArea;
using InGame.GameFlow;
using InGame.Score;
using InGame.ThoughtBubble;
using InGame.UI;

namespace Common.Editor
{
    /// <summary>
    /// InGameシーン用のScriptableObjectとPrefabを一括生成するエディタヘルパー。
    /// </summary>
    public static class SceneSetupHelper
    {
        [MenuItem("Tools/InGame Setup/Wire Up Scene References")]
        public static void WireUpSceneReferences()
        {
            // 各GameObjectを取得
            var inGameGo           = GameObject.Find("InGame");
            var canvasGo           = GameObject.Find("Canvas");
            var postInputHandlerGo = GameObject.Find("PostInputHandler");
            var postSpawnerGo      = GameObject.Find("PostSpawner");
            var thoughtBubbleItems = GameObject.Find("ThoughtBubbleItems");

            var headerGo        = canvasGo.transform.Find("[UI] Header");
            var scoreTextGo     = headerGo?.Find("ScoreText");
            var thoughtBubbleGo = canvasGo.transform.Find("[UI] ThoughtBubble");
            var publicPhoneGo   = canvasGo.transform.Find("[UI] PublicPhone");
            var privatePhoneGo  = canvasGo.transform.Find("[UI] PrivatePhone");
            var gameOverPanelGo = canvasGo.transform.Find("[UI] GameOverPanel");
            var titlePanelGo    = canvasGo.transform.Find("[UI] TitlePanel");

            var spawnPointGo        = postSpawnerGo?.transform.Find("SpawnPoint");
            var instrCanvasGroupGo  = titlePanelGo?.Find("InstructionCanvasGroup");

            // --- InGamePresenter ---
            if (inGameGo != null)
            {
                var presenter = inGameGo.GetComponent<InGamePresenter>();
                if (presenter != null)
                {
                    var so = new SerializedObject(presenter);
                    so.FindProperty("_inputHandler").objectReferenceValue      = postInputHandlerGo?.GetComponent<PostInputHandler>();
                    so.FindProperty("_postSpawner").objectReferenceValue       = postSpawnerGo?.GetComponent<PostSpawner>();
                    so.FindProperty("_scoreView").objectReferenceValue         = scoreTextGo?.gameObject.GetComponent<ScoreView>();
                    so.FindProperty("_gameOverView").objectReferenceValue      = gameOverPanelGo?.gameObject.GetComponent<GameOverView>();
                    so.FindProperty("_thoughtBubbleParent").objectReferenceValue = thoughtBubbleItems?.transform;
                    so.ApplyModifiedProperties();
                    Debug.Log("[SceneSetupHelper] InGamePresenter 参照設定完了");
                }
            }

            // --- PostInputHandler ---
            if (postInputHandlerGo != null)
            {
                var handler = postInputHandlerGo.GetComponent<PostInputHandler>();
                if (handler != null)
                {
                    var so = new SerializedObject(handler);
                    so.FindProperty("_mainCamera").objectReferenceValue = Camera.main;
                    var sortAreasProp = so.FindProperty("_sortAreas");
                    sortAreasProp.arraySize = 2;
                    sortAreasProp.GetArrayElementAtIndex(0).objectReferenceValue = publicPhoneGo?.gameObject.GetComponent<SortAreaView>();
                    sortAreasProp.GetArrayElementAtIndex(1).objectReferenceValue = privatePhoneGo?.gameObject.GetComponent<SortAreaView>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[SceneSetupHelper] PostInputHandler 参照設定完了");
                }
            }

            // --- PostSpawner ---
            if (postSpawnerGo != null)
            {
                var spawner = postSpawnerGo.GetComponent<PostSpawner>();
                if (spawner != null)
                {
                    var prefab  = AssetDatabase.LoadAssetAtPath<PostItemView>("Assets/1_Features/0_InGame/Post/Prefabs/PostItem.prefab");
                    var config  = AssetDatabase.LoadAssetAtPath<SpawnerConfigSO>("Assets/Resources/SpawnerConfig.asset");
                    var so = new SerializedObject(spawner);
                    so.FindProperty("_config").objectReferenceValue            = config;
                    so.FindProperty("_postItemPrefab").objectReferenceValue    = prefab;
                    so.FindProperty("_spawnPoint").objectReferenceValue        = spawnPointGo;
                    so.FindProperty("_thoughtBubbleParent").objectReferenceValue = thoughtBubbleItems?.transform;
                    so.FindProperty("_thoughtBubbleView").objectReferenceValue = thoughtBubbleGo?.gameObject.GetComponent<ThoughtBubbleView>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[SceneSetupHelper] PostSpawner 参照設定完了");
                }
            }

            // --- SortAreaView (PublicPhone) ---
            if (publicPhoneGo != null)
            {
                var area = publicPhoneGo.gameObject.GetComponent<SortAreaView>();
                if (area != null)
                {
                    var so = new SerializedObject(area);
                    so.FindProperty("_acceptedPostType").enumValueIndex = (int)PostType.Public;
                    so.FindProperty("_rectTransform").objectReferenceValue    = publicPhoneGo.gameObject.GetComponent<RectTransform>();
                    so.FindProperty("_highlightImage").objectReferenceValue   = publicPhoneGo.Find("HighlightImage")?.GetComponent<Image>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[SceneSetupHelper] PublicPhone SortAreaView 参照設定完了");
                }
            }

            // --- SortAreaView (PrivatePhone) ---
            if (privatePhoneGo != null)
            {
                var area = privatePhoneGo.gameObject.GetComponent<SortAreaView>();
                if (area != null)
                {
                    var so = new SerializedObject(area);
                    so.FindProperty("_acceptedPostType").enumValueIndex = (int)PostType.Private;
                    so.FindProperty("_rectTransform").objectReferenceValue    = privatePhoneGo.gameObject.GetComponent<RectTransform>();
                    so.FindProperty("_highlightImage").objectReferenceValue   = privatePhoneGo.Find("HighlightImage")?.GetComponent<Image>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[SceneSetupHelper] PrivatePhone SortAreaView 参照設定完了");
                }
            }

            // --- GameOverView ---
            if (gameOverPanelGo != null)
            {
                var view = gameOverPanelGo.gameObject.GetComponent<GameOverView>();
                if (view != null)
                {
                    var so = new SerializedObject(view);
                    so.FindProperty("_panelCanvasGroup").objectReferenceValue = gameOverPanelGo.gameObject.GetComponent<CanvasGroup>();
                    so.FindProperty("_reasonText").objectReferenceValue       = gameOverPanelGo.Find("ReasonText")?.GetComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro"));
                    so.FindProperty("_finalScoreText").objectReferenceValue   = gameOverPanelGo.Find("FinalScoreText")?.GetComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro"));
                    so.FindProperty("_retryButton").objectReferenceValue      = gameOverPanelGo.Find("RetryButton")?.GetComponent<Button>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[SceneSetupHelper] GameOverView 参照設定完了");
                }
            }

            // --- TitleView ---
            if (titlePanelGo != null)
            {
                var view = titlePanelGo.gameObject.GetComponent<TitleView>();
                if (view != null)
                {
                    var so = new SerializedObject(view);
                    so.FindProperty("_titleText").objectReferenceValue            = titlePanelGo.Find("TitleText")?.GetComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro"));
                    so.FindProperty("_instructionCanvasGroup").objectReferenceValue = instrCanvasGroupGo?.GetComponent<CanvasGroup>();
                    so.FindProperty("_startButton").objectReferenceValue          = titlePanelGo.Find("StartButton")?.GetComponent<Button>();
                    so.ApplyModifiedProperties();
                    Debug.Log("[SceneSetupHelper] TitleView 参照設定完了");
                }
            }

            // SpawnerConfigSOのPostConfigs配列にも全PostItemConfigSOを登録
            WireSpawnerPostConfigs();

            // シーン保存
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[SceneSetupHelper] シーン参照配線完了・保存しました");
        }

        [MenuItem("Tools/InGame Setup/Apply Fonts and Sprites")]
        public static void ApplyFontsAndSprites()
        {
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/0_Common/Fonts/Zen_Kaku_Gothic_Antique/ZenKakuGothicAntique-Medium SDF.asset");
            var roundedRect = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/0_Common/Sprites/RoundedRect.png");
            var ellipse     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/0_Common/Sprites/Ellipse.png");
            var circle      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/0_Common/Sprites/Circle.png");
            var phoneFrame  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/0_Common/Sprites/PhoneFrame.png");

            if (fontAsset == null) { Debug.LogError("[SceneSetupHelper] フォントアセットが見つかりません"); return; }

            var canvasGo        = GameObject.Find("Canvas");
            var headerGo        = canvasGo?.transform.Find("[UI] Header");
            var thoughtBubbleGo = canvasGo?.transform.Find("[UI] ThoughtBubble");
            var publicPhoneGo   = canvasGo?.transform.Find("[UI] PublicPhone");
            var privatePhoneGo  = canvasGo?.transform.Find("[UI] PrivatePhone");
            var gameOverPanelGo = canvasGo?.transform.Find("[UI] GameOverPanel");
            var titlePanelGo    = canvasGo?.transform.Find("[UI] TitlePanel");

            var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");

            // --- 全TextMeshProUGUIにフォントを適用 ---
            void ApplyFont(Transform parent, string childName)
            {
                var go = parent?.Find(childName);
                if (go == null) return;
                var tmp = go.GetComponent(tmpType) as TMP_Text;
                if (tmp == null) return;
                var so = new SerializedObject(tmp);
                so.FindProperty("m_fontAsset").objectReferenceValue = fontAsset;
                so.ApplyModifiedProperties();
            }

            ApplyFont(headerGo,                                           "ScoreText");
            ApplyFont(gameOverPanelGo,                                    "ReasonText");
            ApplyFont(gameOverPanelGo,                                    "FinalScoreText");
            ApplyFont(titlePanelGo,                                       "TitleText");
            ApplyFont(titlePanelGo?.Find("InstructionCanvasGroup"),       "InstructionText");

            // --- ThoughtBubble 背景Image にEllipseスプライトを設定 ---
            SetSprite(thoughtBubbleGo?.Find("Image"), ellipse, new Color(0.9f, 0.95f, 1f));

            // --- PublicPhone ---
            SetSprite(publicPhoneGo?.Find("Image"),          phoneFrame, new Color(0.68f, 0.85f, 0.90f)); // パステルブルー
            SetSprite(publicPhoneGo?.Find("HighlightImage"), roundedRect, new Color(1f, 1f, 0f, 0.3f));

            // --- PrivatePhone ---
            SetSprite(privatePhoneGo?.Find("Image"),          phoneFrame, new Color(0.18f, 0.1f, 0.22f)); // ダークパープル
            SetSprite(privatePhoneGo?.Find("HighlightImage"), roundedRect, new Color(1f, 1f, 0f, 0.3f));

            // --- PostItem Prefab に Circle スプライトを設定 ---
            var prefabPath = "Assets/1_Features/0_InGame/Post/Prefabs/PostItem.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                using var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
                var root = editScope.prefabContentsRoot;
                SetSprite(root.transform.Find("Background"), circle, Color.white);
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetupHelper] フォント・スプライト適用完了");
        }

        private static void SetSprite(Transform t, Sprite sprite, Color color)
        {
            if (t == null || sprite == null) return;
            var img = t.GetComponent<Image>();
            if (img == null) return;
            var so = new SerializedObject(img);
            so.FindProperty("m_Sprite").objectReferenceValue = sprite;
            so.FindProperty("m_Color").colorValue = color;
            so.ApplyModifiedProperties();
        }

        private static void WireSpawnerPostConfigs()
        {
            var config = AssetDatabase.LoadAssetAtPath<SpawnerConfigSO>("Assets/Resources/SpawnerConfig.asset");
            if (config == null) return;

            var names = new[] { "CafePost", "LikePost", "SunnyPost", "AngerPost", "SkullPost", "TrashPost" };
            var so = new SerializedObject(config);
            var postConfigsProp = so.FindProperty("_postConfigs");
            postConfigsProp.arraySize = names.Length;
            for (var i = 0; i < names.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<PostItemConfigSO>($"Assets/Resources/{names[i]}.asset");
                postConfigsProp.GetArrayElementAtIndex(i).objectReferenceValue = asset;
            }
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetupHelper] SpawnerConfig.PostConfigs 設定完了");
        }

        [MenuItem("Tools/InGame Setup/Apply UI Layout 1920x1080")]
        public static void ApplyUILayout()
        {
            var canvasGo        = GameObject.Find("Canvas");
            var headerGo        = canvasGo?.transform.Find("[UI] Header");
            var scoreTextGo     = headerGo?.Find("ScoreText");
            var publicPhoneGo   = canvasGo?.transform.Find("[UI] PublicPhone");
            var privatePhoneGo  = canvasGo?.transform.Find("[UI] PrivatePhone");
            var thoughtBubbleGo = canvasGo?.transform.Find("[UI] ThoughtBubble");
            var gameOverPanelGo = canvasGo?.transform.Find("[UI] GameOverPanel");
            var titlePanelGo    = canvasGo?.transform.Find("[UI] TitlePanel");
            var instrGroupGo    = titlePanelGo?.Find("InstructionCanvasGroup");

            // ─── Header（画面上部・Transform 親） ──────────────────
            // Canvas の ScreenSpace Overlay 座標系: 中央=(0,0)、上端=y+540
            SetPos(headerGo, new Vector3(0, 490, 0));
            // ScoreText（Header の子・RectTransform）
            SetRect(scoreTextGo, anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: Vector2.zero, size: new Vector2(600, 70));

            // ─── PublicPhone（左上） ─────────────────────────────
            SetPos(publicPhoneGo, new Vector3(-700, 220, 0));
            SetRect(publicPhoneGo?.Find("Image"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: Vector2.zero, size: new Vector2(240, 420));
            SetRect(publicPhoneGo?.Find("HighlightImage"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: Vector2.zero, size: new Vector2(240, 420));

            // ─── PrivatePhone（右上） ────────────────────────────
            SetPos(privatePhoneGo, new Vector3(700, 220, 0));
            SetRect(privatePhoneGo?.Find("Image"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: Vector2.zero, size: new Vector2(240, 420));
            SetRect(privatePhoneGo?.Find("HighlightImage"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: Vector2.zero, size: new Vector2(240, 420));

            // ─── ThoughtBubble（中央エリア） ─────────────────────
            SetPos(thoughtBubbleGo, new Vector3(0, -80, 0));
            SetRect(thoughtBubbleGo?.Find("Image"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: Vector2.zero, size: new Vector2(1100, 520));

            // ─── GameOverPanel（画面中央・Transform 親） ───────────
            SetPos(gameOverPanelGo, Vector3.zero);
            SetRect(gameOverPanelGo?.Find("ReasonText"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: new Vector2(0, 100), size: new Vector2(900, 90));
            SetRect(gameOverPanelGo?.Find("FinalScoreText"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: new Vector2(0, -10), size: new Vector2(600, 70));
            SetRect(gameOverPanelGo?.Find("RetryButton"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: new Vector2(0, -140), size: new Vector2(300, 80));

            // ─── TitlePanel（画面中央・Transform 親） ─────────────
            SetPos(titlePanelGo, Vector3.zero);
            SetRect(titlePanelGo?.Find("TitleText"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: new Vector2(0, 220), size: new Vector2(900, 130));
            // InstructionCanvasGroup（Transform 親）
            SetPos(instrGroupGo, new Vector3(0, 20, 0));
            SetRect(instrGroupGo?.Find("InstructionText"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: Vector2.zero, size: new Vector2(1000, 320));
            SetRect(titlePanelGo?.Find("StartButton"),
                    anchorMin: Vector2.one * 0.5f, anchorMax: Vector2.one * 0.5f,
                    pivot: Vector2.one * 0.5f, pos: new Vector2(0, -220), size: new Vector2(320, 90));

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[SceneSetupHelper] UIレイアウト 1920x1080 適用完了");
        }

        /// <summary>
        /// Transform の LocalPosition を設定する（RectTransform のない Canvas 子オブジェクト用）。
        /// </summary>
        private static void SetPos(Transform t, Vector3 localPos)
        {
            if (t == null) return;
            // RectTransform が存在する場合は anchoredPosition で設定
            var rt = t.GetComponent<RectTransform>();
            if (rt != null)
            {
                var so = new SerializedObject(rt);
                so.FindProperty("m_AnchoredPosition").vector2Value = new Vector2(localPos.x, localPos.y);
                so.ApplyModifiedProperties();
                return;
            }
            // Transform のみの場合は localPosition を直接代入してダーティフラグを立てる
            t.localPosition = localPos;
            EditorUtility.SetDirty(t);
        }

        /// <summary>
        /// RectTransform のアンカー・位置・サイズを一括設定する。
        /// </summary>
        private static void SetRect(Transform t, Vector2 anchorMin, Vector2 anchorMax,
                                    Vector2 pivot, Vector2 pos, Vector2 size)
        {
            if (t == null) return;
            var rt = t.GetComponent<RectTransform>();
            if (rt == null) return;
            var so = new SerializedObject(rt);
            so.FindProperty("m_AnchorMin").vector2Value         = anchorMin;
            so.FindProperty("m_AnchorMax").vector2Value         = anchorMax;
            so.FindProperty("m_Pivot").vector2Value             = pivot;
            so.FindProperty("m_AnchoredPosition").vector2Value  = pos;
            so.FindProperty("m_SizeDelta").vector2Value         = size;
            so.ApplyModifiedProperties();
        }

        [MenuItem("Tools/InGame Setup/Create PostItem Prefab")]
        public static void CreatePostItemPrefab()
        {
            const string prefabPath = "Assets/1_Features/0_InGame/Post/Prefabs/PostItem.prefab";

            // ルートGameObject作成
            var root = new GameObject("PostItem");
            root.AddComponent<RectTransform>();
            var postItemView = root.AddComponent<PostItemView>();
            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1.0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = root.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            // Background (Image)
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(root.transform, false);
            bgGo.AddComponent<RectTransform>();
            var bgImage = bgGo.AddComponent<Image>();

            // Icon (Image)
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(root.transform, false);
            iconGo.AddComponent<RectTransform>();
            var iconImage = iconGo.AddComponent<Image>();

            // Label (TextMeshProUGUI) - リフレクションで参照
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.AddComponent<RectTransform>();
            var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            Component labelComp = null;
            if (tmpType != null)
            {
                labelComp = labelGo.AddComponent(tmpType);
            }

            // PostItemViewのSerializedフィールドを設定
            var so = new SerializedObject(postItemView);
            so.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("_iconImage").objectReferenceValue = iconImage;
            if (labelComp != null)
            {
                so.FindProperty("_labelText").objectReferenceValue = labelComp;
            }
            so.FindProperty("_rigidbody2D").objectReferenceValue = rb;
            so.FindProperty("_collider2D").objectReferenceValue = col;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Prefabとして保存
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SceneSetupHelper] {prefabPath} を作成しました");
        }

        [MenuItem("Tools/InGame Setup/Create All ScriptableObjects")]
        public static void CreateAllScriptableObjects()
        {
            CreateSpawnerConfig();
            CreatePostItemConfigs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneSetupHelper] 全ScriptableObjectの作成完了");
        }

        private static void CreateSpawnerConfig()
        {
            const string path = "Assets/Resources/SpawnerConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<SpawnerConfigSO>(path) != null)
            {
                Debug.Log("[SceneSetupHelper] SpawnerConfig は既に存在します");
                return;
            }

            var so = ScriptableObject.CreateInstance<SpawnerConfigSO>();
            var serialized = new SerializedObject(so);
            serialized.FindProperty("_initialInterval").floatValue = 2.0f;
            serialized.FindProperty("_minInterval").floatValue = 0.3f;
            serialized.FindProperty("_intervalDecreasePerSec").floatValue = 0.02f;
            serialized.FindProperty("_maxCapacity").intValue = 20;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[SceneSetupHelper] {path} を作成しました");
        }

        private static void CreatePostItemConfigs()
        {
            // 表ネタ (Public)
            CreatePostItemConfig("CafePost",  PostType.Public,  "FFE4B5", "☕ カフェに来た");
            CreatePostItemConfig("LikePost",  PostType.Public,  "FFB6C1", "♥ いいね！");
            CreatePostItemConfig("SunnyPost", PostType.Public,  "FFFACD", "☀ 今日も晴れ");
            // 裏ネタ (Private)
            CreatePostItemConfig("AngerPost", PostType.Private, "8B0000", "💢 マジ無理");
            CreatePostItemConfig("SkullPost", PostType.Private, "2F2F2F", "💀 消えたい");
            CreatePostItemConfig("TrashPost", PostType.Private, "4A4A4A", "🗑 ゴミ");
        }

        private static void CreatePostItemConfig(string assetName, PostType postType, string colorHex, string label)
        {
            var path = $"Assets/Resources/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<PostItemConfigSO>(path) != null)
            {
                Debug.Log($"[SceneSetupHelper] {assetName} は既に存在します");
                return;
            }

            var so = ScriptableObject.CreateInstance<PostItemConfigSO>();
            var serialized = new SerializedObject(so);
            serialized.FindProperty("_postType").enumValueIndex = (int)postType;

            if (ColorUtility.TryParseHtmlString("#" + colorHex, out var color))
            {
                serialized.FindProperty("_color").colorValue = color;
            }

            serialized.FindProperty("_label").stringValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[SceneSetupHelper] {path} を作成しました");
        }
    }
}
