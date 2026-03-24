using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Text;

namespace InGame.Score
{
    /// <summary>
    /// スコアの表示を担当するViewコンポーネント。
    /// ハートアイコン画像の横に数値を表示する。
    /// </summary>
    public class ScoreView : MonoBehaviour
    {
        [Header("ハートアイコン画像")]
        [SerializeField] private Image _heartImage;

        [Header("スコア数値テキスト")]
        [SerializeField] private TextMeshProUGUI _scoreText;

        private void Awake()
        {
            // 未アサイン時はHeartImageという名前の子から取得
            if (_heartImage == null)
            {
                _heartImage = transform.Find("HeartImage")?.GetComponent<Image>();
            }

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

            // 数値のみ表示（ハートアイコンは隣のImageで表現）
            using var sb = ZString.CreateStringBuilder();
            sb.Append(score);
            _scoreText.SetText(sb);
        }
    }
}
