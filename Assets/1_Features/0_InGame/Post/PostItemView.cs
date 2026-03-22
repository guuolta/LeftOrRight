using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using R3;

namespace InGame.Post
{
    /// <summary>
    /// 投稿ネタの表示を担当するViewコンポーネント。
    /// ビジネスロジックは持たず、見た目の更新のみを行う。
    /// </summary>
    public class PostItemView : MonoBehaviour, IPostItem
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private PostType _postType;
        private bool _isDragging;

        // 有効期限タイマー
        private readonly Subject<Unit> _onExpired = new();
        private CancellationTokenSource _expiryCts;

        // シェイクアニメーション用（有効期限とは独立してキャンセル可能）
        private CancellationTokenSource _shakeCts;
        private float _expiryDuration;
        private float _expiryStartTime;

        /// <inheritdoc/>
        public PostType PostType => _postType;

        /// <inheritdoc/>
        public Vector3 Position => transform.position;

        /// <summary>MoveToPosition で指定した最終目標ワールド座標。重なり判定に使用。</summary>
        public Vector3 TargetPosition { get; private set; }

        /// <summary>MoveToPosition のアニメーション中は true。分離処理のスキップ判定に使用。</summary>
        public bool IsAnimating { get; private set; }

        /// <inheritdoc/>
        public bool IsDragging => _isDragging;

        /// <summary>有効期限切れ時に通知するObservable。</summary>
        public Observable<Unit> OnExpiredObservable => _onExpired;

        /// <summary>
        /// 投稿ネタの設定を適用して初期化する。
        /// </summary>
        /// <param name="config">投稿ネタの設定</param>
        public void Initialize(PostItemConfigSO config)
        {
            _postType = config.PostType;

            // サイズを265×71に明示的に固定（親のレイアウト影響を防ぐ）
            var rt = GetComponent<RectTransform>();
            if (rt is not null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(265f, 112f);

            // 背景画像のサイズも明示的に合わせる
            if (_backgroundImage is not null)
            {
                var bgRt = _backgroundImage.GetComponent<RectTransform>();
                if (bgRt is not null)
                {
                    bgRt.sizeDelta = new Vector2(265f, 112f);
                }
            }
            }

            // 背景色の設定
            if (_backgroundImage is not null)
            {
                _backgroundImage.color = config.Color;
            }

            // アイコンの設定
            if (_iconImage is not null && config.Icon is not null)
            {
                _iconImage.sprite = config.Icon;
                _iconImage.gameObject.SetActive(true);
            }
            else if (_iconImage is not null)
            {
                _iconImage.gameObject.SetActive(false);
            }

            // ラベルの設定
            if (_labelText is not null)
            {
                _labelText.text = config.Label;
                _labelText.color = config.TextColor;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 指定のワールド座標へDOTweenでアニメーション移動する。
        /// アニメーション中はKinematicに切り替え、完了後にDynamicへ戻す。
        /// </summary>
        /// <param name="target">移動先のワールド座標</param>
        /// <param name="duration">移動時間（秒）</param>
        public void MoveToPosition(Vector3 target, float duration = 0.5f)
        {
            // 目標位置を記録（重なり判定・分離処理用）
            TargetPosition = target;
            IsAnimating = true;

            // 競合する既存アニメーションをキャンセル
            transform.DOKill();

            // 常にKinematicで動作する（Dynamicに戻さない）
            if (_rigidbody2D is not null)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
            }

            transform.DOMove(target, duration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => { IsAnimating = false; });
        }

        /// <summary>
        /// 親オブジェクトのローカル座標へDOTweenでアニメーション移動する。
        /// スマホへのスタック時に使用する（ワールド座標系に依存しない）。
        /// </summary>
        /// <param name="localTarget">移動先のローカル座標</param>
        /// <param name="duration">移動時間（秒）</param>
        public void MoveToLocalPosition(Vector3 localTarget, float duration = 0.25f)
        {
            // 競合する既存アニメーションをキャンセル
            transform.DOKill();

            // スタック後はKinematicのまま維持（Dynamicに戻さない）
            if (_rigidbody2D is not null)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
            }

            transform.DOLocalMove(localTarget, duration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// スマホへスタック時に背景をフェードアウトして透明にする。
        /// </summary>
        public void HideBackground()
        {
            if (_backgroundImage is not null)
            {
                var c = _backgroundImage.color;
                _backgroundImage.color = new Color(c.r, c.g, c.b, 0f);
            }
        }

        /// <inheritdoc/>
        public void BeginDrag()
        {
            _isDragging = true;

            // ドラッグ開始時にシェイクを停止して回転をリセット
            StopShake();

            // ドラッグ中は物理演算を停止
            if (_rigidbody2D is not null)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
            }
        }

        /// <inheritdoc/>
        public void UpdateDragPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        /// <inheritdoc/>
        public void EndDrag()
        {
            _isDragging = false;
            // Kinematicのまま維持（物理演算は使わない）
        }

        /// <summary>
        /// 有効期限タイマーを開始する。<paramref name="duration"/>秒後にOnExpiredObservableを発火。
        /// </summary>
        /// <param name="duration">有効期限（秒）</param>
        public void StartExpiry(float duration = 10f)
        {
            CancelExpiry();
            _expiryCts = new CancellationTokenSource();

            // テキストを徐々に透明にするアニメーション（duration秒かけてα=0へ）
            if (_labelText is not null)
            {
                _labelText.DOKill();
                _labelText.DOFade(0f, duration).SetEase(Ease.Linear);
            }

            _expiryDuration = duration;
            _expiryStartTime = Time.time;

            ExpireAfterAsync(duration, _expiryCts.Token).Forget();
            StartShake();
        }

        /// <summary>
        /// 有効期限タイマーをキャンセルし、テキストの透明度を元に戻す（正常ソート時に呼ぶ）。
        /// </summary>
        public void CancelExpiry()
        {
            _expiryCts?.Cancel();
            _expiryCts?.Dispose();
            _expiryCts = null;

            // テキストの透明度を即座に元に戻す
            if (_labelText is not null)
            {
                _labelText.DOKill();
                _labelText.color = new Color(_labelText.color.r, _labelText.color.g, _labelText.color.b, 1f);
                _labelText.ForceMeshUpdate();
            }

            // シェイクを停止して回転をリセット
            StopShake();
        }

        /// <summary>
        /// 吹き出し内に戻したときにシェイクを再開する。
        /// 有効期限タイマーが動いていない場合は何もしない。
        /// </summary>
        public void ResumeShake()
        {
            if (_expiryCts is null) return;
            StartShake();
        }

        /// <summary>
        /// シェイクアニメーションを開始する（_expiryCts にリンクした独自CTSで管理）。
        /// </summary>
        private void StartShake()
        {
            _shakeCts?.Cancel();
            _shakeCts?.Dispose();
            _shakeCts = CancellationTokenSource.CreateLinkedTokenSource(_expiryCts.Token);
            ShakeAsync(_shakeCts.Token).Forget();
        }

        /// <summary>
        /// シェイクアニメーションを停止して回転をリセットする。
        /// </summary>
        private void StopShake()
        {
            _shakeCts?.Cancel();
            _shakeCts?.Dispose();
            _shakeCts = null;
            DOTween.Kill(gameObject, complete: false);
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 期限が近づくにつれ揺れが強くなるシェイクアニメーション。
        /// 経過時間を Time.time で計算するため、ドラッグ中も時間は進む。
        /// </summary>
        private async UniTaskVoid ShakeAsync(CancellationToken ct)
        {
            const float MaxStrength = 15f;

            while (!ct.IsCancellationRequested)
            {
                var elapsed = Time.time - _expiryStartTime;
                var progress = Mathf.Clamp01(elapsed / _expiryDuration);
                var strength = MaxStrength * progress * progress; // 2乗で加速

                // 強度に応じてインターバルを短くする（後半は高頻度で揺れる）
                var interval = Mathf.Lerp(0.8f, 0.3f, progress);

                if (strength > 0.5f)
                {
                    transform.DOKill(false);
                    transform.localRotation = Quaternion.identity;
                    transform.DOPunchRotation(new Vector3(0f, 0f, strength), interval, vibrato: 3, elasticity: 0.3f)
                        .SetTarget(gameObject);
                }

                await UniTask.WaitForSeconds(interval, cancellationToken: ct);
            }
        }

        /// <summary>
        /// 指定秒後にOnExpiredを発火する非同期処理。
        /// </summary>
        private async UniTaskVoid ExpireAfterAsync(float duration, CancellationToken ct)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: ct);
            if (!ct.IsCancellationRequested)
            {
                _onExpired.OnNext(Unit.Default);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CancelExpiry();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            CancelExpiry();
            _shakeCts?.Cancel();
            _shakeCts?.Dispose();
            _onExpired.Dispose();
        }
    }
}
