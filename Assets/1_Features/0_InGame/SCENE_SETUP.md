# InGameシーン セットアップガイド

## 1. シーン構成

以下の GameObject 階層で InGame シーンを構築する。

```
InGame (空のGameObject - InGamePresenter をアタッチ)
├── Main Camera
├── Canvas (Screen Space - Overlay)
│   ├── [UI] Header
│   │   └── ScoreText (TextMeshProUGUI)
│   ├── [UI] ThoughtBubble (ThoughtBubbleView をアタッチ)
│   │   └── Image (背景)
│   ├── [UI] PublicPhone (SortAreaView をアタッチ / PostType: Public)
│   │   ├── Image (スマホ画面背景 - パステルブルー)
│   │   └── HighlightImage (ドラッグオーバー時ハイライト)
│   ├── [UI] PrivatePhone (SortAreaView をアタッチ / PostType: Private)
│   │   ├── Image (スマホ画面背景 - ダークパープル)
│   │   └── HighlightImage
│   ├── [UI] GameOverPanel (GameOverView をアタッチ)
│   │   ├── CanvasGroup
│   │   ├── ReasonText (TextMeshProUGUI)
│   │   ├── FinalScoreText (TextMeshProUGUI)
│   │   └── RetryButton (Button)
│   └── [UI] TitlePanel (TitleView をアタッチ)
│       ├── TitleText (TextMeshProUGUI)
│       ├── InstructionText (TextMeshProUGUI - 点滅)
│       └── StartButton (Button)
├── PostInputHandler (PostInputHandler をアタッチ)
├── PostSpawner (PostSpawner をアタッチ)
│   └── SpawnPoint (Transform - 左下に配置)
└── ThoughtBubbleItems (空のGameObject - 投稿ネタの親)
```

## 2. PostItemPrefab の作成

```
PostItem (PostItemView をアタッチ / Rigidbody2D + CircleCollider2D)
├── Background (Image)
├── Icon (Image)
└── Label (TextMeshProUGUI)
```

- `Rigidbody2D`: Gravity Scale = 1.0, Collision Detection = Continuous
- `CircleCollider2D`: Radius = 0.5

## 3. ScriptableObject の作成

### SpawnerConfig (SpawnerConfigSO)
- Initial Interval: 2.0
- Min Interval: 0.3
- Interval Decrease Per Sec: 0.02
- Max Capacity: 20

### PostItemConfig × 複数種類 (PostItemConfigSO)

**表ネタ（Public）例:**
| 名前 | PostType | Color | Label |
|------|----------|-------|-------|
| CafePost | Public | #FFE4B5 | ☕ カフェに来た |
| LikePost | Public | #FFB6C1 | ♥ いいね！ |
| SunnyPost | Public | #FFFACD | ☀️ 今日も晴れ |

**裏ネタ（Private）例:**
| 名前 | PostType | Color | Label |
|------|----------|-------|-------|
| AngerPost | Private | #8B0000 | 💢 マジ無理 |
| SkullPost | Private | #2F2F2F | 💀 消えたい |
| TrashPost | Private | #4A4A4A | 🗑 ゴミ |

## 4. レイアウト（画面座標）

| エリア | 位置 | 備考 |
|--------|------|------|
| PublicPhone | 左上 (X: -400, Y: 200) | パステルブルー背景 |
| PrivatePhone | 右上 (X: 400, Y: 200) | ダークパープル背景 |
| ThoughtBubble | 中央 (X: 0, Y: -50) | 大きめの楕円 |
| SpawnPoint | 左下 (X: -500, Y: -300) | 頭アイコン位置 |

## 5. InGamePresenter の Inspector 設定

| フィールド | 参照先 |
|-----------|--------|
| Input Handler | PostInputHandler |
| Post Spawner | PostSpawner |
| Score View | ScoreText の ScoreView |
| Game Over View | GameOverPanel の GameOverView |
| Thought Bubble Parent | ThoughtBubbleItems |

## 6. PostInputHandler の Inspector 設定

| フィールド | 参照先 |
|-----------|--------|
| Sort Areas | PublicPhone, PrivatePhone の SortAreaView |
| Main Camera | Main Camera |
