using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace Rim3D
{
    public class ObjectConfig
    {
        public string Identifier;
        public MatchType MatchType;
        public bool IsPlane;
        public bool IsCube;
        public float HeightOffset;
        public Dictionary<string, string> BoxSettings = new Dictionary<string, string>();
        public Dictionary<string, string> DefaultBoxSettings = new Dictionary<string, string>();
        private const float SMALL_SIZE = 0.1f;

        public void InitializeDefaultBoxSettings(Vector2? size = null)
        {
            float width = 1f;
            float height = 2f;
            float fixedheight = 2f;
            float fixedheight2 = 1f;
            float ajust = 0.5f;
            float ajust2 = 0.5f;
            float ajust3 = 1f;

            if (size.HasValue)
            {
                width = size.Value.x;
                height = size.Value.y;
            }

            if (Identifier.Contains("Bed"))
            {
                ajust = 0.5f;
                ajust2 = 0.25f;
                ajust3 = 0.5f;
                fixedheight = 1;
                fixedheight2 = 0.51f;
            }

            if (!DefaultBoxSettings.ContainsKey("sur_dim")) DefaultBoxSettings["sur_dim"] = $"{SMALL_SIZE},{fixedheight},{width * ajust3}";
            if (!DefaultBoxSettings.ContainsKey("este_dim")) DefaultBoxSettings["este_dim"] = $"{SMALL_SIZE},{fixedheight},{width}";
            if (!DefaultBoxSettings.ContainsKey("oeste_dim")) DefaultBoxSettings["oeste_dim"] = $"{SMALL_SIZE},{fixedheight},{width}";
            if (!DefaultBoxSettings.ContainsKey("base_dim")) DefaultBoxSettings["base_dim"] = $"{SMALL_SIZE},{height},{width}";

            if (!DefaultBoxSettings.ContainsKey("sur_pos")) DefaultBoxSettings["sur_pos"] = $"0,0,{-width * ajust}";
            if (!DefaultBoxSettings.ContainsKey("este_pos")) DefaultBoxSettings["este_pos"] = $"{width * ajust2},0,0";
            if (!DefaultBoxSettings.ContainsKey("oeste_pos")) DefaultBoxSettings["oeste_pos"] = $"{-width * ajust2},0,0";
            if (!DefaultBoxSettings.ContainsKey("base_pos")) DefaultBoxSettings["base_pos"] = $"0,{fixedheight2},0";

            if (!DefaultBoxSettings.ContainsKey("sur_rot_dim")) DefaultBoxSettings["sur_rot_dim"] = $"{SMALL_SIZE},{fixedheight},{height}";
            if (!DefaultBoxSettings.ContainsKey("este_rot_dim")) DefaultBoxSettings["este_rot_dim"] = $"{SMALL_SIZE},{fixedheight},{width * ajust}";
            if (!DefaultBoxSettings.ContainsKey("oeste_rot_dim")) DefaultBoxSettings["oeste_rot_dim"] = $"{SMALL_SIZE},{fixedheight},{width * ajust}";
            if (!DefaultBoxSettings.ContainsKey("base_rot_dim")) DefaultBoxSettings["base_rot_dim"] = $"{SMALL_SIZE},{height},{width}";

            if (!DefaultBoxSettings.ContainsKey("sur_rot_pos")) DefaultBoxSettings["sur_rot_pos"] = $"0,0,{-width * ajust2}";
            if (!DefaultBoxSettings.ContainsKey("este_rot_pos")) DefaultBoxSettings["este_rot_pos"] = $"{width * ajust},0,0";
            if (!DefaultBoxSettings.ContainsKey("oeste_rot_pos")) DefaultBoxSettings["oeste_rot_pos"] = $"{-width * ajust},0,0";
            if (!DefaultBoxSettings.ContainsKey("base_rot_pos")) DefaultBoxSettings["base_rot_pos"] = $"0,{fixedheight2},0";

            if (!DefaultBoxSettings.ContainsKey("sur_uv_offset_top")) DefaultBoxSettings["sur_uv_offset_top"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("sur_uv_offset_down")) DefaultBoxSettings["sur_uv_offset_down"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("sur_uv_zoom_top")) DefaultBoxSettings["sur_uv_zoom_top"] = "200";
            if (!DefaultBoxSettings.ContainsKey("sur_uv_zoom_down")) DefaultBoxSettings["sur_uv_zoom_down"] = "200";
            if (!DefaultBoxSettings.ContainsKey("sur_rot_uv_offset_left")) DefaultBoxSettings["sur_rot_uv_offset_left"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("sur_rot_uv_offset_right")) DefaultBoxSettings["sur_rot_uv_offset_right"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("sur_rot_uv_zoom_left")) DefaultBoxSettings["sur_rot_uv_zoom_left"] = "200";
            if (!DefaultBoxSettings.ContainsKey("sur_rot_uv_zoom_right")) DefaultBoxSettings["sur_rot_uv_zoom_right"] = "200";

            if (!DefaultBoxSettings.ContainsKey("este_uv_offset_top")) DefaultBoxSettings["este_uv_offset_top"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("este_uv_offset_down")) DefaultBoxSettings["este_uv_offset_down"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("este_uv_zoom_top")) DefaultBoxSettings["este_uv_zoom_top"] = "200";
            if (!DefaultBoxSettings.ContainsKey("este_uv_zoom_down")) DefaultBoxSettings["este_uv_zoom_down"] = "200";
            if (!DefaultBoxSettings.ContainsKey("este_rot_uv_offset_left")) DefaultBoxSettings["este_rot_uv_offset_left"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("este_rot_uv_offset_right")) DefaultBoxSettings["este_rot_uv_offset_right"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("este_rot_uv_zoom_left")) DefaultBoxSettings["este_rot_uv_zoom_left"] = "200";
            if (!DefaultBoxSettings.ContainsKey("este_rot_uv_zoom_right")) DefaultBoxSettings["este_rot_uv_zoom_right"] = "200";

            if (!DefaultBoxSettings.ContainsKey("oeste_uv_offset_top")) DefaultBoxSettings["oeste_uv_offset_top"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("oeste_uv_offset_down")) DefaultBoxSettings["oeste_uv_offset_down"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("oeste_uv_zoom_top")) DefaultBoxSettings["oeste_uv_zoom_top"] = "200";
            if (!DefaultBoxSettings.ContainsKey("oeste_uv_zoom_down")) DefaultBoxSettings["oeste_uv_zoom_down"] = "200";
            if (!DefaultBoxSettings.ContainsKey("oeste_rot_uv_offset_left")) DefaultBoxSettings["oeste_rot_uv_offset_left"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("oeste_rot_uv_offset_right")) DefaultBoxSettings["oeste_rot_uv_offset_right"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("oeste_rot_uv_zoom_left")) DefaultBoxSettings["oeste_rot_uv_zoom_left"] = "200";
            if (!DefaultBoxSettings.ContainsKey("oeste_rot_uv_zoom_right")) DefaultBoxSettings["oeste_rot_uv_zoom_right"] = "200";

            if (!DefaultBoxSettings.ContainsKey("base_uv_offset_top")) DefaultBoxSettings["base_uv_offset_top"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("base_uv_offset_down")) DefaultBoxSettings["base_uv_offset_down"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("base_uv_zoom_top")) DefaultBoxSettings["base_uv_zoom_top"] = "100";
            if (!DefaultBoxSettings.ContainsKey("base_uv_zoom_down")) DefaultBoxSettings["base_uv_zoom_down"] = "100";
            if (!DefaultBoxSettings.ContainsKey("base_rot_uv_offset_left")) DefaultBoxSettings["base_rot_uv_offset_left"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("base_rot_uv_offset_right")) DefaultBoxSettings["base_rot_uv_offset_right"] = "0,0";
            if (!DefaultBoxSettings.ContainsKey("base_rot_uv_zoom_left")) DefaultBoxSettings["base_rot_uv_zoom_left"] = "100";
            if (!DefaultBoxSettings.ContainsKey("base_rot_uv_zoom_right")) DefaultBoxSettings["base_rot_uv_zoom_right"] = "100";

            DefaultBoxSettings.Remove("sur_uv_offset");
            DefaultBoxSettings.Remove("sur_uv_zoom");
            DefaultBoxSettings.Remove("sur_rot_uv_offset");
            DefaultBoxSettings.Remove("sur_rot_uv_zoom");

            DefaultBoxSettings.Remove("este_uv_offset");
            DefaultBoxSettings.Remove("este_uv_zoom");
            DefaultBoxSettings.Remove("este_rot_uv_offset");
            DefaultBoxSettings.Remove("este_rot_uv_zoom");

            DefaultBoxSettings.Remove("oeste_uv_offset");
            DefaultBoxSettings.Remove("oeste_uv_zoom");
            DefaultBoxSettings.Remove("oeste_rot_uv_offset");
            DefaultBoxSettings.Remove("oeste_rot_uv_zoom");

            DefaultBoxSettings.Remove("base_uv_offset");
            DefaultBoxSettings.Remove("base_uv_zoom");
            DefaultBoxSettings.Remove("base_rot_uv_offset");
            DefaultBoxSettings.Remove("base_rot_uv_zoom");

            if (!BoxSettings.Any())
            {
                BoxSettings = new Dictionary<string, string>(DefaultBoxSettings);
            }
        }

        public bool IsMatch(Thing thing)
        {
            if (thing?.def?.defName == null) return false;

            switch (MatchType)
            {
                case MatchType.Selected:
                    return thing.def.defName == Identifier;
                case MatchType.Exact:
                    return thing.def.defName == Identifier;
                case MatchType.Contains:
                    return thing.def.defName.Contains(Identifier);
                default:
                    return false;
            }
        }

        public void CopyFrom(ObjectConfig other)
        {
            if (other == null) return;

            IsPlane = other.IsPlane;
            IsCube = other.IsCube;
            HeightOffset = other.HeightOffset;
            BoxSettings = new Dictionary<string, string>(other.BoxSettings);
            DefaultBoxSettings = new Dictionary<string, string>(other.DefaultBoxSettings);
        }
    }

    public enum MatchType
    {
        Selected,
        Exact,
        Contains
    }

    public static class ObjectConfigManager
    {
        private static HashSet<ObjectConfig> configs = new HashSet<ObjectConfig>();
        private static ObjectWindow currentWindow;
        private static ObjectConfig copiedConfig;

        public static void SetCurrentWindow(ObjectWindow window)
        {
            currentWindow = window;
        }

        public static void Initialize()
        {
            configs.Clear();
            CubeRenderer.ClearAll();
            var objects = ConfigSerializer.LoadAllObjectData();
            foreach (var obj in objects)
            {
                AddConfigFromData(obj);
            }
        }

        public static void AddConfig(string identifier, MatchType matchType)
        {
            var config = new ObjectConfig
            {
                Identifier = identifier,
                MatchType = matchType,
                IsPlane = true,
                HeightOffset = 0f,
                BoxSettings = new Dictionary<string, string>()
            };
            configs.Add(config);
            CubeRenderer.LoadFromBoxSettings(identifier, config.BoxSettings);

            if (currentWindow != null)
            {
                Find.WindowStack.TryRemove(typeof(ObjectWindow));
                Find.WindowStack.Add(new ObjectWindow());
            }
        }

        public static void AddConfigFromData(ObjectData data)
        {
            var existingConfig = configs.FirstOrDefault(x => x.Identifier == data.Identifier && x.MatchType == data.MatchType);
            if (existingConfig != null)
            {
                configs.Remove(existingConfig);
            }

            var config = new ObjectConfig
            {
                Identifier = data.Identifier,
                MatchType = data.MatchType,
                IsPlane = data.IsPlane,
                IsCube = data.IsCube,
                HeightOffset = data.HeightOffset,
                BoxSettings = data.GetBoxSettingsDictionary()
            };
            config.InitializeDefaultBoxSettings();
            configs.Add(config);
            CubeRenderer.LoadFromBoxSettings(data.Identifier, config.BoxSettings);
        }

        public static ObjectData CreateDataFromConfig(ObjectConfig config)
        {
            var data = new ObjectData
            {
                Identifier = config.Identifier,
                MatchType = config.MatchType,
                IsPlane = config.IsPlane,
                IsCube = config.IsCube,
                HeightOffset = config.HeightOffset,
                BoxSettings = new List<BoxSettingData>()
            };

            foreach (var kvp in config.BoxSettings)
            {
                data.BoxSettings.Add(new BoxSettingData { Key = kvp.Key, Value = kvp.Value });
            }

            return data;
        }

        public static void UpdateConfigFromData(ObjectData data)
        {
            var config = configs.FirstOrDefault(c => c.Identifier == data.Identifier && c.MatchType == data.MatchType);
            if (config != null)
            {
                config.IsPlane = data.IsPlane;
                config.IsCube = data.IsCube;
                config.HeightOffset = data.HeightOffset;
                config.BoxSettings = data.GetBoxSettingsDictionary();
                config.InitializeDefaultBoxSettings();
                CubeRenderer.LoadFromBoxSettings(data.Identifier, config.BoxSettings);
            }
        }

        public static void RemoveConfig(string identifier, MatchType matchType)
        {
            var config = configs.FirstOrDefault(c => c.Identifier == identifier && c.MatchType == matchType);
            if (config != null)
            {
                configs.Remove(config);
                CubeRenderer.RemoveConfig(identifier);
                ConfigSerializer.DeleteObjectData(identifier);

                if (currentWindow != null)
                {
                    Find.WindowStack.TryRemove(typeof(ObjectWindow));
                    Find.WindowStack.Add(new ObjectWindow());
                }
            }
        }

        public static ObjectConfig GetConfig(string identifier)
        {
            return configs.FirstOrDefault(c => c.Identifier == identifier);
        }

        public static ObjectConfig GetConfigForThing(Thing thing)
        {
            if (thing?.def?.defName == null) return null;

            var exactMatch = configs.FirstOrDefault(c =>
                (c.MatchType == MatchType.Selected || c.MatchType == MatchType.Exact) &&
                c.Identifier == thing.def.defName);
            if (exactMatch != null) return exactMatch;

            return configs.FirstOrDefault(c =>
                c.MatchType == MatchType.Contains &&
                thing.def.defName.Contains(c.Identifier));
        }

        public static IEnumerable<ObjectConfig> GetAllConfigs()
        {
            return configs;
        }

        public static IEnumerable<ObjectConfig> GetConfigsByType(MatchType type)
        {
            return configs.Where(c => c.MatchType == type);
        }

        public static void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Initialize();
            }
        }

        public static void ClearCache()
        {
            configs.Clear();
        }

        public static void CopyConfig(ObjectConfig config)
        {
            if (config == null) return;
            copiedConfig = config;
        }

        public static bool HasCopiedConfig()
        {
            return copiedConfig != null;
        }

        public static void PasteConfig(ObjectConfig target)
        {
            if (copiedConfig == null || target == null) return;
            target.CopyFrom(copiedConfig);
            if (target.IsCube)
            {
                CubeRenderer.LoadFromBoxSettings(target.Identifier, target.BoxSettings);
            }
        }
    }
}