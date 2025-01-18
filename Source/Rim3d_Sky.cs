using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.IO;
using System.Linq;

namespace Rim3D
{
    [StaticConstructorOnStartup]
    public static class SkyManager
    {
        private static GameObject skyboxSphere;
        private static Material skyboxMaterial;
        private static AssetBundle skyboxBundle;
        private static bool skyboxInitialized = false;
        private static float lastDayPercent = -1f;
        private static float lastSunGlow = -1f;
        private static string currentCustomTexturePath = null;
        private static Material lastActiveMaterial = null;
        private static Camera lastCamera = null;

        public static void ActivateSkybox()
        {
            if (!Core.settings.enableSkybox) return;

            if (!skyboxInitialized || lastCamera != Find.Camera)
            {
                if (skyboxInitialized)
                {
                    CleanupSkybox();
                }
                InitializeSkybox();
            }
            else if (skyboxSphere != null)
            {
                skyboxSphere.SetActive(true);
                if (lastActiveMaterial != null)
                {
                    skyboxSphere.GetComponent<MeshRenderer>().material = lastActiveMaterial;
                }
            }
        }

        public static void SetCustomSkyboxTexture(string filename)
        {
            Core.settings.selectedSkybox = filename;
            Core.settings.Write();
            Core.settings.SaveCurrentSettings();

            string fullPath = Path.Combine(Core.Instance.Content.RootDir, "Resources", "Skys", filename);

            if (!File.Exists(fullPath))
            {
                Log.Error($"[Rim3D Sky] Texture file not found at: {fullPath}");
                return;
            }

            if (currentCustomTexturePath == fullPath)
            {
                return;
            }

            try
            {
                byte[] fileData = File.ReadAllBytes(fullPath);

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (texture.LoadImage(fileData))
                {
                    texture.wrapMode = TextureWrapMode.Repeat;
                    texture.filterMode = FilterMode.Bilinear;
                    texture.anisoLevel = 1;

                    if (skyboxMaterial != null && skyboxSphere != null)
                    {
                        skyboxMaterial.SetTexture("_MainTex", texture);
                        skyboxMaterial.SetFloat("_Exposure", Core.settings.skyboxExposure);
                        skyboxMaterial.SetFloat("_Rotation", Core.settings.skyboxRotation);
                        skyboxSphere.GetComponent<MeshRenderer>().material = skyboxMaterial;
                        skyboxSphere.GetComponent<MeshRenderer>().material.renderQueue = 1000;
                        lastActiveMaterial = skyboxMaterial;
                    }
                }
                else
                {
                    Log.Error("[Rim3D Sky] Failed to load image data into texture");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"[Rim3D Sky] Error loading texture file: {e.Message}\n{e.StackTrace}");
            }

            currentCustomTexturePath = fullPath;
        }

        private static void LoadDefaultSkybox()
        {
            string skyboxPath = Path.Combine(Core.Instance.Content.RootDir, "Resources", "Skys");
            if (Directory.Exists(skyboxPath))
            {
                string skyboxToLoad = Core.settings.selectedSkybox;
                if (string.IsNullOrEmpty(skyboxToLoad) || !File.Exists(Path.Combine(skyboxPath, skyboxToLoad)))
                {
                    skyboxToLoad = Directory.GetFiles(skyboxPath)
                        .Where(f => f.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
                                   f.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                        .Select(Path.GetFileName)
                        .FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(skyboxToLoad))
                {
                    SetCustomSkyboxTexture(skyboxToLoad);
                }
                else
                {
                    Log.Error("[Rim3D Sky] No skybox textures found in Skys directory");
                }
            }
            else
            {
                Log.Error("[Rim3D Sky] Skybox directory not found");
            }
        }

        public static void InitializeSkybox()
        {
            if (Core.settings.enableSkybox && !skyboxInitialized)
            {
                skyboxSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                skyboxSphere.transform.localScale = new Vector3(1000f, 1000f, 1000f);
                skyboxSphere.transform.parent = Current.Camera.transform;
                skyboxSphere.transform.localPosition = new Vector3(0f, Core.settings.skyboxDegrees, 0f);
                skyboxSphere.transform.localRotation = Quaternion.Euler(0f, Core.settings.skyboxDegreesHorizontal, Core.settings.skyboxVerticalOffset * 180f);

                Mesh mesh = skyboxSphere.GetComponent<MeshFilter>().mesh;
                Vector3[] normals = mesh.normals;
                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = -normals[i];
                }
                mesh.normals = normals;

                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i];
                    triangles[i] = triangles[i + 2];
                    triangles[i + 2] = temp;
                }
                mesh.triangles = triangles;

                if (skyboxBundle == null)
                {
                    string bundlePath = Path.Combine(Core.Instance.Content.RootDir, "Resources", "AssetBundles", "majausky");
                    skyboxBundle = AssetBundle.LoadFromFile(bundlePath);
                    if (skyboxBundle != null)
                    {
                        skyboxMaterial = skyboxBundle.LoadAsset<Material>("MajauSky");
                    }
                    else
                    {
                        Log.Error("[Rim3D Sky] Failed to load skybox bundle");
                    }
                }

                if (skyboxMaterial != null)
                {
                    LoadDefaultSkybox();
                    skyboxSphere.layer = LayerMask.NameToLayer("Water");
                    skyboxInitialized = true;
                    lastCamera = Current.Camera;
                }
                else
                {
                    Log.Message("[Rim3D] ERROR: Skybox material not found in bundle");
                }
            }

            if (!Core.settings.enableSkybox && skyboxInitialized)
            {
                CleanupSkybox();
            }
        }

        public static void DeactivateSkybox()
        {
            if (skyboxSphere != null)
            {
                skyboxSphere.SetActive(false);
            }
        }

        private static void CleanupSkybox()
        {
            if (skyboxSphere != null)
            {
                GameObject.Destroy(skyboxSphere);
                skyboxSphere = null;
            }

            if (lastActiveMaterial != null)
            {
                GameObject.Destroy(lastActiveMaterial.mainTexture);
                GameObject.Destroy(lastActiveMaterial);
                lastActiveMaterial = null;
            }

            if (skyboxBundle != null)
            {
                skyboxBundle.Unload(true);
                skyboxBundle = null;
            }
            skyboxMaterial = null;
            skyboxInitialized = false;
            currentCustomTexturePath = null;
            lastCamera = null;
        }

        private static void UpdateSkyboxByGameTime()
        {
            if (Find.CurrentMap == null) return;

            float dayPercent = GenLocalDate.DayPercent(Find.CurrentMap);
            float sunGlow = GenCelestial.CurCelestialSunGlow(Find.CurrentMap);

            if (Mathf.Approximately(dayPercent, lastDayPercent) && Mathf.Approximately(sunGlow, lastSunGlow))
                return;

            lastDayPercent = dayPercent;
            lastSunGlow = sunGlow;

            if (skyboxSphere != null && skyboxSphere.GetComponent<MeshRenderer>().material != null)
            {
                float rotation = (dayPercent * 360f) - 180f;
                Core.settings.skyboxRotation = rotation;

                if (Core.settings.enableAutoExposure)
                {
                    float exposure = Mathf.Lerp(0.2f, 1f, sunGlow);
                    Core.settings.skyboxExposure = exposure;
                    skyboxSphere.GetComponent<MeshRenderer>().material.SetFloat("_Exposure", exposure);
                }
                else
                {
                    Core.settings.skyboxExposure = 1f;
                    skyboxSphere.GetComponent<MeshRenderer>().material.SetFloat("_Exposure", 1f);
                }

                skyboxSphere.GetComponent<MeshRenderer>().material.SetFloat("_Rotation", rotation);
            }
        }

        public static void UpdateSkybox()
        {
            if (!Core.settings.enableSkybox)
            {
                if (skyboxInitialized)
                {
                    CleanupSkybox();
                }
                return;
            }

            if (Core.mode3d)
            {
                if (!skyboxInitialized || lastCamera != Find.Camera)
                {
                    InitializeSkybox();
                }
                else if (skyboxSphere != null && skyboxSphere.GetComponent<MeshRenderer>().material != null)
                {
                    Core.settings.skyboxDegrees = Mathf.Clamp(Core.settings.skyboxDegrees, -400f, 400f);
                    skyboxSphere.transform.localPosition = new Vector3(0f, Core.settings.skyboxDegrees, 0f);
                    skyboxSphere.transform.localRotation = Quaternion.Euler(0f, Core.settings.skyboxDegreesHorizontal, Core.settings.skyboxVerticalOffset * 180f);
                    Material material = skyboxSphere.GetComponent<MeshRenderer>().material;

                    if (Core.settings.gameControlSky)
                    {
                        UpdateSkyboxByGameTime();
                    }
                    else
                    {
                        material.SetFloat("_Exposure", Core.settings.skyboxExposure);
                        material.SetFloat("_Rotation", Core.settings.skyboxRotation);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MapEdgeClipDrawer))]
    [HarmonyPatch("DrawClippers")]
    public class Patch_MapEdgeClipDrawer_DrawClippers
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !Core.mode3d;
        }
    }
}