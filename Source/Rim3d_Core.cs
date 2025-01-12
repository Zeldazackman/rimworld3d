using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Rim3D
{
    public class Core : Mod
    {
        public static Core Instance;
        public static Settings settings;
        public static WindowRim3D optionsWindow;
        public static BoxSettingsWindow boxSettingsWindow;
        public static SkyboxWindow skyboxWindow;
        public static FireOptionsWindow fireOptionsWindow;
        public static bool mode3d = false;
        private bool lastKeyState = false;
        private bool lastOptionsKeyState = false;
        private bool lastPerformanceKeyState = false;
        private bool showPerformance = false;
        private float lastSkyUpdate = 0f;
        private const float SKY_UPDATE_INTERVAL = 0.001f;
        public CameraController cameraController;
        public StateManager stateManager;
        public string selectedObjectType;

        public Core(ModContentPack content) : base(content)
        {
            Instance = this;
            ConfigSerializer.Initialize(content.RootDir);
            settings = GetSettings<Settings>();
            settings.Initialize();
            cameraController = new CameraController();
            stateManager = new StateManager();
        }

        public void LoadObjectConfig(string identifier)
        {
            selectedObjectType = identifier;
            var data = ConfigSerializer.LoadObjectData(identifier);
            if (data != null)
            {
                ObjectConfigManager.UpdateConfigFromData(data);
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            WindowRim3D window = new WindowRim3D();
            window.DoWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Rim3D";
        }

        public void OnGUI()
        {
            if (Current.ProgramState == ProgramState.Playing && Find.CurrentMap != null)
            {
                bool currentKeyState = KeyBindings.ToggleRim3D.IsDown;
                if (currentKeyState && !lastKeyState && !TransitionRenderer.IsTransitioning)
                {
                    if (Core.settings.enableTransition)
                    {
                        TransitionRenderer.StartTransition();
                    }
                    else
                    {
                        mode3d = !mode3d;
                        GlobalTextureAtlasManager.rebakeAtlas = true;
                        if (mode3d)
                        {
                            stateManager.on();
                        }
                        else
                        {
                            stateManager.off();
                        }
                    }
                }
                lastKeyState = currentKeyState;

                if (KeyBindings.ToggleRim3DOptions.IsDown && !lastOptionsKeyState)
                {
                    if (Find.WindowStack.WindowOfType<WindowRim3D>() == null)
                    {
                        optionsWindow = new WindowRim3D();
                        Find.WindowStack.Add(optionsWindow);
                    }
                    else
                    {
                        Find.WindowStack.TryRemove(typeof(WindowRim3D), true);
                    }
                }
                lastOptionsKeyState = KeyBindings.ToggleRim3DOptions.IsDown;

                if (KeyBindings.ToggleRim3DPerformance.IsDown && !lastPerformanceKeyState)
                {
                    showPerformance = !showPerformance;
                }
                lastPerformanceKeyState = KeyBindings.ToggleRim3DPerformance.IsDown;

                if (showPerformance)
                {
                    PerformanceMonitor.OnGUI();
                }

                TransitionRenderer.RenderTransitionTexture();
                PostProcessManager.RenderPostProcess();
            }
        }

        public void Tick()
        {
            cameraController.Tick();
            if (Current.ProgramState == ProgramState.Playing)
            {
                if (showPerformance)
                {
                    PerformanceMonitor.Update();
                }
                TransitionRenderer.UpdateTransition();
                PostProcessManager.UpdatePostProcess();

                float currentTime = Time.realtimeSinceStartup;
                if (currentTime - lastSkyUpdate >= SKY_UPDATE_INTERVAL)
                {
                    lastSkyUpdate = currentTime;
                    if (Core.mode3d && settings.enableSkybox)
                    {
                        stateManager.UpdateSkybox();
                    }
                }
            }
        }
    }
}