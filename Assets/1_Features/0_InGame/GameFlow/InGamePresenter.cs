using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;
using InGame.Audio;
using InGame.Score;
using InGame.Input;
using InGame.Spawner;
using InGame.SortArea;
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
        [SerializeField] private TitleView _titleView;
        [SerializeField] private CountdownView _countdownView;
        [SerializeField] private GameOverEffectView _gameOverEffectView;
        [SerializeField] private InGameAudioView _audioView;

        private GameModel _gameModel;
        private ScoreModel _scoreModel;

        // ゲームループのキャンセルトークンソース
        private CancellationTokenSource _gameLoopCts;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初期化してタイトル画面を表示する。
        /// </summary>
        private void Initialize()
        {
            // マウスカーソルをウィンドウ内に制限
            Cursor.lockState = CursorLockMode.Confined;

            _gameModel = new GameModel();
            _scoreModel = new ScoreModel();

            // スポーナーに依存を注入
            _postSpawner.Initialize(_inputHandler, _gameModel, _scoreModel);

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

            // 仕分け成功購読：スコア加算 + SE再生
            _inputHandler.OnSortSuccessObservable
                .Subscribe(_ =>
                {
                    _scoreModel.AddScore();
                    _audioView?.PlayPostSE();
                })
                .AddTo(destroyCancellationToken);

            // 操作ミス購読：ゲームオーバー
            _inputHandler.OnMistakeObservable
                .Subscribe(reason => _gameModel.TriggerGameOver(reason))
                .AddTo(destroyCancellationToken);

            // タイトル表示 → カウントダウン → ゲーム開始
            ShowTitleAndStartAsync(destroyCancellationToken).Forget();
        }

        /// <summary>
        /// タイトル画面を表示し、クリック後にカウントダウンを経てゲームを開始する。
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        private async UniTask ShowTitleAndStartAsync(CancellationToken ct)
        {
            // タイトル画面が閉じられるまで待機
            var tcs = new UniTaskCompletionSource();
            _titleView.Initialize(() => tcs.TrySetResult());
            await tcs.Task.AttachExternalCancellation(ct);

            // タイトルクリック時のSE
            _audioView?.PlayButtonSE();

            // カウントダウン表示
            await _countdownView.ShowAsync(ct);

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

            // BGM再生開始
            _audioView?.PlayBGM();

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

            // BGM停止
            _audioView?.StopBGM();

            // 爆発エフェクトを再生してから
            if (_gameOverEffectView is not null)
            {
                await _gameOverEffectView.PlayAsync(ct);
            }

            // ゲームオーバー画面を表示
            await _gameOverView.ShowAsync(reason, _scoreModel.ScoreProp.CurrentValue, ct);
        }

        /// <summary>
        /// リトライ時の処理。カウントダウンを表示してからゲームを再開する。
        /// </summary>
        private void OnRetry()
        {
            RetryAsync(destroyCancellationToken).Forget();
        }

        /// <summary>
        /// リトライの非同期処理。
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        private async UniTask RetryAsync(CancellationToken ct)
        {
            // リトライボタンSE
            _audioView?.PlayButtonSE();

            _gameOverView.Hide();
            ClearAllItems();
            _inputHandler.ClearAllItems();

            // スマホのスタックをクリア
            if (_phoneStackViews is not null)
            {
                foreach (var phone in _phoneStackViews)
                {
                    phone?.Clear();
                }
            }

            // スコアをリセット
            _scoreModel.Reset();
            _scoreView.UpdateScore(0);

            // カウントダウン表示
            await _countdownView.ShowAsync(ct);

            StartGame();
        }

        [Header("吹き出しエリアの親Transform（リトライ時のクリア用）")]
        [SerializeField] private Transform _thoughtBubbleParent;

        [Header("スマホのスタックView（リトライ時のクリア用）")]
        [SerializeField] private PhoneStackView[] _phoneStackViews;

        /// <summary>
        /// フィールド上の全投稿ネタを削除する。
        /// </summary>
        private void ClearAllItems()
        {
            if (_thoughtBubbleParent is null)
            {
                return;
            }

            // 子オブジェクトを逆順に即時非表示にしてから破棄（Destroy遅延による残像を防ぐ）
            for (var i = _thoughtBubbleParent.childCount - 1; i >= 0; i--)
            {
                var child = _thoughtBubbleParent.GetChild(i);
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        private void OnDestroy()
        {
            _gameLoopCts?.Cancel();
            _gameLoopCts?.Dispose();
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
