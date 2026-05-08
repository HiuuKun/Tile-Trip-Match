using UnityEngine;

[CreateAssetMenu(menuName = "Tile Match/Tile Definition", fileName = "NewTileDefinition")]
public class TileDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("Visual")]
    public Sprite sprite;
}