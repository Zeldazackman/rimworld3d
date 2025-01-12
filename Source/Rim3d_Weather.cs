using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace Rim3D
{
    [HarmonyPatch(typeof(SkyOverlay))]
    [HarmonyPatch("DrawOverlay")]
    public static class SkyOverlay_DrawOverlay_Patch
    {
        public static readonly Dictionary<Material, Material> MaterialCache = new Dictionary<Material, Material>();

        public static bool Prefix(SkyOverlay __instance, Map map)
        {
            if (!Core.mode3d) return true;
            if (Core.mode3d && !Core.settings.enableWeatherLayer) return false;

            if (map.weatherManager.lastWeather != map.weatherManager.curWeather)
            {
                return false;
            }

            Camera camera = Find.Camera;
            float size = Core.settings.weatherLayerSize * 2f;
            Vector3 scale = new Vector3(size * camera.aspect * 0.02f, 1f, size * 0.02f);

            Vector3 cameraOffset = camera.transform.forward * Core.settings.weatherCameraOffset;
            Quaternion rotation = Quaternion.Euler(Core.settings.weatherRotationX, Core.settings.weatherRotationY, Core.settings.weatherRotationZ);

            Matrix4x4 matrix = Matrix4x4.TRS(
                camera.transform.position + cameraOffset,
                rotation,
                scale);

            Material baseMaterial = __instance.worldOverlayMat;
            if (baseMaterial != null)
            {
                Material scaledMaterial;
                if (!MaterialCache.TryGetValue(baseMaterial, out scaledMaterial))
                {
                    foreach (var material in MaterialCache.Values)
                    {
                        if (material != null)
                        {
                            Object.Destroy(material);
                        }
                    }
                    MaterialCache.Clear();

                    scaledMaterial = new Material(baseMaterial.shader);
                    scaledMaterial.CopyPropertiesFromMaterial(baseMaterial);
                    MaterialCache[baseMaterial] = scaledMaterial;
                }

                scaledMaterial.color = baseMaterial.color;
                Vector2 textureScale = new Vector2(Core.settings.textureScaleX, Core.settings.textureScaleY);
                scaledMaterial.mainTextureScale = textureScale;

                if (scaledMaterial.HasProperty("_MainTex2"))
                {
                    scaledMaterial.SetTextureScale("_MainTex2", textureScale);
                }

                if (baseMaterial.HasProperty("_MainTex"))
                {
                    Vector2 textureOffset = baseMaterial.GetTextureOffset("_MainTex");
                    scaledMaterial.SetTextureOffset("_MainTex", textureOffset * Core.settings.speedPrimary);
                }

                if (baseMaterial.HasProperty("_MainTex2"))
                {
                    Vector2 textureOffset = baseMaterial.GetTextureOffset("_MainTex2");
                    scaledMaterial.SetTextureOffset("_MainTex2", textureOffset * Core.settings.speedSecondary);
                }

                Graphics.DrawMesh(MeshPool.plane10, matrix, scaledMaterial, 0);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(WeatherManager))]
    [HarmonyPatch("TransitionTo")]
    public static class WeatherManager_TransitionTo_Patch
    {
        public static void Prefix()
        {
            if (Core.mode3d)
            {
                foreach (var material in SkyOverlay_DrawOverlay_Patch.MaterialCache.Values)
                {
                    if (material != null)
                    {
                        Object.Destroy(material);
                    }
                }
                SkyOverlay_DrawOverlay_Patch.MaterialCache.Clear();
            }
        }
    }

    // Snow mesh regeneration can cause significant performance drops in 3D mode when drawing the full map,
    // as each snow depth change triggers mesh updates. This patch prevents constant mesh regeneration by:
    // 1. Allowing snow to accumulate normally so game mechanics (movement speed, etc) work as intended
    // 2. Only allowing visual mesh regeneration every N snow changes when in 3D mode with full map drawing
    // 3. Always allowing path cost recalculation so pathing remains accurate
    // This provides a good balance between visual feedback and performance.
    [HarmonyPatch(typeof(SnowGrid))]
    [HarmonyPatch("CheckVisualOrPathCostChange")]
    public static class SnowGrid_CheckVisualOrPathCostChange_Patch
    {
        private static int updateCounter = 0;

        public static void ResetCounter()
        {
            updateCounter = 0;
        }

        public static bool Prefix(IntVec3 c, float oldDepth, float newDepth, SnowGrid __instance)
        {
            if (Core.mode3d && !Core.settings.enableSnowRefresh) return false;
            if (Core.mode3d && Core.settings.drawFullMap)
            {
                updateCounter++;

                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                if (SnowUtility.GetSnowCategory(oldDepth) != SnowUtility.GetSnowCategory(newDepth))
                {
                    map.pathing.RecalculatePerceivedPathCostAt(c);
                }
                return (updateCounter % Core.settings.updateSnowInterval) == 0;
            }
            return true;
        }
    }
}