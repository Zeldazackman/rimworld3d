# Rim3D

Mod that allows RimWorld to be visualized in 3D. It only affects the visual aspect of the game and does not modify the mechanics.

## Usage
- Key `0`: Switch between 2D and 3D modes
- Key `9`: Open configuration menu
- Key `8`: Show FPS
- ASDW to move the camera, mouse wheel to move the camera up or down
- Shift + Mouse wheel: Rotate the camera.
- Video tutorial comming soon.

## Features

- Switch between 2D and 3D modes
- Transition effect between modes (can be disabled)
- Camera controls.
- Create “3D” objects; you can choose whether objects will be flat, elevated, or cubes. Cubes are formed by rendering the same object 4 times.
- Custom save system for the mod, does not affect the game's original save system.
- The mod includes default presets with preconfigured objects and settings.
- Utilizes the game’s original rendering system, modified for 3D visualization, making it highly efficient as it does not create new objects.
- The original weather system works (improvements with particles planned for the future).
- The game's original lighting works in 3D.
- Configurable mountains at the edges of the map.
- 3D mountains from the game, also configurable.
- Configurable particles for various fire sources.
- Configurable skies. Two large skyboxes can be downloaded here:  
  https://github.com/majausone/rimworld3d/tree/main/Resources/Skys

## Known Issues

- Large animals may appear incorrectly (improvements planned for the future).
- The original plant shader is replaced (can be disabled and requires a game restart). The original shader had visualization issues with the perspective camera when applying autumn colors.

- **Mod Compatibility:**
  - It's best to test mods individually. Pawns are cached, so mods that modify their facial expressions will not work, while others that add hands will work, but the 3D hands may appear in a different position.
  - In general, the mod modifies the plane where objects are rendered, so compatibility with other mods should be high.
  - Except for the plant shader, everything else is deactivated when the 3D mode is turned off. This means that even incompatible mods will work when 3D mode is disabled, as long as the mod is still active.
