using UnityEngine;

namespace InGame.Post
{
    /// <summary>
    /// 投稿ネタ1種類の見た目・種別設定。ScriptableObjectとして管理する。
    /// </summary>
    [CreateAssetMenu(fileName = "PostItemConfig", menuName = "InGame/PostItemConfig")]
    public class PostItemConfigSO : ScriptableObject
    {
        [Header("種別")] [SerializeField] private PostType _postType;

        [Header("見た目")] [SerializeField] private Color _color = Color.white;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _label;

        /// <summary>投稿の種別</summary>
        public PostType PostType => _postType;

        /// <summary>背景カラー</summary>
        public Color Color => _color;

        /// <summary>アイコンスプライト</summary>
        public Sprite Icon => _icon;

        /// <summary>表示ラベル文字列</summary>
        public string Label => _label;
    }
}