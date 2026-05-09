using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image progressBar;

    private async void Start()
    {
        await LoadResourcesRoutine();
        SceneLoader.LoadHome();
    }

    private async Task LoadResourcesRoutine()
    {
        progressBar.fillAmount = 0f;

        await UpdateProgress(0f, 0.4f, 0.4f);

        Sprite[] sprites = Resources.LoadAll<Sprite>("Tiles");
        TileDefinition[] defs = Resources.LoadAll<TileDefinition>("TileDefinitions");
        
        Debug.Log($"Preloaded {sprites.Length} sprites and {defs.Length} definitions.");

        await UpdateProgress(0.4f, 1f, 0.4f);
        
        await Task.Delay(200);
    }

    private async Task UpdateProgress(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progressBar.fillAmount = Mathf.Lerp(start, end, elapsed / duration);
            await Task.Yield();
        }
        progressBar.fillAmount = end;
    }
}