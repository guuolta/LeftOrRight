using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using InGame.Post;
using InGame.SortArea;
using InGame.GameFlow;

namespace InGame.Input
{
    /// <summary>
    /// 投稿ネタのドラッグ＆ドロップ入力を処理するハンドラー。
    /// 左クリック→表ネタのみ掴める、右クリック→裏ネタのみ掴める。
    /// </summary>
    public class PostInputHandler : MonoBehaviour
    {
        [Header("仕分けエリア一覧")]
        [SerializeField] private SortAreaView[] _sortAreas;

        [Header("メインカメラ")]
        [SerializeField] private Camera _mainCamera;

        // ゲームオーバー通知（誤操作）
        private readonly Subject<GameOverReason> _onMistake = new();

        // 仕分け成功通知
        private readonly Subject<Unit> _onSortSuccess = new();

        // 現在ドラッグ中の投稿
        private IPostItem _draggingItem;

        // ドラッグ開始時に想定している仕分け種別
        private PostType _expectedPostType;

        // フィールド内の全投稿ネタを管理するリスト
        private readonly List<PostItemView> _activeItems = new();

        /// <summary>操作ミス（誤爆）時に通知するObservable</summary>
        public Observable<GameOverReason> OnMistakeObservable => _onMistake;

        /// <summary>仕分け成功時に通知するObservable</summary>
        public Observable<Unit> OnSortSuccessObservable => _onSortSuccess;

        /// <summary>
        /// フィールドへ投稿ネタを登録する。
        /// </summary>
        /// <param name="item">登録する投稿ネタのView</param>
        public void RegisterItem(PostItemView item)
        {
            _activeItems.Add(item);
        }

        /// <summary>
        /// フィールドから投稿ネタを登録解除する。
        /// </summary>
        /// <param name="item">解除する投稿ネタのView</param>
        public void UnregisterItem(PostItemView item)
        {
            _activeItems.Remove(item);
        }

        private void Update()
        {
            HandleLeftClick();
            HandleRightClick();
            UpdateDrag();
        }

        /// <summary>
        /// 左クリック入力を処理する（表ネタの掴み/ドロップ）。
        /// </summary>
        private void HandleLeftClick()
        {
            var mouse = Mouse.current;
            if (mouse is null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                // 左クリック押下：表ネタ（Public）のみ掴める
                TryBeginDrag(PostType.Public, GetMouseWorldPosition());
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _draggingItem is not null)
            {
                EndDrag(GetMouseWorldPosition());
            }
        }

        /// <summary>
        /// 右クリック入力を処理する（裏ネタの掴み/ドロップ）。
        /// </summary>
        private void HandleRightClick()
        {
            var mouse = Mouse.current;
            if (mouse is null)
            {
                return;
            }

            if (mouse.rightButton.wasPressedThisFrame)
            {
                // 右クリック押下：裏ネタ（Private）のみ掴める
                TryBeginDrag(PostType.Private, GetMouseWorldPosition());
            }
            else if (mouse.rightButton.wasReleasedThisFrame && _draggingItem is not null)
            {
                EndDrag(GetMouseWorldPosition());
            }
        }

        /// <summary>
        /// ドラッグ中の座標更新を処理する。
        /// </summary>
        private void UpdateDrag()
        {
            if (_draggingItem is null)
            {
                return;
            }

            _draggingItem.UpdateDragPosition(GetMouseWorldPosition());

            // ドラッグオーバー中のエリアをハイライト
            var mousePos = GetMouseWorldPosition();
            foreach (var area in _sortAreas)
            {
                area.SetHighlight(area.Contains(mousePos));
            }
        }

        /// <summary>
        /// 指定位置にある投稿ネタのドラッグを開始する。
        /// クリック種別と異なる種別の投稿に触れた場合はゲームオーバー。
        /// </summary>
        /// <param name="clickType">クリックで期待される投稿種別</param>
        /// <param name="worldPosition">クリック位置のワールド座標</param>
        private void TryBeginDrag(PostType clickType, Vector3 worldPosition)
        {
            // クリック位置にある投稿ネタを取得
            var hit = Physics2D.OverlapPoint(worldPosition);
            if (hit is null)
            {
                return;
            }

            var item = hit.GetComponent<PostItemView>();
            if (item is null || !item.gameObject.activeSelf)
            {
                return;
            }

            // 誤爆チェック：期待種別と異なる投稿をクリックした場合
            if (item.PostType != clickType)
            {
                _onMistake.OnNext(GameOverReason.WrongOperation);
                return;
            }

            _expectedPostType = clickType;
            _draggingItem = item;
            _draggingItem.BeginDrag();
        }

        /// <summary>
        /// ドラッグを終了し、ドロップ位置の仕分けエリアを判定する。
        /// </summary>
        /// <param name="worldPosition">ドロップ位置のワールド座標</param>
        private void EndDrag(Vector3 worldPosition)
        {
            if (_draggingItem is null)
            {
                return;
            }

            // ハイライトをすべてリセット
            foreach (var area in _sortAreas)
            {
                area.SetHighlight(false);
            }

            // ドロップ先のエリアを判定
            SortAreaView droppedArea = null;
            foreach (var area in _sortAreas)
            {
                if (area.Contains(worldPosition))
                {
                    droppedArea = area;
                    break;
                }
            }

            if (droppedArea is not null)
            {
                if (droppedArea.AcceptedPostType == _draggingItem.PostType)
                {
                    // 正しいエリアにドロップ → 仕分け成功
                    droppedArea.NotifyDropped(_draggingItem);
                    _draggingItem.Dispose();
                    UnregisterItem(_draggingItem as PostItemView);
                    _onSortSuccess.OnNext(Unit.Default);
                }
                else
                {
                    // 間違ったエリアにドロップ → ゲームオーバー
                    _draggingItem.EndDrag();
                    _onMistake.OnNext(GameOverReason.WrongOperation);
                }
            }
            else
            {
                // エリア外にドロップ → 物理演算に戻す
                _draggingItem.EndDrag();
            }

            _draggingItem = null;
        }

        /// <summary>
        /// マウス座標をワールド座標に変換する。
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            var screenPos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            var worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _mainCamera.nearClipPlane));
            return new Vector3(worldPos.x, worldPos.y, 0f);
        }

        private void OnDestroy()
        {
            _onMistake.Dispose();
            _onSortSuccess.Dispose();
        }
    }
}
