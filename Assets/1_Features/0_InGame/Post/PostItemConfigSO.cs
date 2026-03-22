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
        [SerializeField] private Color _textColor = Color.black;
        [SerializeField] private Sprite _icon;

        [Header("ランダム表示するラベル候補（スポーン時に1つ選ばれる）")]
        [SerializeField, TextArea] private string[] _labels;

        /// <summary>投稿の種別</summary>
        public PostType PostType => _postType;

        /// <summary>背景カラー</summary>
        public Color Color => _color;

        /// <summary>テキストカラー</summary>
        public Color TextColor => _textColor;

        /// <summary>アイコンスプライト</summary>
        public Sprite Icon => _icon;

        /// <summary>ラベル候補からランダムに1つ選んで返す。候補が空の場合は空文字を返す。</summary>
        public string Label => _labels is { Length: > 0 }
            ? _labels[UnityEngine.Random.Range(0, _labels.Length)]
            : string.Empty;
    }
}