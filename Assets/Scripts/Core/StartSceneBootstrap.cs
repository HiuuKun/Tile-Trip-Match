using UnityEngine;

public class StartSceneBootstrap : MonoBehaviour
{
    private void Start()
    {
        PlayerPrefs.DeleteAll();
        SceneLoader.LoadLoading();
    }
}