using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public class AIGameAgent : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private SlotContainerManager slotContainerManager;
    [SerializeField] private WordValidator wordValidator;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private float actionDelay = 0.3f;

    private bool isAITurn = false;
    private const int MAX_WORD_LENGTH = 7;

    public Dictionary<int, Letter> allLettersOnBoard;

    private HashSet<int> destroyedLetterIds = new HashSet<int>();

    private void Awake()
    {
        if (gameController == null) Debug.LogError("GameController atanmad�!");
        if (slotContainerManager == null) Debug.LogError("SlotContainerManager atanmad�!");
        if (wordValidator == null) Debug.LogError("WordValidator atanmad�!");
        if (levelLoader == null) Debug.LogError("LevelLoader atanmad�!");
    }

    public void StartAIGame()
    {
        if (isAITurn) return;
        Debug.Log("AI oyuna ba�l�yor...");
        isAITurn = true;

        destroyedLetterIds.Clear();

        allLettersOnBoard = levelLoader.GetAllSpawnedLetters();

        StartCoroutine(AIGameLoop());
    }

    private List<Letter> GetAvailableLettersForAI()
    {
        List<Letter> availableLetters = new List<Letter>();

        foreach (var kvp in allLettersOnBoard)
        {
            Letter letter = kvp.Value;

            if (letter == null || destroyedLetterIds.Contains(kvp.Key))
                continue;


            try
            {
                if (!letter.gameObject.activeInHierarchy)
                    continue;
            }
            catch (MissingReferenceException)
            {

                destroyedLetterIds.Add(kvp.Key);
                continue;
            }


            if (!letter.isSelected && letter.IsFaceUp() && letter.blockedBy.Count == 0)
            {
                availableLetters.Add(letter);
            }
        }

        return availableLetters;
    }


    public void StopAIGame()
    {
        isAITurn = false;
        StopAllCoroutines();
        Debug.Log("AI oyunu durduruldu.");
    }

    private IEnumerator AIGameLoop()
    {
        while (isAITurn)
        {
            yield return new WaitForSeconds(actionDelay * 2);

            if (!slotContainerManager.isPlacingLetter)
            {
                List<Letter> currentlyAvailableLetters = GetAvailableLettersForAI();
                if (currentlyAvailableLetters.Count == 0 && !slotContainerManager.HasLettersInSlots)
                {
                    Debug.Log("AI: Oynanabilir harf kalmad� ve slotlar bo�. Oyun sonu kontrol ediliyor.");
                    if (!gameController.GetGameOverPanel().gameObject.activeSelf)
                    {
                        Debug.LogWarning("AI: Oynanabilir harf yok ama oyun bitmedi. D�ng�den ��k�l�yor.");
                    }
                    StopAIGame();
                    yield break;
                }


                (string bestWord, List<Letter> lettersToUse) = FindBestWord_ConsideringPenalty(currentlyAvailableLetters);

                if (!string.IsNullOrEmpty(bestWord))
                {
                    Debug.Log($"AI en iyi kelimeyi buldu: {bestWord}");


                    yield return StartCoroutine(PlaceLettersForWord(lettersToUse));


                    yield return new WaitForSeconds(actionDelay);
                    slotContainerManager.SubmitWord();


                    yield return new WaitForSeconds(actionDelay);
                }
                else
                {
                    Debug.Log("AI olu�turulacak yeni bir kelime bulamad�.");

                    if (slotContainerManager.HasLettersInSlots)
                    {
                        Debug.Log("AI: Slotlardaki harfleri geri al�yor.");
                        slotContainerManager.UndoAllLetters();
                        yield return new WaitForSeconds(actionDelay);
                    }
                    else
                    {
                        Debug.Log("AI: Yapacak hamle bulamad� ve slotlar bo�. Oyun sonu kontrol ediliyor.");

                        if (!gameController.GetGameOverPanel().gameObject.activeSelf)
                        {

                            yield return new WaitForSeconds(actionDelay * 5);
                        }
                    }
                }
            }
            else
            {

                yield return null;
            }


            if (gameController.GetGameOverPanel().gameObject.activeSelf)
            {
                StopAIGame();
            }
        }
    }

    private List<Letter> GetCurrentlyPlayableLetters()
    {
        return FindObjectsOfType<Letter>()
                    .Where(letter => letter != null && letter.gameObject.activeInHierarchy && !letter.isSelected && letter.IsFaceUp() && letter.blockedBy.Count == 0)
                    .ToList();
    }



    private (string bestWord, List<Letter> lettersToUse) FindBestWord_ConsideringPenalty(List<Letter> availableLetters)
    {
        string bestWord = "";
        int bestTotalScore = int.MinValue;
        List<Letter> bestLetterCombo = new List<Letter>();

        var dictionaryWordsField = typeof(WordValidator).GetField("dictionaryWords", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        HashSet<string> allDictionaryWords = (HashSet<string>)dictionaryWordsField.GetValue(wordValidator);

        foreach (string word in allDictionaryWords)
        {
            if (word.Length > MAX_WORD_LENGTH)
                continue;

            string upperWord = word.ToUpperInvariant();
            Dictionary<char, int> neededCounts = new Dictionary<char, int>();
            foreach (char c in upperWord)
            {
                if (!neededCounts.ContainsKey(c))
                    neededCounts[c] = 0;
                neededCounts[c]++;
            }

            List<Letter> candidateLetters = new List<Letter>();
            List<Letter> unusedLetters = new List<Letter>(availableLetters);
            HashSet<int> usedIds = new HashSet<int>();
            bool canForm = true;

            foreach (char c in upperWord)
            {
                Letter match = unusedLetters.FirstOrDefault(l =>
                    !usedIds.Contains(l.id) &&
                    l.characterTextMesh != null &&
                    char.ToUpperInvariant(l.characterTextMesh.text[0]) == c);

                if (match == null)
                {
                    canForm = false;
                    break;
                }

                candidateLetters.Add(match);
                usedIds.Add(match.id);
                unusedLetters.Remove(match);
            }

            if (canForm)
            {
                int wordScore = candidateLetters.Sum(l => Letter.GetPointsForCharacter(l.characterTextMesh.text[0]));

                int remainingPenalty = (availableLetters.Count - candidateLetters.Count) * 10;
                int totalScore = wordScore - remainingPenalty;

                if (totalScore > bestTotalScore)
                {
                    bestTotalScore = totalScore;
                    bestWord = word;
                    bestLetterCombo = candidateLetters;
                }
            }
        }

        return (bestWord, bestLetterCombo);
    }

    private IEnumerator PlaceLettersForWord(List<Letter> letters)
    {
        List<Letter> validLetters = new List<Letter>();
        foreach (Letter letter in letters)
        {
            bool isLetterValid = false;
            try
            {
                if (letter != null && letter.gameObject != null && letter.gameObject.activeInHierarchy)
                {
                    if (!letter.IsFaceUp() || letter.blockedBy.Count > 0 || letter.isSelected)
                    {
                        Debug.LogWarning($"AI, harf '{letter.characterTextMesh.text}' �zerine t�klayam�yor. Kelime atland�.");
                        slotContainerManager.UndoAllLetters();
                        yield break;
                    }
                    isLetterValid = true;
                }
            }
            catch (MissingReferenceException)
            {
                Debug.LogWarning("AI: Yok olan harf bulundu. Kelime atland�.");
                destroyedLetterIds.Add(letter.id);
                slotContainerManager.UndoAllLetters();
                yield break;
            }

            if (isLetterValid)
            {
                validLetters.Add(letter);
            }
            else
            {
                Debug.LogWarning("AI: Ge�ersiz harf bulundu. Kelime atland�.");
                slotContainerManager.UndoAllLetters();
                yield break;
            }
        }

        foreach (Letter letter in validLetters)
        {
            bool letterPlacementSuccessful = false;

            try
            {
                letter.AIPickUpLetter();
                letterPlacementSuccessful = true;
            }
            catch (MissingReferenceException)
            {
                Debug.LogWarning("AI: Harf yerle�tirme s�ras�nda harf yok oldu.");
                destroyedLetterIds.Add(letter.id);
                slotContainerManager.UndoAllLetters();
                yield break;
            }

            if (letterPlacementSuccessful)
            {
                yield return new WaitForSeconds(actionDelay + 0.5f);

                bool isPlaced = false;
                try
                {
                    isPlaced = letter.isSelected;
                }
                catch (MissingReferenceException)
                {
                    Debug.LogWarning("AI: Harf kontrol s�ras�nda yok oldu.");
                    destroyedLetterIds.Add(letter.id);
                    slotContainerManager.UndoAllLetters();
                    yield break;
                }

                if (!isPlaced)
                {
                    Debug.LogWarning($"Harf yerle�tirilemedi. Kelime atland�.");
                    slotContainerManager.UndoAllLetters();
                    yield break;
                }
            }
        }
    }

    public void OnWordSubmitted(List<Letter> submittedLetters)
    {
        foreach (Letter letter in submittedLetters)
        {
            if (letter != null)
            {
                destroyedLetterIds.Add(letter.id);
            }
        }
    }

    public bool IsAIAgentActive()
    {
        return isAITurn;
    }
}