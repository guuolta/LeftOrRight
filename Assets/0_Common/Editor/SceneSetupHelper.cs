using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
