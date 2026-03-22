using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

namespace InGame.UI
{
    /// <summary>
    /// タイトル画面の表示を担当するViewコンポーネント。
    /// 左クリックでゲーム開始コールバックを発火する。
    /// </summary>
    public class TitleView : MonoBehaviour
    {
        [Header("タイトルテキスト（使用しない場合はnull可）")]
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("操作説明テキスト（点滅用CanvasGroupをアタッチ）")]
        [SerializeField] private CanvasGroup _instructionCanvasGroup;

        [Header("テキストの点滅間隔（秒）")]
        [SerializeField] private float _blinkInterval = 0.8f;

        private System.Action _onStart;
        private Tweener _blinkTweener;
        private bool _isWaitingForClick;

        /// <summary>
        /// 初期化する。左クリックを待機してゲーム開始コールバックを呼ぶ。
        /// </summary>
        /// <param name="onStart">ゲーム開始時のコールバック</param>
        public void Initialize(System.Action onStart)
        {
            _onStart = onStart;
            _isWaitingForClick = true;

            // 操作説明テキストを点滅させる（CanvasGroupのalphaをアニメーション）
            if (_instructionCanvasGroup is not null)
            {
                _blinkTweener = _instructionCanvasGroup
                    .DOFade(0f, _blinkInterval)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void Update()
        {
            if (!_isWaitingForClick)
            {
                return;
            }

            var mouse = Mouse.current;
            if (mouse is null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isWaitingForClick = false;
                _blinkTweener?.Kill();
                gameObject.SetActive(false);
                _onStart?.Invoke();
            }
        }

        private void OnDestroy()
        {
            _blinkTweener?.Kill();
        }
    }
}
