using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace InGame.UI
{
    /// <summary>
    /// タイトル画面の表示を担当するViewコンポーネント。
    /// </summary>
    public class TitleView : MonoBehaviour
    {
        [Header("タイトルテキスト")]
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("操作説明テキスト（点滅用CanvasGroupをアタッチ）")]
        [SerializeField] private CanvasGroup _instructionCanvasGroup;

        [Header("スタートボタン")]
        [SerializeField] private Button _startButton;

        [Header("テキストの点滅間隔（秒）")]
        [SerializeField] private float _blinkInterval = 0.8f;

        private System.Action _onStart;
        private Tweener _blinkTweener;

        /// <summary>
        /// 初期化する。
        /// </summary>
        /// <param name="onStart">ゲーム開始時のコールバック</param>
        public void Initialize(System.Action onStart)
        {
            _onStart = onStart;
            _startButton?.onClick.AddListener(OnStartClicked);

            // 操作説明テキストを点滅させる（DOVirtualでalphaを直接操作）
            if (_instructionCanvasGroup is not null)
            {
                _blinkTweener = DOVirtual
                    .Float(1f, 0f, _blinkInterval, v => _instructionCanvasGroup.alpha = v)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void OnStartClicked()
        {
            _blinkTweener?.Kill();
            gameObject.SetActive(false);
            _onStart?.Invoke();
        }

        private void OnDestroy()
        {
            _blinkTweener?.Kill();
            _startButton?.onClick.RemoveListener(OnStartClicked);
        }
    }
}
