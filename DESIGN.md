# Tile Trip Match - Design Documentation

## 1. Project Overview

This project is a simplified version of a Tile Trip Match style game built in Unity.

The core gameplay loop is:

1. The player sees a board of layered tiles.
2. Each tile has a symbol/image.
3. Some tiles are blocked by tiles above them.
4. The player can only select unblocked tiles.
5. Selected tiles move into a tray.
6. If three identical tiles are collected in the tray, they are removed.
7. The player wins when all board tiles are cleared.
8. The player fails if the tray becomes full before the board is cleared.

The implementation focuses on clean gameplay logic, reusable UI, JSON-based level data, and support for both portrait and landscape layouts.

---

## 2. Architecture

The project is separated into small systems with clear responsibilities. Gameplay logic, UI logic, scene flow, and level data are kept separate so the project is easier to review, test, and extend.

### 2.1 Scene Flow

The project uses multiple scenes:

```text
Start Scene
    ↓
Loading Scene
    ↓
Home Scene
    ↓
Gameplay Scene
```

### Start Scene

The Start scene is a lightweight bootstrap scene.

Responsibilities:

- Start the game flow.
- Initialize persistent objects if needed.
- Move immediately to the Loading scene.

Main components:

- `StartSceneBootstrap`
- Optional `MouseCursorManager`

### Loading Scene

The Loading scene prepares the game before the player reaches the Home screen.

Responsibilities:

- Display loading progress.
- Load or prepare assets from `Resources`.
- Load level metadata if needed.
- Transition to the Home scene.

Main component:

- `LoadingUI`

### Home Scene

The Home scene is the main menu.

Responsibilities:

- Display the game title.
- Display the current level.
- Show a Play button.
- Support portrait and landscape layout switching.
- Start gameplay when the player presses Play.

Main components:

- `HomeUI`
- `AspectPanelSwitcher`

### Gameplay Scene

The Gameplay scene contains the board, tray, gameplay UI, and win/fail flow.

Responsibilities:

- Load the current level.
- Spawn tiles.
- Calculate blocked/free tile states.
- Handle tile selection.
- Move selected tiles into the tray.
- Resolve matches.
- Check win/fail conditions.
- Show result panels.

Main components:

- `TileBoard`
- `Tile`
- `TrayManager`
- `GameplayUI`
- `AspectPanelSwitcher`

---

## 3. Main Systems

### 3.1 `SceneLoader`

`SceneLoader` is a static helper class used to move between scenes.

Responsibilities:

- Load the Loading scene.
- Load the Home scene.
- Load the Gameplay scene.
- Restart the Gameplay scene.

Reason for design:

- Keeps scene names in one place.
- Avoids hardcoded scene loading logic across many scripts.
- Makes UI button scripts cleaner.

Example:

```csharp
SceneLoader.LoadGameplay();
```

---

### 3.2 `SaveManager`

`SaveManager` handles simple local progress using `PlayerPrefs`.

Responsibilities:

- Store the current level.
- Read the current level.
- Advance to the next level after winning.
- Reset progress if needed.

Stored data:

```text
CurrentLevel
```

Reason for design:

- `PlayerPrefs` is simple and suitable for this assessment.
- No external save system is required.
- Easy to inspect and reset during testing.

---

### 3.3 `GameConstants`

`GameConstants` stores shared gameplay constants.

Responsibilities:

- Store scene names.
- Store default tray capacity.
- Store tile grid blocking size.

Example:

```csharp
public const int TileGridSize = 2;
public const int DefaultTrayCapacity = 7;
```

Current tile rule:

```text
Each tile counts as a 2x2 block in the board grid.
```

The visual size of a tile is controlled by the prefab scale, while the logical blocking size is controlled by the grid size.

---

## 4. Gameplay Architecture

### 4.1 `TileBoard`

`TileBoard` is the main gameplay orchestrator.

Responsibilities:

- Load level JSON from `Resources/Levels`.
- Convert level data into tile objects.
- Spawn tiles onto the board.
- Store active board tiles.
- Determine which tiles are blocked.
- Process tile selection requests.
- Remove selected tiles from the board.
- Send selected tiles to the tray.
- Recalculate blocked states after every move.
- Check win/fail conditions.

`TileBoard` does not directly handle tray matching logic. That responsibility belongs to `TrayManager`.

This keeps responsibilities separated:

```text
TileBoard  = board state and level flow
TrayManager = tray state and match clearing
Tile       = individual tile behavior
GameplayUI = UI display and feedback
```

---

### 4.2 `Tile`

`Tile` represents one tile/card on the board.

Responsibilities:

- Store tile ID/symbol.
- Store grid position.
- Store layer value.
- Store selected state.
- Store blocked state.
- Display the correct sprite.
- React to player input.
- Show blocked visual feedback.
- Animate movement.
- Animate removal.

Important fields:

```text
TileId
GridX
GridY
Layer
IsSelected
IsBlocked
BlockingCount
```

The tile does not decide whether it is selectable. Instead, it asks the board:

```text
Player clicks tile
    ↓
Tile calls TileBoard.TrySelectTile(this)
    ↓
TileBoard checks game state and blocking state
```

Reason for design:

- Tiles remain simple.
- Board keeps authority over gameplay rules.
- Prevents duplicate selection logic inside each tile.

---

### 4.3 `TrayManager`

`TrayManager` manages selected tiles after they leave the board.

Responsibilities:

- Store selected tiles currently in the tray.
- Check tray capacity.
- Insert new tiles into the correct tray position.
- Move tray tiles visually into slots.
- Detect three matching tiles.
- Remove matched tiles.
- Report tray count/full state to the board.

#### Tray Insertion Rule

The tray does not simply append every tile.

The rule is:

```text
If the selected symbol already exists in the tray:
    Insert the new tile after the last tile with the same symbol.

If the selected symbol does not exist in the tray:
    Add the new tile to the end of the tray.
```

Reason for design:

- Matches the behavior of many tile match games.
- Makes the tray easier to read visually.
- Keeps identical symbols grouped together.

---

### 4.4 `GameplayUI`

`GameplayUI` controls the gameplay user interface.

Responsibilities:

- Show current level.
- Show remaining tile count.
- Show invalid selection message.
- Show win panel.
- Show fail panel.
- Handle Continue, Replay, and Home buttons.

`GameplayUI` does not contain gameplay rules. It only receives instructions from gameplay systems, such as:

```text
ShowWinPanel()
ShowFailPanel()
SetProgress(...)
ShowInvalidSelectionMessage()
```

Reason for design:

- UI remains separate from gameplay logic.
- Easier to modify layout without changing game rules.
- Easier to support portrait and landscape panels.

---

### 4.5 `HomeUI`

`HomeUI` controls one Home screen panel.

Responsibilities:

- Show current level.
- Handle Play button click.
- Load Gameplay scene.

In the responsive layout, `HomeUI` can be attached separately to:

```text
PortraitPanel
LandscapePanel
```

Each panel has its own text and button references.

Reason for design:

- Avoids one large script with many portrait/landscape references.
- Each panel manages only its own UI.
- Easier to maintain and debug.

---

### 4.6 `AspectPanelSwitcher`

`AspectPanelSwitcher` enables or disables UI objects based on screen aspect ratio.

Responsibilities:

- Detect whether the screen is portrait or landscape.
- Enable portrait UI objects when height >= width.
- Enable landscape UI objects when width > height.
- Support a list of objects for each orientation.

Example:

```text
Portrait Objects:
- PortraitPanel
- PortraitBackground

Landscape Objects:
- LandscapePanel
- LandscapeBackground
```

Reason for design:

- Allows different layouts for portrait and landscape.
- Avoids complicated anchor-only layouts.
- Keeps both layouts inside the same scene.
- Makes it easy to design UI visually in the Unity Editor.

---

### 4.7 `LoadingUI`

`LoadingUI` controls the loading screen.

Responsibilities:

- Display loading text.
- Display progress bar.
- Load sprites or level resources.
- Move to Home scene when loading is complete.

The loading screen currently uses `Resources.Load` / `Resources.LoadAll`.

Reason for design:

- Simple and suitable for the assessment.
- Easy to run without additional packages.
- Avoids missing external dependencies.

---

### 4.8 `MouseCursorManager`
Responsibilities:

- Apply a custom cursor texture.
- Keep the cursor active across scenes.

Reason for design:

- Improves presentation for desktop gameplay recordings.
- Keeps cursor logic separate from gameplay/UI systems.

---

## 5. Level Data Structure

Level data is stored as JSON files in:

```text
Assets/Resources/Levels/
```

Example files:

```text
level_1.json
level_2.json
level_3.json
```

Unity loads the current level with:

```csharp
Resources.Load<TextAsset>("Levels/level_1");
```

---

### 5.1 Level JSON Format

Example:

```json
{
  "level": 1,
  "trayCapacity": 7,
  "tiles": [
    { "id": "apple", "gridX": 0, "gridY": 0, "layer": 1 },
    { "id": "apple", "gridX": 2, "gridY": 0, "layer": 1 },
    { "id": "apple", "gridX": 4, "gridY": 0, "layer": 1 },

    { "id": "coconut", "gridX": 0, "gridY": 0, "layer": 0 },
    { "id": "coconut", "gridX": 2, "gridY": 0, "layer": 0 },
    { "id": "coconut", "gridX": 4, "gridY": 0, "layer": 0 }
  ]
}
```

---

### 5.2 Level Fields

#### `level`

The level number.

Used for:

- UI display.
- Player progress.
- Debugging.

Example:

```json
"level": 1
```

#### `trayCapacity`

The number of tiles the tray can hold before the player fails.

Example:

```json
"trayCapacity": 7
```

If missing or not used, the game can fall back to:

```text
GameConstants.DefaultTrayCapacity
```

#### `tiles`

An array of tile entries.

Each tile entry contains:

```text
id
gridX
gridY
layer
```

---

### 5.3 Tile Fields

#### `id`

The symbol/image ID of the tile.

Example:

```json
"id": "apple"
```

This is used to load the sprite from:

```text
Resources/Tiles/apple
```

It is also used by the tray matching system.

Three tiles with the same `id` form a match.

#### `gridX` and `gridY`

The logical grid position of the tile.

Example:

```json
"gridX": 2,
"gridY": 0
```

The board converts grid position into world position.

Reason for using grid coordinates:

- Easier to reason about blocking.
- Easier to author levels.
- Easier to check tile overlap.
- Keeps blocking independent from visual prefab scale.

#### `layer`

The depth/layer of the tile.

Example:

```json
"layer": 1
```

Layer rules:

```text
Layer 0 = bottom
Layer 1 = above layer 0
Layer 2 = above layer 1
```

A tile can only be blocked by another tile with a higher layer.

---

## 6. Board Coordinate and Blocking System

### 6.1 Logical Grid

The board uses a logical grid.

Each tile has:

```text
gridX
gridY
layer
```

The tile’s actual visual position is calculated from the grid position.

Example:

```text
worldX = boardOrigin.x + gridX * gridCellWorldSize
worldY = boardOrigin.y + gridY * gridCellWorldSize
```

This allows designers to change visual spacing without changing the blocking rules.

---

### 6.2 Tile Grid Footprint

Each tile counts as a 2x2 grid block.

Example:

```text
Tile at gridX = 0, gridY = 0

Occupied cells:
(0, 0)
(1, 0)
(0, 1)
(1, 1)
```

This is controlled by:

```text
GameConstants.TileGridSize = 2
```

---

### 6.3 Blocking Rule

A tile is blocked if:

```text
Another active tile:
1. Is on a higher layer.
2. Overlaps this tile's 2x2 grid footprint.
```

This means a tile can be blocked by multiple tiles above it.


---

### 6.4 Why Grid-Based Blocking Was Used

The first version used world-space bounds or distance checks. The final approach uses grid overlap because:

- It is simpler to reason about.
- It is independent of sprite size.
- It works even if the tile prefab scale changes.
- It makes JSON levels easier to design.
- It supports the rule that each tile occupies a fixed 2x2 logical space.

The visual size of the tile is controlled by the prefab, while the gameplay blocking footprint is controlled by the logical grid.

---

## 7. Tray System Design

### 7.1 Tray Capacity

The tray has a maximum capacity, usually 7.

If the tray becomes full and no match is cleared, the player fails.

The capacity can come from level data:

```json
"trayCapacity": 7
```

or from a default constant.

---

### 7.2 Tray Slots

The tray uses UI slot positions.

When a tile is selected:

1. The tile is removed from the board list.
2. The tile is inserted into the tray list.
3. The tile animates to its assigned tray slot.
4. Existing tray tiles may shift position.
5. Matching is resolved.

---

### 7.3 Insert Position Logic

The tray insertion algorithm is:

```text
Find the last tile in the tray with the same id.

If found:
    Insert the new tile after it.

If not found:
    Insert the tile at the end.
```

Example:

```text
Before:
A B C A D _

Insert A

After:
A B C A A D _
```

This is done before resolving a match.

---

### 7.4 Match Resolution

After insertion, the tray checks whether there are three tiles with the inserted tile's ID.

If there are three matching tiles:

1. Remove those three tiles from the tray list.
2. Play remove animation.
3. Destroy their GameObjects.
4. Rearrange remaining tiles.

Example:

```text
Before:
A A B C _

Insert A

After insertion:
A A A B C _

After match:
B C _ _ _
```

---

## 8. Win and Fail Conditions

### 8.1 Win Condition

The player wins when:

```text
There are no active board tiles
and
The tray is empty
```

This ensures the player has cleared every tile properly.

When the player wins:

- Show win panel.
- Advance current level with `SaveManager`.
- Allow the player to continue.

---

### 8.2 Fail Condition

The player fails when:

```text
Tray is full
and
The board is not cleared
```

When the player fails:

- Show fail panel.
- Allow replay.
- Allow returning home.

---

## 9. Orientation and UI Design

### 9.1 Supported Aspect Ratios

The game supports both:

```text
16:9 landscape
9:16 portrait
```

The UI can change depending on orientation.

---

### 9.2 Panel-Based Orientation Switching

Instead of forcing one UI layout to work for all aspect ratios, the project uses separate panels:

```text
PortraitPanel
LandscapePanel
```

Only one panel is active at a time.

This is controlled by `AspectPanelSwitcher`.

Reason for design:

- Portrait and landscape layouts can be designed freely.
- Avoids overly complex anchor logic.
- Makes visual testing easier.
- Keeps one scene instead of separate portrait/landscape scenes.

---

### 9.3 Canvas Settings

The UI uses Unity Canvas with a Canvas Scaler.

Recommended settings:

```text
Render Mode: Screen Space - Overlay

Canvas Scaler:
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1080 x 1920
Screen Match Mode: Match Width Or Height
Match: 0.5
```

This allows the UI to scale consistently across different screen sizes.

---

## 10. Asset Loading

Assets are loaded from `Resources`.

### 10.1 Tile Sprites

Tile images are stored in:

```text
Assets/Resources/Tiles/
```

Example:

```text
apple.png
coconut.png
carrot.png
```

A tile with ID:

```json
"id": "apple"
```

loads:

```text
Resources/Tiles/apple
```

---

### 10.2 Level JSON

Level files are stored in:

```text
Assets/Resources/Levels/
```

Example:

```text
level_1.json
```

Loaded with:

```csharp
Resources.Load<TextAsset>("Levels/level_1");
```

---

## 11. Level Solvability

### 11.1 Current Approach

Level solvability is ensured through manual design-time validation.

This means levels are authored carefully so that:

1. Every symbol count is a multiple of 3.
2. The top layer always contains selectable tiles.
3. Removing top tiles unlocks lower tiles.
4. The tray capacity is large enough for the intended selection order.
5. There is at least one path to clear all tiles.

---

### 11.2 Multiple of Three Rule

Every tile ID must appear in a multiple of three.

Valid:

```text
apple x3
coconut x3
carrot x6
```

Invalid:

```text
apple x2
coconut x4
```

Reason:

The game only clears tiles in sets of three. If a symbol does not appear in a multiple of three, the level may become impossible to fully clear.

---

### 11.3 Layering Rule

Tiles on higher layers should unlock the tiles below them.

A simple solvable pattern is:

```text
Layer 1:
apple apple apple

Layer 0:
coconut coconut coconut
```

The player clears apples first. Then coconuts become available.

For more complex layouts, level design should still guarantee that after removing available top-layer tiles, new selectable tiles are revealed.

---

### 11.4 Tray Capacity Rule

The tray capacity is usually 7.

A level should be designed so the player does not need to hold more than 7 unmatched tiles at once.

Simple safe sequence:

```text
A A A B B B C C C
```

This never fills the tray beyond 3 because each group clears immediately.

More complex sequence:

```text
A B A C A B B C C
```

This is still solvable, but it temporarily stores more tiles in the tray.

Designers should test the intended order to make sure it does not exceed tray capacity.

---

### 11.5 Practical Manual Validation Checklist

Before accepting a level, check:

```text
1. Total tile count is divisible by 3.
2. Each tile ID count is divisible by 3.
3. At least one tile is selectable at the start.
4. Removing top-layer matches reveals lower tiles.
5. Tray does not exceed capacity during the intended solution.
6. The level can be completed in Play Mode.
```

---

### 11.6 Future Automated Solver

A future improvement would be adding a solver to automatically validate levels.

The solver would simulate:

1. Active board tiles.
2. Which tiles are currently selectable.
3. Tray insertion.
4. Match clearing.
5. Fail state if tray becomes full.
6. Win state if all tiles are cleared.

The solver could be used in editor tools to reject impossible levels before runtime.

---

## 12. Trade-offs and Areas for Improvement

### 12.1 Blocking Performance

Current blocking update checks every tile against every other tile.

Complexity:

```text
O(N^2)
```

For each tile, the board checks all other tiles to see if they block it.

This is acceptable for small and medium levels, such as:

```text
50 to 200 tiles
```

Possible improvement:

- Use spatial partitioning.
- Use a grid hash.
- Store which tiles occupy each grid cell.
- Only check nearby cells instead of all tiles.

---

### 12.2 Manual Level Authoring

Current levels are manually written in JSON.

Benefits:

- Easy to inspect.
- Easy to version control.
- Simple to load with `Resources`.

Drawbacks:

- Hard to visualize while authoring.
- Easy to make placement mistakes.
- Solvability must be tested manually.

Possible improvement:

- Build a Unity Editor level editor.
- Allow drag-and-drop tile placement.
- Preview blocked states in editor.
- Export JSON automatically.
- Run an automatic solver before saving.

---

### 12.3 Resources Folder Usage

The project uses `Resources.Load`.

Benefits:

- Simple setup.
- No external dependencies.
- Easy for assessment reviewers to run.

Drawbacks:

- Less control over memory.
- All Resources assets can be included in builds.
- Not ideal for large production projects.

Possible improvement:

- Replace `Resources` with Addressables.
- Load levels and sprites asynchronously.
- Support remote content updates.
- Improve memory unloading and dependency tracking.

---

### 12.4 Static Tile Grid Size

Currently all tiles occupy the same logical grid size:

```text
2x2
```

Benefits:

- Simple blocking logic.
- Easy to understand.
- Easy to author JSON levels.

Drawbacks:

- No support for different tile sizes.
- Cannot make special large or small tiles.

Possible improvement:

- Add `width` and `height` to each tile in JSON.
- Update overlap logic to support different footprints.

Example future JSON:

```json
{ "id": "apple", "gridX": 0, "gridY": 0, "width": 2, "height": 2, "layer": 1 }
```

---

### 12.5 Input Method

The project currently uses simple click/tap behavior.

Benefits:

- Works well in Unity Editor.
- Simple for assessment review.
- Easy to test with mouse input.

Possible improvement:

- Add a dedicated input controller.
- Use Physics2D raycasts.
- Support both mouse and touch explicitly.
- Add drag/highlight effects.

---

### 12.6 Animation System

Current animations are lightweight script-based transitions.

Examples:

- Tile move to tray.
- Tile shake on invalid selection.
- Tile scale down on match clear.

Benefits:

- No external plugins.
- Simple and readable.
- Easy to control sequencing.

Drawbacks:

- Limited animation polish.
- No animation curves or timeline control.

Possible improvement:

- Add animation curves.
- Add particle effects.
- Add sound effects.
- Add selectable glow/highlight animation.

---

### 12.7 Orientation Layout Duplication

The project supports different portrait and landscape panels.

Benefits:

- Easy to design visually.
- Clean separation of portrait and landscape layouts.
- Avoids complicated responsive UI calculations.

Drawbacks:

- Some UI elements may be duplicated.
- Buttons/texts in both panels need their own references.
- UI changes may need to be applied twice.

Possible improvement:

- Create shared reusable UI prefabs.
- Use a layout controller for common elements.
- Use one data-binding script for duplicated text/buttons.

---

## 13. Why This Architecture Was Chosen

The architecture was designed to be:

```text
Simple
Readable
Easy to review
Easy to run
Close to the assessment requirements
```

Main decisions:

- Use separate scenes for clear game flow.
- Use JSON for levels so data is separated from code.
- Use grid-based blocking so tile rules are deterministic.
- Use `PlayerPrefs` for simple level progress.
- Use `Resources` for simple asset loading.
- Keep UI logic separate from gameplay logic.
- Use panel switching for portrait/landscape support.

This keeps the project small enough for a technical assessment while still demonstrating clean Unity structure and expandable gameplay systems.

---

## 14. Summary

The project implements the required Tile Trip Match mechanics:

- Loading scene
- Home screen
- Gameplay screen
- Layered tile board
- Blocked tile logic
- 2x2 grid tile footprint
- Tray insertion and matching
- Win/fail flow
- Level progress saving
- Portrait and landscape UI support

The current system is intentionally simple and assessment-friendly, but it is structured so that future improvements such as Addressables, editor level tools, automated solvers, and more advanced animations can be added without rewriting the core gameplay architecture.
