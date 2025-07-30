using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class LevelSelectPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel; 
    [SerializeField] private Transform contentParent; 
    [SerializeField] private GameObject levelItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private float animationDuration = 0.3f;

    [SerializeField] private GameController gameController; 

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
        popupPanel.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
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

        int levelCount = 1;
        while (true)
        {
            TextAsset levelFile = Resources.Load<TextAsset>($"level_{levelCount}");
            if (levelFile == null)
            {
                break;
            }

            Level levelData = JsonUtility.FromJson<Level>(levelFile.text);

            GameObject itemGO = Instantiate(levelItemPrefab, contentParent);
            LevelItemUI itemUI = itemGO.GetComponent<LevelItemUI>();

            int currentHighScore = highScores.ContainsKey(levelCount) ? highScores[levelCount] : 0;
            bool isLocked = levelCount > highestLevelUnlocked;

            itemUI.Setup(levelCount, levelData.title, currentHighScore, isLocked, gameController);

            levelCount++;
        }
    }
}