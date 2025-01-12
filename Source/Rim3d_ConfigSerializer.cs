using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using Verse;

namespace Rim3D
{
    public class ObjectIndex
    {
        public List<string> ObjectIdentifiers { get; set; } = new List<string>();
    }

    public class BoxSettingData
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ObjectData
    {
        public string Identifier { get; set; }
        public MatchType MatchType { get; set; }
        public bool IsPlane { get; set; }
        public bool IsCube { get; set; }
        public float HeightOffset { get; set; }
        public List<BoxSettingData> BoxSettings { get; set; } = new List<BoxSettingData>();

        public Dictionary<string, string> GetBoxSettingsDictionary()
        {
            var dict = new Dictionary<string, string>();
            foreach (var setting in BoxSettings)
            {
                dict[setting.Key] = setting.Value;
            }
            return dict;
        }
    }

    public class CameraSettings
    {
        public float CamSpeed { get; set; }
        public float Angle { get; set; }
        public float Fov { get; set; }
        public float Z { get; set; }
        public float Far { get; set; }
        public bool DrawFullMap { get; set; }
        public bool Orthographic { get; set; }
        public float OrthographicSize { get; set; }
        public bool EnableTransition { get; set; }
        public float TransitionDuration { get; set; }
        public float FogHeight { get; set; }
        public float FogMaxHeight { get; set; }
        public bool RefreshAdjacentFog { get; set; }
        public bool EnableMapMountains { get; set; }
        public float LightingOverlayHeight { get; set; }
        public bool EnableSkybox { get; set; }
        public float SkyboxDegrees { get; set; }
        public float SkyboxDegreesHorizontal { get; set; }
        public bool GameControlSky { get; set; }
        public bool EnableAutoExposure { get; set; }
        public float SkyboxRotation { get; set; }
        public float SkyboxExposure { get; set; }
        public float SkyboxVerticalOffset { get; set; }
        public string SelectedSkybox { get; set; }
        public bool PlantShaderFix { get; set; }
        public bool EnableMountains { get; set; }
        public float MountainHeight { get; set; }
        public float MountainBaseHeight { get; set; }
        public int MountainSeed { get; set; }
        public float MountainNorthDistance { get; set; }
        public float MountainEastDistance { get; set; }
        public float MountainWestDistance { get; set; }
        public float MountainQuality { get; set; }
        public bool EnableFireCampfire { get; set; }
        public bool EnableFireTorchLamp { get; set; }
        public bool EnableFireTorchWallLamp { get; set; }
        public bool EnableOtherFires { get; set; }
        public bool EnablePostProcess { get; set; }
        public float PospoExposure { get; set; }
        public float PospoContrast { get; set; }
        public float PospoSaturation { get; set; }
        public float PospoBloom { get; set; }
        public float PospoBloomThreshold { get; set; }
        public bool EnableWeatherLayer { get; set; }
        public float WeatherLayerSize { get; set; }
        public float TextureScaleX { get; set; }
        public float TextureScaleY { get; set; }
        public float WeatherRotationX { get; set; }
        public float WeatherRotationY { get; set; }
        public float WeatherRotationZ { get; set; }
        public float WeatherCameraOffset { get; set; }
        public float SpeedPrimary { get; set; }
        public float SpeedSecondary { get; set; }
        public bool EnableSnowRefresh { get; set; }
        public int UpdateSnowInterval { get; set; }
    }

    public static class ConfigSerializer
    {
        private static readonly string ConfigFolder = Path.Combine(GenFilePaths.ConfigFolderPath, "Rim3D");
        private static string PresetsFolder;
        private static string BaseFolder = Path.Combine(GenFilePaths.ConfigFolderPath, "Rim3D");
        private static string IndexPath => Path.Combine(BaseFolder, "ObjectIndex.xml");
        private static string CameraPath => Path.Combine(BaseFolder, "CameraSettings.xml");
        private static readonly XmlSerializer indexSerializer = new XmlSerializer(typeof(ObjectIndex));
        private static readonly XmlSerializer objectSerializer = new XmlSerializer(typeof(ObjectData));
        private static readonly XmlSerializer cameraSerializer = new XmlSerializer(typeof(CameraSettings));

        private static Dictionary<string, ObjectData> objectDataCache = new Dictionary<string, ObjectData>();
        private static CameraSettings cameraSettingsCache = null;
        private static ObjectIndex indexCache = null;

        public static void Initialize()
        {
            Initialize("");
        }

        public static void Initialize(string modRootDir)
        {
            try
            {
                PresetsFolder = string.IsNullOrEmpty(modRootDir) ? Path.Combine("Mods", "Rim3D", "Presets") : Path.Combine(modRootDir, "Presets");
                //Log.Message($"[Rim3D] Using presets folder: {PresetsFolder}");

                if (!Directory.Exists(ConfigFolder))
                {
                    Directory.CreateDirectory(ConfigFolder);
                }

                string presetIndexPath = Path.Combine(PresetsFolder, "ObjectIndex.xml");
                if (!Directory.GetFiles(ConfigFolder, "*.xml").Any())
                {
                    if (File.Exists(presetIndexPath))
                    {
                        //Log.Message($"[Rim3D] Loading from presets at: {presetIndexPath}");
                        BaseFolder = PresetsFolder;

                        using (StreamReader reader = new StreamReader(presetIndexPath))
                        {
                            indexCache = (ObjectIndex)indexSerializer.Deserialize(reader);
                        }

                        BaseFolder = ConfigFolder;
                        SaveIndex(indexCache);

                        BaseFolder = PresetsFolder;
                        var settings = LoadCameraSettings();
                        if (settings != null)
                        {
                            BaseFolder = ConfigFolder;
                            SaveCameraSettings(settings);
                        }

                        BaseFolder = PresetsFolder;
                        var objects = LoadAllObjectData();
                        //Log.Message($"[Rim3D] Loaded {objects.Count} objects from presets");
                        foreach (var obj in objects)
                        {
                            BaseFolder = ConfigFolder;
                            SaveObjectData(obj);
                        }
                    }
                    else
                    {
                        //Log.Message($"[Rim3D] No presets found at: {presetIndexPath}");
                    }
                }
                else
                {
                    //Log.Message($"[Rim3D] Using existing config at: {ConfigFolder}");
                }

                BaseFolder = ConfigFolder;
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Init] Error initializing config system: {ex}");
            }
        }

        public static ObjectData LoadObjectData(string identifier)
        {
            try
            {
                if (objectDataCache.ContainsKey(identifier))
                {
                    return objectDataCache[identifier];
                }

                var index = LoadIndex();
                string matchingIdentifier = index.ObjectIdentifiers.FirstOrDefault(id => id.StartsWith($"{identifier}_"));
                if (matchingIdentifier == null) return null;

                string objPath = Path.Combine(BaseFolder, $"obj_{matchingIdentifier}.xml");
                if (!File.Exists(objPath)) return null;

                using (StreamReader reader = new StreamReader(objPath))
                {
                    var data = (ObjectData)objectSerializer.Deserialize(reader);
                    objectDataCache[identifier] = data;
                    return data;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Load] Error loading object data: {ex}");
                return null;
            }
        }

        public static void SaveObjectData(ObjectData data)
        {
            try
            {
                string objPath = Path.Combine(ConfigFolder, $"obj_{data.Identifier}_{data.MatchType}.xml");
                using (StreamWriter writer = new StreamWriter(objPath))
                {
                    objectSerializer.Serialize(writer, data);
                }

                var index = LoadIndex();
                string indexId = $"{data.Identifier}_{data.MatchType}";
                if (!index.ObjectIdentifiers.Contains(indexId))
                {
                    index.ObjectIdentifiers.Add(indexId);
                    SaveIndex(index);
                }

                objectDataCache[data.Identifier] = data;
                BaseFolder = ConfigFolder;
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Save] Error saving object data: {ex}");
            }
        }

        public static void SaveCameraSettings(CameraSettings settings)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(CameraPath))
                {
                    cameraSerializer.Serialize(writer, settings);
                }
                cameraSettingsCache = settings;
                BaseFolder = ConfigFolder;
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Save] Error saving camera settings: {ex}");
            }
        }

        public static CameraSettings LoadCameraSettings()
        {
            try
            {
                if (cameraSettingsCache != null)
                {
                    return cameraSettingsCache;
                }

                if (!File.Exists(CameraPath))
                {
                    return null;
                }

                using (StreamReader reader = new StreamReader(CameraPath))
                {
                    cameraSettingsCache = (CameraSettings)cameraSerializer.Deserialize(reader);
                    return cameraSettingsCache;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Load] Error loading camera settings: {ex}");
                return null;
            }
        }

        private static ObjectIndex LoadIndex()
        {
            try
            {
                if (indexCache != null)
                {
                    return indexCache;
                }

                if (!File.Exists(IndexPath))
                {
                    indexCache = new ObjectIndex();
                    return indexCache;
                }

                using (StreamReader reader = new StreamReader(IndexPath))
                {
                    indexCache = (ObjectIndex)indexSerializer.Deserialize(reader);
                    return indexCache;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Load] Error loading index: {ex}");
                return new ObjectIndex();
            }
        }

        private static void SaveIndex(ObjectIndex index)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(IndexPath))
                {
                    indexSerializer.Serialize(writer, index);
                }
                indexCache = index;
                BaseFolder = ConfigFolder;
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Save] Error saving index: {ex}");
            }
        }

        public static void DeleteObjectData(string identifier)
        {
            try
            {
                var index = LoadIndex();
                var idsToRemove = index.ObjectIdentifiers.Where(id => id.StartsWith($"{identifier}_")).ToList();

                foreach (var id in idsToRemove)
                {
                    string objPath = Path.Combine(ConfigFolder, $"obj_{id}.xml");
                    if (File.Exists(objPath))
                    {
                        File.Delete(objPath);
                    }
                    index.ObjectIdentifiers.Remove(id);
                }

                SaveIndex(index);

                if (objectDataCache.ContainsKey(identifier))
                {
                    objectDataCache.Remove(identifier);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Save] Error deleting object data: {ex}");
            }
        }

        public static void DeleteAllData()
        {
            try
            {
                if (Directory.Exists(ConfigFolder))
                {
                    Directory.Delete(ConfigFolder, true);
                }
                objectDataCache.Clear();
                cameraSettingsCache = null;
                indexCache = null;
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Error($"[Rim3D Save] Error deleting all data: {ex}");
            }
        }

        public static List<ObjectData> LoadAllObjectData()
        {
            var result = new List<ObjectData>();
            var index = LoadIndex();

            foreach (var identifier in index.ObjectIdentifiers)
            {
                string objPath = Path.Combine(BaseFolder, $"obj_{identifier}.xml");
                if (File.Exists(objPath))
                {
                    try
                    {
                        if (objectDataCache.ContainsKey(identifier))
                        {
                            result.Add(objectDataCache[identifier]);
                            continue;
                        }

                        using (StreamReader reader = new StreamReader(objPath))
                        {
                            var data = (ObjectData)objectSerializer.Deserialize(reader);
                            if (data != null)
                            {
                                result.Add(data);
                                objectDataCache[data.Identifier] = data;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[Rim3D Load] Error loading object data from {identifier}: {ex}");
                        index.ObjectIdentifiers.Remove(identifier);
                        SaveIndex(index);
                    }
                }
                else
                {
                    index.ObjectIdentifiers.Remove(identifier);
                    SaveIndex(index);
                }
            }

            return result;
        }

        public static void ClearCache()
        {
            objectDataCache.Clear();
            cameraSettingsCache = null;
            indexCache = null;
        }
    }
}