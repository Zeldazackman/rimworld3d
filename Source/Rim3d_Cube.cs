using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rim3D
{
    public static class CubeRenderer
    {
        private static int renderPass = 0;

        private static Dictionary<string, Dictionary<string, Vector3>> dimensions = new Dictionary<string, Dictionary<string, Vector3>>();
        private static Dictionary<string, Dictionary<string, Vector3>> positions = new Dictionary<string, Dictionary<string, Vector3>>();
        private static Dictionary<string, Dictionary<string, Vector2>> uvOffsets = new Dictionary<string, Dictionary<string, Vector2>>();
        private static Dictionary<string, Dictionary<string, float>> uvZooms = new Dictionary<string, Dictionary<string, float>>();

        public static void ClearAll()
        {
            dimensions.Clear();
            positions.Clear();
            uvOffsets.Clear();
            uvZooms.Clear();
            EnsureDictionariesInitialized();
            RenderCache.ClearCache();
        }

        public static void RemoveConfig(string objectType)
        {
            foreach (var dict in dimensions.Values)
                dict.Remove(objectType);
            foreach (var dict in positions.Values)
                dict.Remove(objectType);
            foreach (var dict in uvOffsets.Values)
                dict.Remove(objectType);
            foreach (var dict in uvZooms.Values)
                dict.Remove(objectType);
            RenderCache.InvalidateCache(objectType);
        }

        private static void EnsureDictionariesInitialized()
        {
            if (dimensions.Count == 0)
            {
                var dimensionKeys = new[]
                {
                    "sur_dim", "este_dim", "oeste_dim", "base_dim",
                    "sur_rot_dim", "este_rot_dim", "oeste_rot_dim", "base_rot_dim"
                };
                foreach (var key in dimensionKeys)
                {
                    dimensions[key] = new Dictionary<string, Vector3>();
                }
            }

            if (positions.Count == 0)
            {
                var positionKeys = new[]
                {
                    "sur_pos", "este_pos", "oeste_pos", "base_pos",
                    "sur_rot_pos", "este_rot_pos", "oeste_rot_pos", "base_rot_pos"
                };
                foreach (var key in positionKeys)
                {
                    positions[key] = new Dictionary<string, Vector3>();
                }
            }

            if (uvOffsets.Count == 0)
            {
                var uvOffsetKeys = new[]
                {
                    "sur_uv_offset_top", "sur_uv_offset_down", "este_uv_offset_top", "este_uv_offset_down",
                    "oeste_uv_offset_top", "oeste_uv_offset_down", "base_uv_offset_top", "base_uv_offset_down",
                    "sur_rot_uv_offset_left", "sur_rot_uv_offset_right", "este_rot_uv_offset_left", "este_rot_uv_offset_right",
                    "oeste_rot_uv_offset_left", "oeste_rot_uv_offset_right", "base_rot_uv_offset_left", "base_rot_uv_offset_right"
                };
                foreach (var key in uvOffsetKeys)
                {
                    uvOffsets[key] = new Dictionary<string, Vector2>();
                }
            }

            if (uvZooms.Count == 0)
            {
                var uvZoomKeys = new[]
                {
                    "sur_uv_zoom_top", "sur_uv_zoom_down", "este_uv_zoom_top", "este_uv_zoom_down",
                    "oeste_uv_zoom_top", "oeste_uv_zoom_down", "base_uv_zoom_top", "base_uv_zoom_down",
                    "sur_rot_uv_zoom_left", "sur_rot_uv_zoom_right", "este_rot_uv_zoom_left", "este_rot_uv_zoom_right",
                    "oeste_rot_uv_zoom_left", "oeste_rot_uv_zoom_right", "base_rot_uv_zoom_left", "base_rot_uv_zoom_right"
                };
                foreach (var key in uvZoomKeys)
                {
                    uvZooms[key] = new Dictionary<string, float>();
                }
            }
        }

        public static void LoadFromBoxSettings(string objectType, Dictionary<string, string> boxSettings)
        {
            EnsureDictionariesInitialized();
            RenderCache.InvalidateCache(objectType);

            foreach (var kvp in boxSettings)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                if (key.EndsWith("_dim") && dimensions.ContainsKey(key))
                {
                    var parts = value.Split(',');
                    if (parts.Length == 3)
                    {
                        dimensions[key][objectType] = new Vector3(
                            float.Parse(parts[0]),
                            float.Parse(parts[1]),
                            float.Parse(parts[2])
                        );
                    }
                }
                else if (key.EndsWith("_pos") && positions.ContainsKey(key))
                {
                    var parts = value.Split(',');
                    if (parts.Length == 3)
                    {
                        positions[key][objectType] = new Vector3(
                            float.Parse(parts[0]),
                            float.Parse(parts[1]),
                            float.Parse(parts[2])
                        );
                    }
                }
                else if ((key.EndsWith("_top") || key.EndsWith("_down") || key.EndsWith("_left") || key.EndsWith("_right")) && key.Contains("_uv_offset"))
                {
                    var parts = value.Split(',');
                    if (parts.Length == 2)
                    {
                        uvOffsets[key][objectType] = new Vector2(
                            float.Parse(parts[0]),
                            float.Parse(parts[1])
                        );
                    }
                }
                else if ((key.EndsWith("_top") || key.EndsWith("_down") || key.EndsWith("_left") || key.EndsWith("_right")) && key.Contains("_uv_zoom"))
                {
                    uvZooms[key][objectType] = float.Parse(value);
                }
            }
        }

        private static void SetDefaultValues(string objectType)
        {
            dimensions["sur_dim"][objectType] = new Vector3(1f, 0.5f, 0.1f);
            dimensions["este_dim"][objectType] = new Vector3(0.1f, 0.5f, 4f);
            dimensions["oeste_dim"][objectType] = new Vector3(0.1f, 0.5f, 4f);
            dimensions["base_dim"][objectType] = new Vector3(1f, 0.1f, 1f);

            positions["sur_pos"][objectType] = new Vector3(0f, 0f, 0.001f);
            positions["este_pos"][objectType] = new Vector3(0.001f, 0f, 0f);
            positions["oeste_pos"][objectType] = new Vector3(-0.001f, 0f, 0f);
            positions["base_pos"][objectType] = new Vector3(0f, 0f, 0f);

            dimensions["sur_rot_dim"][objectType] = new Vector3(1f, 0.5f, 0.1f);
            dimensions["este_rot_dim"][objectType] = new Vector3(0.1f, 0.5f, 1f);
            dimensions["oeste_rot_dim"][objectType] = new Vector3(0.1f, 0.5f, 1f);
            dimensions["base_rot_dim"][objectType] = new Vector3(1f, 0.1f, 1f);

            positions["sur_rot_pos"][objectType] = new Vector3(0f, 0f, 0.001f);
            positions["este_rot_pos"][objectType] = new Vector3(0.001f, 0f, 0f);
            positions["oeste_rot_pos"][objectType] = new Vector3(-0.001f, 0f, 0f);
            positions["base_rot_pos"][objectType] = new Vector3(0f, 0f, 0f);

            var uvTypes = new[] { "_top", "_down", "_left", "_right" };
            var pieces = new[] { "sur", "este", "oeste", "base" };
            var rotations = new[] { "", "_rot" };

            foreach (var piece in pieces)
            {
                foreach (var rot in rotations)
                {
                    var uvSuffixes = rot == "" ? new[] { "_top", "_down" } : new[] { "_left", "_right" };
                    foreach (var suffix in uvSuffixes)
                    {
                        uvOffsets[$"{piece}{rot}_uv_offset{suffix}"][objectType] = Vector2.zero;
                        uvZooms[$"{piece}{rot}_uv_zoom{suffix}"][objectType] = 100f;
                    }
                }
            }
        }

        private static void InitializeDefaultValues(string objectType)
        {
            EnsureDictionariesInitialized();

            var config = ObjectConfigManager.GetConfig(objectType);
            if (config != null)
            {
                LoadFromBoxSettings(objectType, config.BoxSettings);
            }
            else
            {
                SetDefaultValues(objectType);
            }
        }

        public static Vector3 GetDimension(string type, string objectType)
        {
            return RenderCache.GetDimension(type, objectType);
        }

        public static void SetDimension(string type, string objectType, Vector3 value)
        {
            InitializeDefaultValues(objectType);
            dimensions[type][objectType] = value;
            var config = ObjectConfigManager.GetConfig(objectType);
            if (config != null)
            {
                config.BoxSettings[type] = $"{value.x},{value.y},{value.z}";
            }
            RenderCache.InvalidateCache(objectType);
        }

        public static Vector3 GetPosition(string type, string objectType)
        {
            return RenderCache.GetPosition(type, objectType);
        }

        public static void SetPosition(string type, string objectType, Vector3 value)
        {
            InitializeDefaultValues(objectType);
            positions[type][objectType] = value;
            var config = ObjectConfigManager.GetConfig(objectType);
            if (config != null)
            {
                config.BoxSettings[type] = $"{value.x},{value.y},{value.z}";
            }
            RenderCache.InvalidateCache(objectType);
        }

        public static Vector2 GetUVOffset(string type, string objectType, bool isTop, bool isLeft)
        {
            string key = type;
            if (type.Contains("_rot"))
            {
                key = key.Replace("_uv_offset", isLeft ? "_uv_offset_left" : "_uv_offset_right");
            }
            else
            {
                key = key.Replace("_uv_offset", isTop ? "_uv_offset_top" : "_uv_offset_down");
            }
            return RenderCache.GetUVOffset(key, objectType);
        }

        public static void SetUVOffset(string type, string objectType, Vector2 value, bool isTop, bool isLeft)
        {
            InitializeDefaultValues(objectType);
            string key = type;
            if (type.Contains("_rot"))
            {
                key = key.Replace("_uv_offset", isLeft ? "_uv_offset_left" : "_uv_offset_right");
            }
            else
            {
                key = key.Replace("_uv_offset", isTop ? "_uv_offset_top" : "_uv_offset_down");
            }
            uvOffsets[key][objectType] = value;
            var config = ObjectConfigManager.GetConfig(objectType);
            if (config != null)
            {
                config.BoxSettings[key] = $"{value.x},{value.y}";
            }
            RenderCache.InvalidateCache(objectType);
        }

        public static float GetUVZoom(string type, string objectType, bool isTop, bool isLeft)
        {
            string key = type;
            if (type.Contains("_rot"))
            {
                key = key.Replace("_uv_zoom", isLeft ? "_uv_zoom_left" : "_uv_zoom_right");
            }
            else
            {
                key = key.Replace("_uv_zoom", isTop ? "_uv_zoom_top" : "_uv_zoom_down");
            }
            return RenderCache.GetUVZoom(key, objectType);
        }

        public static void SetUVZoom(string type, string objectType, float value, bool isTop, bool isLeft)
        {
            InitializeDefaultValues(objectType);
            string key = type;
            if (type.Contains("_rot"))
            {
                key = key.Replace("_uv_zoom", isLeft ? "_uv_zoom_left" : "_uv_zoom_right");
            }
            else
            {
                key = key.Replace("_uv_zoom", isTop ? "_uv_zoom_top" : "_uv_zoom_down");
            }
            uvZooms[key][objectType] = value;
            var config = ObjectConfigManager.GetConfig(objectType);
            if (config != null)
            {
                config.BoxSettings[key] = value.ToString();
            }
            RenderCache.InvalidateCache(objectType);
        }

        public static void RenderCube(LayerSubMesh subMesh, Vector3 center, Vector2 size, float rot = 0f, Vector2[] uvs = null, Color32[] colors = null, float topVerticesAltitudeBias = 0.01f, float uvzPayload = 0f)
        {
            if (PrintingTracker.CurrentThing == null) return;

            var config = ObjectConfigManager.GetConfigForThing(PrintingTracker.CurrentThing);
            if (config == null) return;

            string objectType = config.Identifier;
            bool isRotated = PrintingTracker.CurrentThing.Rotation == Rot4.East || PrintingTracker.CurrentThing.Rotation == Rot4.West;
            bool isTopOrLeft = isRotated ?
                (PrintingTracker.CurrentThing.Rotation == Rot4.East) :
                (PrintingTracker.CurrentThing.Rotation == Rot4.North || PrintingTracker.CurrentThing.Rotation == Rot4.East);

            Vector2[] currentUVs = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                currentUVs[i] = uvs[i];
            }

            Vector2 uvCenter = Vector2.zero;
            for (int i = 0; i < 4; i++)
            {
                uvCenter += currentUVs[i];
            }
            uvCenter /= 4f;

            int count = subMesh.verts.Count;
            Vector3 adjustedCenter = center;

            RenderCache.CleanupCache();

            if (isRotated)
            {
                switch (renderPass)
                {
                    case 0:
                        adjustedCenter = center + GetPosition("base_rot_pos", objectType);
                        float baseWidth = GetDimension("base_rot_dim", objectType).z * 0.5f;
                        float baseHeight = GetDimension("base_rot_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(-baseHeight, 0f, -baseWidth));
                        subMesh.verts.Add(new Vector3(-baseHeight, 0f, baseWidth));
                        subMesh.verts.Add(new Vector3(baseHeight, 0f, baseWidth));
                        subMesh.verts.Add(new Vector3(baseHeight, 0f, -baseWidth));
                        ApplyUVTransform(currentUVs, GetUVZoom("base_rot_uv_zoom", objectType, false, isTopOrLeft) / 100f, GetUVOffset("base_rot_uv_offset", objectType, false, isTopOrLeft) / 100f, uvCenter);
                        break;

                    case 1:
                        adjustedCenter = center + GetPosition("sur_rot_pos", objectType);
                        float southWidth = GetDimension("sur_rot_dim", objectType).z * 0.5f;
                        float southHeight = GetDimension("sur_rot_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(-southWidth, 0f, 0f));
                        subMesh.verts.Add(new Vector3(-southWidth, southHeight, 0f));
                        subMesh.verts.Add(new Vector3(southWidth, southHeight, 0f));
                        subMesh.verts.Add(new Vector3(southWidth, 0f, 0f));
                        ApplyUVTransform(currentUVs, GetUVZoom("sur_rot_uv_zoom", objectType, false, isTopOrLeft) / 100f, GetUVOffset("sur_rot_uv_offset", objectType, false, isTopOrLeft) / 100f, uvCenter);
                        break;

                    case 2:
                        adjustedCenter = center + GetPosition("este_rot_pos", objectType);
                        float eastLength = GetDimension("este_rot_dim", objectType).z * 0.5f;
                        float eastHeight = GetDimension("este_rot_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(0f, 0f, -eastLength));
                        subMesh.verts.Add(new Vector3(0f, eastHeight, -eastLength));
                        subMesh.verts.Add(new Vector3(0f, eastHeight, eastLength));
                        subMesh.verts.Add(new Vector3(0f, 0f, eastLength));
                        ApplyUVTransform(currentUVs, GetUVZoom("este_rot_uv_zoom", objectType, false, isTopOrLeft) / 100f, GetUVOffset("este_rot_uv_offset", objectType, false, isTopOrLeft) / 100f, uvCenter);
                        break;

                    case 3:
                        adjustedCenter = center + GetPosition("oeste_rot_pos", objectType);
                        float westLength = GetDimension("oeste_rot_dim", objectType).z * 0.5f;
                        float westHeight = GetDimension("oeste_rot_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(0f, 0f, westLength));
                        subMesh.verts.Add(new Vector3(0f, westHeight, westLength));
                        subMesh.verts.Add(new Vector3(0f, westHeight, -westLength));
                        subMesh.verts.Add(new Vector3(0f, 0f, -westLength));
                        ApplyUVTransform(currentUVs, GetUVZoom("oeste_rot_uv_zoom", objectType, false, isTopOrLeft) / 100f, GetUVOffset("oeste_rot_uv_offset", objectType, false, isTopOrLeft) / 100f, uvCenter);
                        break;
                }
            }
            else
            {
                switch (renderPass)
                {
                    case 0:
                        adjustedCenter = center + GetPosition("base_pos", objectType);
                        float baseWidth = GetDimension("base_dim", objectType).z * 0.5f;
                        float baseHeight = GetDimension("base_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(-baseWidth, 0f, -baseHeight));
                        subMesh.verts.Add(new Vector3(-baseWidth, 0f, baseHeight));
                        subMesh.verts.Add(new Vector3(baseWidth, 0f, baseHeight));
                        subMesh.verts.Add(new Vector3(baseWidth, 0f, -baseHeight));
                        ApplyUVTransform(currentUVs, GetUVZoom("base_uv_zoom", objectType, isTopOrLeft, false) / 100f, GetUVOffset("base_uv_offset", objectType, isTopOrLeft, false) / 100f, uvCenter);
                        break;

                    case 1:
                        adjustedCenter = center + GetPosition("sur_pos", objectType);
                        float southWidth = GetDimension("sur_dim", objectType).z * 0.5f;
                        float southHeight = GetDimension("sur_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(-southWidth, 0f, 0f));
                        subMesh.verts.Add(new Vector3(-southWidth, southHeight, 0f));
                        subMesh.verts.Add(new Vector3(southWidth, southHeight, 0f));
                        subMesh.verts.Add(new Vector3(southWidth, 0f, 0f));
                        ApplyUVTransform(currentUVs, GetUVZoom("sur_uv_zoom", objectType, isTopOrLeft, false) / 100f, GetUVOffset("sur_uv_offset", objectType, isTopOrLeft, false) / 100f, uvCenter);
                        break;

                    case 2:
                        adjustedCenter = center + GetPosition("este_pos", objectType);
                        float eastWidth = GetDimension("este_dim", objectType).z * 0.5f;
                        float eastHeight = GetDimension("este_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(0f, 0f, -eastWidth));
                        subMesh.verts.Add(new Vector3(0f, eastHeight, -eastWidth));
                        subMesh.verts.Add(new Vector3(0f, eastHeight, eastWidth));
                        subMesh.verts.Add(new Vector3(0f, 0f, eastWidth));
                        ApplyUVTransform(currentUVs, GetUVZoom("este_uv_zoom", objectType, isTopOrLeft, false) / 100f, GetUVOffset("este_uv_offset", objectType, isTopOrLeft, false) / 100f, uvCenter);
                        break;

                    case 3:
                        adjustedCenter = center + GetPosition("oeste_pos", objectType);
                        float westWidth = GetDimension("oeste_dim", objectType).z * 0.5f;
                        float westHeight = GetDimension("oeste_dim", objectType).y * 0.5f;
                        subMesh.verts.Add(new Vector3(0f, 0f, westWidth));
                        subMesh.verts.Add(new Vector3(0f, westHeight, westWidth));
                        subMesh.verts.Add(new Vector3(0f, westHeight, -westWidth));
                        subMesh.verts.Add(new Vector3(0f, 0f, -westWidth));
                        ApplyUVTransform(currentUVs, GetUVZoom("oeste_uv_zoom", objectType, isTopOrLeft, false) / 100f, GetUVOffset("oeste_uv_offset", objectType, isTopOrLeft, false) / 100f, uvCenter);
                        break;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                Vector3 before = subMesh.verts[count + i];
                if (rot != 0f)
                {
                    float angle = rot * ((float)System.Math.PI / 180f) * -1f;
                    float x = before.x;
                    float z = before.z;
                    float cos = Mathf.Cos(angle);
                    float sin = Mathf.Sin(angle);
                    before = new Vector3(
                        x * cos - z * sin,
                        before.y,
                        x * sin + z * cos
                    );
                }
                subMesh.verts[count + i] = before + adjustedCenter;
                subMesh.uvs.Add(new Vector3(currentUVs[i].x, currentUVs[i].y, uvzPayload));
                subMesh.colors.Add(colors[i]);
            }

            subMesh.tris.Add(count);
            subMesh.tris.Add(count + 1);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count + 2);
            subMesh.tris.Add(count + 3);
            subMesh.tris.Add(count);

            renderPass++;
            if (renderPass >= 4)
            {
                renderPass = 0;
            }
        }

        private static void ApplyUVTransform(Vector2[] uvs, float zoom, Vector2 offset, Vector2 focalPoint)
        {
            float scale = 1f / zoom;

            for (int i = 0; i < uvs.Length; i++)
            {
                Vector2 uv = uvs[i];
                uv = focalPoint + (uv - focalPoint) * scale;
                uv += offset;
                uvs[i] = uv;
            }
        }
    }
}