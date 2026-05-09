using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private SpriteRenderer holderRenderer;
    [SerializeField] private SpriteRenderer symbolRenderer;

    [Header("Visual Settings")]
    [SerializeField] private Color normalTint = Color.white;
    [SerializeField] private Color blockedTint = new Color(0.45f, 0.45f, 0.45f, 1f);


    public TileDefinition Definition { get; private set; }

    public string TileId => Definition != null ? Definition.id : string.Empty;

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public int Layer { get; private set; }

    public bool IsSelected { get; private set; }
    public bool IsBlocked { get; private set; }

    public int BlockingCount { get; private set; }

    private TileBoard board;
    private BoxCollider2D boxCollider;

    public void Initialize(
        TileDefinition definition,
        int gridX,
        int gridY,
        int layer,
        int visualOrder,
        float scale,
        TileBoard ownerBoard
    )
    {
        Definition = definition;

        GridX = gridX;
        GridY = gridY;

        Layer = layer;
        board = ownerBoard;

        IsSelected = false;
        IsBlocked = false;
        BlockingCount = 0;

        boxCollider = GetComponent<BoxCollider2D>();

        ValidateReferences();
        ApplyDefinition();
        ApplySize(scale);
        ApplyLayerVisual(visualOrder);
        ApplyNormalVisual();
    }

    private void ValidateReferences()
    {
        if (holderRenderer == null)
        {
            Debug.LogError($"{name}: Holder SpriteRenderer is not assigned.");
        }

        if (symbolRenderer == null)
        {
            Debug.LogError($"{name}: Symbol SpriteRenderer is not assigned.");
        }
    }

    private void ApplyDefinition()
    {
        if (Definition == null)
        {
            Debug.LogError($"{name}: TileDefinition is missing.");
            return;
        }

        if (Definition.sprite == null)
        {
            Debug.LogError($"{name}: Sprite is missing on TileDefinition: {Definition.name}");
            return;
        }

        if (symbolRenderer != null)
        {
            symbolRenderer.sprite = Definition.sprite;
        }
    }

    private void ApplySize(float scale)
    {
        transform.localScale = new Vector3(
            GameConstants.TileWidth * scale,
            GameConstants.TileHeight * scale,
            1f
        );

        if (boxCollider != null)
        {
            boxCollider.offset = Vector2.zero;
        }

        if (holderRenderer != null)
        {
            holderRenderer.transform.localPosition = Vector3.zero;
        }

        if (symbolRenderer != null)
        {
            symbolRenderer.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        }
    }


    private void ApplyLayerVisual(int visualOrder)
    {
        int baseOrder = CalculateSortingOrder(visualOrder);

        if (holderRenderer != null)
        {
            holderRenderer.sortingLayerName = "Tiles";
            holderRenderer.sortingOrder = baseOrder;
        }

        if (symbolRenderer != null)
        {
            symbolRenderer.sortingLayerName = "Tiles";
            symbolRenderer.sortingOrder = baseOrder + 1;
        }

        Vector3 position = transform.position;
        position.z = -Layer * 0.1f;
        transform.position = position;
    }
    private int CalculateSortingOrder(int visualOrder)
    {
        int clampedY = Mathf.Clamp(GridY, -25, 25);
        int clampedX = Mathf.Clamp(GridX, -12, 12);

        int layerOrder = Layer * 2600;
        int yOrder = (25 - clampedY) * 50;
        int xOrder = (clampedX + 12) * 2;

        return layerOrder + yOrder + xOrder;
    }

    public void BringToFront(int orderIndex)
    {
        int baseOrder = 32000 + (orderIndex * 2);

        if (holderRenderer != null)
        {
            holderRenderer.sortingOrder = baseOrder;
        }

        if (symbolRenderer != null)
        {
            symbolRenderer.sortingOrder = baseOrder + 1;
        }

        Vector3 position = transform.position;
        position.z = -5f;
        transform.position = position;
    }

    private void ApplyNormalVisual()
    {
        SetRenderersTint(normalTint);
    }

    private void OnMouseDown()
    {
        if (IsSelected)
            return;

        Debug.Log($"Clicked tile {TileId} at GridX: {GridX}, GridY: {GridY}, Layer: {Layer}");

        if (board == null)
        {
            Debug.Log($"Clicked tile: {TileId}, but no TileBoard is assigned.");
            return;
        }

        board.TrySelectTile(this);
    }

    public void SetBlocked(bool blocked, int blockingCount)
    {
        IsBlocked = blocked;
        BlockingCount = blockingCount;

        SetRenderersTint(blocked ? blockedTint : normalTint);
    }

    private void SetRenderersTint(Color color)
    {
        if (holderRenderer != null)
        {
            holderRenderer.color = color;
        }

        if (symbolRenderer != null)
        {
            symbolRenderer.color = color;
        }
    }

    public void MarkSelected()
    {
        IsSelected = true;
        IsBlocked = false;
        BlockingCount = 0;

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        SetRenderersTint(normalTint);
    }

    public async Task MoveTo(Vector3 targetPosition, float duration, Vector3? targetScale = null)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 finalScale = targetScale ?? new Vector3(GameConstants.TileWidth, GameConstants.TileHeight, 1f);
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(startScale, finalScale, t);

            await Task.Yield();
        }

        transform.position = targetPosition;
        transform.localScale = finalScale;
    }


    public async Task RemoveVisual()
    {
        Vector3 startScale = transform.localScale;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            await Task.Yield();
        }

        Destroy(gameObject);
    }
}