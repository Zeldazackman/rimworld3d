using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace Rim3D
{
    public static class PrintingTracker
    {
        public static Thing CurrentThing = null;
    }

    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("Print")]
    public class Patch_Thing_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Thing __instance)
        {
            PrintingTracker.CurrentThing = __instance;
        }
        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Plant))]
    [HarmonyPatch("Print")]
    public class Patch_Plant_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Plant __instance)
        {
            PrintingTracker.CurrentThing = __instance;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Graphic_Cluster))]
    [HarmonyPatch("Print")]
    public class Patch_GraphicCluster_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Thing thing)
        {
            PrintingTracker.CurrentThing = thing;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Graphic_Random))]
    [HarmonyPatch("Print")]
    public class Patch_GraphicRandom_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Thing thing)
        {
            PrintingTracker.CurrentThing = thing;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Graphic_RandomRotated))]
    [HarmonyPatch("Print")]
    public class Patch_GraphicRandomRotated_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Thing thing)
        {
            PrintingTracker.CurrentThing = thing;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Graphic_Linked))]
    [HarmonyPatch("Print")]
    public class Patch_GraphicLinked_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Thing thing)
        {
            PrintingTracker.CurrentThing = thing;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Graphic_LinkedCornerFiller))]
    [HarmonyPatch("Print")]
    public class Patch_GraphicLinkedCornerFiller_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Thing thing)
        {
            PrintingTracker.CurrentThing = thing;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Graphic_LinkedCornerOverlay))]
    [HarmonyPatch("Print")]
    public class Patch_GraphicLinkedCornerOverlay_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Thing thing)
        {
            PrintingTracker.CurrentThing = thing;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(Blight))]
    [HarmonyPatch("Print")]
    public class Patch_Blight_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Blight __instance)
        {
            PrintingTracker.CurrentThing = __instance;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(MinifiedThing))]
    [HarmonyPatch("Print")]
    public class Patch_MinifiedThing_Print
    {
        [HarmonyPrefix]
        public static void Prefix(MinifiedThing __instance)
        {
            PrintingTracker.CurrentThing = __instance;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }


    [HarmonyPatch(typeof(SectionLayer_BuildingsDamage))]
    [HarmonyPatch("PrintDamageVisualsFrom")]
    public class Patch_BuildingsDamage_Print
    {
        [HarmonyPrefix]
        public static void Prefix(Building b)
        {
            PrintingTracker.CurrentThing = b;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(SectionLayer_BuildingsDamage))]
    [HarmonyPatch("DrawLinkableCornersAndEdges")]
    public class Patch_BuildingsDamage_DrawCorners
    {
        [HarmonyPrefix]
        public static void Prefix(Building b)
        {
            PrintingTracker.CurrentThing = b;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }

    [HarmonyPatch(typeof(SectionLayer_BuildingsDamage))]
    [HarmonyPatch("DrawFullThingCorners")]
    public class Patch_BuildingsDamage_DrawFullCorners
    {
        [HarmonyPrefix]
        public static void Prefix(Building b)
        {
            PrintingTracker.CurrentThing = b;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            PrintingTracker.CurrentThing = null;
        }
    }
}
