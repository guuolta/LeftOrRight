using UnityEngine;
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

        [Header("投稿を積み重ねるスマホスタックView")]
        [SerializeField] private PhoneStackView _phoneStack;

        // 投稿ドロップ通知
        private readonly Subject<IPostItem> _onDropped = new();

        /// <inheritdoc/>
        public PostType AcceptedPostType => _acceptedPostType;

        private void Awake()
        {
            // _rectTransform 未アサイン時はImage子オブジェクトから取得
            if (_rectTransform == null)
            {
                var imageChild = transform.Find("Image");
                if (imageChild != null)
                {
                    _rectTransform = imageChild.GetComponent<RectTransform>();
                }
            }

            // _phoneStack 未アサイン時は自身のPhoneStackViewを使用（なければ追加）
            if (_phoneStack == null)
            {
                _phoneStack = GetComponent<PhoneStackView>();
                if (_phoneStack == null)
                {
                    _phoneStack = gameObject.AddComponent<PhoneStackView>();
                }
            }
        }

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
        /// 投稿の矩形がこのエリアの矩形と少しでも重なっているか判定する。
        /// </summary>
        /// <param name="postWorldCenter">投稿のワールド座標中心</param>
        /// <param name="postHalfSize">投稿の半サイズ（width/2, height/2）</param>
        public bool OverlapsPost(Vector3 postWorldCenter, Vector2 postHalfSize)
        {
            if (_rectTransform is null)
            {
                return false;
            }

            // スマホエリアの4隅をワールド座標で取得してAABBを算出
            var corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);
            var areaMin = new Vector2(corners[0].x, corners[0].y);
            var areaMax = new Vector2(corners[2].x, corners[2].y);

            // 投稿のAABB
            var postMin = new Vector2(postWorldCenter.x - postHalfSize.x, postWorldCenter.y - postHalfSize.y);
            var postMax = new Vector2(postWorldCenter.x + postHalfSize.x, postWorldCenter.y + postHalfSize.y);

            // AABB同士が重なっていればtrue
            return postMin.x < areaMax.x && postMax.x > areaMin.x
                && postMin.y < areaMax.y && postMax.y > areaMin.y;
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
        /// 投稿をスマホのスタックに追加する。
        /// </summary>
        /// <param name="post">スタックに追加する投稿View</param>
        public void StackPost(PostItemView post)
        {
            _phoneStack?.AddPost(post);
        }

        private void OnDestroy()
        {
            _onDropped.Dispose();
        }
    }
}
