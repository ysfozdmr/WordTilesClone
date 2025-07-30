using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelSelectPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject levelItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private float animationDuration = 0.3f;

    [SerializeField] private GameController gameController;
    [SerializeField] private ScrollRect scrollRect;

    private void Start()
    {
        popupPanel.transform.localScale = Vector3.zero;
        popupPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        PopulateLevels();
        popupPanel.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            ScrollToHighestUnlockedLevel();
        });
    }

    public void ClosePopup()
    {
        popupPanel.transform.DOScale(0f, animationDuration).SetEase(Ease.InBack).OnComplete(() => {
            popupPanel.SetActive(false);
        });
    }

    private void PopulateLevels()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        int highestLevelUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        Dictionary<int, int> highScores = gameController.GetHighScores();

        List<LevelItemData> unlockedLevelsData = new List<LevelItemData>();
        List<LevelItemData> lockedLevelsData = new List<LevelItemData>();

        int levelCount = 1;
        while (true)
        {
            TextAsset levelFile = Resources.Load<TextAsset>($"level_{levelCount}");
            if (levelFile == null)
            {
                break;
            }

            Level levelData = JsonUtility.FromJson<Level>(levelFile.text);

            int currentHighScore = highScores.ContainsKey(levelCount) ? highScores[levelCount] : 0;
            bool isLocked = levelCount > highestLevelUnlocked;

            LevelItemData itemData = new LevelItemData
            {
                levelIndex = levelCount,
                title = levelData.title,
                score = currentHighScore,
                isLocked = isLocked
            };

            if (isLocked)
            {
                lockedLevelsData.Add(itemData);
            }
            else
            {
                unlockedLevelsData.Add(itemData);
            }

            levelCount++;
        }

        foreach (var data in unlockedLevelsData)
        {
            GameObject itemGO = Instantiate(levelItemPrefab, contentParent);
            LevelItemUI itemUI = itemGO.GetComponent<LevelItemUI>();
            itemUI.Setup(data.levelIndex, data.title, data.score, data.isLocked, gameController);
        }

        foreach (var data in lockedLevelsData)
        {
            GameObject itemGO = Instantiate(levelItemPrefab, contentParent);
            LevelItemUI itemUI = itemGO.GetComponent<LevelItemUI>();
            itemUI.Setup(data.levelIndex, data.title, data.score, data.isLocked, gameController);
        }
    }

    private void ScrollToHighestUnlockedLevel()
    {
        int highestLevelUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        RectTransform targetLevelItemRect = null;

        foreach (Transform child in contentParent)
        {
            LevelItemUI itemUI = child.GetComponent<LevelItemUI>();
            if (itemUI != null && itemUI.GetLevelIndex() == highestLevelUnlocked)
            {
                targetLevelItemRect = child.GetComponent<RectTransform>();
                break;
            }
        }

        if (targetLevelItemRect != null && scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();

            RectTransform contentRect = scrollRect.content;
            RectTransform viewportRect = scrollRect.viewport;

            float targetTopRelativeToContentTop = -targetLevelItemRect.anchoredPosition.y;

            float scrollableHeight = contentRect.rect.height - viewportRect.rect.height;

            if (scrollableHeight <= 0)
            {
                scrollRect.verticalNormalizedPosition = 1f;
                return;
            }

            float desiredScrollY = targetTopRelativeToContentTop;

            desiredScrollY = Mathf.Clamp(desiredScrollY, 0, scrollableHeight);

            float normalizedPosition = 1f - (desiredScrollY / scrollableHeight);

            DOTween.To(() => scrollRect.verticalNormalizedPosition, x => scrollRect.verticalNormalizedPosition = x, normalizedPosition, animationDuration)
                   .SetEase(Ease.OutCubic);
        }
        else if (scrollRect != null)
        {
            DOTween.To(() => scrollRect.verticalNormalizedPosition, x => scrollRect.verticalNormalizedPosition = x, 1f, animationDuration)
                   .SetEase(Ease.OutCubic);
        }
    }

    private class LevelItemData
    {
        public int levelIndex;
        public string title;
        public int score;
        public bool isLocked;
    }
}