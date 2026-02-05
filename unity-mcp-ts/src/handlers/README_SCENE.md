# SceneCommandHandler

シーン操作ツール（`scene_*`）を提供するハンドラです。Unity 側の `SceneCommandHandler`（C#）に `scene.${action}` で転送します。

## 登録

`HandlerDiscovery` が `handlers/` 配下の **コンパイル後の .js** を自動読み込みするため、**手動登録は不要**です。  
`npm run build` で `SceneCommandHandler.js` が生成されれば自動で検出・登録されます。

## ツール一覧

| ツール名 | Unity アクション | 必須パラメータ | 任意パラメータ |
|----------|-------------------|----------------|----------------|
| scene_CreateGameObject | scene.CreateGameObject | （なし） | name, parentPath |
| scene_AddComponent | scene.AddComponent | path, componentType | （なし） |
| scene_SetPosition | scene.SetPosition | path | x, y, z |
| scene_SetParent | scene.SetParent | path | parentPath |
| scene_Find | scene.Find | path | （なし） |
| scene_SaveScene | scene.SaveScene | （なし） | （なし） |
| scene_OpenScene | scene.OpenScene | path | （なし） |
| scene_GetActiveSceneName | scene.GetActiveSceneName | （なし） | （なし） |
| scene_InstantiatePrefab | scene.InstantiatePrefab | prefabPath | parentPath |
| scene_ListRootObjects | scene.ListRootObjects | （なし） | （なし） |

## 動作

- `HandlerAdapter` がツール名から action を抽出（例: `scene_CreateGameObject` → `CreateGameObject`）。
- `executeCommand(action, parameters)` で `scene.${action}` を Unity に送信。
