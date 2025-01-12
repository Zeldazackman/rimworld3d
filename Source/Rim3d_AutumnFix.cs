using HarmonyLib;
using UnityEngine;
using Verse;
using System.IO;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public static class PlantMaterialManager
    {
        public static AssetBundle plantBundle;
        public static Material plantMaterial;

        static PlantMaterialManager()
        {
            string bundlePath = Path.Combine(GenFilePaths.ModsFolderPath, "rim3d-majaus", "Resources", "AssetBundles", "plantcutout3d");
            //Log.Message("[Rim3D Plant] Loading bundle from: " + bundlePath);

            plantBundle = AssetBundle.LoadFromFile(bundlePath);
            if (plantBundle != null)
            {
                plantMaterial = plantBundle.LoadAsset<Material>("PlantCutout3d");
                if (plantMaterial != null)
                {
                    //Log.Message("[Rim3D Plant] Successfully loaded plant material");
                }
                else
                {
                    Log.Error("[Rim3D Plant] Failed to load PlantCutout3d material from bundle");
                }
            }
            else
            {
                Log.Error("[Rim3D Plant] Failed to load plant bundle from: " + bundlePath);
            }
        }
    }

    [StaticConstructorOnStartup]
    public class Patch_MaterialPool
    {
        private static Dictionary<MaterialRequest, Material> matDictionary;
        private static Dictionary<Material, MaterialRequest> matDictionaryReverse;
        private static MethodInfo createMethod;

        static Patch_MaterialPool()
        {
            var harmony = new Harmony("rimworld.rim3d.materialpool");

            matDictionary = AccessTools.StaticFieldRefAccess<Dictionary<MaterialRequest, Material>>(typeof(MaterialPool), "matDictionary");
            matDictionaryReverse = AccessTools.StaticFieldRefAccess<Dictionary<Material, MaterialRequest>>(typeof(MaterialPool), "matDictionaryReverse");
            createMethod = AccessTools.Method("Verse.MaterialAllocator:Create", new[] { typeof(Shader) });

            harmony.Patch(AccessTools.Method(typeof(MaterialPool), "MatFrom", new[] { typeof(MaterialRequest) }),
                         new HarmonyMethod(typeof(Patch_MaterialPool), nameof(MatFrom)));
        }

        public static bool MatFrom(MaterialRequest req, ref Material __result)
        {
            if (!UnityData.IsInMainThread)
            {
                Log.Error("Tried to get a material from a different thread.");
                __result = null;
                return false;
            }
            if (req.mainTex == null && req.needsMainTex)
            {
                Log.Error("MatFrom with null sourceTex.");
                __result = BaseContent.BadMat;
                return false;
            }
            if (req.shader == null)
            {
                Log.Warning("Matfrom with null shader.");
                __result = BaseContent.BadMat;
                return false;
            }
            if (req.maskTex != null && !req.shader.SupportsMaskTex())
            {
                Log.Error("MaterialRequest has maskTex but shader does not support it. req=" + req.ToString());
                req.maskTex = null;
            }
            req.color = (Color32)req.color;
            req.colorTwo = (Color32)req.colorTwo;
            if (!matDictionary.TryGetValue(req, out var value))
            {
                if (req.shader == ShaderDatabase.CutoutPlant && Core.settings.plantShaderFix)
                {
                    value = (Material)createMethod.Invoke(null, new object[] { PlantMaterialManager.plantMaterial.shader });
                }
                else
                {
                    value = (Material)createMethod.Invoke(null, new object[] { req.shader });
                }
                value.name = req.shader.name;
                if (req.mainTex != null)
                {
                    Material material = value;
                    material.name = material.name + "_" + req.mainTex.name;
                    value.mainTexture = req.mainTex;
                }
                value.color = req.color;
                if (req.maskTex != null)
                {
                    value.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
                    value.SetColor(ShaderPropertyIDs.ColorTwo, req.colorTwo);
                }
                if (req.renderQueue != 0)
                {
                    value.renderQueue = req.renderQueue;
                }
                if (!req.shaderParameters.NullOrEmpty())
                {
                    for (int i = 0; i < req.shaderParameters.Count; i++)
                    {
                        req.shaderParameters[i].Apply(value);
                    }
                }
                matDictionary.Add(req, value);
                matDictionaryReverse.Add(value, req);
                if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
                {
                    WindManager.Notify_PlantMaterialCreated(value);
                }
            }
            __result = value;
            return false;
        }
    }
}