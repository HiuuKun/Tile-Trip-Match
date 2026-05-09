using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private float currentLevelScale = 1f;

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

        if (levelAsset == null)
        {
            Debug.LogError("No level file found at Resources/Levels/level_1.");
            return;
        }

        LevelData levelData = JsonUtility.FromJson<LevelData>(levelAsset.text);

        if (levelData == null || levelData.tiles == null)
        {
            Debug.LogError("Level data is invalid.");
            return;
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetLevelText(levelData.level);
        }

        if (trayManager != null)
        {
            trayManager.SetCapacity(levelData.trayCapacity);
        }

        currentLevelScale = levelData.scale > 0f ? levelData.scale : 1f;

        SpawnTiles(levelData);

        initialTileCount = activeTiles.Count;

        UpdateBlockedStates();
        UpdateProgressUI();

        Debug.Log($"Loaded level {levelData.level}. Spawned {activeTiles.Count} tiles.");
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
        if (tileData == null)
        {
            Debug.LogError("TileData is null.");
            return;
        }

        if (string.IsNullOrEmpty(tileData.id))
        {
            Debug.LogError("TileData has an empty id.");
            return;
        }

        string resourcePath = $"TileDefinitions/{tileData.id}";

        TileDefinition definition = Resources.Load<TileDefinition>(resourcePath);

        if (definition == null)
        {
            Debug.LogError(
                $"Missing TileDefinition for tile id: {tileData.id}. " +
                $"Expected path: Assets/Resources/{resourcePath}.asset"
            );

            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned in TileBoard inspector.");
            return;
        }

        Vector3 position = GridToWorldPosition(
            tileData.gridX,
            tileData.gridY,
            tileData.layer
        );

        Tile tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);

        tile.Initialize(
            definition,
            tileData.gridX,
            tileData.gridY,
            tileData.layer,
            visualOrder,
            currentLevelScale,
            this
        );

        activeTiles.Add(tile);

        Debug.Log(
            $"Spawned {tileData.id} | grid=({tileData.gridX},{tileData.gridY}) " +
            $"world=({position.x},{position.y}) layer={tileData.layer}"
        );
    }

    private Vector3 GridToWorldPosition(int gridX, int gridY, int layer)
    {
        return new Vector3(
            gridX * GameConstants.TileStepX * currentLevelScale,
            gridY * GameConstants.TileStepY * currentLevelScale,
            -layer * 0.1f
        );
    }

    public void TrySelectTile(Tile tile)
    {
        if (isGameOver || isBusy)
            return;

        if (tile == null)
            return;

        if (tile.IsBlocked)
        {
            gameplayUI.ShowInvalidSelectionMessage();
            return;
        }

        if (trayManager == null)
        {
            Debug.LogWarning("TrayManager is not assigned. Tile selection ignored.");
            return;
        }

        if (!trayManager.CanAddTile())
        {
            FailLevel();
            return;
        }

        _ = SelectTileRoutine(tile);
    }

    private async Task SelectTileRoutine(Tile tile)
    {
        isBusy = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTap();
        }
        tile.MarkSelected();
        activeTiles.Remove(tile);
        UpdateBlockedStates();
        UpdateProgressUI();
        await trayManager.AddTile(tile);
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


    private void UpdateBlockedStates()
    {
        foreach (Tile tile in activeTiles)
        {
            int blockingCount = CountBlockingTiles(tile);
            tile.SetBlocked(blockingCount > 0, blockingCount);

            Debug.Log(
                $"{tile.TileId} grid=({tile.GridX},{tile.GridY}) layer={tile.Layer} " +
                $"blockingCount={blockingCount}"
            );
        }
    }

    private int CountBlockingTiles(Tile tile)
    {
        int count = 0;

        foreach (Tile other in activeTiles)
        {
            if (other == tile)
                continue;

            // Only higher layer tiles can block this tile.
            if (other.Layer <= tile.Layer)
                continue;

            if (DoTilesOverlapByGridPosition(tile, other))
            {
                count++;

                Debug.Log(
                    $"{other.TileId} layer {other.Layer} grid=({other.GridX},{other.GridY}) " +
                    $"blocks {tile.TileId} layer {tile.Layer} grid=({tile.GridX},{tile.GridY})"
                );
            }
        }

        return count;
    }

    private bool DoTilesOverlapByGridPosition(Tile tileA, Tile tileB)
    {
        int deltaX = Mathf.Abs(tileA.GridX - tileB.GridX);
        int deltaY = Mathf.Abs(tileA.GridY - tileB.GridY);

        bool overlaps =
            deltaX < GameConstants.TileGridBlockWidth &&
            deltaY < GameConstants.TileGridBlockHeight;

        return overlaps;
    }

    private void UpdateProgressUI()
    {
        if (gameplayUI != null)
        {
            gameplayUI.SetProgress(activeTiles.Count, initialTileCount);
        }
    }

    private void CompleteLevel()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        SaveManager.AdvanceLevel();

        if (gameplayUI != null)
        {
            gameplayUI.ShowWinPanel();
        }

        Debug.Log("Level complete.");
    }

    private void FailLevel()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (gameplayUI != null)
        {
            gameplayUI.ShowFailPanel();
        }

        Debug.Log("Level failed.");
    }
}