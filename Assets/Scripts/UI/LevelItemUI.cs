using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class LevelItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelInfoText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject lockedOverlay;

    private int levelIndex;
    private GameController gameController;

    public void Setup(int level, string title, int score, bool isLocked, GameController controller, bool isNewlyUnlocked = false)
    {
        levelIndex = level;
        gameController = controller;

        levelInfoText.text = $"Level {level} - {title.ToUpper()}";
        highScoreText.text = score > 0 ? $"High Score: {score}" : "High Score: -";

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayButtonClicked);

        if (isLocked)
        {
            playButton.gameObject.SetActive(false);
            lockedOverlay.SetActive(true);
        }
        else if (isNewlyUnlocked)
        {
            playButton.gameObject.SetActive(false);
            lockedOverlay.SetActive(true);
        }
        else
        {
            playButton.gameObject.SetActive(true);
            lockedOverlay.SetActive(false);
        }
    }

    public IEnumerator PlayUnlockAnimation()
    {
        yield return new WaitForSeconds(0.2f);

        lockedOverlay.transform.localScale = Vector3.one;
        lockedOverlay.SetActive(true);

        lockedOverlay.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            lockedOverlay.SetActive(false);
            playButton.gameObject.SetActive(true);
            playButton.transform.localScale = Vector3.zero;

            CanvasGroup canvasGroup = playButton.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = playButton.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0;

            Sequence seq = DOTween.Sequence();
            seq.Append(playButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            seq.Join(canvasGroup.DOFade(1f, 0.3f));
        });
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