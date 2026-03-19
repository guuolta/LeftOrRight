# LeftOrRight（仮）

表垢・裏垢をテーマにした、SNS投稿ネタ仕分けアクションゲームです。

---

## ゲーム概要

左下の頭アイコンから「投稿ネタ」が次々と出てきます。
それが **表に出せるネタ（Public）** なのか **裏に隠すネタ（Private）** なのかを素早く判断して、
左右のスマホに振り分けてください。
時間が経つほど投稿が増えていき、頭のキャパシティがパンクするとゲームオーバー。
誤った方向に投稿しても即ゲームオーバーです。

### 操作方法

| 操作 | 効果 |
|------|------|
| **左クリック** でドラッグ | 表ネタ（Public）として掴む → 左上のスマホへ |
| **右クリック** でドラッグ | 裏ネタ（Private）として掴む → 右上のスマホへ |
| ドラッグ先でリリース | 仕分け完了 |

### ゲームオーバー条件

| 条件 | メッセージ |
|------|-----------|
| 表ネタを右クリック or 裏ネタを左クリック | 炎上！！投稿を誤爆しました... |
| 吹き出しエリアがキャパシティ上限（20個）を超える | 脳内パンク！処理しきれなくなりました... |

---

## 画面構成

```
┌─────────────────────────────┐
│  [表垢スマホ]   [裏垢スマホ] │  ← PublicPhone / PrivatePhone
│                             │
│       [吹き出しエリア]       │  ← 投稿ネタがスポーンする場所
│                             │
│  [頭アイコン]               │  ← SpawnPoint（左下）
└─────────────────────────────┘
         スコア表示（上部）
```

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
| ObservableCollections | R3連携コレクション |
| ZString | 高速文字列処理（GC削減） |
| TextMesh Pro | UIテキスト |
| Unity InputSystem | 入力処理 |
| NuGetForUnity | NuGetパッケージ管理 |
| UniCLI | Unityエディタ操作CLI |

---

## プロジェクト構成

```
Assets/
├── 0_Common/               # 共通アセット・ユーティリティ
│   ├── Editor/             # エディタ拡張ツール（後述）
│   ├── Fonts/              # フォントアセット
│   └── Sprites/            # 共通スプライト
└── 1_Features/
    └── 0_InGame/           # ゲーム本体
        ├── GameFlow/       # ゲーム状態管理（Model・Presenter）
        ├── Input/          # 入力ハンドラー
        ├── Post/           # 投稿ネタ（Model・View・Config）
        │   ├── Prefabs/    # PostItem.prefab
        │   └── Icons/      # アイコン画像（手動配置が必要）
        ├── Score/          # スコア管理
        ├── SortArea/       # 仕分けエリア
        ├── Spawner/        # 投稿スポーナー
        ├── ThoughtBubble/  # 吹き出しエリア
        └── UI/             # タイトル・ゲームオーバー画面

Packages/
└── manifest.json           # パッケージ依存定義

Assets/Resources/           # ScriptableObject（自動生成）
├── SpawnerConfig.asset     # スポーン設定
├── CafePost.asset          # 表ネタ設定 ×3
├── LikePost.asset
├── SunnyPost.asset
├── AngerPost.asset         # 裏ネタ設定 ×3
├── SkullPost.asset
└── TrashPost.asset
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
| `GameModel` | ゲーム状態（Idle / Playing / GameOver）・経過時間管理 |
| `ScoreModel` | スコアのReactiveProperty管理 |
| `PostItemView` | 投稿ネタの表示・ドラッグ制御（Rigidbody2D連携） |
| `PostInputHandler` | 左/右クリックドラッグ＆ドロップ・誤爆判定 |
| `PostSpawner` | 非同期スポーンループ・時間加速 |
| `SortAreaView` | 仕分けエリアのヒット判定・ハイライト |
| `InGamePresenter` | 全体を束ねるゲームループ制御 |

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
全コンポーネントの参照配線・ScriptableObjectは自動生成済みです。

---

## エディタツール（Tools/InGame Setup）

`Tools → InGame Setup` メニューから以下の操作が行えます。

| メニュー | 内容 |
|---------|------|
| **Wire Up Scene References** | シーン内の全コンポーネント参照を自動配線し直す |
| **Create All ScriptableObjects** | SpawnerConfig・PostItemConfig×6を再生成 |
| **Create PostItem Prefab** | PostItemプレハブを再生成 |
| **Create Japanese Font Asset** | ヒラギノSDF フォントアセットを生成（macOS必須） |
| **Create All Sprites** | 基本スプライト（角丸矩形・楕円・円）を生成 |
| **Apply Fonts and Sprites** | フォント・スプライトをシーンに適用 |
| **Set TMP Default Font** | TMP Settings のデフォルトフォントを設定 |

> シーンを壊してしまった場合は **Wire Up Scene References** → **Apply Fonts and Sprites** の順で実行すれば復元できます。

---

## ゲームパラメータ

`Assets/Resources/SpawnerConfig.asset` で調整できます。

| パラメータ | デフォルト値 | 説明 |
|-----------|------------|------|
| Initial Interval | 2.0 秒 | 最初のスポーン間隔 |
| Min Interval | 0.3 秒 | スポーン間隔の下限 |
| Interval Decrease Per Sec | 0.02 秒/秒 | 時間経過による間隔の短縮量 |
| Max Capacity | 20 個 | 吹き出しエリアのキャパシティ上限 |

投稿ネタの種類・色・ラベルは `Assets/Resources/〇〇Post.asset` で変更できます。

---

## 投稿ネタ一覧

### 表ネタ（Public → 左スマホへ）

| 名前 | 色 | ラベル |
|------|----|-------|
| CafePost | #FFE4B5（ベージュ） | ☕ カフェに来た |
| LikePost | #FFB6C1（ピンク） | ♥ いいね！ |
| SunnyPost | #FFFACD（クリーム） | ☀ 今日も晴れ |

### 裏ネタ（Private → 右スマホへ）

| 名前 | 色 | ラベル |
|------|----|-------|
| AngerPost | #8B0000（ダークレッド） | 💢 マジ無理 |
| SkullPost | #2F2F2F（黒） | 💀 消えたい |
| TrashPost | #4A4A4A（グレー） | 🗑 ゴミ |

---

## 未対応・今後のTODO

### アセット

- [ ] リリース用フォント（Noto Sans JP など商用可フォントへの差し替え）
  　現在はヒラギノフォント（macOS専用ライセンス）を使用
- [ ] 投稿ネタのアイコン画像（各PostItemConfigSOのIconフィールドに設定）
- [ ] 頭アイコン画像（SpawnPoint左下に表示）
- [ ] BGM・SE の実装と音声ファイルの用意

### 機能

- [ ] タイトル画面の実装（TitleViewは存在するが未接続）
- [ ] スコアランキング・ハイスコア保存
- [ ] 難易度設定・ステージ追加
- [ ] ポーズ機能

詳細は `Assets/1_Features/0_InGame/ASSET_TODO.md` を参照してください。

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
