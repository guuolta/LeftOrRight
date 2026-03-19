namespace InGame.GameFlow
{
    /// <summary>
    /// ゲームの進行状態。
    /// </summary>
    public enum GameState
    {
        /// <summary>待機中（タイトル表示）</summary>
        Idle,

        /// <summary>プレイ中</summary>
        Playing,

        /// <summary>ゲームオーバー</summary>
        GameOver,
    }
}
