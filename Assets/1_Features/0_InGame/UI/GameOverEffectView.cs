using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace InGame.UI
{
    /// <summary>
    /// ゲームオーバー時の爆発エフェクトを再生するViewコンポーネント。
    /// 中央→左下→右上→右下→左上の順に5回連鎖爆発し、爆発ごとにSEを鳴らす。
    /// </summary>
    public class GameOverEffectView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _explosionClip;

        /// <summary>連鎖爆発の位置（Canvasローカル座標）。中央→左下→右上→右下→左上の順。</summary>
        private static readonly Vector3[] ChainPositions =
        {
            new(0f,     0f,    0f),  // 中央
            new(-600f, -300f,  0f),  // 左下
            new( 600f,  300f,  0f),  // 右上
            new( 600f, -300f,  0f),  // 右下
            new(-600f,  300f,  0f),  // 左上
        };

        /// <summary>連鎖爆発の間隔（秒）</summary>
        private const float ChainInterval = 0.5f;

        /// <summary>
        /// 連鎖爆発エフェクトを再生し、完了するまで待機する。
        /// 爆発ごとにSEを1回再生する。
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            // 5回連鎖爆発
            for (var i = 0; i < ChainPositions.Length; i++)
            {
                if (_particleSystem is not null)
                {
                    _particleSystem.transform.localPosition = ChainPositions[i];
                    _particleSystem.Play();
                }

                // 爆発SEを1回再生
                if (_audioSource is not null && _explosionClip is not null)
                {
                    _audioSource.PlayOneShot(_explosionClip);
                }

                await UniTask.WaitForSeconds(ChainInterval, cancellationToken: ct);
            }

            // 全パーティクルが消えるまで待機
            await UniTask.WaitWhile(
                () => _particleSystem != null && _particleSystem.IsAlive(true),
                cancellationToken: ct
            );

            gameObject.SetActive(false);
        }
    }
}
