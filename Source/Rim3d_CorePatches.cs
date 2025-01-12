using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Rim3D
{
    [HarmonyPatch(typeof(Root))]
    [HarmonyPatch("Update")]
    public class Patch_RootUpdate
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Core.Instance == null) return;
            try
            {
                if (Core.settings != null)
                {
                    Core.Instance.cameraController.UpdateCamera();
                }
            }
            catch (Exception)
            {
            }
        }
    }

    [HarmonyPatch(typeof(Root))]
    [HarmonyPatch("OnGUI")]
    public class Patch_RootOnGUI
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Core.Instance == null) return;
            try
            {
                if (Core.settings != null)
                {
                    Core.Instance.OnGUI();
                }
            }
            catch (Exception)
            {
            }
        }
    }

    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("UpdatePlay")]
    public class Patch_GameUpdatePlay
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Core.Instance == null) return;
            try
            {
                if (Core.settings != null)
                {
                    Core.Instance.Tick();
                }
            }
            catch (Exception)
            {
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_SaveFileList_Load))]
    [HarmonyPatch("DoFileInteraction")]
    public class Patch_DoFileInteraction
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            CorePatches.EnsureMode2D();
        }
    }

    [HarmonyPatch(typeof(GenScene))]
    [HarmonyPatch("GoToMainMenu")]
    public class Patch_GoToMainMenu
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            CorePatches.EnsureMode2D();
        }
    }

    public static class CorePatches
    {
        public static void EnsureMode2D()
        {
            if (Core.Instance == null) return;
            if (Core.mode3d)
            {
                Core.Instance.stateManager.off();
                Core.mode3d = false;
            }
        }
    }
}