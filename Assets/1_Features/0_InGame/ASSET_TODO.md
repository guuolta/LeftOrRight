# アセット準備状況・TODO

## ✅ 自動生成済み

### フォント
| ファイル | 説明 |
|---------|------|
| `Assets/0_Common/Fonts/HiraginoSans-W4.ttc` | macOSシステムフォントをコピー |
| `Assets/0_Common/Fonts/HiraginoSans-W4 SDF.asset` | TMPro Dynamic SDFフォントアセット |

> **注意**: ヒラギノフォントはmacOS専用ライセンスです。
> **リリース時はNoto Sans JP など商用利用可能なフォントに差し替えてください。**

### スプライト（プログラム生成）
| ファイル | 用途 | 状態 |
|---------|------|------|
| `Assets/0_Common/Sprites/RoundedRect.png` | ハイライト・汎用パネル背景 | ✅ 白単色 |
| `Assets/0_Common/Sprites/Ellipse.png` | 吹き出しエリア背景 | ✅ 白単色 |
| `Assets/0_Common/Sprites/Circle.png` | PostItemの背景 | ✅ 白単色 |
| `Assets/0_Common/Sprites/PhoneFrame.png` | スマホ枠（PublicPhone/PrivatePhone） | ✅ 白単色 |

---

## ❌ 手動で準備が必要なアセット

### 1. フォント（リリース用）

**問題**: ヒラギノフォントはmacOS専用ライセンスのためリリース不可

**推奨フォント（無料・商用可）:**
- [Noto Sans JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) — Googleフォント、OFLライセンス
- [M PLUS Rounded 1c](https://fonts.google.com/specimen/M+PLUS+Rounded+1c) — 丸みがあってゲームUI向き
- [Kosugi Maru](https://fonts.google.com/specimen/Kosugi+Maru) — 軽量な丸ゴシック

**手順:**
1. 上記リンクからTTFをダウンロード
2. `Assets/0_Common/Fonts/` に配置
3. `Tools/InGame Setup/Create Japanese Font Asset` を実行（AssetGeneratorHelperのフォントパスを変更）
4. `Tools/InGame Setup/Apply Fonts and Sprites` を再実行

---

### 2. ゲームキャラクター・アイコン画像

#### 頭アイコン（SpawnPoint用）
| 用途 | 推奨サイズ | ファイル名案 |
|------|-----------|-------------|
| プレイヤーの頭（左下） | 128×128 px | `HeadIcon.png` |

→ `Assets/1_Features/0_InGame/Spawner/` に配置し、SpawnPoint GameObjectにSpriteRendererでアタッチ

#### 投稿ネタアイコン（PostItemConfig用）
各PostItemConfigSOの `_icon` フィールドに設定するスプライト。

| 名前 | 種別 | 内容例 | 推奨サイズ |
|------|------|-------|-----------|
| CafePost | Public | コーヒーカップ | 64×64 px |
| LikePost | Public | ハートマーク | 64×64 px |
| SunnyPost | Public | 太陽 | 64×64 px |
| AngerPost | Private | 怒りマーク | 64×64 px |
| SkullPost | Private | ドクロ | 64×64 px |
| TrashPost | Private | ゴミ箱 | 64×64 px |

**入手方法（例）:**
- [Google Fonts Icons](https://fonts.google.com/icons) — Apache 2.0ライセンス
- [Game-icons.net](https://game-icons.net/) — CC BY 3.0ライセンス
- [Kenney.nl](https://kenney.nl/assets) — パブリックドメイン

**配置先:** `Assets/1_Features/0_InGame/Post/Icons/`

**設定手順:**
1. 画像をインポートしてTexture Type = Sprite に設定
2. Unityのインスペクターで各 `PostItemConfigSO` の `Icon` フィールドに割り当て

---

### 3. スマホ背景の精度向上（オプション）

現在は単色の`PhoneFrame.png`（プログラム生成）を使用中。
より見た目にこだわる場合は以下を手動で作成:

| 用途 | 説明 |
|------|------|
| 表垢スマホ画面 | パステルブルー系の可愛いスマホUI風画像 |
| 裏垢スマホ画面 | ダークパープル系のシックなスマホUI風画像 |
| 吹き出し背景 | 思考バブル風の楕円グラフィック |

**推奨サイズ:** 400×600 px（縦長）

---

### 4. BGM・SE（音声）

ゲームプレイに必要な音声アセット（現在未実装）:

| 種類 | タイミング | ファイル名案 |
|------|----------|-------------|
| BGM | ゲーム中ループ | `bgm_ingame.mp3` |
| SE | 仕分け成功 | `se_correct.wav` |
| SE | 誤爆（ゲームオーバー） | `se_mistake.wav` |
| SE | ゲームオーバー演出 | `se_gameover.wav` |
| SE | 投稿スポーン | `se_spawn.wav` |

**入手方法（例）:**
- [Freesound.org](https://freesound.org/) — CC BY などライセンス確認必要
- [魔王魂](https://maoudamashii.jokersounds.com/) — 無料・クレジット表記で商用可

---

## セットアップメニュー一覧

Unityの `Tools/InGame Setup/` から以下を実行可能:

| メニュー | 内容 |
|---------|------|
| Create All ScriptableObjects | SpawnerConfig・PostItemConfig×6を生成 |
| Create PostItem Prefab | PostItemプレハブを生成 |
| Wire Up Scene References | シーン内の全参照を自動配線 |
| Create Japanese Font Asset | ヒラギノSDF フォントアセットを生成 |
| Create All Sprites | 基本スプライトを生成 |
| Apply Fonts and Sprites | フォント・スプライトをシーンに適用 |
