using RimWorld;
using UnityEngine;
using Verse;
using System.Linq;

namespace Rim3D
{
    public class Settings : ModSettings
    {
        public float camSpeed = 2f;
        public float angle = 25.7f;
        public float fov = 32f;
        public float z = 11.1f;
        public float far = 100.1f;
        public bool drawFullMap = false;
        public bool orthographic = false;
        public float orthographicSize = 15f;
        public bool enableTransition = true;
        public float transitionDuration = 1.5f;
        public float fogHeight = DEFAULT_FOG_HEIGHT;
        public bool enableSkybox = false;
        public float skyboxDegrees = 0f;
        public float skyboxDegreesHorizontal = 0f;
        public bool gameControlSky = true;
        public bool enableAutoExposure = true;
        public float skyboxRotation = 0f;
        public float skyboxExposure = 1f;
        public float skyboxVerticalOffset = 0f;
        public string selectedSkybox = "";
        public float lightingOverlayHeight = DEFAULT_LIGHTING_OVERLAY_HEIGHT;
        public bool plantShaderFix = false;
        public bool enableMountains = true;
        public bool enableMapMountains = true;
        public float mountainHeight = 15f;
        public float mountainBaseHeight = -2f;
        public int mountainSeed = 0;
        public float mountainNorthDistance = 10f;
        public float mountainEastDistance = 10f;
        public float mountainWestDistance = 10f;
        public float mountainQuality = 0.5f;
        public float fogMaxHeight = DEFAULT_FOG_MAX_HEIGHT;
        public bool refreshAdjacentFog = false;
        public bool enableFireCampfire = true;
        public bool enableFireTorchLamp = true;
        public bool enableFireTorchWallLamp = true;
        public bool enableOtherFires = true;
        public bool enablePostProcess = false;
        public float pospoExposure = 1f;
        public float pospoContrast = 1f;
        public float pospoSaturation = 1f;
        public float pospoBloom = 0.5f;
        public float pospoBloomThreshold = 0.5f;
        public bool enableWeatherLayer = true;
        public float weatherLayerSize = 60f;
        public float textureScaleX = -30f;
        public float textureScaleY = -100f;
        public float weatherRotationX = 310f;
        public float weatherRotationY = 0f;
        public float weatherRotationZ = 0f;
        public float weatherCameraOffset = 1.2f;
        public float speedPrimary = -0.9f;
        public float speedSecondary = -0.7f;
        public bool enableSnowRefresh = true;
        public int updateSnowInterval = 300;
        private bool hasLoadedSettings = false;
        private bool initialized = false;

        private const float DEFAULT_CAM_SPEED = 2f;
        private const float DEFAULT_ANGLE = 25.7f;
        private const float DEFAULT_FOV = 40f;
        private const float DEFAULT_Z = 60f;
        private const float DEFAULT_FAR = 100.1f;
        private const bool DEFAULT_DRAW_FULL_MAP = false;
        private const float DEFAULT_ORTHOGRAPHIC_SIZE = 15f;
        private const bool DEFAULT_ENABLE_TRANSITION = true;
        private const float DEFAULT_TRANSITION_DURATION = 1.5f;
        private const float DEFAULT_FOG_HEIGHT = 1f;
        private const float DEFAULT_FOG_MAX_HEIGHT = 5f;
        private const float DEFAULT_LIGHTING_OVERLAY_HEIGHT = 2.5f;
        private const bool DEFAULT_ENABLE_SKYBOX = false;
        private const float DEFAULT_SKYBOX_DEGREES = 0f;
        private const float DEFAULT_SKYBOX_DEGREES_HORIZONTAL = 0f;
        private const bool DEFAULT_GAME_CONTROL_SKY = true;
        private const bool DEFAULT_ENABLE_AUTO_EXPOSURE = true;
        private const float DEFAULT_SKYBOX_ROTATION = 0f;
        private const float DEFAULT_SKYBOX_EXPOSURE = 1f;
        private const float DEFAULT_SKYBOX_VERTICAL_OFFSET = 0f;
        private const string DEFAULT_SELECTED_SKYBOX = "";
        private const bool DEFAULT_REFRESH_ADJACENT_FOG = false;
        private const bool DEFAULT_PLANT_SHADER_FIX = false;
        private const bool DEFAULT_ENABLE_MOUNTAINS = true;
        private const bool DEFAULT_ENABLE_MAP_MOUNTAINS = true;
        private const float DEFAULT_MOUNTAIN_HEIGHT = 15f;
        private const float DEFAULT_MOUNTAIN_BASE_HEIGHT = -2f;
        private const int DEFAULT_MOUNTAIN_SEED = 0;
        private const float DEFAULT_MOUNTAIN_NORTH_DISTANCE = 10f;
        private const float DEFAULT_MOUNTAIN_EAST_DISTANCE = 10f;
        private const float DEFAULT_MOUNTAIN_WEST_DISTANCE = 10f;
        private const float DEFAULT_MOUNTAIN_QUALITY = 0.5f;
        private const bool DEFAULT_ENABLE_FIRE_CAMPFIRE = true;
        private const bool DEFAULT_ENABLE_FIRE_TORCHLAMP = true;
        private const bool DEFAULT_ENABLE_FIRE_TORCHWALLLAMP = true;
        private const bool DEFAULT_ENABLE_OTHER_FIRES = true;
        private const bool DEFAULT_ENABLE_POST_PROCESS = false;
        private const float DEFAULT_POSPO_EXPOSURE = 1f;
        private const float DEFAULT_POSPO_CONTRAST = 1f;
        private const float DEFAULT_POSPO_SATURATION = 1f;
        private const float DEFAULT_POSPO_BLOOM = 0.5f;
        private const float DEFAULT_POSPO_BLOOM_THRESHOLD = 0.5f;
        private const bool DEFAULT_ENABLE_WEATHER_LAYER = true;
        private const float DEFAULT_WEATHER_LAYER_SIZE = 60f;
        private const float DEFAULT_TEXTURE_SCALE_X = -30f;
        private const float DEFAULT_TEXTURE_SCALE_Y = -100f;
        private const float DEFAULT_WEATHER_ROTATION_X = 310f;
        private const float DEFAULT_WEATHER_ROTATION_Y = 0f;
        private const float DEFAULT_WEATHER_ROTATION_Z = 0f;
        private const float DEFAULT_WEATHER_CAMERA_OFFSET = 1.2f;
        private const float DEFAULT_SPEED_PRIMARY = -0.9f;
        private const float DEFAULT_SPEED_SECONDARY = -0.7f;
        private const bool DEFAULT_ENABLE_SNOW_REFRESH = true;
        private const int DEFAULT_UPDATE_SNOW_INTERVAL = 300;

        public void Initialize()
        {
            if (!initialized)
            {
                LoadDefaults();
                LoadFromXml();
                initialized = true;
            }
        }

        private void LoadDefaults()
        {
            camSpeed = DEFAULT_CAM_SPEED;
            angle = DEFAULT_ANGLE;
            fov = DEFAULT_FOV;
            z = DEFAULT_Z;
            far = DEFAULT_FAR;
            drawFullMap = DEFAULT_DRAW_FULL_MAP;
            orthographic = false;
            orthographicSize = DEFAULT_ORTHOGRAPHIC_SIZE;
            enableTransition = DEFAULT_ENABLE_TRANSITION;
            transitionDuration = DEFAULT_TRANSITION_DURATION;
            fogHeight = DEFAULT_FOG_HEIGHT;
            fogMaxHeight = DEFAULT_FOG_MAX_HEIGHT;
            refreshAdjacentFog = DEFAULT_REFRESH_ADJACENT_FOG;
            lightingOverlayHeight = DEFAULT_LIGHTING_OVERLAY_HEIGHT;
            enableSkybox = DEFAULT_ENABLE_SKYBOX;
            skyboxDegrees = DEFAULT_SKYBOX_DEGREES;
            skyboxDegreesHorizontal = DEFAULT_SKYBOX_DEGREES_HORIZONTAL;
            gameControlSky = DEFAULT_GAME_CONTROL_SKY;
            enableAutoExposure = DEFAULT_ENABLE_AUTO_EXPOSURE;
            skyboxRotation = DEFAULT_SKYBOX_ROTATION;
            skyboxExposure = DEFAULT_SKYBOX_EXPOSURE;
            skyboxVerticalOffset = DEFAULT_SKYBOX_VERTICAL_OFFSET;
            selectedSkybox = DEFAULT_SELECTED_SKYBOX;
            plantShaderFix = DEFAULT_PLANT_SHADER_FIX;
            enableMountains = DEFAULT_ENABLE_MOUNTAINS;
            enableMapMountains = DEFAULT_ENABLE_MAP_MOUNTAINS;
            mountainHeight = DEFAULT_MOUNTAIN_HEIGHT;
            mountainBaseHeight = DEFAULT_MOUNTAIN_BASE_HEIGHT;
            mountainSeed = DEFAULT_MOUNTAIN_SEED;
            mountainNorthDistance = DEFAULT_MOUNTAIN_NORTH_DISTANCE;
            mountainEastDistance = DEFAULT_MOUNTAIN_EAST_DISTANCE;
            mountainWestDistance = DEFAULT_MOUNTAIN_WEST_DISTANCE;
            mountainQuality = DEFAULT_MOUNTAIN_QUALITY;
            enableFireCampfire = DEFAULT_ENABLE_FIRE_CAMPFIRE;
            enableFireTorchLamp = DEFAULT_ENABLE_FIRE_TORCHLAMP;
            enableFireTorchWallLamp = DEFAULT_ENABLE_FIRE_TORCHWALLLAMP;
            enableOtherFires = DEFAULT_ENABLE_OTHER_FIRES;
            enablePostProcess = DEFAULT_ENABLE_POST_PROCESS;
            pospoExposure = DEFAULT_POSPO_EXPOSURE;
            pospoContrast = DEFAULT_POSPO_CONTRAST;
            pospoSaturation = DEFAULT_POSPO_SATURATION;
            pospoBloom = DEFAULT_POSPO_BLOOM;
            pospoBloomThreshold = DEFAULT_POSPO_BLOOM_THRESHOLD;
            enableWeatherLayer = DEFAULT_ENABLE_WEATHER_LAYER;
            weatherLayerSize = DEFAULT_WEATHER_LAYER_SIZE;
            textureScaleX = DEFAULT_TEXTURE_SCALE_X;
            textureScaleY = DEFAULT_TEXTURE_SCALE_Y;
            weatherRotationX = DEFAULT_WEATHER_ROTATION_X;
            weatherRotationY = DEFAULT_WEATHER_ROTATION_Y;
            weatherRotationZ = DEFAULT_WEATHER_ROTATION_Z;
            weatherCameraOffset = DEFAULT_WEATHER_CAMERA_OFFSET;
            speedPrimary = DEFAULT_SPEED_PRIMARY;
            speedSecondary = DEFAULT_SPEED_SECONDARY;
            enableSnowRefresh = DEFAULT_ENABLE_SNOW_REFRESH;
            updateSnowInterval = DEFAULT_UPDATE_SNOW_INTERVAL;
        }

        private void LoadFromXml()
        {
            var cameraSettings = ConfigSerializer.LoadCameraSettings();
            if (cameraSettings != null)
            {
                LoadFromCameraSettings(cameraSettings);
                hasLoadedSettings = true;
            }

            var objects = ConfigSerializer.LoadAllObjectData();
            foreach (var obj in objects)
            {
                ObjectConfigManager.AddConfigFromData(obj);
            }
        }

        public void LoadFromCameraSettings(CameraSettings settings)
        {
            camSpeed = settings.CamSpeed;
            angle = settings.Angle;
            fov = settings.Fov;
            z = settings.Z;
            far = settings.Far;
            drawFullMap = settings.DrawFullMap;
            orthographic = settings.Orthographic;
            orthographicSize = settings.OrthographicSize;
            enableTransition = settings.EnableTransition;
            transitionDuration = settings.TransitionDuration;
            fogHeight = settings.FogHeight;
            fogMaxHeight = settings.FogMaxHeight;
            refreshAdjacentFog = settings.RefreshAdjacentFog;
            enableMapMountains = settings.EnableMapMountains;
            lightingOverlayHeight = settings.LightingOverlayHeight;
            enableSkybox = settings.EnableSkybox;
            skyboxDegrees = settings.SkyboxDegrees;
            skyboxDegreesHorizontal = settings.SkyboxDegreesHorizontal;
            gameControlSky = settings.GameControlSky;
            enableAutoExposure = settings.EnableAutoExposure;
            skyboxRotation = settings.SkyboxRotation;
            skyboxExposure = settings.SkyboxExposure;
            skyboxVerticalOffset = settings.SkyboxVerticalOffset;
            selectedSkybox = settings.SelectedSkybox;
            plantShaderFix = settings.PlantShaderFix;
            enableMountains = settings.EnableMountains;
            mountainHeight = settings.MountainHeight;
            mountainBaseHeight = settings.MountainBaseHeight;
            mountainQuality = settings.MountainQuality;
            mountainSeed = settings.MountainSeed;
            mountainNorthDistance = settings.MountainNorthDistance;
            mountainEastDistance = settings.MountainEastDistance;
            mountainWestDistance = settings.MountainWestDistance;
            enableFireCampfire = settings.EnableFireCampfire;
            enableFireTorchLamp = settings.EnableFireTorchLamp;
            enableFireTorchWallLamp = settings.EnableFireTorchWallLamp;
            enableOtherFires = settings.EnableOtherFires;
            enablePostProcess = settings.EnablePostProcess;
            pospoExposure = settings.PospoExposure;
            pospoContrast = settings.PospoContrast;
            pospoSaturation = settings.PospoSaturation;
            pospoBloom = settings.PospoBloom;
            pospoBloomThreshold = settings.PospoBloomThreshold;
            enableWeatherLayer = settings.EnableWeatherLayer;
            weatherLayerSize = settings.WeatherLayerSize;
            textureScaleX = settings.TextureScaleX;
            textureScaleY = settings.TextureScaleY;
            weatherRotationX = settings.WeatherRotationX;
            weatherRotationY = settings.WeatherRotationY;
            weatherRotationZ = settings.WeatherRotationZ;
            weatherCameraOffset = settings.WeatherCameraOffset;
            speedPrimary = settings.SpeedPrimary;
            speedSecondary = settings.SpeedSecondary;
            enableSnowRefresh = settings.EnableSnowRefresh;
            updateSnowInterval = settings.UpdateSnowInterval;

            if (Find.WindowStack != null)
            {
                var windows = Find.WindowStack.Windows.OfType<MapMountainsWindow>();
                if (windows != null)
                {
                    foreach (var window in windows)
                    {
                        if (window != null)
                        {
                            window.ReloadSettings();
                        }
                    }
                }
            }
        }

        public CameraSettings CreateCameraSettings()
        {
            return new CameraSettings
            {
                CamSpeed = camSpeed,
                Angle = angle,
                Fov = fov,
                Z = z,
                Far = far,
                DrawFullMap = drawFullMap,
                Orthographic = orthographic,
                OrthographicSize = orthographicSize,
                EnableTransition = enableTransition,
                TransitionDuration = transitionDuration,
                FogHeight = fogHeight,
                FogMaxHeight = fogMaxHeight,
                RefreshAdjacentFog = refreshAdjacentFog,
                EnableMapMountains = enableMapMountains,
                LightingOverlayHeight = lightingOverlayHeight,
                EnableSkybox = enableSkybox,
                SkyboxDegrees = skyboxDegrees,
                SkyboxDegreesHorizontal = skyboxDegreesHorizontal,
                GameControlSky = gameControlSky,
                EnableAutoExposure = enableAutoExposure,
                SkyboxRotation = skyboxRotation,
                SkyboxExposure = skyboxExposure,
                SkyboxVerticalOffset = skyboxVerticalOffset,
                SelectedSkybox = selectedSkybox,
                PlantShaderFix = plantShaderFix,
                EnableMountains = enableMountains,
                MountainHeight = mountainHeight,
                MountainBaseHeight = mountainBaseHeight,
                MountainQuality = mountainQuality,
                MountainSeed = mountainSeed,
                MountainNorthDistance = mountainNorthDistance,
                MountainEastDistance = mountainEastDistance,
                MountainWestDistance = mountainWestDistance,
                EnableFireCampfire = enableFireCampfire,
                EnableFireTorchLamp = enableFireTorchLamp,
                EnableFireTorchWallLamp = enableFireTorchWallLamp,
                EnableOtherFires = enableOtherFires,
                EnablePostProcess = enablePostProcess,
                PospoExposure = pospoExposure,
                PospoContrast = pospoContrast,
                PospoSaturation = pospoSaturation,
                PospoBloom = pospoBloom,
                PospoBloomThreshold = pospoBloomThreshold,
                EnableWeatherLayer = enableWeatherLayer,
                WeatherLayerSize = weatherLayerSize,
                TextureScaleX = textureScaleX,
                TextureScaleY = textureScaleY,
                WeatherRotationX = weatherRotationX,
                WeatherRotationY = weatherRotationY,
                WeatherRotationZ = weatherRotationZ,
                WeatherCameraOffset = weatherCameraOffset,
                SpeedPrimary = speedPrimary,
                SpeedSecondary = speedSecondary,
                EnableSnowRefresh = enableSnowRefresh,
                UpdateSnowInterval = updateSnowInterval
            };
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                LoadFromXml();
                ObjectConfigManager.Initialize();
            }
        }

        public void ResetToDefaults()
        {
            LoadDefaults();
            CubeRenderer.ClearAll();
            ObjectConfigManager.Initialize();
            hasLoadedSettings = false;

            if (Core.mode3d)
            {
                Core.Instance.stateManager.off();
                Core.Instance.stateManager.on();
            }
        }

        public void SaveCurrentSettings()
        {
            var cameraSettings = CreateCameraSettings();
            ConfigSerializer.SaveCameraSettings(cameraSettings);
            hasLoadedSettings = true;

            foreach (var config in ObjectConfigManager.GetAllConfigs())
            {
                var data = ObjectConfigManager.CreateDataFromConfig(config);
                ConfigSerializer.SaveObjectData(data);
            }
        }

        public void LoadSavedSettings()
        {
            ConfigSerializer.ClearCache();
            var cameraSettings = ConfigSerializer.LoadCameraSettings();
            if (cameraSettings != null)
            {
                LoadFromCameraSettings(cameraSettings);
                NorthMountains.GenerateMountainMesh();

                ObjectConfigManager.ClearCache();
                ObjectConfigManager.Initialize();
                CubeRenderer.ClearAll();

                var objects = ConfigSerializer.LoadAllObjectData();
                foreach (var obj in objects)
                {
                    ObjectConfigManager.AddConfigFromData(obj);
                }

                if (Core.boxSettingsWindow != null && Core.Instance.selectedObjectType != null)
                {
                    Core.boxSettingsWindow.ReloadConfig(ObjectConfigManager.GetConfig(Core.Instance.selectedObjectType));
                }
            }
        }

        public bool AreSettingsSaved()
        {
            return hasLoadedSettings;
        }
    }
}