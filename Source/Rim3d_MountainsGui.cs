using UnityEngine;
using Verse;
using RimWorld;

namespace Rim3D
{
    public class MountainsWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;

        public MountainsWindow()
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
            container.AddFixedBox(40f, r => Widgets.Label(r, "Border Mountains"));
            Text.Font = GameFont.Small;

            container.AddGap();

            container.AddFixedBox(24f, r =>
            {
                bool wasEnabled = Core.settings.enableMountains;
                Widgets.CheckboxLabeled(r, "Enable Mountains", ref Core.settings.enableMountains);
                if (wasEnabled != Core.settings.enableMountains)
                {
                    if (Find.CurrentMap != null)
                    {
                        Find.CurrentMap.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.Things);
                    }
                }
            });

            if (Core.settings.enableMountains)
            {
                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Mountain Height: " + Core.settings.mountainHeight.ToString("0.0"));
                    float newHeight = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.mountainHeight, 5f, 100f);
                    if (newHeight != Core.settings.mountainHeight)
                    {
                        Core.settings.mountainHeight = newHeight;
                        NorthMountains.GenerateMountainMesh();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Base Height: " + Core.settings.mountainBaseHeight.ToString("0.0"));
                    float newHeight = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.mountainBaseHeight, -10f, 10f);
                    if (newHeight != Core.settings.mountainBaseHeight)
                    {
                        Core.settings.mountainBaseHeight = newHeight;
                        NorthMountains.GenerateMountainMesh();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "North Mountains Distance: " + Core.settings.mountainNorthDistance.ToString("0.0"));
                    float newDist = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.mountainNorthDistance, 0f, 100f);
                    if (newDist != Core.settings.mountainNorthDistance)
                    {
                        Core.settings.mountainNorthDistance = newDist;
                        NorthMountains.GenerateMountainMesh();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "East Mountains Distance: " + Core.settings.mountainEastDistance.ToString("0.0"));
                    float newDist = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.mountainEastDistance, 0f, 100f);
                    if (newDist != Core.settings.mountainEastDistance)
                    {
                        Core.settings.mountainEastDistance = newDist;
                        NorthMountains.GenerateMountainMesh();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "West Mountains Distance: " + Core.settings.mountainWestDistance.ToString("0.0"));
                    float newDist = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.mountainWestDistance, 0f, 100f);
                    if (newDist != Core.settings.mountainWestDistance)
                    {
                        Core.settings.mountainWestDistance = newDist;
                        NorthMountains.GenerateMountainMesh();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddGap();
                
                container.AddFixedBox(30f, r =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(r.LeftHalf(), "Mountain Detail: " + Core.settings.mountainQuality.ToString("0.0"));
                    float newQuality = Widgets.HorizontalSlider(r.RightHalf().ContractedBy(0, 2), Core.settings.mountainQuality, 0f, 1f);
                    if (newQuality != Core.settings.mountainQuality)
                    {
                        Core.settings.mountainQuality = newQuality;
                        NorthMountains.GenerateMountainMesh();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;
                });

                container.AddGap();

                container.AddFixedBox(30f, r =>
                {
                    if (Widgets.ButtonText(r, "Randomize Mountains"))
                    {
                        Core.settings.mountainSeed = (new System.Random()).Next();
                        NorthMountains.GenerateMountainMesh();
                    }
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