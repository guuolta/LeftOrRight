using UnityEngine;
using TMPro;
using Cysharp.Text;

namespace InGame.Score
{
    /// <summary>
    /// スコアの表示を担当するViewコンポーネント。
    /// </summary>
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;

        private void Awake()
        {
            // 未アサイン時は子オブジェクトのTextMeshProUGUIを自動取得
            if (_scoreText == null)
            {
                _scoreText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// スコア表示を更新する。
        /// </summary>
        /// <param name="score">表示するスコア値</param>
        public void UpdateScore(int score)
        {
            if (_scoreText is null)
            {
                return;
            }

            // ZStringでGCを削減しながら文字列を構築
            using var sb = ZString.CreateStringBuilder();
            sb.Append("SCORE: ");
            sb.Append(score);
            _scoreText.SetText(sb);
        }
    }
}
