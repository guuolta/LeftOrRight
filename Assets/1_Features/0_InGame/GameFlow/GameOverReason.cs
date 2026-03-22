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

        /// <summary>有効期限切れ: ポストが5秒以内に処理されなかった</summary>
        TimeOver,

        /// <summary>吹き出し外ドロップ: 吹き出しエリア外に投稿を落とした</summary>
        OutsideDrop,
    }
}
