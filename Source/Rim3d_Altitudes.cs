using HarmonyLib;
using UnityEngine;
using Verse;

namespace Rim3D
{
    [HarmonyPatch(typeof(Altitudes))]
    [HarmonyPatch("AltitudeFor")]
    [HarmonyPatch(new[] { typeof(AltitudeLayer) })]
    public class Patch_Altitudes_AltitudeFor
    {
        private const float ORIGINAL_WEATHER_HEIGHT = 2.5f;
        private const float ORIGINAL_FOGOFWAR_HEIGHT = 1f;
        private const float ORIGINAL_MAPDATAOVERLAY_HEIGHT = 13.84615392f;
        private const float ORIGINAL_SILHOUETTES_HEIGHT = 13.46153846f;
        private const float ORIGINAL_MOTE_HEIGHT = 1f;

        [HarmonyPrefix]
        public static bool Prefix(AltitudeLayer alt, ref float __result)
        {
            switch (alt)
            {
                case AltitudeLayer.Weather:
                    __result = ORIGINAL_WEATHER_HEIGHT;
                    return false;
                case AltitudeLayer.LightingOverlay:
                    __result = Core.settings.lightingOverlayHeight;
                    return false;
                case AltitudeLayer.FogOfWar:
                    __result = ORIGINAL_FOGOFWAR_HEIGHT;
                    return false;
                case AltitudeLayer.MapDataOverlay:
                    __result = ORIGINAL_MAPDATAOVERLAY_HEIGHT;
                    return false;
                case AltitudeLayer.Silhouettes:
                    __result = ORIGINAL_SILHOUETTES_HEIGHT;
                    return false;
                case AltitudeLayer.MoteLow:
                case AltitudeLayer.MoteOverhead:
                    __result = ORIGINAL_MOTE_HEIGHT;
                    return false;
            }

            __result = (float)alt * 0.01f;
            return false;
        }
    }
}