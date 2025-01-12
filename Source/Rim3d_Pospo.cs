using UnityEngine;
using Verse;
using System.IO;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public static class PostProcessManager
    {
        public static AssetBundle pospoBundle;
        public static Material pospoMaterial;
        private static RenderTexture postProcessTexture;

        static PostProcessManager()
        {
            string bundlePath = Path.Combine(GenFilePaths.ModsFolderPath, "rim3d-majaus", "Resources", "AssetBundles", "posprohdr");
            //Log.Message("[Rim3D] Loading pospo bundle from: " + bundlePath);

            pospoBundle = AssetBundle.LoadFromFile(bundlePath);
            if (pospoBundle != null)
            {
                pospoMaterial = pospoBundle.LoadAsset<Material>("PospoHdr");
                if (pospoMaterial != null)
                {
                    //Log.Message("[Rim3D] Successfully loaded pospo material");
                }
                else
                {
                    Log.Error("[Rim3D] Failed to load pospo material from bundle");
                }
            }
            else
            {
                Log.Error("[Rim3D] Failed to load pospo bundle from: " + bundlePath);
            }
        }

        public static void UpdatePostProcess()
        {
            if (!Core.mode3d || !Core.settings.enablePostProcess || pospoMaterial == null) return;

            if (postProcessTexture == null || postProcessTexture.width != Screen.width || postProcessTexture.height != Screen.height)
            {
                if (postProcessTexture != null)
                {
                    postProcessTexture.Release();
                }

                postProcessTexture = new RenderTexture(Screen.width, Screen.height, 0);
                postProcessTexture.Create();
            }

            pospoMaterial.SetFloat("_Exposure", Core.settings.pospoExposure);
            pospoMaterial.SetFloat("_Contrast", Core.settings.pospoContrast);
            pospoMaterial.SetFloat("_Saturation", Core.settings.pospoSaturation);
            pospoMaterial.SetFloat("_Bloom", Core.settings.pospoBloom);
            pospoMaterial.SetFloat("_BloomThreshold", Core.settings.pospoBloomThreshold);
        }

        public static void RenderPostProcess()
        {
            if (!Core.mode3d || !Core.settings.enablePostProcess || pospoMaterial == null) return;

            if (Event.current.type != EventType.Repaint) return;

            RenderTexture source = RenderTexture.active;
            Graphics.Blit(source, postProcessTexture);
            Graphics.Blit(postProcessTexture, source, pospoMaterial);
        }

        public static void Cleanup()
        {
            if (postProcessTexture != null)
            {
                postProcessTexture.Release();
                postProcessTexture = null;
            }

            if (pospoBundle != null)
            {
                pospoBundle.Unload(true);
                pospoBundle = null;
            }

            if (pospoMaterial != null)
            {
                Object.Destroy(pospoMaterial);
                pospoMaterial = null;
            }
        }
    }
}