using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using InGame.Post;

namespace InGame.SortArea
{
    /// <summary>
    /// スマホ画面に投稿をScrollViewでスタック表示するコンポーネント。
    /// 新着順に上から積み重ね、ScrollRectでスクロール可能。
    /// 一番下のポストが欠けて見えるようにViewportで自然クリップする。
    /// </summary>
    public class PhoneStackView : MonoBehaviour
    {
        [Header("ScrollRect（スクロール管理）")]
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Content（VerticalLayoutGroup付き投稿の親）")]
        [SerializeField] private RectTransform _content;

        // クリア時の参照管理用
        private readonly List<PostItemView> _stack = new();

        /// <summary>
        /// 投稿をスタックの先頭（上端）に追加し、最新投稿が見えるようにスクロールする。
        /// </summary>
        /// <param name="post">追加する投稿View</param>
        public void AddPost(PostItemView post)
        {
            if (_content is null)
            {
                return;
            }

            // 物理演算を止め、Contentの子として管理
            post.BeginDrag();
            post.transform.SetParent(_content, false);

            // 新着を先頭（一番上）に配置
            post.transform.SetAsFirstSibling();
            _stack.Insert(0, post);

            // レイアウトを即時更新してトップへスクロール
            Canvas.ForceUpdateCanvases();
            if (_scrollRect is not null)
            {
                _scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }
        }

        /// <summary>
        /// スタック内の全投稿を即座に破棄する（リトライ時に使用）。
        /// </summary>
        public void Clear()
        {
            foreach (var post in _stack)
            {
                if (post != null)
                {
                    post.transform.DOKill();
                    post.CancelExpiry();
                    Destroy(post.gameObject);
                }
            }
            _stack.Clear();

            // _stack外に残っているPostItemView子オブジェクトも破棄
            if (_content is not null)
            {
                for (var i = _content.childCount - 1; i >= 0; i--)
                {
                    var child = _content.GetChild(i);
                    if (child.GetComponent<PostItemView>() is not null)
                    {
                        child.DOKill();
                        child.gameObject.SetActive(false);
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _stack.Clear();
        }
    }
}
