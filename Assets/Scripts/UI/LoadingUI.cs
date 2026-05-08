using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image progressBar;

    private IEnumerator Start()
    {
        yield return LoadResourcesRoutine();
        SceneLoader.LoadHome();
    }

    private IEnumerator LoadResourcesRoutine()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime;
            progressBar.fillAmount = progress;
            yield return null;
        }

        //Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Tiles");

        //Debug.Log($"Loaded tile sprites: {loadedSprites.Length}");
    }
}