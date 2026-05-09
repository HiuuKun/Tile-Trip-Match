using UnityEngine;

public class MouseCursorManager : MonoBehaviour
{
    [Header("Cursor Texture")]
    [SerializeField] private Texture2D cursorTexture;

    [Header("Click Position")]
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private static MouseCursorManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyCursor();
    }

    private void ApplyCursor()
    {
        if (cursorTexture == null)
        {
            Debug.LogWarning("MouseCursorManager: Cursor texture is missing.");
            return;
        }

        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        Cursor.visible = true;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursor();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
        }
    }
}