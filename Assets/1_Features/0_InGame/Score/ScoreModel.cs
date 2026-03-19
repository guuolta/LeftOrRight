using R3;

namespace InGame.Score
{
    /// <summary>
    /// スコアの状態を管理するモデル。
    /// </summary>
    public class ScoreModel
    {
        // 現在のスコア
        private readonly ReactiveProperty<int> _scoreProp = new(0);

        /// <summary>現在スコアの読み取り専用プロパティ</summary>
        public ReadOnlyReactiveProperty<int> ScoreProp => _scoreProp;

        /// <summary>
        /// スコアを1加算する。
        /// </summary>
        public void AddScore()
        {
            _scoreProp.Value++;
        }

        /// <summary>
        /// スコアをリセットする。
        /// </summary>
        public void Reset()
        {
            _scoreProp.Value = 0;
        }
    }
}
