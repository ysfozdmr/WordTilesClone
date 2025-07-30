using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelInfoText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject lockedOverlay;

    private int levelIndex;
    private GameController gameController;

    public void Setup(int level, string title, int score, bool isLocked, GameController controller)
    {
        levelIndex = level;
        gameController = controller;

        levelInfoText.text = $"Level {level} - {title.ToUpper()}";

        if (score > 0)
        {
            highScoreText.text = $"High Score: {score}";
        }
        else
        {
            highScoreText.text = "High Score: -";
        }

        if (isLocked)
        {
            playButton.gameObject.SetActive(false);
            lockedOverlay.SetActive(true);
        }
        else
        {
            playButton.gameObject.SetActive(true);
            lockedOverlay.SetActive(false);
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
    }

    private void OnPlayButtonClicked()
    {
      
        Debug.Log($"Playing Level {levelIndex}");
        gameController.SendMessage("StartLevel", levelIndex, SendMessageOptions.RequireReceiver);
        FindObjectOfType<LevelSelectPopup>().ClosePopup();
    }
}