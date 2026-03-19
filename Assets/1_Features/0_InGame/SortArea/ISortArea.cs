using R3;
using InGame.Post;

namespace InGame.SortArea
{
    /// <summary>
    /// 仕分けエリア（表垢/裏垢スマホ）のインターフェース。
    /// </summary>
    public interface ISortArea
    {
        /// <summary>このエリアが受け付ける投稿種別</summary>
        PostType AcceptedPostType { get; }

        /// <summary>投稿がドロップされたときに通知するObservable</summary>
        Observable<IPostItem> OnDroppedObservable { get; }

        /// <summary>
        /// 指定座標がこのエリア内に含まれるかどうかを判定する。
        /// </summary>
        /// <param name="worldPosition">判定するワールド座標</param>
        bool Contains(UnityEngine.Vector3 worldPosition);
    }
}
