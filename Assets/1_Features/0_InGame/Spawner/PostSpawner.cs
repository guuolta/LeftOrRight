using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using InGame.Post;
using InGame.Input;
using InGame.GameFlow;
using InGame.Score;

namespace InGame.Spawner
{
    /// <summary>
    /// 投稿ネタを一定間隔でスポーンするコンポーネント。
    /// 時間経過とともにスポーン間隔が短くなる。
    /// </summary>
    public class PostSpawner : MonoBehaviour
    {
        [Header("スポーン設定")]
        [SerializeField] private SpawnerConfigSO _config;

        [Header("投稿ネタのプレハブ")]
        [SerializeField] private PostItemView _postItemPrefab;

        [Header("スポーン位置（頭アイコンの位置）")]
        [SerializeField] private Transform _spawnPoint;

        [Header("吹き出しエリアの親オブジェクト（生成した投稿ネタの親にする）")]
        [SerializeField] private Transform _thoughtBubbleParent;

        [Header("吹き出しエリアのView（キャパシティ表示更新用）")]
        [SerializeField] private ThoughtBubble.ThoughtBubbleView _thoughtBubbleView;

        [Header("吹き出しエリアのRectTransform（境界クランプ用）")]
        [SerializeField] private RectTransform _thoughtBubbleBounds;

        [Header("吹き出し境界コライダー（ポストをエリア内に留める）")]
        [SerializeField] private PolygonCollider2D _thoughtBubbleBoundary;

        private PostInputHandler _inputHandler;
        private GameModel _gameModel;
        private ScoreModel _scoreModel;

        // 現在フィールドにある投稿ネタ数
        private int _activeItemCount;

        // 分離処理用バッファ（毎フレームのGC削減）
        private readonly List<PostItemView> _separationBuffer = new();

        // 吹き出しエリアのワールド四隅（キャッシュ）
        private readonly Vector3[] _bubbleCorners = new Vector3[4];

        /// <summary>
        /// 依存オブジェクトを注入して初期化する。
        /// </summary>
        /// <param name="inputHandler">入力ハンドラー</param>
        /// <param name="gameModel">ゲームモデル</param>
        /// <param name="scoreModel">スコアモデル（スポーン数の難易度計算に使用）</param>
        public void Initialize(PostInputHandler inputHandler, GameModel gameModel, ScoreModel scoreModel)
        {
            _inputHandler = inputHandler;
            _gameModel = gameModel;
            _scoreModel = scoreModel;
        }

        /// <summary>
        /// スポーンループを開始する。
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask StartSpawningAsync(CancellationToken ct)
        {
            // 最初の1個はすぐにスポーン
            if (_activeItemCount < _config.MaxCapacity)
            {
                SpawnItem();
            }

            while (!ct.IsCancellationRequested)
            {
                // キャパシティオーバーチェック
                if (_activeItemCount >= _config.MaxCapacity)
                {
                    _gameModel.TriggerGameOver(GameOverReason.CapacityOver);
                    return;
                }

                // 現在スコアに応じたインターバルで待機
                var score = _scoreModel.ScoreProp.CurrentValue;
                await UniTask.WaitForSeconds(GetSpawnInterval(score), cancellationToken: ct);

                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // スコア20で2個、80で3個に増加
                score = _scoreModel.ScoreProp.CurrentValue;
                var extraCount = score >= 80 ? 2 : score >= 20 ? 1 : 0;
                var spawnCount = Mathf.Min(
                    _config.InitialSpawnCount + extraCount,
                    _config.MaxSpawnCount
                );

                for (var i = 0; i < spawnCount; i++)
                {
                    // キャパシティを超えない範囲でスポーン
                    if (_activeItemCount + i >= _config.MaxCapacity)
                    {
                        break;
                    }
                    SpawnItem();
                }
            }
        }

        /// <summary>
        /// 現在スコアに応じたスポーン間隔を返す。
        /// スコア80までは従来通り、80〜150は10刻みで1.7f→1.5fへ緩やかに減少。
        /// </summary>
        /// <param name="score">現在スコア</param>
        private float GetSpawnInterval(int score)
        {
            if (score >= 150) return 1.5f;
            if (score >= 140) return 1.53f;
            if (score >= 130) return 1.56f;
            if (score >= 120) return 1.59f;
            if (score >= 110) return 1.61f;
            if (score >= 100) return 1.64f;
            if (score >= 90)  return 1.67f;
            if (score >= 80)  return 1.7f;
            if (score >= 70)  return 1.75f;
            if (score >= 60)  return 1.8f;
            if (score >= 50)  return 1.85f;
            if (score >= 40)  return 1.95f;
            if (score >= 10)  return 2.0f;
            return _config.SpawnInterval;
        }

        /// <summary>
        /// 投稿ネタを1つスポーンする。
        /// </summary>
        private void SpawnItem()
        {
            if (_config.PostConfigs is null || _config.PostConfigs.Length == 0)
            {
                return;
            }

            // ランダムに投稿設定を選択
            var index = Random.Range(0, _config.PostConfigs.Length);
            var config = _config.PostConfigs[index];

            // プレハブを生成して初期化
            var item = Instantiate(_postItemPrefab, _spawnPoint.position, Quaternion.identity, _thoughtBubbleParent);
            item.Initialize(config);

            // 既存アイテムと重ならない位置を取得してアニメーション移動
            var center = _thoughtBubbleView.transform.position;
            var target = GetNonOverlappingPosition(center, halfW: 380f, halfH: 170f, itemW: 285f, itemH: 91f);
            item.MoveToPosition(target);

            // 入力ハンドラーに登録
            _inputHandler.RegisterItem(item);
            _activeItemCount++;
        }

        /// <summary>
        /// 既存アイテムと重ならないランダム位置を取得する。
        /// 最大<c>maxAttempts</c>回試行し、見つからなければ最後の候補を返す。
        /// </summary>
        /// <param name="center">探索中心のワールド座標</param>
        /// <param name="halfW">X方向の探索半径</param>
        /// <param name="halfH">Y方向の探索半径</param>
        /// <param name="itemW">アイテムの幅（重なり判定用、マージン込み）</param>
        /// <param name="itemH">アイテムの高さ（重なり判定用、マージン込み）</param>
        private Vector3 GetNonOverlappingPosition(Vector3 center, float halfW, float halfH, float itemW, float itemH)
        {
            const int MaxAttempts = 30;
            var candidate = center;

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                candidate = center + new Vector3(
                    Random.Range(-halfW, halfW),
                    Random.Range(-halfH, halfH),
                    0f
                );

                var overlaps = false;
                foreach (Transform child in _thoughtBubbleParent)
                {
                    if (!child.gameObject.activeSelf)
                    {
                        continue;
                    }

                    // アニメーション中でも目標位置で判定する（現在位置は不正確なため）
                    var postItem = child.GetComponent<PostItemView>();
                    var checkPos = postItem is not null ? postItem.TargetPosition : child.position;
                    var diff = checkPos - candidate;
                    if (Mathf.Abs(diff.x) < itemW && Mathf.Abs(diff.y) < itemH)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    return candidate;
                }
            }

            // 全試行で重なりが解消できなかった場合は最後の候補をそのまま返す
            return candidate;
        }

        private void Update()
        {
            // アクティブな投稿数を再カウント（GameObjectのアクティブ状態で判定）
            if (_thoughtBubbleParent is null)
            {
                return;
            }

            var count = 0;
            foreach (Transform child in _thoughtBubbleParent)
            {
                if (child.gameObject.activeSelf)
                {
                    count++;
                }
            }

            _activeItemCount = count;

            // 吹き出しViewのキャパシティ表示を更新
            _thoughtBubbleView?.UpdateCapacity(_activeItemCount, _config.MaxCapacity);

            // ポスト同士の重なりを毎フレーム解消する
            SeparatePosts();
        }

        /// <summary>
        /// アニメーション完了済みのポスト同士が重なっている場合に押し退ける。
        /// ドラッグ中・アニメーション中のポストはスキップする。
        /// </summary>
        private void SeparatePosts()
        {
            // 対象ポストを収集
            _separationBuffer.Clear();
            foreach (Transform child in _thoughtBubbleParent)
            {
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                var pv = child.GetComponent<PostItemView>();
                if (pv is not null && !pv.IsAnimating && !pv.IsDragging)
                {
                    _separationBuffer.Add(pv);
                }
            }

            const float PostW = 265f;
            const float PostH = 71f;

            // 全ペアについて重なりをチェックして押し退ける
            for (var i = 0; i < _separationBuffer.Count; i++)
            {
                for (var j = i + 1; j < _separationBuffer.Count; j++)
                {
                    var a = _separationBuffer[i];
                    var b = _separationBuffer[j];
                    var diff = b.transform.position - a.transform.position;
                    var overlapX = PostW - Mathf.Abs(diff.x);
                    var overlapY = PostH - Mathf.Abs(diff.y);

                    if (overlapX <= 0f || overlapY <= 0f)
                    {
                        continue;
                    }

                    // 重なりが小さい軸方向に均等に押し退ける
                    Vector3 push;
                    if (overlapX < overlapY)
                    {
                        var signX = diff.x != 0f ? Mathf.Sign(diff.x) : 1f;
                        push = new Vector3(overlapX * 0.5f * signX, 0f, 0f);
                    }
                    else
                    {
                        var signY = diff.y != 0f ? Mathf.Sign(diff.y) : 1f;
                        push = new Vector3(0f, overlapY * 0.5f * signY, 0f);
                    }

                    a.transform.position -= push;
                    b.transform.position += push;
                }
            }

            // 吹き出し境界コライダーでクランプ（ポストの4隅すべてが境界内に収まるよう押し戻す）
            if (_thoughtBubbleBoundary is not null)
            {
                foreach (var pv in _separationBuffer)
                {
                    var pos = (Vector2)pv.transform.position;
                    // ポストの4隅を検査して、はみ出た分を補正ベクトルとして合算
                    var correction = Vector2.zero;
                    var corners2d = new[]
                    {
                        pos + new Vector2(-PostW * 0.5f, -PostH * 0.5f),
                        pos + new Vector2( PostW * 0.5f, -PostH * 0.5f),
                        pos + new Vector2(-PostW * 0.5f,  PostH * 0.5f),
                        pos + new Vector2( PostW * 0.5f,  PostH * 0.5f),
                    };
                    foreach (var corner in corners2d)
                    {
                        if (!_thoughtBubbleBoundary.OverlapPoint(corner))
                        {
                            var closest = _thoughtBubbleBoundary.ClosestPoint(corner);
                            correction += closest - corner;
                        }
                    }
                    if (correction != Vector2.zero)
                    {
                        pv.transform.position = new Vector3(pos.x + correction.x, pos.y + correction.y, pv.transform.position.z);
                    }
                }
            }
        }
    }
}
