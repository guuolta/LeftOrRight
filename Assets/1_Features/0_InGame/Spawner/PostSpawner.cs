using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using InGame.Post;
using InGame.Input;
using InGame.GameFlow;

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

        private PostInputHandler _inputHandler;
        private GameModel _gameModel;

        // 現在フィールドにある投稿ネタ数
        private int _activeItemCount;

        /// <summary>
        /// 依存オブジェクトを注入して初期化する。
        /// </summary>
        /// <param name="inputHandler">入力ハンドラー</param>
        /// <param name="gameModel">ゲームモデル</param>
        public void Initialize(PostInputHandler inputHandler, GameModel gameModel)
        {
            _inputHandler = inputHandler;
            _gameModel = gameModel;
        }

        /// <summary>
        /// スポーンループを開始する。
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask StartSpawningAsync(CancellationToken ct)
        {
            var elapsed = 0f;

            while (!ct.IsCancellationRequested)
            {
                // キャパシティオーバーチェック
                if (_activeItemCount >= _config.MaxCapacity)
                {
                    _gameModel.TriggerGameOver(GameOverReason.CapacityOver);
                    return;
                }

                await UniTask.WaitForSeconds(_config.SpawnInterval, cancellationToken: ct);

                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // 経過時間に応じてスポーン数を増加
                var spawnCount = Mathf.Min(
                    _config.InitialSpawnCount + Mathf.FloorToInt(elapsed / _config.SpawnCountIncreaseInterval),
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

                elapsed += _config.SpawnInterval;
            }
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

                    var diff = child.position - candidate;
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
        }
    }
}
