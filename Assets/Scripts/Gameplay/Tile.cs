using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private SpriteRenderer holderRenderer;
    [SerializeField] private SpriteRenderer symbolRenderer;

    [Header("Optional Visual Settings")]
    [SerializeField] private Color blockedTint = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color normalTint = Color.white;
    [SerializeField] private float symbolScale = 0.75f;

    public TileDefinition Definition { get; private set; }

    public string TileId => Definition != null ? Definition.id : string.Empty;

    public int Layer { get; private set; }

    public bool IsSelected { get; private set; }

    public bool IsBlocked { get; private set; }

    public int BlockingCount { get; private set; }

    private TileBoard board;
    private BoxCollider2D boxCollider;

    public void Initialize(TileDefinition definition, int layer, int visualOrder, TileBoard ownerBoard)
    {
        Definition = definition;
        Layer = layer;
        board = ownerBoard;

        IsSelected = false;
        IsBlocked = false;
        BlockingCount = 0;

        boxCollider = GetComponent<BoxCollider2D>();

        ValidateReferences();
        ApplyDefinition();
        ApplySize();
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

    private void ApplySize()
    {
        // Tile gameplay size is always 2x2.
        boxCollider.size = new Vector2(GameConstants.TileWidth, GameConstants.TileHeight);
        boxCollider.offset = Vector2.zero;

        // Holder should fill the full 2x2 tile area.
        if (holderRenderer != null && holderRenderer.sprite != null)
        {
            ScaleRendererToSize(
                holderRenderer,
                GameConstants.TileWidth,
                GameConstants.TileHeight
            );
        }

        // Symbol should be slightly smaller than the holder.
        if (symbolRenderer != null && symbolRenderer.sprite != null)
        {
            ScaleRendererToSize(
                symbolRenderer,
                GameConstants.TileWidth * symbolScale,
                GameConstants.TileHeight * symbolScale
            );
        }

        // Keep children centered.
        if (holderRenderer != null)
        {
            holderRenderer.transform.localPosition = Vector3.zero;
        }

        if (symbolRenderer != null)
        {
            symbolRenderer.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        }
    }

    private void ScaleRendererToSize(SpriteRenderer renderer, float targetWidth, float targetHeight)
    {
        Vector2 spriteSize = renderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            Debug.LogWarning($"{renderer.name}: Invalid sprite size.");
            return;
        }

        float scaleX = targetWidth / spriteSize.x;
        float scaleY = targetHeight / spriteSize.y;

        renderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private void ApplyLayerVisual(int visualOrder)
    {
        int baseOrder = Layer * 100 + visualOrder * 2;

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

    private void ApplyNormalVisual()
    {
        SetRenderersTint(normalTint);
    }

    private void OnMouseDown()
    {
        if (IsSelected)
            return;

        if (board == null)
        {
            Debug.Log($"Clicked tile: {TileId}, but no TileBoard is assigned.");
            return;
        }

        board.TrySelectTile(this);
    }

    public Bounds GetBoardBounds()
    {
        Vector3 center = transform.position;

        Vector3 size = new Vector3(
            GameConstants.TileWidth,
            GameConstants.TileHeight,
            0.1f
        );

        return new Bounds(center, size);
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

    public IEnumerator MoveTo(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
    }

    public IEnumerator InvalidSelectionFeedback()
    {
        Vector3 originalPosition = transform.position;

        for (int i = 0; i < 2; i++)
        {
            transform.position = originalPosition + Vector3.right * 0.08f;
            yield return new WaitForSeconds(0.05f);

            transform.position = originalPosition + Vector3.left * 0.08f;
            yield return new WaitForSeconds(0.05f);
        }

        transform.position = originalPosition;
    }

    public IEnumerator RemoveVisual()
    {
        Vector3 startScale = transform.localScale;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}