using UnityEngine;

public static class SaveManager
{
    private const string CurrentLevelKey = "CurrentLevel";

    public static int CurrentLevel
    {
        get => PlayerPrefs.GetInt(CurrentLevelKey, 1);
        set
        {
            PlayerPrefs.SetInt(CurrentLevelKey, value);
            PlayerPrefs.Save();
        }
    }

    public static void AdvanceLevel()
    {
        CurrentLevel++;
    }
}