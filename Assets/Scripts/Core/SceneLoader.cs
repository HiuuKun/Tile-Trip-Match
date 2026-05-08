using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadLoading()
    {
        SceneManager.LoadScene("Loading");
    }

    public static void LoadHome()
    {
        SceneManager.LoadScene("Home");
    }

    public static void LoadGameplay()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public static void RestartGameplay()
    {
        SceneManager.LoadScene("Gameplay");
    }
}