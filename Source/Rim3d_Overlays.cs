using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    [HarmonyPatch(typeof(SilhouetteUtility))]
    [HarmonyPatch("CanHighlightAny")]
    public class Patch_SilhouetteUtility_CanHighlightAny
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (Core.mode3d)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(OverlayDrawer))]
    [HarmonyPatch("DrawAllOverlays")]
    public class Patch_OverlayDrawer_DrawAllOverlays
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !Core.mode3d;
        }
    }
}
