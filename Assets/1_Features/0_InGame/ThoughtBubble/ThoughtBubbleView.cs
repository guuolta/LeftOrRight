using UnityEngine;
using UnityEngine.UI;
using R3;
using DG.Tweening;

namespace InGame.ThoughtBubble
{
    /// <summary>
    /// 吹き出しエリアの表示・状態管理を担当するViewコンポーネント。
    /// 投稿ネタが溜まりすぎた際に警告表示を行う。
    /// </summary>
    public class ThoughtBubbleView : MonoBehaviour
    {
        [Header("背景イメージ（警告色変化に使用）")]
        [SerializeField] private Image _backgroundImage;

        [Header("通常時の背景色")]
        [SerializeField] private Color _normalColor = new(0.9f, 0.9f, 1.0f, 0.8f);

        [Header("警告時の背景色（キャパシティ80%以上）")]
        [SerializeField] private Color _warningColor = new(1.0f, 0.6f, 0.4f, 0.9f);

        [Header("危険時の背景色（キャパシティ90%以上）")]
        [SerializeField] private Color _dangerColor = new(1.0f, 0.2f, 0.2f, 1.0f);

        [Header("子オブジェクトの投稿ネタの上限ライン（割合 0〜1）")]
        [SerializeField] private float _warningThreshold = 0.8f;

        [SerializeField] private float _dangerThreshold = 0.9f;

        // 警告アニメーション用Tweener
        private Tweener _shakeTweener;

        // キャパシティ変化通知
        private readonly ReactiveProperty<float> _capacityRatioProp = new(0f);

        /// <summary>現在のキャパシティ比率（0〜1）</summary>
        public ReadOnlyReactiveProperty<float> CapacityRatioProp => _capacityRatioProp;

        /// <summary>
        /// キャパシティ比率を更新して表示に反映する。
        /// </summary>
        /// <param name="currentCount">現在のアイテム数</param>
        /// <param name="maxCapacity">最大キャパシティ</param>
        public void UpdateCapacity(int currentCount, int maxCapacity)
        {
            if (maxCapacity <= 0)
            {
                return;
            }

            var ratio = (float)currentCount / maxCapacity;
            _capacityRatioProp.Value = ratio;

            if (_backgroundImage is null)
            {
                return;
            }

            // キャパシティ状態に応じて背景色を変化させる
            if (ratio >= _dangerThreshold)
            {
                _backgroundImage.color = _dangerColor;
                TriggerShake();
            }
            else if (ratio >= _warningThreshold)
            {
                _backgroundImage.color = _warningColor;
            }
            else
            {
                _backgroundImage.color = _normalColor;
                _shakeTweener?.Kill(true);
            }
        }

        /// <summary>
        /// 危険状態の揺れエフェクトをトリガーする。
        /// </summary>
        private void TriggerShake()
        {
            if (_shakeTweener is { active: true })
            {
                return;
            }

            _shakeTweener = transform
                .DOShakePosition(0.3f, strength: 3f, vibrato: 10, randomness: 45f)
                .SetLoops(-1, LoopType.Restart);
        }

        private void OnDestroy()
        {
            _shakeTweener?.Kill();
            _capacityRatioProp.Dispose();
        }
    }
}
