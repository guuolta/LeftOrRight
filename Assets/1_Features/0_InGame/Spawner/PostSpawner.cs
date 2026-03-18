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

                // スポーン間隔を経過時間で短くする
                var interval = Mathf.Max(
                    _config.MinInterval,
                    _config.InitialInterval - elapsed * _config.IntervalDecreasePerSec
                );

                await UniTask.WaitForSeconds(interval, cancellationToken: ct);

                if (ct.IsCancellationRequested)
                {
                    break;
                }

                SpawnItem();
                elapsed += interval;
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

            // 入力ハンドラーに登録
            _inputHandler.RegisterItem(item);
            _activeItemCount++;

            // 仕分け完了時にカウントを減らす（Disposeを検知）
            // GameObjectの非アクティブ化をUpdateで検知する方法を使用
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
