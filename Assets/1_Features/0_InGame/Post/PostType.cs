namespace InGame.Post
{
    /// <summary>
    /// 投稿ネタの種別（表垢向け / 裏垢向け）。
    /// </summary>
    public enum PostType
    {
        /// <summary>表垢（本アカウント）向けの投稿。左クリックで仕分け。</summary>
        Public,

        /// <summary>裏垢（愚痴アカウント）向けの投稿。右クリックで仕分け。</summary>
        Private,
    }
}
