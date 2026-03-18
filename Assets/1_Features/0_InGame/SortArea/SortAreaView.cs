using UnityEngine;
using UnityEngine.UI;
using R3;
using InGame.Post;

namespace InGame.SortArea
{
    /// <summary>
    /// 仕分けエリア（表垢/裏垢スマホ画面）のViewコンポーネント。
    /// 投稿のドロップ受付とエリア判定を担当する。
    /// </summary>
    public class SortAreaView : MonoBehaviour, ISortArea
    {
        [Header("受け付ける投稿種別")]
        [SerializeField] private PostType _acceptedPostType;

        [Header("エリアの当たり判定に使うコライダー")]
        [SerializeField] private RectTransform _rectTransform;

        [Header("ハイライト時のイメージ（ドラッグオーバー演出用）")]
        [SerializeField] private Image _highlightImage;

        // 投稿ドロップ通知
        private readonly Subject<IPostItem> _onDropped = new();

        /// <inheritdoc/>
        public PostType AcceptedPostType => _acceptedPostType;

        /// <inheritdoc/>
        public Observable<IPostItem> OnDroppedObservable => _onDropped;

        /// <inheritdoc/>
        public bool Contains(Vector3 worldPosition)
        {
            if (_rectTransform is null)
            {
                return false;
            }

            // ワールド座標をスクリーン座標に変換してRectTransform内に含まれるか判定
            var camera = Camera.main;
            if (camera is null)
            {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(
                _rectTransform,
                RectTransformUtility.WorldToScreenPoint(camera, worldPosition),
                camera
            );
        }

        /// <summary>
        /// 投稿がこのエリアにドロップされたことを通知する。
        /// </summary>
        /// <param name="postItem">ドロップされた投稿ネタ</param>
        public void NotifyDropped(IPostItem postItem)
        {
            _onDropped.OnNext(postItem);
        }

        /// <summary>
        /// ドラッグオーバー時のハイライト表示を切り替える。
        /// </summary>
        /// <param name="isHighlighted">ハイライト状態にするか</param>
        public void SetHighlight(bool isHighlighted)
        {
            if (_highlightImage is null)
            {
                return;
            }

            _highlightImage.enabled = isHighlighted;
        }

        private void OnDestroy()
        {
            _onDropped.Dispose();
        }
    }
}
