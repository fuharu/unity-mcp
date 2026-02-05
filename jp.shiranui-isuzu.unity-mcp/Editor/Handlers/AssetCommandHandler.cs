using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityMCP.Editor.Core;

namespace UnityMCP.Editor.Handlers
{
    /// <summary>
    /// Command handler for searching assets in the Unity project.
    /// </summary>
    internal sealed class AssetCommandHandler : IMcpCommandHandler
    {
        public string CommandPrefix => "asset";
        public string Description => "Search assets in the project by name or type";

        public JObject Execute(string action, JObject parameters)
        {
            return action.ToLower() switch
            {
                "search" => Search(parameters),
                _ => new JObject { ["success"] = false, ["error"] = $"Unknown action: {action}. Use 'search'." }
            };
        }

        private static JObject Search(JObject parameters)
        {
            var query = parameters["query"]?.ToString();
            if (string.IsNullOrWhiteSpace(query))
                return new JObject { ["success"] = false, ["error"] = "query is required" };

            var limit = 50;
            if (parameters["limit"] != null && (parameters["limit"].Type == JTokenType.Integer || parameters["limit"].Type == JTokenType.Float))
                limit = Math.Max(1, Math.Min(200, (int)parameters["limit"]));

            var typeFilter = parameters["type"]?.ToString();
            var searchQuery = string.IsNullOrWhiteSpace(typeFilter)
                ? query
                : $"t:{typeFilter} {query}";

            try
            {
                var guids = AssetDatabase.FindAssets(searchQuery);
                var results = new JArray();
                var count = Math.Min(guids.Length, limit);

                for (int i = 0; i < count; i++)
                {
                    var guid = guids[i];
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(assetPath)) continue;

                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (asset == null) continue;

                    var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                    results.Add(new JObject
                    {
                        ["path"] = assetPath,
                        ["name"] = assetName,
                        ["type"] = asset.GetType().Name,
                        ["guid"] = guid
                    });
                }

                return new JObject
                {
                    ["success"] = true,
                    ["count"] = results.Count,
                    ["total"] = guids.Length,
                    ["results"] = results
                };
            }
            catch (Exception ex)
            {
                return new JObject { ["success"] = false, ["error"] = ex.Message };
            }
        }
    }
}
