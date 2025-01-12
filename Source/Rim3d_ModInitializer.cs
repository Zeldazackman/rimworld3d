using HarmonyLib;
using Verse;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public class ModMain
    {
        static ModMain()
        {
            try
            {
                KeyBindings.ToggleRim3D = DefDatabase<KeyBindingDef>.GetNamed("ToggleRim3D");
                KeyBindings.ToggleRim3DOptions = DefDatabase<KeyBindingDef>.GetNamed("ToggleRim3DOptions");
                KeyBindings.ToggleRim3DPerformance = DefDatabase<KeyBindingDef>.GetNamed("ToggleRim3DPerformance");

                var harmony = new Harmony("com.majaus.rim3d");
                harmony.PatchAll();
                var methods = harmony.GetPatchedMethods();
            }
            catch (System.Exception ex)
            {
                DebugLog.Log($"Error in ModMain constructor: {ex}");
            }
        }
    }
}
