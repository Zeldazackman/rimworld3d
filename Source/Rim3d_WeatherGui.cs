using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    public class WeatherLayerWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;

        public WeatherLayerWindow()
        {
            this.forcePause = false;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.preventCameraMotion = false;
        }

        public override Vector2 InitialSize => new Vector2(450f, 400f);

        public override void DoWindowContents(Rect inRect)
        {
            ScrollingContainer container = new ScrollingContainer(inRect);
            container.Begin(ref scrollPosition, lastContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(40f, r => Widgets.Label(r, "Weather Layer Options"));
            Text.Font = GameFont.Small;

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                Widgets.CheckboxLabeled(r, "Enable Weather Layer", ref Core.settings.enableWeatherLayer);
            });

            container.AddFixedBox(24f, r =>
            {
                Widgets.CheckboxLabeled(r, "Enable Snow Refresh", ref Core.settings.enableSnowRefresh);
            });

            if (Core.settings.enableWeatherLayer)
            {
                container.AddGap();

                if (Core.settings.enableSnowRefresh)
                {
                    container.AddFixedBox(30f, r =>
                    {
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(r.LeftHalf(), "Snow Update Interval: " + Core.settings.updateSnowInterval.ToString());
                        Core.settings.updateSnowInterval = (int)Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.updateSnowInterval, 1, 1000);
                        Text.Anchor = TextAnchor.UpperLeft;
                    });
                }

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Layer Size: " + Core.settings.weatherLayerSize.ToString("0.0"));
                    Core.settings.weatherLayerSize = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.weatherLayerSize, 1f, 100f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Texture Scale X: " + Core.settings.textureScaleX.ToString("0.0"));
                    Core.settings.textureScaleX = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.textureScaleX, -100f, 0f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Texture Scale Y: " + Core.settings.textureScaleY.ToString("0.0"));
                    Core.settings.textureScaleY = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.textureScaleY, -100f, 0f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Rotation X: " + Core.settings.weatherRotationX.ToString("0.0"));
                    Core.settings.weatherRotationX = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.weatherRotationX, 0f, 360f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Rotation Y: " + Core.settings.weatherRotationY.ToString("0.0"));
                    Core.settings.weatherRotationY = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.weatherRotationY, 0f, 360f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Rotation Z: " + Core.settings.weatherRotationZ.ToString("0.0"));
                    Core.settings.weatherRotationZ = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.weatherRotationZ, 0f, 360f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Camera Offset: " + Core.settings.weatherCameraOffset.ToString("0.0"));
                    Core.settings.weatherCameraOffset = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.weatherCameraOffset, -10f, 10f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Primary Speed: " + Core.settings.speedPrimary.ToString("0.0"));
                    Core.settings.speedPrimary = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.speedPrimary, -2f, 0f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Secondary Speed: " + Core.settings.speedSecondary.ToString("0.0"));
                    Core.settings.speedSecondary = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.speedSecondary, -2f, 0f);
                    Text.Anchor = TextAnchor.UpperLeft;
                });
            }

            lastContentHeight = container.ContentHeight;
            container.End();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.windowRect.x = UI.screenWidth - this.windowRect.width - 50f;
            this.windowRect.y = 50f;
        }
    }
}