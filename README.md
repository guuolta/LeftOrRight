# RightOrLeft

表垢・裏垢をテーマにした、SNS投稿ネタ仕分けアクションゲームです。

---

## ゲーム概要

左下の頭アイコンから「投稿ネタ」が次々と出てきます。
それが **表に出せるネタ（Public）** なのか **裏に隠すネタ（Private）** なのかを素早く判断して、
左右のスマホに振り分けてください。

投稿ネタには **有効期限（10秒）** があり、時間が経つほど激しく揺れ始めます。
スコアが上がるほどスポーン間隔が短くなり、同時に出てくる数も増えていきます。

### 操作方法

| 操作 | 効果 |
|------|------|
| **左クリック / タップ** でドラッグ | 投稿ネタを掴む（種別問わず） |
| ドラッグ先でリリース | 仕分け完了 |

- **左スマホ（表垢）** → 表ネタ（Public）を置く
- **右スマホ（裏垢）** → 裏ネタ（Private）を置く
- スマホに少しでも触れれば置いた判定になる
- 吹き出しエリア内に戻せば掴み直せる

### ゲームオーバー条件

| 条件 | ヘッダー | 内容 |
|------|---------|------|
| 間違ったスマホにドロップ | 炎上！！ | 投稿を誤爆してしまった... |
| 吹き出しがキャパシティ超過 | 脳内パンク！ | 処理しきれなくなってしまった... |
| 有効期限切れ（放置） | 記憶喪失！ | アイデアをすっかり忘れてしまった... |
| 吹き出しエリア外にドロップ | ど忘れ！ | アイデアが頭から抜けてしまった... |

---

## 難易度

### スポーン間隔

スコアに応じてスポーン間隔が短くなります。

| スコア | スポーン間隔 |
|--------|------------|
| 0〜9   | SpawnerConfig の初期値 |
| 10〜39 | 2.0 秒 |
| 40〜49 | 1.95 秒 |
| 50〜59 | 1.85 秒 |
| 60〜69 | 1.8 秒 |
| 70〜79 | 1.75 秒 |
| 80〜89 | 1.7 秒 |
| 90〜99 | 1.67 秒 |
| 100〜109 | 1.64 秒 |
| 110〜119 | 1.61 秒 |
| 120〜129 | 1.59 秒 |
| 130〜139 | 1.56 秒 |
| 140〜149 | 1.53 秒 |
| 150 以上 | 1.5 秒（最速） |

### スポーン数

| スコア | 1ティックあたりのスポーン数 |
|--------|--------------------------|
| 0〜19  | 1個 |
| 20〜79 | 2個 |
| 80 以上 | 3個 |

---

## 画面構成

```
┌─────────────────────────────┐
│  [表垢スマホ]   [裏垢スマホ] │  ← PublicPhone / PrivatePhone
│                             │
│       [吹き出しエリア]       │  ← 投稿ネタがスポーンする場所
│  ♥ スコア表示               │
│  [頭アイコン]               │  ← SpawnPoint（左下）
└─────────────────────────────┘
```

スマホ画面には仕分けた投稿がスタック表示されます（ScrollViewで新着が上に積まれ、下が少し欠けて見えます）。

---

## 開発環境

| 項目 | バージョン |
|------|-----------|
| Unity | 6000.3.9f1 (Unity 6) |
| レンダリング | Universal Render Pipeline (URP) |
| 言語 | C# |

### 使用ライブラリ

| ライブラリ | 用途 |
|-----------|------|
| VContainer | DIコンテナ |
| UniTask | 非同期処理 |
| DOTween | アニメーション |
| R3 | リアクティブプログラミング |
| ZString | 高速文字列処理（GC削減） |
| TextMesh Pro | UIテキスト |
| Unity InputSystem | 入力処理（マウス・タッチ対応） |
| NuGetForUnity | NuGetパッケージ管理 |
| UniCLI | Unityエディタ操作CLI |

---

## プロジェクト構成

```
Assets/
├── 0_Common/               # 共通アセット・ユーティリティ
│   ├── Editor/             # エディタ拡張ツール
│   ├── Fonts/              # フォントアセット
│   ├── Sprites/            # 共通スプライト・TitleLogo
│   └── UI/                 # 共通UIコンポーネント
└── 1_Features/
    └── 0_InGame/           # ゲーム本体
        ├── GameFlow/       # ゲーム状態管理（Model・Presenter）
        ├── Input/          # 入力ハンドラー（マウス・タッチ統合）
        ├── Post/           # 投稿ネタ（Model・View・Config）
        │   └── Prefabs/    # PostItem.prefab
        ├── Score/          # スコア管理
        ├── SortArea/       # 仕分けエリア・スマホScrollView表示
        ├── Spawner/        # 投稿スポーナー
        ├── ThoughtBubble/  # 吹き出しエリア
        └── UI/             # タイトル・カウントダウン・ゲームオーバー画面

Assets/Resources/           # ScriptableObject
├── SpawnerConfig.asset     # スポーン設定
├── PublicPost.asset        # 表ネタ設定（ランダムラベル対応）
└── PrivatePost.asset       # 裏ネタ設定（ランダムラベル対応）
```

---

## アーキテクチャ

MVP パターンで構成しています。

```
Model      ビジネスロジック・状態管理（UI非依存）
View       UI表示のみ。IXxxView インターフェースで定義
Presenter  ViewとModelをバインド。ゲームループを管理
```

主要クラス：

| クラス | 役割 |
|-------|------|
| `GameModel` | ゲーム状態（Idle / Playing / GameOver）管理 |
| `ScoreModel` | スコアのReactiveProperty管理 |
| `InGamePresenter` | タイトル→カウントダウン→ゲームループ全体を制御 |
| `TitleView` | タイトル画面表示・左クリック/タップ待機 |
| `CountdownView` | 3→2→1カウントダウンアニメーション |
| `GameOverView` | ゲームオーバー画面表示（ヘッダー/内容/スコア）・リトライ処理 |
| `PostItemView` | 投稿ネタの表示・ドラッグ制御・有効期限シェイクアニメーション |
| `PostItemConfigSO` | 投稿ネタの設定（ラベル複数候補からランダム選択） |
| `PostInputHandler` | 左クリック/タップによるドラッグ＆ドロップ（マウス・タッチ統合） |
| `PostSpawner` | 非同期スポーンループ・スコア連動難易度 |
| `SortAreaView` | 仕分けエリアのAABB判定・スマホスタック管理 |
| `PhoneStackView` | スマホへの投稿ScrollView表示（新着が上・下端クリップ） |
| `ThoughtBubbleView` | 吹き出しエリアの色変化・揺れ警告 |

---

## 初回セットアップ

### 1. リポジトリをクローン

```bash
git clone https://github.com/guuolta/LeftOrRight.git
cd LeftOrRight
```

### 2. Unity Hub でプロジェクトを開く

Unity 6000.3.9f1 で開いてください。
パッケージの解決（UniTask・VContainer・R3等）は自動的に行われます。

### 3. DOTween セットアップ

`Tools → DOTween Utility Panel` を開き、**Setup DOTween** を実行してください。
`Assets/Plugins/Demigiant/DOTween/Modules/DOTween.Modules.asmdef` が生成されます。

### 4. シーンを開く

`Assets/Scenes/InGame.unity` を開けばすぐにプレイできます。
全コンポーネントの参照配線・ScriptableObjectは設定済みです。

---

## ゲームパラメータ

`Assets/Resources/SpawnerConfig.asset` で調整できます。

| パラメータ | 説明 |
|-----------|------|
| Spawn Interval | 基本スポーン間隔（スコア0〜9で適用） |
| Initial Spawn Count | 1ティックあたりの初期スポーン数 |
| Max Spawn Count | スポーン数の上限 |
| Max Capacity | 吹き出しエリアのキャパシティ上限 |

投稿ネタの種類・色・ラベル候補は `Assets/Resources/PublicPost.asset` / `PrivatePost.asset` で変更できます。

---

## 未対応・今後のTODO

### アセット

- [ ] リリース用フォント（Noto Sans JP など商用可フォントへの差し替え）
  　現在はヒラギノフォント（macOS専用ライセンス）を使用
- [ ] BGM・SE の実装と音声ファイルの用意

### 機能

- [ ] スコアランキング・ハイスコア保存
- [ ] ポーズ機能

---

## ブランチ戦略

```
feature/* → develop → main
```

- 新機能・修正は `feature/〇〇` ブランチで開発
- PRのベースは `develop`
- `main` へは `develop` からのみマージ

---

## ライセンス

本プロジェクトは個人開発のゲームです。
使用ライブラリのライセンスはそれぞれのリポジトリを参照してください。

> **注意**: `Assets/0_Common/Fonts/HiraginoSans-W4.ttc` はmacOSシステムフォントです。
> リリース時には必ず商用利用可能なフォントに差し替えてください。
