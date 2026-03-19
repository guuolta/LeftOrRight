using R3;
using UnityEngine;

namespace InGame.Post
{
    /// <summary>
    /// 投稿ネタオブジェクトのインターフェース。
    /// </summary>
    public interface IPostItem
    {
        /// <summary>投稿の種別</summary>
        PostType PostType { get; }

        /// <summary>現在のワールド座標</summary>
        Vector3 Position { get; }

        /// <summary>ドラッグ中かどうか</summary>
        bool IsDragging { get; }

        /// <summary>
        /// ドラッグを開始する。
        /// </summary>
        void BeginDrag();

        /// <summary>
        /// ドラッグ中に座標を更新する。
        /// </summary>
        /// <param name="worldPosition">マウスのワールド座標</param>
        void UpdateDragPosition(Vector3 worldPosition);

        /// <summary>
        /// ドラッグを終了し、物理演算を再開する。
        /// </summary>
        void EndDrag();

        /// <summary>
        /// 仕分け完了として非アクティブにする。
        /// </summary>
        void Dispose();
    }
}
