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
        [Header("スポーン間隔（秒）")]
        [SerializeField] private float _spawnInterval = 2.0f;

        [Header("初期スポーン数（1ティックあたりの生成数）")]
        [SerializeField] private int _initialSpawnCount = 1;

        [Header("スポーン数の上限")]
        [SerializeField] private int _maxSpawnCount = 5;

        [Header("スポーン数が+1される間隔（秒）")]
        [SerializeField] private float _spawnCountIncreaseInterval = 10f;

        [Header("吹き出しエリアのキャパシティ上限（個数）")]
        [SerializeField] private int _maxCapacity = 20;

        [Header("スポーン可能な投稿設定一覧")]
        [SerializeField] private PostItemConfigSO[] _postConfigs;

        /// <summary>スポーン間隔（秒）</summary>
        public float SpawnInterval => _spawnInterval;

        /// <summary>初期スポーン数</summary>
        public int InitialSpawnCount => _initialSpawnCount;

        /// <summary>スポーン数の上限</summary>
        public int MaxSpawnCount => _maxSpawnCount;

        /// <summary>スポーン数が+1される間隔（秒）</summary>
        public float SpawnCountIncreaseInterval => _spawnCountIncreaseInterval;

        /// <summary>吹き出しエリアの最大キャパシティ</summary>
        public int MaxCapacity => _maxCapacity;

        /// <summary>スポーン対象の投稿設定一覧</summary>
        public PostItemConfigSO[] PostConfigs => _postConfigs;
    }
}
