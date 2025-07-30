using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LevelItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelInfoText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    private int levelIndex;
    private GameController gameController;
    private bool _isLocked;

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

        if (isLocked == _isLocked)
        {
            playButton.gameObject.SetActive(!isLocked);
            lockedOverlay.SetActive(isLocked);
            playButton.transform.localScale = isLocked ? Vector3.zero : Vector3.one;
            lockedOverlay.transform.localScale = isLocked ? Vector3.one : Vector3.zero;
        }
        else
        {
            if (isLocked)
            {
                playButton.transform.DOScale(0f, animationDuration).SetEase(hideEase).OnComplete(() =>
                {
                    playButton.gameObject.SetActive(false);
                });
                lockedOverlay.gameObject.SetActive(true);
                lockedOverlay.transform.DOScale(1f, animationDuration).SetEase(showEase);
            }
            else
            {
                lockedOverlay.transform.DOScale(0f, animationDuration).SetEase(hideEase).OnComplete(() =>
                {
                    lockedOverlay.gameObject.SetActive(false);
                });
                playButton.gameObject.SetActive(true);
                playButton.transform.DOScale(1f, animationDuration).SetEase(showEase);

                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
        }
        _isLocked = isLocked;
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log($"Playing Level {levelIndex}");
        gameController.SendMessage("StartLevel", levelIndex, SendMessageOptions.RequireReceiver);
        FindObjectOfType<LevelSelectPopup>().ClosePopup();
    }

    public int GetLevelIndex()
    {
        return levelIndex;
    }
}