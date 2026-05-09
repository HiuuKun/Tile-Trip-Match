using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private Text levelText;
    [SerializeField] private Button homeButton;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject failPanel;

    [Header("Win Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button winHomeButton;

    [Header("Fail Buttons")]
    [SerializeField] private Button replayButton;
    [SerializeField] private Button failHomeButton;

    private void Awake()
    {
        winPanel.SetActive(false);
        failPanel.SetActive(false);

        homeButton.onClick.AddListener(OnHomeClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        winHomeButton.onClick.AddListener(OnHomeClicked);
        replayButton.onClick.AddListener(OnReplayClicked);
        failHomeButton.onClick.AddListener(OnHomeClicked);
    }

    private void OnDestroy()
    {
        homeButton.onClick.RemoveListener(OnHomeClicked);
        continueButton.onClick.RemoveListener(OnContinueClicked);
        winHomeButton.onClick.RemoveListener(OnHomeClicked);
        replayButton.onClick.RemoveListener(OnReplayClicked);
        failHomeButton.onClick.RemoveListener(OnHomeClicked);
    }

    public void SetLevelText(int level)
    {
        levelText.text = $"Level {level}";
    }

    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }

    public void ShowFailPanel()
    {
        failPanel.SetActive(true);
    }

    private void OnContinueClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTap();
        }

        SceneLoader.LoadGameplay();
    }

    private void OnReplayClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTap();
        }
        SceneLoader.RestartGameplay();
    }

    private void OnHomeClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTap();
        }
        SceneLoader.LoadHome();
    }
}