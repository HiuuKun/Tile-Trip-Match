using UnityEngine;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private Button playButton;

    private void Start()
    {
        levelText.text = $"Level {SaveManager.CurrentLevel}";
        playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTap();
        }
        SceneLoader.LoadGameplay();
    }   
}