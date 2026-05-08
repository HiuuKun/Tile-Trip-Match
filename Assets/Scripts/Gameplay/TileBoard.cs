using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private TrayManager trayManager;
    [SerializeField] private GameplayUI gameplayUI;

    private readonly List<Tile> activeTiles = new();

    private bool isBusy;
    private bool isGameOver;
    private int initialTileCount;

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        int currentLevel = SaveManager.CurrentLevel;

        TextAsset levelAsset = Resources.Load<TextAsset>($"Levels/level_{currentLevel}");

        if (levelAsset == null)
        {
            Debug.LogWarning($"Level {currentLevel} not found. Loading level 1.");
            currentLevel = 1;
            SaveManager.CurrentLevel = 1;
            levelAsset = Resources.Load<TextAsset>("Levels/level_1");
        }

        LevelData levelData = JsonUtility.FromJson<LevelData>(levelAsset.text);

        gameplayUI.SetLevelText(levelData.level);

        SpawnTiles(levelData);

        initialTileCount = activeTiles.Count;

        UpdateBlockedStates();
        UpdateProgressUI();
    }

    private void SpawnTiles(LevelData levelData)
    {
        for (int i = 0; i < levelData.tiles.Length; i++)
        {
            SpawnTile(levelData.tiles[i], i);
        }
    }

    private void SpawnTile(TileData tileData, int visualOrder)
    {
        TileDefinition definition = Resources.Load<TileDefinition>($"TileDefinitions/{tileData.id}");

        if (definition == null)
        {
            Debug.LogError($"Missing TileDefinition for tile id: {tileData.id}");
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned in TileBoard inspector.");
            return;
        }

        Vector3 position = new Vector3(
            tileData.x,
            tileData.y,
            -tileData.layer * 0.1f
        );

        Tile tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);

        tile.Initialize(definition, tileData.layer, visualOrder, this);

        activeTiles.Add(tile);
    }
    public void TrySelectTile(Tile tile)
    {
        if (isGameOver || isBusy)
            return;

        if (tile.IsBlocked)
        {
            StartCoroutine(HandleInvalidTileSelection(tile));
            return;
        }

        if (!trayManager.CanAddTile())
        {
            FailLevel();
            return;
        }

        StartCoroutine(SelectTileRoutine(tile));
    }

    private IEnumerator SelectTileRoutine(Tile tile)
    {
        isBusy = true;

        tile.MarkSelected();
        activeTiles.Remove(tile);

        yield return trayManager.AddTile(tile);

        UpdateBlockedStates();
        UpdateProgressUI();

        if (activeTiles.Count == 0 && trayManager.Count == 0)
        {
            CompleteLevel();
        }
        else if (trayManager.IsFull)
        {
            FailLevel();
        }

        isBusy = false;
    }

    private IEnumerator HandleInvalidTileSelection(Tile tile)
    {
        gameplayUI.ShowInvalidSelectionMessage();
        yield return tile.InvalidSelectionFeedback();
    }

    private void UpdateBlockedStates()
    {
        foreach (Tile tile in activeTiles)
        {
            int blockingCount = CountBlockingTiles(tile);
            tile.SetBlocked(blockingCount > 0, blockingCount);
        }
    }

    private int CountBlockingTiles(Tile tile)
    {
        int count = 0;
        Bounds tileBounds = tile.GetBoardBounds();

        foreach (Tile other in activeTiles)
        {
            if (other == tile)
                continue;

            if (other.Layer <= tile.Layer)
                continue;

            Bounds otherBounds = other.GetBoardBounds();

            if (otherBounds.Intersects(tileBounds))
            {
                count++;
            }
        }

        return count;
    }

    private void UpdateProgressUI()
    {
        gameplayUI.SetProgress(activeTiles.Count, initialTileCount);
    }

    private void CompleteLevel()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        SaveManager.AdvanceLevel();
        gameplayUI.ShowWinPanel();
    }

    private void FailLevel()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        gameplayUI.ShowFailPanel();
    }
}