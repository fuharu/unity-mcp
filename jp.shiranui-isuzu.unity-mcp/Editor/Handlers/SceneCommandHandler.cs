using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityMCP.Editor.Core;

namespace UnityMCP.Editor.Handlers
{
    /// <summary>
    /// Command handler for scene operations: create/modify GameObjects, save/open scene, get active scene name.
    /// </summary>
    internal sealed class SceneCommandHandler : IMcpCommandHandler
    {
        public string CommandPrefix => "scene";
        public string Description => "Create and modify GameObjects in the active scene; save/open scene; get active scene name";

        public JObject Execute(string action, JObject parameters)
        {
            return action switch
            {
                "CreateGameObject" => CreateGameObject(parameters),
                "AddComponent" => AddComponent(parameters),
                "SetPosition" => SetPosition(parameters),
                "SetParent" => SetParent(parameters),
                "Find" => Find(parameters),
                "SaveScene" => SaveScene(parameters),
                "OpenScene" => OpenScene(parameters),
                "GetActiveSceneName" => GetActiveSceneName(parameters),
                "InstantiatePrefab" => InstantiatePrefab(parameters),
                "ListRootObjects" => ListRootObjects(parameters),
                _ => new JObject { ["success"] = false, ["error"] = $"Unknown action: {action}" }
            };
        }

        private static GameObject FindByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded) return null;
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;
            var roots = scene.GetRootGameObjects();
            GameObject current = roots.FirstOrDefault(r => r.name == parts[0]);
            if (current == null) return null;
            for (int i = 1; i < parts.Length; i++)
            {
                var t = current.transform.Find(parts[i]);
                if (t == null) return null;
                current = t.gameObject;
            }
            return current;
        }

        private static JObject CreateGameObject(JObject parameters)
        {
            try
            {
                var name = parameters["name"]?.ToString() ?? "GameObject";
                var parentPath = parameters["parentPath"]?.ToString();
                var go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, "Create GameObject");
                if (!string.IsNullOrEmpty(parentPath))
                {
                    var parent = FindByPath(parentPath);
                    if (parent != null)
                        go.transform.SetParent(parent.transform, worldPositionStays: false);
                }
                return new JObject
                {
                    ["success"] = true,
                    ["path"] = GetPath(go),
                    ["name"] = go.name
                };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var p = go.transform.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }

        private static JObject AddComponent(JObject parameters)
        {
            var path = parameters["path"]?.ToString();
            var componentType = parameters["componentType"]?.ToString();
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(componentType))
                return new JObject { ["success"] = false, ["error"] = "path and componentType are required" };
            var go = FindByPath(path);
            if (go == null)
                return new JObject { ["success"] = false, ["error"] = $"GameObject not found: {path}" };
            var type = Type.GetType(componentType)
                ?? AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                    .FirstOrDefault(t => t.Name == componentType || t.FullName == componentType);
            if (type == null)
                return new JObject { ["success"] = false, ["error"] = $"Component type not found: {componentType}" };
            try
            {
                Undo.AddComponent(go, type);
                return new JObject { ["success"] = true, ["path"] = path, ["componentType"] = componentType };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }

        private static JObject SetPosition(JObject parameters)
        {
            var path = parameters["path"]?.ToString();
            if (string.IsNullOrEmpty(path))
                return new JObject { ["success"] = false, ["error"] = "path is required" };
            var go = FindByPath(path);
            if (go == null)
                return new JObject { ["success"] = false, ["error"] = $"GameObject not found: {path}" };
            var pos = go.transform.position;
            if (parameters["x"] != null && (parameters["x"].Type == JTokenType.Float || parameters["x"].Type == JTokenType.Integer))
                pos.x = (float)parameters["x"];
            if (parameters["y"] != null && (parameters["y"].Type == JTokenType.Float || parameters["y"].Type == JTokenType.Integer))
                pos.y = (float)parameters["y"];
            if (parameters["z"] != null && (parameters["z"].Type == JTokenType.Float || parameters["z"].Type == JTokenType.Integer))
                pos.z = (float)parameters["z"];
            Undo.RecordObject(go.transform, "Set Position");
            go.transform.position = pos;
            return new JObject { ["success"] = true, ["path"] = path };
        }

        private static JObject SetParent(JObject parameters)
        {
            var path = parameters["path"]?.ToString();
            if (string.IsNullOrEmpty(path))
                return new JObject { ["success"] = false, ["error"] = "path is required" };
            var go = FindByPath(path);
            if (go == null)
                return new JObject { ["success"] = false, ["error"] = $"GameObject not found: {path}" };
            var parentPath = parameters["parentPath"]?.ToString();
            Transform parent = string.IsNullOrEmpty(parentPath) ? null : FindByPath(parentPath)?.transform;
            Undo.RecordObject(go.transform, "Set Parent");
            go.transform.SetParent(parent, worldPositionStays: true);
            return new JObject { ["success"] = true, ["path"] = path };
        }

        private static JObject Find(JObject parameters)
        {
            var path = parameters["path"]?.ToString();
            if (string.IsNullOrEmpty(path))
                return new JObject { ["success"] = false, ["error"] = "path is required" };
            var go = FindByPath(path);
            if (go == null)
                return new JObject { ["success"] = false, ["error"] = $"GameObject not found: {path}" };
            return new JObject
            {
                ["success"] = true,
                ["path"] = GetPath(go),
                ["name"] = go.name,
                ["instanceId"] = go.GetInstanceID()
            };
        }

        private static JObject SaveScene(JObject parameters)
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                if (!scene.IsValid() || !scene.isLoaded)
                    return new JObject { ["success"] = false, ["error"] = "No active scene loaded" };
                var saved = EditorSceneManager.SaveScene(scene);
                return new JObject
                {
                    ["success"] = saved,
                    ["sceneName"] = scene.name,
                    ["path"] = scene.path
                };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }

        private static JObject OpenScene(JObject parameters)
        {
            var path = parameters["path"]?.ToString();
            if (string.IsNullOrEmpty(path))
                return new JObject { ["success"] = false, ["error"] = "path is required (e.g. Assets/Scenes/BaseScene.unity)" };
            try
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                return new JObject
                {
                    ["success"] = true,
                    ["sceneName"] = scene.name,
                    ["path"] = scene.path
                };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }

        private static JObject GetActiveSceneName(JObject parameters)
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                return new JObject
                {
                    ["success"] = true,
                    ["sceneName"] = scene.name,
                    ["path"] = scene.path,
                    ["isLoaded"] = scene.isLoaded
                };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }

        private static JObject InstantiatePrefab(JObject parameters)
        {
            var prefabPath = parameters["prefabPath"]?.ToString();
            if (string.IsNullOrEmpty(prefabPath))
                return new JObject { ["success"] = false, ["error"] = "prefabPath is required (e.g. Assets/Prefabs/MyPrefab.prefab)" };
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                    return new JObject { ["success"] = false, ["error"] = $"Prefab not found: {prefabPath}" };
                var scene = SceneManager.GetActiveScene();
                if (!scene.IsValid() || !scene.isLoaded)
                    return new JObject { ["success"] = false, ["error"] = "No active scene loaded" };
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                if (instance == null)
                    return new JObject { ["success"] = false, ["error"] = "Failed to instantiate prefab" };
                Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");
                var parentPath = parameters["parentPath"]?.ToString();
                if (!string.IsNullOrEmpty(parentPath))
                {
                    var parent = FindByPath(parentPath);
                    if (parent != null)
                    {
                        Undo.SetTransformParent(instance.transform, parent.transform, "Reparent");
                        instance.transform.SetParent(parent.transform, worldPositionStays: true);
                    }
                }
                return new JObject
                {
                    ["success"] = true,
                    ["path"] = GetPath(instance),
                    ["name"] = instance.name,
                    ["prefabPath"] = prefabPath
                };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }

        private static JObject ListRootObjects(JObject parameters)
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                if (!scene.IsValid() || !scene.isLoaded)
                    return new JObject { ["success"] = false, ["error"] = "No active scene loaded" };
                var roots = scene.GetRootGameObjects();
                var names = new JArray();
                foreach (var go in roots)
                    names.Add(go.name);
                return new JObject
                {
                    ["success"] = true,
                    ["sceneName"] = scene.name,
                    ["rootNames"] = names,
                    ["count"] = roots.Length
                };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }
    }
}
