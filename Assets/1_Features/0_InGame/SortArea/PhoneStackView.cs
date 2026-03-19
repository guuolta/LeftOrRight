using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using InGame.Post;

namespace InGame.SortArea
{
    /// <summary>
    /// スマホ画面に投稿をスタック表示するコンポーネント。
    /// 新着順に上から積み重ね、はみ出た古い投稿はフェードアウトして破棄する。
    /// </summary>
    public class PhoneStackView : MonoBehaviour
    {
        [Header("スマホ画面の高さ（ピクセル）")]
        [SerializeField] private float _phoneHeight = 420f;

        [Header("投稿1件の高さ（ピクセル）")]
        [SerializeField] private float _postHeight = 71f;

        [Header("投稿間のスペース（ピクセル）")]
        [SerializeField] private float _postSpacing = 5f;

        [Header("古い投稿のフェードアウト時間（秒）")]
        [SerializeField] private float _fadeOutDuration = 0.3f;

        private readonly List<PostItemView> _stack = new();

        // 表示できる最大件数
        private int MaxPosts => Mathf.FloorToInt(_phoneHeight / (_postHeight + _postSpacing));

        /// <summary>
        /// 投稿をスタックに追加する。
        /// 上限を超えた古い投稿はフェードアウト後に破棄する。
        /// </summary>
        /// <param name="post">追加する投稿View</param>
        public void AddPost(PostItemView post)
        {
            // 物理演算を止め、スマホの子として管理
            post.BeginDrag();
            post.transform.SetParent(transform, true);
            _stack.Insert(0, post);

            // 上限を超えたら古い投稿をフェードアウトして破棄
            while (_stack.Count > MaxPosts)
            {
                var oldest = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
                FadeOutAndDispose(oldest);
            }

            RefreshPositions();
        }

        /// <summary>
        /// スタック内の全投稿をスマホ上端から順に整列させる。
        /// ローカル座標系を使用するためスマホのワールド座標に依存しない。
        /// </summary>
        private void RefreshPositions()
        {
            // スマホ上端の投稿中心Y（ローカル座標：スマホ中心が原点）
            var startLocalY = _phoneHeight * 0.5f - _postHeight * 0.5f - _postSpacing;

            for (var i = 0; i < _stack.Count; i++)
            {
                var localTarget = new Vector3(
                    0f,
                    startLocalY - i * (_postHeight + _postSpacing),
                    0f
                );
                _stack[i].MoveToLocalPosition(localTarget);
            }
        }

        /// <summary>
        /// 投稿をフェードアウトさせてから破棄する。
        /// </summary>
        private void FadeOutAndDispose(PostItemView post)
        {
            var canvasGroup = post.GetComponent<CanvasGroup>();
            if (canvasGroup is null)
            {
                canvasGroup = post.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.DOFade(0f, _fadeOutDuration)
                .OnComplete(() => post.Dispose());
        }

        private void OnDestroy()
        {
            _stack.Clear();
        }
    }
}
