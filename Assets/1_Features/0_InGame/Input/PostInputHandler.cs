using System;
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
    /// 左クリック／タップで種別問わず掴める。タッチ入力にも対応。
    /// </summary>
    public class PostInputHandler : MonoBehaviour
    {
        [Header("仕分けエリア一覧")]
        [SerializeField] private SortAreaView[] _sortAreas;

        [Header("メインカメラ")]
        [SerializeField] private Camera _mainCamera;

        [Header("吹き出しエリアのRectTransform（範囲外ドロップ判定用）")]
        [SerializeField] private RectTransform _thoughtBubbleBounds;

        [Header("デバッグ：有効期限タイマーを無効化する")]
        [SerializeField] private bool _disableExpiry = false;

        // ゲームオーバー通知（誤ドロップ）
        private readonly Subject<GameOverReason> _onMistake = new();

        // 仕分け成功通知
        private readonly Subject<Unit> _onSortSuccess = new();

        // 現在ドラッグ中の投稿
        private IPostItem _draggingItem;

        // フィールド内の全投稿ネタを管理するリスト
        private readonly List<PostItemView> _activeItems = new();

        // アイテムごとの有効期限購読を管理
        private readonly Dictionary<PostItemView, IDisposable> _expirySubscriptions = new();

        /// <summary>誤ドロップ時に通知するObservable</summary>
        public Observable<GameOverReason> OnMistakeObservable => _onMistake;

        /// <summary>仕分け成功時に通知するObservable</summary>
        public Observable<Unit> OnSortSuccessObservable => _onSortSuccess;

        /// <summary>
        /// フィールドへ投稿ネタを登録し、有効期限タイマーを開始する。
        /// </summary>
        public void RegisterItem(PostItemView item)
        {
            _activeItems.Add(item);

            // 常にシェイクタイマーを開始（_disableExpiry 中もシェイクは動作する）
            item.StartExpiry();

            if (!_disableExpiry)
            {
                // 有効期限切れ時にゲームオーバー通知
                var sub = item.OnExpiredObservable
                    .Take(1)
                    .Subscribe(_ => _onMistake.OnNext(GameOverReason.TimeOver));
                _expirySubscriptions[item] = sub;
            }
        }

        /// <summary>
        /// 全アイテムを一括クリアする。リトライ時に呼ぶ。
        /// </summary>
        public void ClearAllItems()
        {
            foreach (var sub in _expirySubscriptions.Values)
            {
                sub.Dispose();
            }
            _expirySubscriptions.Clear();
            _activeItems.Clear();
            _draggingItem = null;
        }

        /// <summary>
        /// フィールドから投稿ネタを登録解除し、有効期限タイマーをキャンセルする。
        /// </summary>
        public void UnregisterItem(PostItemView item)
        {
            _activeItems.Remove(item);
            item.CancelExpiry();

            if (_expirySubscriptions.TryGetValue(item, out var sub))
            {
                sub.Dispose();
                _expirySubscriptions.Remove(item);
            }
        }

        private void Update()
        {
            if (TryGetPointerDown(out var downScreenPos))
            {
                TryBeginDrag(ScreenToWorld(downScreenPos));
            }
            else if (TryGetPointerUp(out var upScreenPos) && _draggingItem is not null)
            {
                EndDrag(ScreenToWorld(upScreenPos));
            }
            else if (_draggingItem is not null)
            {
                _draggingItem.UpdateDragPosition(ScreenToWorld(GetPointerPosition()));
            }
        }

        /// <summary>
        /// 指定位置にある投稿ネタのドラッグを開始する。
        /// </summary>
        /// <param name="worldPosition">押下位置のワールド座標</param>
        private void TryBeginDrag(Vector3 worldPosition)
        {
            // 押下位置にある投稿ネタをAABBで取得
            PostItemView hitItem = null;
            foreach (var item in _activeItems)
            {
                if (!item.gameObject.activeSelf)
                {
                    continue;
                }

                var diff = item.transform.position - worldPosition;
                // PostItemのサイズは265×107、半サイズ（132.5×53.5）で判定
                if (Mathf.Abs(diff.x) <= 132.5f && Mathf.Abs(diff.y) <= 56f)
                {
                    hitItem = item;
                    break;
                }
            }

            if (hitItem is null)
            {
                return;
            }

            _draggingItem = hitItem;
            _draggingItem.BeginDrag();
        }

        /// <summary>
        /// ドラッグを終了し、ドロップ位置の仕分けエリアを判定する。
        /// </summary>
        /// <param name="worldPosition">離し位置のワールド座標</param>
        private void EndDrag(Vector3 worldPosition)
        {
            if (_draggingItem is null)
            {
                return;
            }

            // ドロップ先のエリアを判定（投稿がエリアに少しでも触れていればOK）
            var postHalfSize = new Vector2(132.5f, 56f); // 265/2, 112/2
            SortAreaView droppedArea = null;
            foreach (var area in _sortAreas)
            {
                if (area.OverlapsPost(_draggingItem.Position, postHalfSize))
                {
                    droppedArea = area;
                    break;
                }
            }

            if (droppedArea is not null)
            {
                if (droppedArea.AcceptedPostType == _draggingItem.PostType)
                {
                    // 正しいエリアにドロップ → スマホにスタック・仕分け成功
                    var postView = _draggingItem as PostItemView;
                    droppedArea.NotifyDropped(_draggingItem);
                    droppedArea.StackPost(postView);
                    UnregisterItem(postView);
                    postView?.HideBackground();
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
                // エリア外にドロップ → 吹き出し内なら元の位置へ、外ならゲームオーバー
                _draggingItem.EndDrag();
                if (!IsInsideThoughtBubble(_draggingItem.Position))
                {
                    _onMistake.OnNext(GameOverReason.OutsideDrop);
                }
                else
                {
                    // 吹き出し内に戻した → シェイク再開
                    (_draggingItem as PostItemView)?.ResumeShake();
                }
            }

            _draggingItem = null;
        }

        /// <summary>
        /// 押下開始（左クリック押下 or タッチ開始）を検知してスクリーン座標を返す。
        /// </summary>
        private bool TryGetPointerDown(out Vector2 screenPos)
        {
            // タッチ入力（優先）
            var touch = Touchscreen.current;
            if (touch is not null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = touch.primaryTouch.position.ReadValue();
                return true;
            }

            // マウス左ボタン
            var mouse = Mouse.current;
            if (mouse is not null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPos = mouse.position.ReadValue();
                return true;
            }

            screenPos = Vector2.zero;
            return false;
        }

        /// <summary>
        /// 離し（左クリック離し or タッチ終了）を検知してスクリーン座標を返す。
        /// </summary>
        private bool TryGetPointerUp(out Vector2 screenPos)
        {
            var touch = Touchscreen.current;
            if (touch is not null && touch.primaryTouch.press.wasReleasedThisFrame)
            {
                screenPos = touch.primaryTouch.position.ReadValue();
                return true;
            }

            var mouse = Mouse.current;
            if (mouse is not null && mouse.leftButton.wasReleasedThisFrame)
            {
                screenPos = mouse.position.ReadValue();
                return true;
            }

            screenPos = Vector2.zero;
            return false;
        }

        /// <summary>
        /// 現在のポインター（タッチ or マウス）のスクリーン座標を返す。
        /// </summary>
        private Vector2 GetPointerPosition()
        {
            var touch = Touchscreen.current;
            if (touch is not null && touch.primaryTouch.press.isPressed)
            {
                return touch.primaryTouch.position.ReadValue();
            }

            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// スクリーン座標をワールド座標に変換する。
        /// </summary>
        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            var worldPos = _mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, _mainCamera.nearClipPlane)
            );
            return new Vector3(worldPos.x, worldPos.y, 0f);
        }

        /// <summary>
        /// 指定ワールド座標が吹き出しエリア内かどうかを判定する。
        /// </summary>
        private bool IsInsideThoughtBubble(Vector3 worldPos)
        {
            if (_thoughtBubbleBounds is null)
            {
                // バウンズ未設定時は制限なし（常にtrue）
                return true;
            }

            // RectTransformのワールド四隅座標で判定
            var corners = new Vector3[4];
            _thoughtBubbleBounds.GetWorldCorners(corners);
            // corners[0]=左下, corners[1]=左上, corners[2]=右上, corners[3]=右下
            return worldPos.x >= corners[0].x && worldPos.x <= corners[2].x
                && worldPos.y >= corners[0].y && worldPos.y <= corners[2].y;
        }

        private void OnDestroy()
        {
            _onMistake.Dispose();
            _onSortSuccess.Dispose();

            foreach (var sub in _expirySubscriptions.Values)
            {
                sub.Dispose();
            }
            _expirySubscriptions.Clear();
        }
    }
}
