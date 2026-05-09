using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TrayManager : MonoBehaviour
{
    [Header("Tray Slots")]
    [SerializeField] private Transform[] traySlots;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.25f;

    private readonly List<Tile> tilesInTray = new();

    private int currentCapacity;
    public int Capacity => currentCapacity;
    public int Count => tilesInTray.Count;
    public bool IsFull => tilesInTray.Count >= Capacity;

    private void Awake()
    {
        currentCapacity = traySlots.Length;
    }

    public void SetCapacity(int capacity)
    {
        currentCapacity = Mathf.Clamp(capacity, 0, traySlots.Length);

        for (int i = 0; i < traySlots.Length; i++)
        {
            if (traySlots[i] != null)
            {
                traySlots[i].gameObject.SetActive(i < currentCapacity);
            }
        }
    }

    public bool CanAddTile()
    {
        return tilesInTray.Count < Capacity;
    }

    public async Task AddTile(Tile tile)
    {
        int insertIndex = GetInsertIndex(tile.TileId);

        tilesInTray.Insert(insertIndex, tile);

        await MoveAllTilesToSlots();

        await ResolveMatch(tile.TileId);

        await MoveAllTilesToSlots();
    }

    private int GetInsertIndex(string tileId)
    {
        int lastSameIndex = -1;

        for (int i = 0; i < tilesInTray.Count; i++)
        {
            if (tilesInTray[i].TileId == tileId)
            {
                lastSameIndex = i;
            }
        }

        if (lastSameIndex >= 0)
        {
            return lastSameIndex + 1;
        }

        return tilesInTray.Count;
    }

    private async Task ResolveMatch(string tileId)
    {
        List<Tile> matches = tilesInTray.Where(tile => tile.TileId == tileId).Take(3).ToList();

        if (matches.Count < 3)
            return;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMatch();
        }
        foreach (Tile match in matches)
        {
            tilesInTray.Remove(match);
        }

        foreach (Tile match in matches)
        {
            _ = match.RemoveVisual();
        }

        await Task.Delay(250);
    }

    private async Task MoveAllTilesToSlots()
    {
        List<Task> tasks = new();

        for (int i = 0; i < tilesInTray.Count; i++)
        {
            if (i >= traySlots.Length)
                break;

            Tile tile = tilesInTray[i];

            Task task = tile.MoveTo(traySlots[i].position, moveDuration);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
}