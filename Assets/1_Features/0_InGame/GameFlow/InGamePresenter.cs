using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;
using InGame.Score;
using InGame.Input;
using InGame.Spawner;
using InGame.UI;

namespace InGame.GameFlow
{
    /// <summary>
    /// インゲーム全体の進行を制御するPresenter。
    /// GameModel と各Viewをバインドし、ゲームループを管理する。
    /// </summary>
    public class InGamePresenter : MonoBehaviour
    {
        [Header("各種View/Component参照")]
        [SerializeField] private PostInputHandler _inputHandler;
        [SerializeField] private PostSpawner _postSpawner;
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private GameOverView _gameOverView;

        private GameModel _gameModel;
        private ScoreModel _scoreModel;

        // ゲームループのキャンセルトークンソース
        private CancellationTokenSource _gameLoopCts;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初期化してゲームを開始する。
        /// </summary>
        private void Initialize()
        {
            _gameModel = new GameModel();
            _scoreModel = new ScoreModel();

            // スポーナーに依存を注入
            _postSpawner.Initialize(_inputHandler, _gameModel);

            // ゲームオーバーViewの初期化
            _gameOverView.Initialize(OnRetry);

            // スコア購読：変化時にViewを更新
            _scoreModel.ScoreProp
                .Subscribe(score => _scoreView.UpdateScore(score))
                .AddTo(destroyCancellationToken);

            // ゲームオーバー購読
            _gameModel.OnGameOverObservable
                .Subscribe(reason => OnGameOverAsync(reason, destroyCancellationToken).Forget())
                .AddTo(destroyCancellationToken);

            // 仕分け成功購読：スコア加算
            _inputHandler.OnSortSuccessObservable
                .Subscribe(_ => _scoreModel.AddScore())
                .AddTo(destroyCancellationToken);

            // 操作ミス購読：ゲームオーバー
            _inputHandler.OnMistakeObservable
                .Subscribe(reason => _gameModel.TriggerGameOver(reason))
                .AddTo(destroyCancellationToken);

            // ゲーム開始
            StartGame();
        }

        /// <summary>
        /// ゲームを開始する。
        /// </summary>
        private void StartGame()
        {
            _gameLoopCts?.Cancel();
            _gameLoopCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            _gameModel.StartGame();
            _scoreModel.Reset();
            _scoreView.UpdateScore(0);

            // スポーンループ開始
            _postSpawner.StartSpawningAsync(_gameLoopCts.Token).Forget();

            // 経過時間更新ループ開始
            UpdateElapsedTimeAsync(_gameLoopCts.Token).Forget();
        }

        /// <summary>
        /// 経過時間を毎フレーム更新するループ。
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        private async UniTask UpdateElapsedTimeAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                _gameModel.UpdateElapsedTime(Time.deltaTime);
                await UniTask.NextFrame(ct);
            }
        }

        /// <summary>
        /// ゲームオーバー処理を行う。
        /// </summary>
        /// <param name="reason">ゲームオーバーの原因</param>
        /// <param name="ct">キャンセルトークン</param>
        private async UniTask OnGameOverAsync(GameOverReason reason, CancellationToken ct)
        {
            // ゲームループを停止
            _gameLoopCts?.Cancel();

            // ゲームオーバー画面を表示
            await _gameOverView.ShowAsync(reason, _scoreModel.ScoreProp.CurrentValue, ct);
        }

        /// <summary>
        /// リトライ時の処理。
        /// </summary>
        private void OnRetry()
        {
            _gameOverView.Hide();
            // フィールド上の投稿ネタをすべてクリア
            ClearAllItems();
            StartGame();
        }

        /// <summary>
        /// フィールド上の全投稿ネタを削除する。
        /// </summary>
        private void ClearAllItems()
        {
            var thoughtBubble = _postSpawner.GetComponent<Transform>().Find("ThoughtBubble");
            if (thoughtBubble is null)
            {
                return;
            }

            foreach (Transform child in thoughtBubble)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnDestroy()
        {
            _gameLoopCts?.Cancel();
            _gameLoopCts?.Dispose();
        }
    }
}
