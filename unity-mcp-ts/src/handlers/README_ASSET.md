# AssetCommandHandler

プロジェクト内のアセットを検索するツールを提供します。Unity 側の `AssetCommandHandler`（C#）に `asset.${action}` で転送します。

## ツール一覧

| ツール名 | Unity アクション | パラメータ |
|----------|-------------------|------------|
| asset_Search | asset.Search | query（必須）, type（任意: Prefab, Scene, Script 等）, limit（任意: 1–200, 既定50） |

## 例

- 「Button という名前のプレハブを検索して」→ `asset_Search`（query: "Button", type: "Prefab"）
- 「Assets/Scenes にあるシーン一覧」→ `asset_Search`（query: "Scenes", type: "Scene"）
