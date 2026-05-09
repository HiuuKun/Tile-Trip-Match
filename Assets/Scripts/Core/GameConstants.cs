using UnityEngine;

public static class GameConstants
{
    public const string SceneStart = "Start";
    public const string SceneLoading = "Loading";
    public const string SceneHome = "Home";
    public const string SceneGameplay = "Gameplay";

    // Portrait
    public const float TileWidthPortrait = 0.75f;
    public const float TileHeightPortrait = 0.75f;
    public const float TileStepXPortrait = 0.5625f;
    public const float TileStepYPortrait = 0.5625f;

    // Landscape
    public const float TileWidthLandscape = 1f;
    public const float TileHeightLandscape = 1f;
    public const float TileStepXLandscape = 0.75f;
    public const float TileStepYLandscape = 0.75f;

    public static float TileWidth => Screen.height >= Screen.width ? TileWidthPortrait : TileWidthLandscape;
    public static float TileHeight => Screen.height >= Screen.width ? TileHeightPortrait : TileHeightLandscape;
    public static float TileStepX => Screen.height >= Screen.width ? TileStepXPortrait : TileStepXLandscape;
    public static float TileStepY => Screen.height >= Screen.width ? TileStepYPortrait : TileStepYLandscape;

    public const int TileGridBlockWidth = 2;
    public const int TileGridBlockHeight = 2;
    public const int DefaultTrayCapacity = 7;
}