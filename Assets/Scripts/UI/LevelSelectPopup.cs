using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

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
        // İlk açılışta kontrol
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        if (!PlayerPrefs.HasKey("PreviousHighestLevel"))
        {
            PlayerPrefs.SetInt("PreviousHighestLevel", highestUnlocked);
            PlayerPrefs.Save();
        }

        popupPanel.transform.localScale = Vector3.zero;
        popupPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    public void OpenPopup() // Bu Unity'den çağrılan versiyon
    {
        OpenPopup(null);
    }

    public void OpenPopup(System.Action onOpened = null)
    {
        popupPanel.SetActive(true);
        PopulateLevels();

        popupPanel.transform.DOScale(1f, animationDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                ScrollToLevel(PlayerPrefs.GetInt("HighestLevelUnlocked", 1));
                onOpened?.Invoke(); // sadece dışarıdan çağrıldığında çalışır
        });
    }

    public void ClosePopup()
    {
        popupPanel.transform.DOScale(0f, animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
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
        int previousHighest = PlayerPrefs.GetInt("PreviousHighestLevel", 1);

        Dictionary<int, int> highScores = gameController.GetHighScores();

        int levelCount = 1;
        while (true)
        {
            TextAsset levelFile = Resources.Load<TextAsset>($"level_{levelCount}");
            if (levelFile == null) break;

            Level levelData = JsonUtility.FromJson<Level>(levelFile.text);
            GameObject itemGO = Instantiate(levelItemPrefab, contentParent);
            LevelItemUI itemUI = itemGO.GetComponent<LevelItemUI>();

            int currentHighScore = highScores.ContainsKey(levelCount) ? highScores[levelCount] : 0;
            bool isLocked = levelCount > highestLevelUnlocked;
            bool isNewlyUnlocked = levelCount == highestLevelUnlocked && highestLevelUnlocked > previousHighest;

            itemUI.Setup(levelCount, levelData.title, currentHighScore, isLocked, gameController, isNewlyUnlocked);
            levelCount++;
        }
    }

    private void ScrollToLevel(int targetLevel)
    {
        StartCoroutine(ScrollToLevelCoroutine(targetLevel));
    }

    private IEnumerator ScrollToLevelCoroutine(int targetLevel)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform content = contentParent.GetComponent<RectTransform>();

        for (int i = 0; i < contentParent.childCount; i++)
        {
            LevelItemUI item = contentParent.GetChild(i).GetComponent<LevelItemUI>();
            if (item != null && item.GetLevelIndex() == targetLevel)
            {
                RectTransform itemRect = item.GetComponent<RectTransform>();
                float contentHeight = content.rect.height;
                float viewportHeight = scrollRect.viewport.rect.height;

                float itemPosY = Mathf.Abs(itemRect.anchoredPosition.y);
                float offset = 250;

                float normalizedY = (itemPosY - offset) / (contentHeight - viewportHeight);
                float targetPos = 1f - Mathf.Clamp01(normalizedY);

                DOTween.To(
                    () => scrollRect.verticalNormalizedPosition,
                    x => scrollRect.verticalNormalizedPosition = x,
                    targetPos,
                    0.4f
                ).SetEase(Ease.OutCubic);

                yield break;
            }
        }
    }

    public LevelItemUI GetUnlockedLevelItem()
    {
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);

        for (int i = 0; i < contentParent.childCount; i++)
        {
            LevelItemUI item = contentParent.GetChild(i).GetComponent<LevelItemUI>();
            if (item != null && item.GetLevelIndex() == highestUnlocked)
            {
                return item;
            }
        }

        return null;
    }
}
