using System;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int level;
    public int trayCapacity;
    public TileData[] tiles;
}

[Serializable]
public class TileData
{
    public string id;
    public float x;
    public float y;
    public int layer;
}