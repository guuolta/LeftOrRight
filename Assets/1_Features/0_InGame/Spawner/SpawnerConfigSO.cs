using UnityEngine;
using InGame.Post;

namespace InGame.Spawner
{
    /// <summary>
    /// スポーナーの動作パラメータ設定。ScriptableObjectとして管理する。
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnerConfig", menuName = "InGame/SpawnerConfig")]
    public class SpawnerConfigSO : ScriptableObject
    {
        [Header("初期スポーン間隔（秒）")]
        [SerializeField] private float _initialInterval = 2.0f;

        [Header("最小スポーン間隔（秒）")]
        [SerializeField] private float _minInterval = 0.3f;

        [Header("時間経過による間隔減少量（秒/秒）")]
        [SerializeField] private float _intervalDecreasePerSec = 0.02f;

        [Header("吹き出しエリアのキャパシティ上限（個数）")]
        [SerializeField] private int _maxCapacity = 20;

        [Header("スポーン可能な投稿設定一覧")]
        [SerializeField] private PostItemConfigSO[] _postConfigs;

        /// <summary>初期スポーン間隔（秒）</summary>
        public float InitialInterval => _initialInterval;

        /// <summary>最小スポーン間隔（秒）</summary>
        public float MinInterval => _minInterval;

        /// <summary>時間経過による間隔減少量</summary>
        public float IntervalDecreasePerSec => _intervalDecreasePerSec;

        /// <summary>吹き出しエリアの最大キャパシティ</summary>
        public int MaxCapacity => _maxCapacity;

        /// <summary>スポーン対象の投稿設定一覧</summary>
        public PostItemConfigSO[] PostConfigs => _postConfigs;
    }
}
