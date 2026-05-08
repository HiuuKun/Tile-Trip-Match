using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrayManager : MonoBehaviour
{
    [Header("Tray Slots")]
    [SerializeField] private Transform[] traySlots;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.25f;

    private readonly List<Tile> tilesInTray = new();

    public int Capacity => traySlots.Length;
    public int Count => tilesInTray.Count;
    public bool IsFull => tilesInTray.Count >= Capacity;

    public bool CanAddTile()
    {
        return tilesInTray.Count < Capacity;
    }

    public IEnumerator AddTile(Tile tile)
    {
        int insertIndex = GetInsertIndex(tile.TileId);

        tilesInTray.Insert(insertIndex, tile);

        yield return MoveAllTilesToSlots();

        yield return ResolveMatch(tile.TileId);

        yield return MoveAllTilesToSlots();
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

    private IEnumerator ResolveMatch(string tileId)
    {
        List<Tile> matches = tilesInTray
            .Where(tile => tile.TileId == tileId)
            .Take(3)
            .ToList();

        if (matches.Count < 3)
            yield break;

        foreach (Tile match in matches)
        {
            tilesInTray.Remove(match);
        }

        foreach (Tile match in matches)
        {
            StartCoroutine(match.RemoveVisual());
        }

        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator MoveAllTilesToSlots()
    {
        List<Coroutine> coroutines = new();

        for (int i = 0; i < tilesInTray.Count; i++)
        {
            Tile tile = tilesInTray[i];

            Coroutine coroutine = StartCoroutine(
                tile.MoveTo(traySlots[i].position, moveDuration)
            );

            coroutines.Add(coroutine);
        }

        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }
    }
}