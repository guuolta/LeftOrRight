using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InGame.GameFlow;

namespace InGame.UI
{
    /// <summary>
    /// ゲームオーバー画面の表示を担当するViewコンポーネント。
    /// </summary>
    public class GameOverView : MonoBehaviour
    {
        [Header("ゲームオーバーパネル")]
        [SerializeField] private CanvasGroup _panelCanvasGroup;

        [Header("ヘッダーテキスト（炎上など）")]
        [SerializeField] private TextMeshProUGUI _headerText;

        [Header("内容テキスト（ゲームオーバーの理由）")]
        [SerializeField] private TextMeshProUGUI _reasonText;

        [Header("最終スコアテキスト")]
        [SerializeField] private TextMeshProUGUI _finalScoreText;

        [Header("リトライボタン")]
        [SerializeField] private Button _retryButton;

        [Header("フェードイン時間（秒）")]
        [SerializeField] private float _fadeInDuration = 0.5f;

        // リトライボタン押下通知
        private System.Action _onRetry;

        /// <summary>
        /// 初期化する。
        /// </summary>
        /// <param name="onRetry">リトライ時のコールバック</param>
        public void Initialize(System.Action onRetry)
        {
            _onRetry = onRetry;
            _retryButton?.onClick.AddListener(OnRetryClicked);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// ゲームオーバー画面をフェードインで表示する。
        /// </summary>
        /// <param name="reason">ゲームオーバーの原因</param>
        /// <param name="finalScore">最終スコア</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask ShowAsync(GameOverReason reason, int finalScore, CancellationToken ct)
        {
            gameObject.SetActive(true);

            // ヘッダーテキストの設定
            if (_headerText is not null)
            {
                _headerText.text = reason switch
                {
                    GameOverReason.WrongOperation => "炎上！！",
                    GameOverReason.CapacityOver => "脳内パンク！",
                    GameOverReason.TimeOver => "記憶喪失！",
                    GameOverReason.OutsideDrop => "ど忘れ！",
                    _ => "ゲームオーバー",
                };
            }

            // 内容テキストの設定
            if (_reasonText is not null)
            {
                _reasonText.text = reason switch
                {
                    GameOverReason.WrongOperation => "投稿を誤爆してしまった...",
                    GameOverReason.CapacityOver => "処理しきれなくなってしまった...",
                    GameOverReason.TimeOver => "アイデアをすっかり忘れてしまった...",
                    GameOverReason.OutsideDrop => "アイデアが頭から抜けてしまった...",
                    _ => "",
                };
            }

            // 最終スコアの設定
            if (_finalScoreText is not null)
            {
                _finalScoreText.text = $"{finalScore}いいね";
            }

            // フェードイン
            if (_panelCanvasGroup is not null)
            {
                _panelCanvasGroup.alpha = 0f;
                await _panelCanvasGroup
                    .DOFade(1f, _fadeInDuration)
                    .SetEase(Ease.OutQuad)
                    .ToUniTask(cancellationToken: ct);
            }
        }

        /// <summary>
        /// ゲームオーバー画面を非表示にする。
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnRetryClicked()
        {
            _onRetry?.Invoke();
        }

        private void OnDestroy()
        {
            _retryButton?.onClick.RemoveListener(OnRetryClicked);
        }
    }
}
