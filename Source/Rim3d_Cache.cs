using UnityEngine;
using System.Collections.Generic;

namespace Rim3D
{
    public static class RenderCache
    {
        private class ObjectCache
        {
            public Dictionary<string, Vector3> Dimensions = new Dictionary<string, Vector3>();
            public Dictionary<string, Vector3> Positions = new Dictionary<string, Vector3>();
            public Dictionary<string, Vector2> UvOffsets = new Dictionary<string, Vector2>();
            public Dictionary<string, float> UvZooms = new Dictionary<string, float>();
            public Dictionary<string, float> ParsedFloats = new Dictionary<string, float>();
            public Dictionary<string, Vector2> ParsedVector2 = new Dictionary<string, Vector2>();
            public Dictionary<string, Vector3> ParsedVector3 = new Dictionary<string, Vector3>();
            public long LastUsedFrame;
        }

        private static Dictionary<string, ObjectCache> objectCaches = new Dictionary<string, ObjectCache>();
        private const int CACHE_CLEANUP_INTERVAL = 1000;
        private const int CACHE_MAX_AGE_FRAMES = 2000;
        private static long currentFrame = 0;

        public static void ClearCache()
        {
            objectCaches.Clear();
        }

        public static void CleanupCache()
        {
            currentFrame++;
            if (currentFrame % CACHE_CLEANUP_INTERVAL == 0)
            {
                var keysToRemove = new List<string>();
                foreach (var kvp in objectCaches)
                {
                    if (currentFrame - kvp.Value.LastUsedFrame > CACHE_MAX_AGE_FRAMES)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                foreach (var key in keysToRemove)
                {
                    objectCaches.Remove(key);
                }
            }
        }

        private static ObjectCache GetOrCreateCache(string objectType)
        {
            if (!objectCaches.TryGetValue(objectType, out var cache))
            {
                cache = new ObjectCache();
                objectCaches[objectType] = cache;
            }
            cache.LastUsedFrame = currentFrame;
            return cache;
        }

        public static Vector3 GetDimension(string key, string objectType)
        {
            var cache = GetOrCreateCache(objectType);
            if (!cache.Dimensions.TryGetValue(key, out var value))
            {
                value = ParseVector3FromConfig(objectType, key);
                cache.Dimensions[key] = value;
            }
            return value;
        }

        public static Vector3 GetPosition(string key, string objectType)
        {
            var cache = GetOrCreateCache(objectType);
            if (!cache.Positions.TryGetValue(key, out var value))
            {
                value = ParseVector3FromConfig(objectType, key);
                cache.Positions[key] = value;
            }
            return value;
        }

        public static Vector2 GetUVOffset(string key, string objectType)
        {
            var cache = GetOrCreateCache(objectType);
            if (!cache.UvOffsets.TryGetValue(key, out var value))
            {
                value = ParseVector2FromConfig(objectType, key);
                cache.UvOffsets[key] = value;
            }
            return value;
        }

        public static float GetUVZoom(string key, string objectType)
        {
            var cache = GetOrCreateCache(objectType);
            if (!cache.UvZooms.TryGetValue(key, out var value))
            {
                value = ParseFloatFromConfig(objectType, key);
                cache.UvZooms[key] = value;
            }
            return value;
        }

        private static float ParseFloatFromConfig(string objectType, string key)
        {
            var cache = GetOrCreateCache(objectType);
            if (!cache.ParsedFloats.TryGetValue(key, out var value))
            {
                var config = ObjectConfigManager.GetConfig(objectType);
                if (config?.BoxSettings != null && config.BoxSettings.TryGetValue(key, out var strValue))
                {
                    float.TryParse(strValue, out value);
                }
                cache.ParsedFloats[key] = value;
            }
            return value;
        }

        private static Vector2 ParseVector2FromConfig(string objectType, string key)
        {
            var cache = GetOrCreateCache(objectType);
            if (!cache.ParsedVector2.TryGetValue(key, out var value))
            {
                var config = ObjectConfigManager.GetConfig(objectType);
                if (config?.BoxSettings != null && config.BoxSettings.TryGetValue(key, out var strValue))
                {
                    var parts = strValue.Split(',');
                    if (parts.Length == 2)
                    {
                        value = new Vector2(
                            float.Parse(parts[0]),
                            float.Parse(parts[1])
                        );
                    }
                }
                cache.ParsedVector2[key] = value;
            }
            return value;
        }

        private static Vector3 ParseVector3FromConfig(string objectType, string key)
        {
            var cache = GetOrCreateCache(objectType);
            if (!cache.ParsedVector3.TryGetValue(key, out var value))
            {
                var config = ObjectConfigManager.GetConfig(objectType);
                if (config?.BoxSettings != null && config.BoxSettings.TryGetValue(key, out var strValue))
                {
                    var parts = strValue.Split(',');
                    if (parts.Length == 3)
                    {
                        value = new Vector3(
                            float.Parse(parts[0]),
                            float.Parse(parts[1]),
                            float.Parse(parts[2])
                        );
                    }
                }
                cache.ParsedVector3[key] = value;
            }
            return value;
        }

        public static void InvalidateCache(string objectType)
        {
            objectCaches.Remove(objectType);
        }
    }
}
