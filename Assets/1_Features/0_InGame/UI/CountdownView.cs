using System.Threading;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Cysharp.Text;

namespace InGame.UI
{
    /// <summary>
    /// 3→2→1のカウントダウンをアニメーションで表示するViewコンポーネント。
    /// </summary>
    public class CountdownView : MonoBehaviour
    {
        [Header("カウント表示テキスト")]
        [SerializeField] private TextMeshProUGUI _countText;

        [Header("1カウントのアニメーション時間（秒）")]
        [SerializeField] private float _countDuration = 0.8f;

        /// <summary>
        /// 3→2→1のカウントダウンアニメーションを表示する。
        /// 完了後に自動的に非表示にする。
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            for (var count = 3; count >= 1; count--)
            {
                using var sb = ZString.CreateStringBuilder();
                sb.Append(count);
                _countText.SetText(sb);

                // スケール0→1（OutBackでポンと飛び出す）
                _countText.transform.localScale = Vector3.zero;
                await _countText.transform
                    .DOScale(1f, _countDuration * 0.5f)
                    .SetEase(Ease.OutBack)
                    .ToUniTask(cancellationToken: ct);

                // 少し表示したまま待つ
                await UniTask.Delay((int)(_countDuration * 200f), cancellationToken: ct);

                // スケール1→0（InBackで引っ込む）
                await _countText.transform
                    .DOScale(0f, _countDuration * 0.3f)
                    .SetEase(Ease.InBack)
                    .ToUniTask(cancellationToken: ct);
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _countText?.transform.DOKill();
        }
    }
}
