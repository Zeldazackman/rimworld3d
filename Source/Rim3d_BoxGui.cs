using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Rim3D
{
    public class BoxSettingsWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float lastContentHeight = 580f;
        private bool isRotatedMode = false;
        private bool isTopTexture = true;
        private bool isLeftTexture = true;
        private bool refreshConstant = false;
        private ObjectConfig currentConfig;
        private Dictionary<string, Vector3> dimensions = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector2> uvOffsets = new Dictionary<string, Vector2>();
        private Dictionary<string, float> uvZooms = new Dictionary<string, float>();
        private Dictionary<string, string> initialValues = new Dictionary<string, string>();
        private static readonly Color backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        private static readonly Color frameColor = new Color(0.3f, 0.3f, 0.3f);

        private List<Thing> thingsToUpdate;

        public BoxSettingsWindow()
        {
            this.forcePause = false;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.preventCameraMotion = false;
            currentConfig = ObjectConfigManager.GetConfig(Core.Instance.selectedObjectType);
            if (thingsToUpdate != null)
            {
                thingsToUpdate.Clear();
            }
            if (Find.CurrentMap != null)
            {
                thingsToUpdate = Find.CurrentMap.listerThings.AllThings
                    .Where(t => ObjectConfigManager.GetConfigForThing(t)?.Identifier == currentConfig.Identifier)
                    .ToList();
            }
            StoreInitialValues();
            LoadConfigValues();
        }

        private void StoreInitialValues()
        {
            initialValues.Clear();
            foreach (var kvp in currentConfig.BoxSettings)
            {
                initialValues[kvp.Key] = kvp.Value;
            }
        }

        private void LoadConfigValues()
        {
            string objectType = Core.Instance.selectedObjectType;
            var dimKeys = new[] { "sur_dim", "este_dim", "oeste_dim", "base_dim", "sur_rot_dim", "este_rot_dim", "oeste_rot_dim", "base_rot_dim" };
            var posKeys = new[] { "sur_pos", "este_pos", "oeste_pos", "base_pos", "sur_rot_pos", "este_rot_pos", "oeste_rot_pos", "base_rot_pos" };
            var uvOffsetKeys = new[] {
                "sur_uv_offset_top", "sur_uv_offset_down", "este_uv_offset_top", "este_uv_offset_down",
                "oeste_uv_offset_top", "oeste_uv_offset_down", "base_uv_offset_top", "base_uv_offset_down",
                "sur_rot_uv_offset_left", "sur_rot_uv_offset_right", "este_rot_uv_offset_left", "este_rot_uv_offset_right",
                "oeste_rot_uv_offset_left", "oeste_rot_uv_offset_right", "base_rot_uv_offset_left", "base_rot_uv_offset_right"
            };
            var uvZoomKeys = new[] {
                "sur_uv_zoom_top", "sur_uv_zoom_down", "este_uv_zoom_top", "este_uv_zoom_down",
                "oeste_uv_zoom_top", "oeste_uv_zoom_down", "base_uv_zoom_top", "base_uv_zoom_down",
                "sur_rot_uv_zoom_left", "sur_rot_uv_zoom_right", "este_rot_uv_zoom_left", "este_rot_uv_zoom_right",
                "oeste_rot_uv_zoom_left", "oeste_rot_uv_zoom_right", "base_rot_uv_zoom_left", "base_rot_uv_zoom_right"
            };

            foreach (var key in dimKeys)
            {
                var parts = currentConfig.BoxSettings[key].Split(',');
                dimensions[key] = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }

            foreach (var key in posKeys)
            {
                var parts = currentConfig.BoxSettings[key].Split(',');
                positions[key] = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }

            foreach (var key in uvOffsetKeys)
            {
                var parts = currentConfig.BoxSettings[key].Split(',');
                uvOffsets[key] = new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }

            foreach (var key in uvZoomKeys)
            {
                uvZooms[key] = float.Parse(currentConfig.BoxSettings[key]);
            }
        }

        private void UpdateDimension(string key, Vector3 value)
        {
            dimensions[key] = value;
            currentConfig.BoxSettings[key] = $"{value.x},{value.y},{value.z}";
        }

        private void UpdatePosition(string key, Vector3 value)
        {
            positions[key] = value;
            currentConfig.BoxSettings[key] = $"{value.x},{value.y},{value.z}";
        }

        private void UpdateUVOffset(string key, Vector2 value)
        {
            string baseKey = key;
            if (!isRotatedMode)
            {
                baseKey = key.Replace("_top", isTopTexture ? "_top" : "_down").Replace("_down", isTopTexture ? "_top" : "_down");
            }
            else
            {
                baseKey = key.Replace("_left", isLeftTexture ? "_left" : "_right").Replace("_right", isLeftTexture ? "_left" : "_right");
            }

            uvOffsets[baseKey] = value;
            currentConfig.BoxSettings[baseKey] = $"{value.x},{value.y}";
        }

        private void UpdateUVZoom(string key, float value)
        {
            string baseKey = key;
            if (!isRotatedMode)
            {
                baseKey = key.Replace("_top", isTopTexture ? "_top" : "_down").Replace("_down", isTopTexture ? "_top" : "_down");
            }
            else
            {
                baseKey = key.Replace("_left", isLeftTexture ? "_left" : "_right").Replace("_right", isLeftTexture ? "_left" : "_right");
            }

            uvZooms[baseKey] = value;
            currentConfig.BoxSettings[baseKey] = value.ToString();
        }

        private void DrawControl(string label, float x, float y, float width, float value, float min, float max, float defaultValue, System.Action<float> setter)
        {
            float labelWidth = 60f;
            float sliderWidth = width - 180f;
            float buttonSize = 24f;
            float smallButtonSize = buttonSize / 2f;
            float valueWidth = 40f;
            float height = 30f;

            Rect labelRect = new Rect(x, y, labelWidth, height);
            Rect sliderRect = new Rect(x + labelWidth + 5f, y, sliderWidth, height);
            Rect buttonRect = new Rect(x + width - valueWidth - buttonSize - smallButtonSize * 4 - 5f, y, buttonSize, height);
            Rect valueRect = new Rect(x + width - valueWidth, y, valueWidth, height);

            Rect upButtonRect = new Rect(x + width - valueWidth - smallButtonSize * 4 - 5f, y, smallButtonSize, smallButtonSize);
            Rect downButtonRect = new Rect(x + width - valueWidth - smallButtonSize * 4 - 5f, y + smallButtonSize, smallButtonSize, smallButtonSize);

            Rect upFastButtonRect = new Rect(x + width - valueWidth - smallButtonSize * 2 - 5f, y, smallButtonSize, smallButtonSize);
            Rect downFastButtonRect = new Rect(x + width - valueWidth - smallButtonSize * 2 - 5f, y + smallButtonSize, smallButtonSize, smallButtonSize);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);

            float newValue = Widgets.HorizontalSlider(sliderRect, value, min, max);
            if (newValue != value)
            {
                setter(newValue);
                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(buttonRect, "R"))
            {
                string keyToFind = null;
                int componentIndex = -1;

                foreach (var pair in currentConfig.BoxSettings)
                {
                    var parts = pair.Value.Split(',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (float.TryParse(parts[i], out float storedValue))
                        {
                            if (Mathf.Approximately(storedValue, value))
                            {
                                if ((label == "Width" && i == 2) ||
                                    (label == "Height" && i == 1) ||
                                    (label == "H Pos" && i == 0) ||
                                    (label == "V Pos" && i == 1) ||
                                    (label == "Z Pos" && i == 2) ||
                                    (label == "UV H" && i == 0) ||
                                    (label == "UV V" && i == 1) ||
                                    (label == "UV Zoom" && i == 0))
                                {
                                    keyToFind = pair.Key;
                                    componentIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    if (keyToFind != null) break;
                }

                if (keyToFind != null && initialValues.ContainsKey(keyToFind))
                {
                    var initialParts = initialValues[keyToFind].Split(',');
                    if (initialParts.Length == 1)
                    {
                        setter(float.Parse(initialParts[0]));
                    }
                    else if (initialParts.Length == 2 && componentIndex >= 0)
                    {
                        var currentParts = currentConfig.BoxSettings[keyToFind].Split(',');
                        var newValue2D = new Vector2(
                            componentIndex == 0 ? float.Parse(initialParts[0]) : float.Parse(currentParts[0]),
                            componentIndex == 1 ? float.Parse(initialParts[1]) : float.Parse(currentParts[1])
                        );
                        UpdateUVOffset(keyToFind, newValue2D);
                    }
                    else if (initialParts.Length == 3 && componentIndex >= 0)
                    {
                        var currentParts = currentConfig.BoxSettings[keyToFind].Split(',');
                        var newValue3D = new Vector3(
                            componentIndex == 0 ? float.Parse(initialParts[0]) : float.Parse(currentParts[0]),
                            componentIndex == 1 ? float.Parse(initialParts[1]) : float.Parse(currentParts[1]),
                            componentIndex == 2 ? float.Parse(initialParts[2]) : float.Parse(currentParts[2])
                        );
                        if (keyToFind.Contains("dim"))
                        {
                            UpdateDimension(keyToFind, newValue3D);
                        }
                        else if (keyToFind.Contains("pos"))
                        {
                            UpdatePosition(keyToFind, newValue3D);
                        }
                    }
                    CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                    if (refreshConstant)
                    {
                        MarkThingsDirty();
                    }
                }
            }

            if (Widgets.ButtonText(upButtonRect, "▲"))
            {
                float adjustedValue = Mathf.Clamp(value + 0.01f, min, max);
                setter(adjustedValue);
                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(downButtonRect, "▼"))
            {
                float adjustedValue = Mathf.Clamp(value - 0.01f, min, max);
                setter(adjustedValue);
                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(upFastButtonRect, "▲"))
            {
                float adjustedValue = Mathf.Clamp(value + 0.1f, min, max);
                setter(adjustedValue);
                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(downFastButtonRect, "▼"))
            {
                float adjustedValue = Mathf.Clamp(value - 0.1f, min, max);
                setter(adjustedValue);
                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(valueRect, value.ToString("F2"));
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override Vector2 InitialSize => new Vector2(900f, 800f);

        private void MarkThingsDirty()
        {
            if (Find.CurrentMap != null)
            {
                foreach (Thing thing in thingsToUpdate)
                {
                    Find.CurrentMap.mapDrawer.MapMeshDirty(thing.Position, MapMeshFlagDefOf.Things, false, false);
                }
            }
        }

        private void DrawEmptyRow(float x, float y, float width)
        {
            Rect rect = new Rect(x, y, width, 30f);
        }

        private void DrawWallSection(Rect rect, string title, string dimKey, string posKey, string uvOffsetKey, string uvZoomKey)
        {
            Widgets.DrawBox(rect);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect.TopPartPixels(30f), title);
            Text.Font = GameFont.Small;

            float contentWidth = (rect.width - 60f) / 3f;
            float startY = rect.y + 50f;
            float spacing = 45f;
            float padding = 20f;
            Vector3 newDim = dimensions[dimKey];
            Vector3 newPos = positions[posKey];

            string baseOffsetKey = uvOffsetKey;
            string baseZoomKey = uvZoomKey;

            if (!isRotatedMode)
            {
                baseOffsetKey += isTopTexture ? "_top" : "_down";
                baseZoomKey += isTopTexture ? "_top" : "_down";
            }
            else
            {
                baseOffsetKey += isLeftTexture ? "_left" : "_right";
                baseZoomKey += isLeftTexture ? "_left" : "_right";
            }

            Vector2 newOffset = uvOffsets[baseOffsetKey];
            float newZoom = uvZooms[baseZoomKey];

            float col1X = rect.x + padding;
            float col2X = rect.x + padding + contentWidth + 10f;
            float col3X = rect.x + padding + (contentWidth + 10f) * 2f;

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(col1X, startY, contentWidth, 30f), "Size");
            Widgets.Label(new Rect(col2X, startY, contentWidth, 30f), "Position");
            Widgets.Label(new Rect(col3X, startY, contentWidth, 30f), "Texture");
            Text.Anchor = TextAnchor.UpperLeft;

            startY += spacing;

            DrawControl("Width", col1X, startY, contentWidth, newDim.z, 0.1f, 20f, 0.1f, value =>
            {
                newDim.z = value;
                UpdateDimension(dimKey, newDim);
            });

            DrawControl("H Pos", col2X, startY, contentWidth, newPos.x, -20f, 20f, 0f, value =>
            {
                newPos.x = value;
                UpdatePosition(posKey, newPos);
            });

            DrawControl("UV H", col3X, startY, contentWidth, newOffset.x, -100f, 100f, 0f, value =>
            {
                newOffset.x = value;
                UpdateUVOffset(baseOffsetKey, newOffset);
            });

            startY += spacing;

            DrawControl("Height", col1X, startY, contentWidth, newDim.y, 0.1f, 20f, 0.5f, value =>
            {
                newDim.y = value;
                UpdateDimension(dimKey, newDim);
            });

            DrawControl("V Pos", col2X, startY, contentWidth, newPos.y, -20f, 20f, 0f, value =>
            {
                newPos.y = value;
                UpdatePosition(posKey, newPos);
            });

            DrawControl("UV V", col3X, startY, contentWidth, newOffset.y, -100f, 100f, 0f, value =>
            {
                newOffset.y = value;
                UpdateUVOffset(baseOffsetKey, newOffset);
            });

            startY += spacing;

            DrawEmptyRow(col1X, startY, contentWidth);

            DrawControl("Z Pos", col2X, startY, contentWidth, newPos.z, -20f, 20f, 0f, value =>
            {
                newPos.z = value;
                UpdatePosition(posKey, newPos);
            });

            DrawControl("UV Zoom", col3X, startY, contentWidth, newZoom, 1f, 500f, 100f, value =>
            {
                UpdateUVZoom(baseZoomKey, value);
            });

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override void PreClose()
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "Save changes?",
                () =>
                {
                    Core.settings.SaveCurrentSettings();
                    base.PreClose();
                },
                () =>
                {
                    base.PreClose();
                }
            ));
        }

        public override void DoWindowContents(Rect inRect)
        {
            string objectType = Core.Instance.selectedObjectType;

            GUI.DrawTexture(inRect, BaseContent.BlackTex);

            Rect contentRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            ScrollingContainer container = new ScrollingContainer(contentRect);
            container.Begin(ref scrollPosition, lastContentHeight);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(container.GetRect(40f), $"Box Settings - {objectType}");
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            container.AddGap(10f);

            Rect topControlsRect = container.GetRect(30f);
            Rect refreshConstantCheckRect = new Rect(topControlsRect.x, topControlsRect.y, 150f, 30f);
            Rect refreshButtonRect = new Rect(refreshConstantCheckRect.xMax + 10f, topControlsRect.y, 100f, 30f);

            Widgets.CheckboxLabeled(refreshConstantCheckRect, "Refresh Constant", ref refreshConstant);
            if (Widgets.ButtonText(refreshButtonRect, "Refresh"))
            {
                MarkThingsDirty();
            }

            container.AddGap(10f);

            Rect checkboxRect = container.GetRect(60f);
            bool wasChecked = isRotatedMode;

            float rightEdge = checkboxRect.xMax - 20f;
            float labelWidth = 90f;
            float checkboxWidth = 24f;
            float spacing = 40f;
            float groupWidth = labelWidth + checkboxWidth + 10f;
            float totalWidth = (groupWidth + spacing) * 2;
            float startX = rightEdge - totalWidth;
            float rowHeight = 30f;
            float buttonSize = 12f;

            Rect heightControlRect = new Rect(checkboxRect.x, checkboxRect.y, labelWidth + 80f, rowHeight);
            Rect heightLabelRect = new Rect(heightControlRect.x, heightControlRect.y, labelWidth, rowHeight);

            Rect heightUpCoarseButtonRect = new Rect(heightControlRect.xMax - 40f, heightControlRect.y, buttonSize, buttonSize);
            Rect heightDownCoarseButtonRect = new Rect(heightControlRect.xMax - 40f, heightControlRect.y + buttonSize, buttonSize, buttonSize);

            Rect heightUpFineButtonRect = new Rect(heightControlRect.xMax - 20f, heightControlRect.y, buttonSize, buttonSize);
            Rect heightDownFineButtonRect = new Rect(heightControlRect.xMax - 20f, heightControlRect.y + buttonSize, buttonSize, buttonSize);

            float baseHeightStartX = heightControlRect.xMax + 20f;
            Rect baseHeightLabelRect = new Rect(baseHeightStartX, heightControlRect.y, 90f, rowHeight);
            Rect baseHeightButtonsRect = new Rect(baseHeightLabelRect.xMax + 5f, heightControlRect.y, 50f, rowHeight);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(baseHeightLabelRect, "Base Height");

            Rect baseHeightUpCoarseButtonRect = new Rect(baseHeightButtonsRect.x, baseHeightButtonsRect.y, buttonSize, buttonSize);
            Rect baseHeightDownCoarseButtonRect = new Rect(baseHeightButtonsRect.x, baseHeightButtonsRect.y + buttonSize, buttonSize, buttonSize);

            Rect baseHeightUpFineButtonRect = new Rect(baseHeightButtonsRect.x + buttonSize + 2f, baseHeightButtonsRect.y, buttonSize, buttonSize);
            Rect baseHeightDownFineButtonRect = new Rect(baseHeightButtonsRect.x + buttonSize + 2f, baseHeightButtonsRect.y + buttonSize, buttonSize, buttonSize);

            Rect widthControlRect = new Rect(checkboxRect.x, checkboxRect.y + rowHeight, labelWidth + 80f, rowHeight);
            Rect widthLabelRect = new Rect(widthControlRect.x, widthControlRect.y, labelWidth, rowHeight);

            Rect widthUpCoarseButtonRect = new Rect(widthControlRect.xMax - 40f, widthControlRect.y, buttonSize, buttonSize);
            Rect widthDownCoarseButtonRect = new Rect(widthControlRect.xMax - 40f, widthControlRect.y + buttonSize, buttonSize, buttonSize);

            Rect widthUpFineButtonRect = new Rect(widthControlRect.xMax - 20f, widthControlRect.y, buttonSize, buttonSize);
            Rect widthDownFineButtonRect = new Rect(widthControlRect.xMax - 20f, widthControlRect.y + buttonSize, buttonSize, buttonSize);

            Rect topDownLabelRect = new Rect(startX, checkboxRect.y, labelWidth, rowHeight);
            Rect topDownToggleRect = new Rect(topDownLabelRect.xMax + 5f, checkboxRect.y + 3f, checkboxWidth, checkboxWidth);

            Rect leftRightLabelRect = new Rect(startX, checkboxRect.y + rowHeight, labelWidth, rowHeight);
            Rect leftRightToggleRect = new Rect(leftRightLabelRect.xMax + 5f, checkboxRect.y + rowHeight + 3f, checkboxWidth, checkboxWidth);

            float secondColumnX = topDownToggleRect.xMax + spacing;
            Rect topTexLabelRect = new Rect(secondColumnX, checkboxRect.y, labelWidth, rowHeight);
            Rect topTexToggleRect = new Rect(topTexLabelRect.xMax + 5f, checkboxRect.y + 3f, checkboxWidth, checkboxWidth);

            Rect bottomTexLabelRect = new Rect(secondColumnX, checkboxRect.y + rowHeight, labelWidth, rowHeight);
            Rect bottomTexToggleRect = new Rect(bottomTexLabelRect.xMax + 5f, checkboxRect.y + rowHeight + 3f, checkboxWidth, checkboxWidth);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(heightLabelRect, "Total Height");
            Widgets.Label(widthLabelRect, "Total Width");

            string baseDimKey = isRotatedMode ? "base_rot_dim" : "base_dim";
            string surDimKey = isRotatedMode ? "sur_rot_dim" : "sur_dim";
            string esteDimKey = isRotatedMode ? "este_rot_dim" : "este_dim";
            string oesteDimKey = isRotatedMode ? "oeste_rot_dim" : "oeste_dim";
            string basePosKey = isRotatedMode ? "base_rot_pos" : "base_pos";
            string surPosKey = isRotatedMode ? "sur_rot_pos" : "sur_pos";
            string estePosKey = isRotatedMode ? "este_rot_pos" : "este_pos";
            string oestePosKey = isRotatedMode ? "oeste_rot_pos" : "oeste_pos";

            if (Widgets.ButtonText(baseHeightUpCoarseButtonRect, "▲"))
            {
                string[] posKeys = isRotatedMode ?
                    new[] { "sur_rot_pos", "este_rot_pos", "oeste_rot_pos", "base_rot_pos" } :
                    new[] { "sur_pos", "este_pos", "oeste_pos", "base_pos" };

                foreach (string key in posKeys)
                {
                    Vector3 pos = positions[key];
                    pos.y += 0.1f;
                    UpdatePosition(key, pos);
                }

                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(baseHeightDownCoarseButtonRect, "▼"))
            {
                string[] posKeys = isRotatedMode ?
                    new[] { "sur_rot_pos", "este_rot_pos", "oeste_rot_pos", "base_rot_pos" } :
                    new[] { "sur_pos", "este_pos", "oeste_pos", "base_pos" };

                foreach (string key in posKeys)
                {
                    Vector3 pos = positions[key];
                    pos.y -= 0.1f;
                    UpdatePosition(key, pos);
                }

                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(baseHeightUpFineButtonRect, "▲"))
            {
                string[] posKeys = isRotatedMode ?
                    new[] { "sur_rot_pos", "este_rot_pos", "oeste_rot_pos", "base_rot_pos" } :
                    new[] { "sur_pos", "este_pos", "oeste_pos", "base_pos" };

                foreach (string key in posKeys)
                {
                    Vector3 pos = positions[key];
                    pos.y += 0.01f;
                    UpdatePosition(key, pos);
                }

                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(baseHeightDownFineButtonRect, "▼"))
            {
                string[] posKeys = isRotatedMode ?
                    new[] { "sur_rot_pos", "este_rot_pos", "oeste_rot_pos", "base_rot_pos" } :
                    new[] { "sur_pos", "este_pos", "oeste_pos", "base_pos" };

                foreach (string key in posKeys)
                {
                    Vector3 pos = positions[key];
                    pos.y -= 0.01f;
                    UpdatePosition(key, pos);
                }

                CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                if (refreshConstant)
                {
                    MarkThingsDirty();
                }
            }

            if (Widgets.ButtonText(heightUpCoarseButtonRect, "▲"))
            {
                string baseKey = isRotatedMode ? "base_rot_pos" : "base_pos";
                Vector3 basePos = positions[baseKey];
                if (basePos.y < 20f)
                {
                    basePos.y += 0.5f;
                    UpdatePosition(baseKey, basePos);

                    string[] wallKeys = isRotatedMode ?
                        new[] { "sur_rot_dim", "este_rot_dim", "oeste_rot_dim" } :
                        new[] { "sur_dim", "este_dim", "oeste_dim" };

                    foreach (string key in wallKeys)
                    {
                        Vector3 dim = dimensions[key];
                        if (dim.y < 20f)
                        {
                            dim.y += 1f;
                            UpdateDimension(key, dim);
                        }
                    }

                    CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                    if (refreshConstant)
                    {
                        MarkThingsDirty();
                    }
                }
            }

            if (Widgets.ButtonText(heightDownCoarseButtonRect, "▼"))
            {
                string baseKey = isRotatedMode ? "base_rot_pos" : "base_pos";
                Vector3 basePos = positions[baseKey];
                if (basePos.y > -20f)
                {
                    basePos.y -= 0.5f;
                    UpdatePosition(baseKey, basePos);

                    string[] wallKeys = isRotatedMode ?
                        new[] { "sur_rot_dim", "este_rot_dim", "oeste_rot_dim" } :
                        new[] { "sur_dim", "este_dim", "oeste_dim" };

                    foreach (string key in wallKeys)
                    {
                        Vector3 dim = dimensions[key];
                        if (dim.y > 0.1f)
                        {
                            dim.y -= 1f;
                            UpdateDimension(key, dim);
                        }
                    }

                    CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                    if (refreshConstant)
                    {
                        MarkThingsDirty();
                    }
                }
            }

            if (Widgets.ButtonText(heightUpFineButtonRect, "▲"))
            {
                string baseKey = isRotatedMode ? "base_rot_pos" : "base_pos";
                Vector3 basePos = positions[baseKey];
                if (basePos.y < 20f)
                {
                    basePos.y += 0.05f;
                    UpdatePosition(baseKey, basePos);

                    string[] wallKeys = isRotatedMode ?
                        new[] { "sur_rot_dim", "este_rot_dim", "oeste_rot_dim" } :
                        new[] { "sur_dim", "este_dim", "oeste_dim" };

                    foreach (string key in wallKeys)
                    {
                        Vector3 dim = dimensions[key];
                        if (dim.y < 20f)
                        {
                            dim.y += 0.1f;
                            UpdateDimension(key, dim);
                        }
                    }

                    CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                    if (refreshConstant)
                    {
                        MarkThingsDirty();
                    }
                }
            }

            if (Widgets.ButtonText(heightDownFineButtonRect, "▼"))
            {
                string baseKey = isRotatedMode ? "base_rot_pos" : "base_pos";
                Vector3 basePos = positions[baseKey];
                if (basePos.y > -20f)
                {
                    basePos.y -= 0.05f;
                    UpdatePosition(baseKey, basePos);

                    string[] wallKeys = isRotatedMode ?
                        new[] { "sur_rot_dim", "este_rot_dim", "oeste_rot_dim" } :
                        new[] { "sur_dim", "este_dim", "oeste_dim" };

                    foreach (string key in wallKeys)
                    {
                        Vector3 dim = dimensions[key];
                        if (dim.y > 0.1f)
                        {
                            dim.y -= 0.1f;
                            UpdateDimension(key, dim);
                        }
                    }

                    CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                    if (refreshConstant)
                    {
                        MarkThingsDirty();
                    }
                }
            }

            void AdjustWidth(float dimensionDelta, float positionDelta)
            {
                Vector3 baseDim = dimensions[baseDimKey];
                if ((dimensionDelta > 0 && baseDim.x < 20f) || (dimensionDelta < 0 && baseDim.x > 0.1f))
                {
                    baseDim.x += dimensionDelta;
                    baseDim.y += dimensionDelta;
                    baseDim.z += dimensionDelta;
                    UpdateDimension(baseDimKey, baseDim);

                    Vector3 surDim = dimensions[surDimKey];
                    surDim.z += dimensionDelta;
                    UpdateDimension(surDimKey, surDim);
                    Vector3 surPos = positions[surPosKey];
                    surPos.z -= positionDelta;
                    UpdatePosition(surPosKey, surPos);

                    Vector3 esteDim = dimensions[esteDimKey];
                    esteDim.z += dimensionDelta;
                    UpdateDimension(esteDimKey, esteDim);
                    Vector3 estePos = positions[estePosKey];
                    estePos.x += positionDelta;
                    UpdatePosition(estePosKey, estePos);

                    Vector3 oesteDim = dimensions[oesteDimKey];
                    oesteDim.z += dimensionDelta;
                    UpdateDimension(oesteDimKey, oesteDim);
                    Vector3 oestePos = positions[oestePosKey];
                    oestePos.x -= positionDelta;
                    UpdatePosition(oestePosKey, oestePos);

                    CubeRenderer.LoadFromBoxSettings(Core.Instance.selectedObjectType, currentConfig.BoxSettings);
                    if (refreshConstant)
                    {
                        MarkThingsDirty();
                    }
                }
            }

            if (Widgets.ButtonText(widthUpCoarseButtonRect, "▲"))
            {
                AdjustWidth(1f, 0.5f);
            }

            if (Widgets.ButtonText(widthDownCoarseButtonRect, "▼"))
            {
                AdjustWidth(-1f, -0.5f);
            }

            if (Widgets.ButtonText(widthUpFineButtonRect, "▲"))
            {
                AdjustWidth(0.1f, 0.05f);
            }

            if (Widgets.ButtonText(widthDownFineButtonRect, "▼"))
            {
                AdjustWidth(-0.1f, -0.05f);
            }

            Text.Anchor = TextAnchor.MiddleRight;

            bool notRotated = !isRotatedMode;
            Widgets.Label(topDownLabelRect, "Top & Down");
            Widgets.DrawBox(topDownToggleRect);
            Widgets.Checkbox(topDownToggleRect.position, ref notRotated);
            if (notRotated != !isRotatedMode)
            {
                isRotatedMode = !notRotated;
            }

            Widgets.Label(leftRightLabelRect, "Left & Right");
            Widgets.DrawBox(leftRightToggleRect);
            bool rotated = isRotatedMode;
            Widgets.Checkbox(leftRightToggleRect.position, ref rotated);
            if (rotated != isRotatedMode)
            {
                isRotatedMode = rotated;
            }

            if (isRotatedMode)
            {
                Widgets.Label(topTexLabelRect, "Left Texture");
                Widgets.DrawBox(topTexToggleRect);
                bool leftState = isLeftTexture;
                Widgets.Checkbox(topTexToggleRect.position, ref leftState);
                if (leftState != isLeftTexture)
                {
                    isLeftTexture = true;
                }

                Widgets.Label(bottomTexLabelRect, "Right Texture");
                Widgets.DrawBox(bottomTexToggleRect);
                bool rightState = !isLeftTexture;
                Widgets.Checkbox(bottomTexToggleRect.position, ref rightState);
                if (rightState != !isLeftTexture)
                {
                    isLeftTexture = false;
                }
            }
            else
            {
                Widgets.Label(topTexLabelRect, "Top Texture");
                Widgets.DrawBox(topTexToggleRect);
                bool topState = isTopTexture;
                Widgets.Checkbox(topTexToggleRect.position, ref topState);
                if (topState != isTopTexture)
                {
                    isTopTexture = true;
                }

                Widgets.Label(bottomTexLabelRect, "Down Texture");
                Widgets.DrawBox(bottomTexToggleRect);
                bool bottomState = !isTopTexture;
                Widgets.Checkbox(bottomTexToggleRect.position, ref bottomState);
                if (bottomState != !isTopTexture)
                {
                    isTopTexture = false;
                }
            }

            Text.Anchor = TextAnchor.MiddleLeft;

            if (wasChecked != isRotatedMode)
            {
                MarkThingsDirty();
            }

            container.AddGap(20f);

            float wallHeight = 240f;
            float wallSpacing = 20f;

            string baseUvOffsetKey = isRotatedMode ? "base_rot_uv_offset" : "base_uv_offset";
            string baseUvZoomKey = isRotatedMode ? "base_rot_uv_zoom" : "base_uv_zoom";

            DrawWallSection(container.GetRect(wallHeight), "Base",
                baseDimKey, basePosKey, baseUvOffsetKey, baseUvZoomKey);

            container.AddGap(wallSpacing);

            string surUvOffsetKey = isRotatedMode ? "sur_rot_uv_offset" : "sur_uv_offset";
            string surUvZoomKey = isRotatedMode ? "sur_rot_uv_zoom" : "sur_uv_zoom";

            DrawWallSection(container.GetRect(wallHeight), "Down Wall",
                surDimKey, surPosKey, surUvOffsetKey, surUvZoomKey);

            container.AddGap(wallSpacing);

            string esteUvOffsetKey = isRotatedMode ? "este_rot_uv_offset" : "este_uv_offset";
            string esteUvZoomKey = isRotatedMode ? "este_rot_uv_zoom" : "este_uv_zoom";

            DrawWallSection(container.GetRect(wallHeight), "Right Wall",
                esteDimKey, estePosKey, esteUvOffsetKey, esteUvZoomKey);

            container.AddGap(wallSpacing);

            string oesteUvOffsetKey = isRotatedMode ? "oeste_rot_uv_offset" : "oeste_uv_offset";
            string oesteUvZoomKey = isRotatedMode ? "oeste_rot_uv_zoom" : "oeste_uv_zoom";

            DrawWallSection(container.GetRect(wallHeight), "Left Wall",
                oesteDimKey, oestePosKey, oesteUvOffsetKey, oesteUvZoomKey);

            lastContentHeight = container.ContentHeight;
            container.End();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.windowRect.x = UI.screenWidth - this.windowRect.width - 50f;
            this.windowRect.y = 50f;
        }

        public void ReloadConfig(ObjectConfig config)
        {
            currentConfig = config;
            StoreInitialValues();
            LoadConfigValues();
            if (thingsToUpdate != null)
            {
                thingsToUpdate.Clear();
            }
            if (Find.CurrentMap != null)
            {
                thingsToUpdate = Find.CurrentMap.listerThings.AllThings
                    .Where(t => ObjectConfigManager.GetConfigForThing(t)?.Identifier == currentConfig.Identifier)
                    .ToList();
            }
        }
    }
}