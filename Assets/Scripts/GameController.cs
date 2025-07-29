using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.UI; 

public class GameController : MonoBehaviour
{
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SlotContainerManager slotContainerManager;
    [SerializeField] private WordValidator wordValidator;
    [SerializeField] private GameOverPanel gameOverPanel;
    [SerializeField] private Button submitButton; 

    private int currentLevel = 1;
    private Dictionary<int, int> highScores = new Dictionary<int, int>();

    private void Awake()
    {
        if (levelLoader == null) Debug.LogError("LevelLoader atanmadı!");
        if (slotContainerManager == null) Debug.LogError("SlotContainerManager atanmadı!");
        if (wordValidator == null) Debug.LogError("WordValidator atanmadı!");
        if (gameOverPanel == null) Debug.LogError("GameOverPanel atanmadı!");
        if (submitButton == null) Debug.LogError("SubmitButton atanmadı!");

    
        slotContainerManager.OnWordStateChanged += UpdateSubmitButtonState; 


        LoadHighScores();
    }

    private void Start()
    {
        StartLevel(currentLevel);
        if (submitButton != null)
        {
            submitButton.interactable = false;
        }
    }

    private void OnDestroy()
    {

        if (slotContainerManager != null)
        {
            slotContainerManager.OnWordStateChanged -= UpdateSubmitButtonState;
        }
       
    }

    private void StartLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        slotContainerManager.ResetTotalScore();
        levelLoader.LoadLevel(currentLevel);
        gameOverPanel.Hide();
        Debug.Log($"Level {currentLevel} started.");
        slotContainerManager.UndoAllLetters();
        if (submitButton != null) 
        {
            submitButton.interactable = false;
        }
    }

    
    public void SubmitWordFromUI()
    {
        WordValidationResult result = slotContainerManager.CheckForWordFormation();
        if (result.IsValid)
        {
            slotContainerManager.SubmitWord(); 
            Debug.Log($"Word '{result.FormedWord}' submitted! Current Level Score: {slotContainerManager.TotalScore}");
            CheckGameEndConditions();
        }
        else
        {
            Debug.LogWarning($"Cannot submit. '{result.FormedWord}' is not a valid word.");

            if (submitButton != null)
            {
                submitButton.interactable = false;
            }
        }
    }


    private void UpdateSubmitButtonState(WordValidationResult result)
    {
        if (submitButton != null)
        {
            submitButton.interactable = result.IsValid;
        }
    }

    public void CheckGameEndConditions()
    {
        List<Letter> availableLettersOnBoard = FindObjectsOfType<Letter>()
            .Where(letter => letter.gameObject.activeInHierarchy && !letter.isSelected)
            .ToList();

        bool noRemainingLettersOnBoard = !availableLettersOnBoard.Any();
        bool canFormAnyWord = wordValidator.CanFormWordFromCharacters(availableLettersOnBoard.Select(l => l.characterTextMesh.text[0]).ToList());

        if (noRemainingLettersOnBoard || !canFormAnyWord)
        {
            EndLevel();
        }
    }

    private void EndLevel()
    {
        
        List<Letter> remainingLetters = FindObjectsOfType<Letter>()
            .Where(letter => letter.gameObject.activeInHierarchy && !letter.isSelected)
            .ToList();

        int unusedLetterCount = remainingLetters.Count;
        string penaltyReasonMessage = "";

        if (unusedLetterCount > 0)
        {
            int penaltyAmount = unusedLetterCount * 10;
            slotContainerManager.DeductScore(penaltyAmount);
            penaltyReasonMessage = $"{penaltyAmount} points were deducted from the total score due to {unusedLetterCount} unused letters. 10 points for each letter.";
            Debug.LogWarning(penaltyReasonMessage);
        }

        Debug.Log($"Level {currentLevel} finished! Final Score: {slotContainerManager.TotalScore}");

        bool isThisNewHighScore=false;
        int currentHighScore = highScores.ContainsKey(currentLevel) ? highScores[currentLevel] : 0;
        if (slotContainerManager.TotalScore > currentHighScore)
        {
            isThisNewHighScore = true;
            highScores[currentLevel] = slotContainerManager.TotalScore;
            SaveHighScores();
            currentHighScore = slotContainerManager.TotalScore;
            Debug.Log($"New High Score for Level {currentLevel}: {currentHighScore}");
        }

        gameOverPanel.Show(slotContainerManager.TotalScore, currentHighScore,isThisNewHighScore, penaltyReason: penaltyReasonMessage);
    }

    private void LoadNextLevel()
    {
        int nextLevel = currentLevel + 1;
        TextAsset nextLevelFile = Resources.Load<TextAsset>($"level_{nextLevel}");

        if (nextLevelFile != null)
        {
            StartLevel(nextLevel);
        }
        else
        {
            Debug.Log("No more levels available! Game completed.");
            gameOverPanel.Hide();
        }
    }

    private void RestartCurrentLevel()
    {
        StartLevel(currentLevel);
    }

    private void SaveHighScores()
    {
        string json = JsonUtility.ToJson(new HighScoreData(highScores));
        PlayerPrefs.SetString("HighScores", json);
        PlayerPrefs.Save();
        Debug.Log("High scores saved.");
    }

    private void LoadHighScores()
    {
        if (PlayerPrefs.HasKey("HighScores"))
        {
            string json = PlayerPrefs.GetString("HighScores");
            HighScoreData data = JsonUtility.FromJson<HighScoreData>(json);
            highScores = data.highScores;
            Debug.Log("High scores loaded.");
        }
        else
        {
            highScores = new Dictionary<int, int>();
            Debug.Log("No saved high scores found. Starting fresh.");
        }
    }

    [System.Serializable]
    private class HighScoreData : ISerializationCallbackReceiver
    {
        public List<int> levels;
        public List<int> scores;

        [System.NonSerialized]
        public Dictionary<int, int> highScores;

        public HighScoreData()
        {
            highScores = new Dictionary<int, int>();
        }

        public HighScoreData(Dictionary<int, int> data)
        {
            highScores = data;
        }

        public void OnAfterDeserialize()
        {
            highScores = new Dictionary<int, int>();
            for (int i = 0; i < levels.Count; i++)
            {
                highScores.Add(levels[i], scores[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            levels = highScores.Keys.ToList();
            scores = highScores.Values.ToList();
        }
    }
}