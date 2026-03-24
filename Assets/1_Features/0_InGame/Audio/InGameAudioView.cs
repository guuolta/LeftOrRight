using UnityEngine;

namespace InGame.Audio
{
    /// <summary>
    /// インゲームの音声再生を担当するViewコンポーネント。
    /// BGM（ループ）とSE（ワンショット）を管理する。
    /// </summary>
    public class InGameAudioView : MonoBehaviour
    {
        [Header("BGM用AudioSource（ループ設定済み）")]
        [SerializeField] private AudioSource _bgmSource;

        [Header("SE用AudioSource")]
        [SerializeField] private AudioSource _seSource;

        [Header("タイトルBGMクリップ")]
        [SerializeField] private AudioClip _titleBgmClip;

        [Header("インゲームBGMクリップ")]
        [SerializeField] private AudioClip _bgmClip;

        [Header("ボタンクリック時のSE")]
        [SerializeField] private AudioClip _buttonSeClip;

        [Header("正しいスマホへドロップ時のSE")]
        [SerializeField] private AudioClip _postSeClip;

        /// <summary>
        /// タイトルBGMを再生する。
        /// </summary>
        public void PlayTitleBGM()
        {
            if (_bgmSource is null || _titleBgmClip is null)
            {
                return;
            }

            _bgmSource.clip = _titleBgmClip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        /// <summary>
        /// インゲームBGMを再生する。すでに再生中の場合は最初から再生し直す。
        /// </summary>
        public void PlayBGM()
        {
            if (_bgmSource is null || _bgmClip is null)
            {
                return;
            }

            _bgmSource.clip = _bgmClip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        /// <summary>
        /// BGMを停止する。
        /// </summary>
        public void StopBGM()
        {
            _bgmSource?.Stop();
        }

        /// <summary>
        /// ボタンクリック時のSEを再生する。
        /// </summary>
        public void PlayButtonSE()
        {
            if (_seSource is null || _buttonSeClip is null)
            {
                return;
            }

            _seSource.PlayOneShot(_buttonSeClip);
        }

        /// <summary>
        /// 正しいスマホへドロップ成功時のSEを再生する。
        /// </summary>
        public void PlayPostSE()
        {
            if (_seSource is null || _postSeClip is null)
            {
                return;
            }

            _seSource.PlayOneShot(_postSeClip);
        }
    }
}
