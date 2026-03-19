using R3;

namespace InGame.GameFlow
{
    /// <summary>
    /// ゲーム全体の状態を管理するモデル。
    /// </summary>
    public class GameModel
    {
        // ゲームの現在状態
        private readonly ReactiveProperty<GameState> _gameStateProp = new(GameState.Idle);

        // ゲームオーバー時の原因通知
        private readonly Subject<GameOverReason> _onGameOver = new();

        // ゲーム開始からの経過時間（秒）
        private readonly ReactiveProperty<float> _elapsedTimeProp = new(0f);

        /// <summary>現在のゲーム状態</summary>
        public ReadOnlyReactiveProperty<GameState> GameStateProp => _gameStateProp;

        /// <summary>ゲームオーバー時に原因を通知するObservable</summary>
        public Observable<GameOverReason> OnGameOverObservable => _onGameOver;

        /// <summary>ゲーム開始からの経過時間</summary>
        public ReadOnlyReactiveProperty<float> ElapsedTimeProp => _elapsedTimeProp;

        /// <summary>
        /// ゲームを開始状態にする。
        /// </summary>
        public void StartGame()
        {
            _elapsedTimeProp.Value = 0f;
            _gameStateProp.Value = GameState.Playing;
        }

        /// <summary>
        /// 経過時間を更新する（毎フレーム呼び出す）。
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        public void UpdateElapsedTime(float deltaTime)
        {
            _elapsedTimeProp.Value += deltaTime;
        }

        /// <summary>
        /// ゲームオーバーにする。
        /// </summary>
        /// <param name="reason">ゲームオーバーの原因</param>
        public void TriggerGameOver(GameOverReason reason)
        {
            if (_gameStateProp.Value is not GameState.Playing)
            {
                return;
            }

            _gameStateProp.Value = GameState.GameOver;
            _onGameOver.OnNext(reason);
        }

        /// <summary>
        /// ゲームをリセットしてIdle状態に戻す。
        /// </summary>
        public void Reset()
        {
            _elapsedTimeProp.Value = 0f;
            _gameStateProp.Value = GameState.Idle;
        }
    }
}
