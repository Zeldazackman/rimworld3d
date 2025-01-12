using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace Rim3D
{
    public class ObjectWindow : Window
    {
        private Vector2 selectedScrollPosition = Vector2.zero;
        private Vector2 exactScrollPosition = Vector2.zero;
        private Vector2 containsScrollPosition = Vector2.zero;
        private string newExactRule = "";
        private string newContainsRule = "";
        private float selectedContentHeight;
        private float exactContentHeight;
        private float containsContentHeight;
        private const float ElementHeight = 110f;
        private const float AddRuleHeight = 64f;
        private const float HeaderHeight = 40f;
        private const float GapHeight = 12f;
        private static float? lastX;
        private static float? lastY;

        public ObjectWindow()
        {
            this.forcePause = false;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.preventCameraMotion = false;
            ObjectConfigManager.SetCurrentWindow(this);
        }

        public override Vector2 InitialSize => new Vector2(1200f, 800f);

        public override void DoWindowContents(Rect inRect)
        {
            float num = 0f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, num, inRect.width, HeaderHeight), "Object Configuration");
            Text.Font = GameFont.Small;
            num += HeaderHeight + GapHeight;

            float columnWidth = inRect.width / 3f - 20f;

            Rect selectedRect = new Rect(0f, num, columnWidth, inRect.height - num);
            Rect exactRect = new Rect(columnWidth + 30f, num, columnWidth, inRect.height - num);
            Rect containsRect = new Rect((columnWidth + 30f) * 2f, num, columnWidth, inRect.height - num);

            DoSelectedColumn(selectedRect);
            DoExactColumn(exactRect);
            DoContainsColumn(containsRect);

            if (this.windowRect.x != lastX || this.windowRect.y != lastY)
            {
                lastX = this.windowRect.x;
                lastY = this.windowRect.y;
            }
        }

        private void RemoveConfigSafe(ObjectConfig config)
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                $"Are you sure you want to remove {config.Identifier}?",
                () =>
                {
                    ObjectConfigManager.RemoveConfig(config.Identifier, config.MatchType);
                }));
        }

        private void MarkThingsDirty(ObjectConfig config)
        {
            if (Find.CurrentMap != null)
            {
                foreach (Thing thing in Find.CurrentMap.listerThings.AllThings.Where(t => config.IsMatch(t)))
                {
                    Find.CurrentMap.mapDrawer.MapMeshDirty(thing.Position, MapMeshFlagDefOf.Things);
                }
            }
        }

        private void DoSelectedColumn(Rect rect)
        {
            ScrollingContainer container = new ScrollingContainer(rect);
            container.Begin(ref selectedScrollPosition, selectedContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(HeaderHeight, headerRect =>
            {
                Rect titleRect = headerRect.LeftHalf();
                Rect buttonRect = headerRect.RightHalf();

                Widgets.Label(titleRect, "Selected Objects");
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(buttonRect.ContractedBy(20f, 0f), "Add Selected"))
                {
                    if (Find.Selector.SingleSelectedThing != null)
                    {
                        var defName = Find.Selector.SingleSelectedThing.def.defName;
                        ObjectConfigManager.AddConfig(defName, MatchType.Selected);
                        var config = ObjectConfigManager.GetConfig(defName);
                        if (config != null)
                        {
                            MarkThingsDirty(config);
                        }
                    }
                }
            });

            Text.Font = GameFont.Small;
            container.AddGap();

            var selectedConfigs = ObjectConfigManager.GetConfigsByType(MatchType.Selected).ToList();
            foreach (var config in selectedConfigs)
            {
                container.AddFixedBox(ElementHeight, rect => DoConfigElement(rect, config));
                container.AddGap();
            }

            selectedContentHeight = container.ContentHeight;
            container.End();
        }

        private void DoExactColumn(Rect rect)
        {
            ScrollingContainer container = new ScrollingContainer(rect);
            container.Begin(ref exactScrollPosition, exactContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(HeaderHeight, rect => Widgets.Label(rect, "Exact Match"));
            Text.Font = GameFont.Small;
            container.AddGap();

            container.AddFixedBox(AddRuleHeight, rect =>
            {
                Widgets.Label(rect.TopHalf().LeftHalf(), "New DefName:");
                newExactRule = Widgets.TextField(rect.TopHalf().RightHalf(), newExactRule);

                if (Widgets.ButtonText(rect.BottomHalf(), "Add Rule") && !string.IsNullOrEmpty(newExactRule))
                {
                    ObjectConfigManager.AddConfig(newExactRule, MatchType.Exact);
                    var config = ObjectConfigManager.GetConfig(newExactRule);
                    if (config != null)
                    {
                        MarkThingsDirty(config);
                    }
                    newExactRule = "";
                }
            });
            container.AddGap();

            var exactConfigs = ObjectConfigManager.GetConfigsByType(MatchType.Exact).ToList();
            foreach (var config in exactConfigs)
            {
                container.AddFixedBox(ElementHeight, rect => DoConfigElement(rect, config));
                container.AddGap();
            }

            exactContentHeight = container.ContentHeight;
            container.End();
        }

        private void DoContainsColumn(Rect rect)
        {
            ScrollingContainer container = new ScrollingContainer(rect);
            container.Begin(ref containsScrollPosition, containsContentHeight);

            Text.Font = GameFont.Medium;
            container.AddFixedBox(HeaderHeight, rect => Widgets.Label(rect, "Contains Match"));
            Text.Font = GameFont.Small;
            container.AddGap();

            container.AddFixedBox(AddRuleHeight, rect =>
            {
                Widgets.Label(rect.TopHalf().LeftHalf(), "Contains Text:");
                newContainsRule = Widgets.TextField(rect.TopHalf().RightHalf(), newContainsRule);

                if (Widgets.ButtonText(rect.BottomHalf(), "Add Rule") && !string.IsNullOrEmpty(newContainsRule))
                {
                    ObjectConfigManager.AddConfig(newContainsRule, MatchType.Contains);
                    var config = ObjectConfigManager.GetConfig(newContainsRule);
                    if (config != null)
                    {
                        MarkThingsDirty(config);
                    }
                    newContainsRule = "";
                }
            });
            container.AddGap();

            var containsConfigs = ObjectConfigManager.GetConfigsByType(MatchType.Contains).ToList();
            foreach (var config in containsConfigs)
            {
                container.AddFixedBox(ElementHeight, rect => DoConfigElement(rect, config));
                container.AddGap();
            }

            containsContentHeight = container.ContentHeight;
            container.End();
        }

        private void DoConfigElement(Rect rect, ObjectConfig config)
        {
            Widgets.DrawBox(rect);
            
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 22f), config.Identifier);
            Text.Anchor = TextAnchor.UpperLeft;

            float buttonWidth = 80f;
            Rect removeRect = new Rect(rect.x + 10f, rect.y + 24f, buttonWidth, 22f);
            if (Widgets.ButtonText(removeRect, "Remove"))
            {
                RemoveConfigSafe(config);
                return;
            }

            Rect copyRect = new Rect(removeRect.xMax + 10f, rect.y + 24f, buttonWidth, 22f);
            if (Widgets.ButtonText(copyRect, "Copy"))
            {
                ObjectConfigManager.CopyConfig(config);
            }

            if (ObjectConfigManager.HasCopiedConfig())
            {
                Rect pasteRect = new Rect(copyRect.xMax + 10f, rect.y + 24f, buttonWidth, 22f);
                if (Widgets.ButtonText(pasteRect, "Paste"))
                {
                    ObjectConfigManager.PasteConfig(config);
                    MarkThingsDirty(config);
                }
            }

            bool wasPlane = config.IsPlane;
            bool wasCube = config.IsCube;

            Rect planeCheckboxRect = new Rect(rect.x + 10f, rect.y + 48f, rect.width / 2f - 15f, 22f);
            Rect cubeCheckboxRect = new Rect(rect.x + rect.width / 2f + 5f, rect.y + 48f, rect.width / 2f - 15f, 22f);

            Widgets.CheckboxLabeled(planeCheckboxRect, "Is Plane", ref config.IsPlane);
            Widgets.CheckboxLabeled(cubeCheckboxRect, "Is Cube", ref config.IsCube);

            if (wasPlane != config.IsPlane || wasCube != config.IsCube)
            {
                MarkThingsDirty(config);
            }

            if (config.IsCube)
            {
                Rect editCubeRect = new Rect(rect.x + 10f, rect.y + 72f, buttonWidth * 1.5f, 22f);
                if (Widgets.ButtonText(editCubeRect, "Edit Cube"))
                {
                    if (Core.boxSettingsWindow == null || !Find.WindowStack.IsOpen(Core.boxSettingsWindow))
                    {
                        Core.Instance.LoadObjectConfig(config.Identifier);
                        Core.boxSettingsWindow = new BoxSettingsWindow();
                        Find.WindowStack.Add(Core.boxSettingsWindow);
                    }
                }
            }
            else
            {
                Rect heightLabelRect = new Rect(rect.x + 10f, rect.y + 72f, 50f, 22f);
                Rect sliderRect = new Rect(heightLabelRect.xMax + 5f, rect.y + 72f, rect.width - heightLabelRect.width - 45f, 22f);
                Rect valueRect = new Rect(sliderRect.xMax + 5f, rect.y + 72f, 35f, 22f);

                Widgets.Label(heightLabelRect, "Height:");
                config.HeightOffset = Widgets.HorizontalSlider(sliderRect, config.HeightOffset, -3f, 3f, false);
                Widgets.Label(valueRect, config.HeightOffset.ToString("F1"));
                
                if (Event.current.type == EventType.MouseUp)
                {
                    MarkThingsDirty(config);
                }
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            if (lastX.HasValue && lastY.HasValue)
            {
                this.windowRect.x = lastX.Value;
                this.windowRect.y = lastY.Value;
            }
            else
            {
                this.windowRect.x = (UI.screenWidth - this.windowRect.width) / 2f;
                this.windowRect.y = (UI.screenHeight - this.windowRect.height) / 2f;
            }
        }
    }
}