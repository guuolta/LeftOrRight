using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InGame.Post
{
    /// <summary>
    /// 投稿ネタの表示を担当するViewコンポーネント。
    /// ビジネスロジックは持たず、見た目の更新のみを行う。
    /// </summary>
    public class PostItemView : MonoBehaviour, IPostItem
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Collider2D _collider2D;

        private PostType _postType;
        private bool _isDragging;

        /// <inheritdoc/>
        public PostType PostType => _postType;

        /// <inheritdoc/>
        public Vector3 Position => transform.position;

        /// <inheritdoc/>
        public bool IsDragging => _isDragging;

        /// <summary>
        /// 投稿ネタの設定を適用して初期化する。
        /// </summary>
        /// <param name="config">投稿ネタの設定</param>
        public void Initialize(PostItemConfigSO config)
        {
            _postType = config.PostType;

            // 背景色の設定
            if (_backgroundImage is not null)
            {
                _backgroundImage.color = config.Color;
            }

            // アイコンの設定
            if (_iconImage is not null && config.Icon is not null)
            {
                _iconImage.sprite = config.Icon;
                _iconImage.gameObject.SetActive(true);
            }
            else if (_iconImage is not null)
            {
                _iconImage.gameObject.SetActive(false);
            }

            // ラベルの設定
            if (_labelText is not null)
            {
                _labelText.text = config.Label;
            }

            gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public void BeginDrag()
        {
            _isDragging = true;

            // ドラッグ中は物理演算を停止
            if (_rigidbody2D is not null)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
            }
        }

        /// <inheritdoc/>
        public void UpdateDragPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        /// <inheritdoc/>
        public void EndDrag()
        {
            _isDragging = false;

            // 物理演算を再開
            if (_rigidbody2D is not null)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            gameObject.SetActive(false);
        }
    }
}
