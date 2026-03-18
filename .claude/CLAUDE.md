Unityの操作は、UniCLIを用いて行う。
以下のリンクがUniCLIのリファレンスである。
https://github.com/yucchiy/UniCli/blob/main/README.md

# Typing2026 コーディング規約

Unity 6 (6000.3.9f1) + C# を使用したタイピングゲームプロジェクトの規約です。

---

## 1. 言語・コミュニケーション

- コード内の **コメントは日本語** で記述する
- 変数名・メソッド名・クラス名は **英語** で記述する
- ドキュメントファイル（MD等）は **日本語** で記述する

---

## 2. 命名規則

| 対象 | 規則 | 例 |
|------|------|----|
| クラス・構造体 | PascalCase | `TypingModel`, `FadeAnimation` |
| インターフェース | `I` + PascalCase | `ITypingPanel`, `IAttachable<T>` |
| メソッド | PascalCase | `PlayAsync()`, `SetQuestion()` |
| プロパティ | PascalCase | `QuestionRomajis`, `InputCount` |
| private フィールド | `_` + camelCase | `_model`, `_canvasGroup` |
| public フィールド（Serializable） | PascalCase | `DurationSec`, `Ease` |
| メソッド引数 | camelCase | `key`, `context`, `ct` |
| ローカル変数 | camelCase | `result`, `index` |
| 定数 | PascalCase | `MaxRetryCount` |
| enum 型・メンバー | PascalCase | `AnimationState.Playing` |
| CancellationToken 引数 | `ct` に統一 | `CancellationToken ct` |

---

## 3. フォルダ構成

```
Assets/
├── 0_Common/           # 全機能で共有する基盤システム
│   ├── Audio/          # 音声システム
│   ├── UI/             # UI共通コンポーネント
│   ├── Systems/        # DI・シーン管理・デバッグ
│   └── Editor/         # エディタ拡張
└── 1_Features/         # 機能別フォルダ
    └── 0_InGame/       # ゲーム内機能
        ├── UI/
        ├── Input/
        ├── Question/
        └── ...
```

### ルール
- フォルダ名の接頭辞番号（`0_`, `1_`）で読み込み順・重要度を示す
- 機能ドメインごとにフォルダを分割する（`Player/`, `Question/`, `Timer/` など）
- エディタ専用コードは必ず `Editor/` フォルダに配置する

---

## 4. アーキテクチャ

### MVP / MVVM パターン

```
Model      : ビジネスロジック・状態管理（UI非依存）
View       : UI表示のみ。インターフェース (IXxxView) で定義
Presenter  : ViewとModelをバインド。イベント購読・コマンド発行
Context    : Presenterへ渡す依存オブジェクトをまとめたクラス
```

**禁止事項:**
- Model が View を直接参照してはならない
- View がビジネスロジックを持ってはならない

### DI（依存性注入）

- **VContainer** を使用する
- DI登録は `LifetimeScope` の `Configure()` に集約する
- 依存関係はコンストラクタインジェクションを優先する

```csharp
// 良い例
builder.Register<PlayerFacade>(Lifetime.Singleton).AsImplementedInterfaces();
builder.RegisterEntryPoint<KeyboardReader>();
```

### インターフェース駆動設計

- 外部から利用されるクラスは必ずインターフェースを定義する
- インターフェース名は `I` + 役割名（例: `IUIAnimation`, `IAttachable<TContext>`）

---

## 5. 非同期処理

- **UniTask** (Cysharp.Threading.Tasks) を使用する（`Task` は使わない）
- 非同期メソッド名は `Async` サフィックスをつける
- `CancellationToken` は必ず引数で受け取り、`ct` という名前で統一する

```csharp
// 良い例
public async UniTask PlayAsync(CancellationToken ct)
{
    await DoSomethingAsync(ct);
}

// 悪い例（Taskを使用・ctなし）
public async Task Play()
{
    await DoSomething();
}
```

---

## 6. リアクティブプログラミング

- **R3**（Reactive Extensions）を使用する
- イベント公開は `Observable<T>` プロパティとして公開する
- 内部の `Subject<T>` は private フィールドとして保持する
- 購読解除は `RegisterTo(ct)` または `disposable.Dispose()` で確実に行う
- `ReactiveProperty`の変数名は、`Prop`サフィックスをつける
- `ReactiveProperty`を外部に公開する場合は、`ReadOnlyReactiveProperty`を使用する

```csharp
// 良い例
private readonly ReactiveProperty<int> _numProp = new(0);
public ReadOnlyReactiveProperty<int> NumProp => _numProp;

private readonly Subject<Unit> _onCorrect = new();
public Observable<Unit> OnCorrectObservable => _onCorrect;

// 購読
_model.OnCorrectObservable
    .Subscribe(_ => OnCorrect())
    .RegisterTo(ct);
```

---

## 7. コメント規則

### XMLドキュメントコメント（public メンバーに必須）

```csharp
/// <summary>
/// フェードアニメーションを再生する。
/// </summary>
/// <param name="ct">キャンセルトークン</param>
/// <param name="useInitial">初期値から開始するか</param>
public async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
```

### インラインコメント

```csharp
// 問題切り替え時のイベント登録
_model.OnQuestionChanged
    .Subscribe(SetQuestion)
    .RegisterTo(ct);
```

---

## 8. アクセス修飾子

- アクセス修飾子は **常に明示する**（省略禁止）
- フィールドは原則 `private` にし、必要なら `public` プロパティで公開する
- `[SerializeField]` を付けた場合は `private` フィールドにする

```csharp
// 良い例
[SerializeField] private Button _submitButton;
public bool IsActive { get; private set; }

// 悪い例（修飾子省略・publicフィールド）
Button submitButton;
public bool isActive;
```

---

## 9. 使用ライブラリ一覧

| ライブラリ | 用途 |
|-----------|------|
| VContainer | DIコンテナ |
| UniTask (Cysharp.Threading.Tasks) | 非同期処理 |
| DOTween (DG.Tweening) | アニメーション |
| R3 (Reactive Extensions) | リアクティブプログラミング |
| ZString (Cysharp.Text) | 高速文字列処理（GC削減） |
| TextMesh Pro | UIテキスト |

---

## 10. その他のルール

- `string` の頻繁な結合には `ZString` を使用し GC を削減する
- ScriptableObject はパラメータ定義に活用する（`UIAnimationParamSO` など）
- `Debug.Log` は開発用途のみ。リリース前に除去または `#if UNITY_EDITOR` で囲む
- エラーは `Debug.LogError` で報告し、例外はできる限り上位に伝播させる
- `null` チェックは `is null` / `is not null` 演算子を使用する（`== null` 非推奨）
---

## 11. ブランチ戦略

- **新機能・修正ブランチ** のプルリクエスト先は必ず `develop` にする
- `main` へのプルリクエストは `develop` からのみ行う（直接マージ禁止）
