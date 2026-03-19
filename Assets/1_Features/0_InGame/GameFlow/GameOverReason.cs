namespace InGame.GameFlow
{
    /// <summary>
    /// ゲームオーバーの原因。
    /// </summary>
    public enum GameOverReason
    {
        /// <summary>操作ミス（誤爆）: 間違ったクリックで掴んだ、または間違ったエリアにドロップ</summary>
        WrongOperation,

        /// <summary>キャパシティオーバー: 吹き出しエリアが溢れた</summary>
        CapacityOver,
    }
}
