using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using System;

public class WordValidationResult
{
    public string FormedWord;
    public bool IsValid;
    public int PotentialBonus;
    public int Length;
}

public class SlotContainerManager : MonoBehaviour
{
    [SerializeField]
    private List<Slot> allSlots = new List<Slot>();

    private Dictionary<int, Letter> occupiedLetters = new Dictionary<int, Letter>();
    private List<Letter> placedLettersOrder = new List<Letter>();

    private int totalScore = 0;

    public int TotalScore => totalScore;
    public bool HasLettersInSlots => placedLettersOrder.Count > 0;

    public bool isPlacingLetter = false;

    private LevelLoader levelLoader;

    [Header("Word Validation")]
    [SerializeField] private WordValidator wordValidator;

    [SerializeField] private SubmittedWordsDisplay submittedWordsDisplay;

    [SerializeField] private ScoreDisplay scoreDisplay;

    [SerializeField] private GameController gameController;

    public event Action<WordValidationResult> OnWordStateChanged;

    [SerializeField] private AIGameAgent aiGameAgent;


    private void Awake()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
        if (levelLoader == null)
        {
            Debug.LogError("Sahneden LevelLoader bulunamadı! Lütfen atandığından emin olun.");
        }

        if (wordValidator == null)
        {
            Debug.LogError("WordValidator atanmadı! Lütfen Inspector'dan atayın.");
        }

        if (submittedWordsDisplay == null)
        {
            Debug.LogError("SubmittedWordsDisplay atanmadı! Lütfen Inspector'dan atayın.");
        }

        if (gameController == null)
        {
            Debug.LogError("GameController atanmadı! Lütfen Inspector'dan atayın.");
        }

        if (allSlots.Count == 0)
        {
            allSlots = GetComponentsInChildren<Slot>().OrderBy(s => s.transform.position.x).ToList();
        }

        foreach (Slot slot in allSlots)
        {
            slot.IsEmpty = true;
            slot.currentLetter = null;
        }
    }

    private void Start()
    {
        UpdateWordStateForUI();
    }

    public void ResetTotalScore()
    {
        totalScore = 0;
        scoreDisplay?.UpdateScoreText(new WordValidationResult { FormedWord = "", IsValid = false, PotentialBonus = 0, Length = 0 });
    }

    public void DeductScore(int amount)
    {
        totalScore = Mathf.Max(0, totalScore - amount);
        scoreDisplay?.UpdateScoreText(new WordValidationResult { FormedWord = "", IsValid = false, PotentialBonus = 0, Length = 0 });
    }

    public Transform GetEmptySlotTransform()
    {
        if (isPlacingLetter)
        {
            Debug.LogWarning("Şu anda başka bir harf slota yerleştiriliyor. Lütfen bekleyin.");
            return null;
        }

        Slot emptySlot = allSlots.FirstOrDefault(s => s.IsEmpty);

        if (emptySlot != null)
        {
            return emptySlot.transform;
        }
        else
        {
            Debug.LogWarning("Tüm slotlar dolu!");
            return null;
        }
    }

    public void MarkSlotAsOccupied(Transform slotTransform, Letter letter)
    {
        Slot targetSlot = allSlots.FirstOrDefault(s => s.transform == slotTransform);

        if (targetSlot != null)
        {
            targetSlot.IsEmpty = false;
            targetSlot.currentLetter = letter;

            int slotIndex = allSlots.IndexOf(targetSlot);
            if (occupiedLetters.ContainsKey(slotIndex))
            {
                occupiedLetters[slotIndex] = letter;
            }
            else
            {
                occupiedLetters.Add(slotIndex, letter);
            }

            placedLettersOrder.Add(letter);

            Debug.Log($"Slot {slotIndex} dolduruldu: Harf '{letter.characterTextMesh.text}' ile.");
            levelLoader?.OnLetterPlacedInSlot(letter);
            UpdateWordStateForUI();

            isPlacingLetter = false;
        }
        else
        {
            Debug.LogError("Belirtilen Transform'a sahip slot bulunamadı!");
            isPlacingLetter = false;
        }
    }

    public void MarkSlotAsEmpty(Transform slotTransform)
    {
        Slot targetSlot = allSlots.FirstOrDefault(s => s.transform == slotTransform);

        if (targetSlot != null)
        {
            int slotIndex = allSlots.IndexOf(targetSlot);

            if (occupiedLetters.ContainsKey(slotIndex) && occupiedLetters[slotIndex] != null)
            {
                occupiedLetters.Remove(slotIndex);
            }

            targetSlot.IsEmpty = true;
            targetSlot.currentLetter = null;

            Debug.Log($"Slot {slotIndex} boşaltıldı.");
            UpdateWordStateForUI();
        }
        else
        {
            Debug.LogError("Belirtilen Transform'a sahip slot bulunamadı!");
        }
    }

    public Dictionary<int, Letter> GetAllOccupiedLetters()
    {
        return occupiedLetters;
    }

    public void HandleLetterClickInSlot(Letter clickedLetter)
    {
        int clickedSlotIndex = -1;
        for (int i = 0; i < allSlots.Count; i++)
        {
            if (allSlots[i].currentLetter == clickedLetter)
            {
                clickedSlotIndex = i;
                break;
            }
        }

        if (clickedSlotIndex == -1)
        {
            Debug.LogWarning("Tıklanan harf dolu slotlarda bulunamadı.");
            return;
        }

        List<Letter> lettersToReturn = new List<Letter>();
        for (int i = clickedSlotIndex; i < allSlots.Count; i++)
        {
            if (allSlots[i].currentLetter != null)
            {
                lettersToReturn.Add(allSlots[i].currentLetter);
            }
        }

        foreach (Letter letter in lettersToReturn)
        {
            placedLettersOrder.Remove(letter);
        }

        for (int i = lettersToReturn.Count - 1; i >= 0; i--)
        {
            Letter letter = lettersToReturn[i];
            Vector3 originalPos = levelLoader.GetLetterOriginalPosition(letter.id);

            Slot currentSlot = allSlots.FirstOrDefault(s => s.currentLetter == letter);
            if (currentSlot != null)
            {
                MarkSlotAsEmpty(currentSlot.transform);
            }

            letter.ReturnToOriginalPosition(originalPos, () =>
            {
                levelLoader.OnLetterReturnedToOriginalPosition(letter);
            });
        }
        UpdateWordStateForUI();
    }

    public WordValidationResult CheckForWordFormation()
    {
        WordValidationResult result = new WordValidationResult
        {
            FormedWord = "",
            IsValid = false,
            PotentialBonus = 0,
            Length = 0
        };

        if (wordValidator == null)
        {
            Debug.LogError("WordValidator referansı eksik. Kelime doğrulama yapılamıyor.");
            return result;
        }

        StringBuilder currentWordBuilder = new StringBuilder();
        List<Letter> formedLetters = new List<Letter>();

        for (int i = 0; i < allSlots.Count; i++)
        {
            if (allSlots[i].currentLetter != null)
            {
                currentWordBuilder.Append(allSlots[i].currentLetter.characterTextMesh.text[0]);
                formedLetters.Add(allSlots[i].currentLetter);
            }
            else if (currentWordBuilder.Length > 0)
            {
                break;
            }
        }

        string currentWord = currentWordBuilder.ToString();
        result.FormedWord = currentWord;
        result.Length = currentWord.Length;

        if (string.IsNullOrEmpty(currentWord))
        {
            return result;
        }

        if (wordValidator.IsValidWord(currentWord))
        {
            result.IsValid = true;
            int calculatedScore = 0;
            foreach (Letter letter in formedLetters)
            {
                calculatedScore += Letter.GetPointsForCharacter(letter.characterTextMesh.text[0]);
            }
            result.PotentialBonus = calculatedScore;
        }

        return result;
    }

    public void SubmitWord()
    {
        WordValidationResult result = CheckForWordFormation();

        if (result.IsValid)
        {
            totalScore += result.PotentialBonus;
            scoreDisplay?.UpdateScoreText(result);

            List<Letter> lettersInSlots = new List<Letter>();
            for (int i = 0; i < allSlots.Count; i++)
            {
                if (allSlots[i].currentLetter != null)
                {
                    lettersInSlots.Add(allSlots[i].currentLetter);
                }
            }

            if (aiGameAgent != null)
            {
                aiGameAgent.OnWordSubmitted(lettersInSlots);
            }

            AnimateAndDisplaySubmittedWord(lettersInSlots, result.FormedWord);
            gameController.CheckGameEndConditions();
        }
        else
        {
            Debug.Log($"Gönderilemedi: '{result.FormedWord}' geçerli bir kelime değil.");
        }
    }

    public void UndoLastLetter()
    {
        if (placedLettersOrder.Count == 0)
        {
            Debug.Log("Geri alınacak harf yok.");
            return;
        }

        Letter lastLetter = placedLettersOrder[placedLettersOrder.Count - 1];
        placedLettersOrder.RemoveAt(placedLettersOrder.Count - 1);

        Slot currentSlot = allSlots.FirstOrDefault(s => s.currentLetter == lastLetter);
        if (currentSlot != null)
        {
            MarkSlotAsEmpty(currentSlot.transform);
        }

        Vector3 originalPos = levelLoader.GetLetterOriginalPosition(lastLetter.id);
        lastLetter.ReturnToOriginalPosition(originalPos, () =>
        {
            levelLoader.OnLetterReturnedToOriginalPosition(lastLetter);
        });

        Debug.Log($"Son harf '{lastLetter.characterTextMesh.text}' geri alındı.");
        UpdateWordStateForUI();
    }

    public void UndoAllLetters()
    {
        if (placedLettersOrder.Count == 0)
        {
            Debug.Log("Geri alınacak harf yok.");
            return;
        }

        List<Letter> lettersToUndo = new List<Letter>(placedLettersOrder);
        placedLettersOrder.Clear();

        foreach (Letter letter in lettersToUndo)
        {
            Slot currentSlot = allSlots.FirstOrDefault(s => s.currentLetter == letter);
            if (currentSlot != null)
            {
                MarkSlotAsEmpty(currentSlot.transform);
            }

            Vector3 originalPos = levelLoader.GetLetterOriginalPosition(letter.id);
            letter.ReturnToOriginalPosition(originalPos, () =>
            {
                levelLoader.OnLetterReturnedToOriginalPosition(letter);
            });
        }
        Debug.Log("Tüm harflere geri alındı.");
        UpdateWordStateForUI();
    }

    private void AnimateAndDisplaySubmittedWord(List<Letter> letters, string word)
    {
        if (letters.Count == 0)
        {
            submittedWordsDisplay.DisplayWord(word);
            ClearSlotsAfterSubmission();
            return;
        }

        Vector3 targetCenter = submittedWordsDisplay.GetAnimationTargetPosition();
        float animationDuration = 0.5f;
        float delayBetweenLetters = 0.05f;

        float totalLetterAnimationTime = animationDuration + (letters.Count - 1) * delayBetweenLetters;
        float finalCleanupDelay = totalLetterAnimationTime + 0.1f;

        DOVirtual.DelayedCall(finalCleanupDelay, () =>
        {
            submittedWordsDisplay.DisplayWord(word);
            ClearSlotsAfterSubmission();
        });

        for (int i = 0; i < letters.Count; i++)
        {
            Letter letter = letters[i];
            if (letter != null)
            {
                float currentLetterDelay = i * delayBetweenLetters;
                DOVirtual.DelayedCall(currentLetterDelay, () =>
                {
                    letter.MoveTo(targetCenter, animationDuration, () =>
                    {
                        Destroy(letter.gameObject);
                    }, 0.5f, 0.2f, 720f);
                });
            }
        }
    }

    private void ClearSlotsAfterSubmission()
    {
        foreach (Slot slot in allSlots)
        {
            slot.IsEmpty = true;
            slot.currentLetter = null;
        }
        occupiedLetters.Clear();
        placedLettersOrder.Clear();
        UpdateWordStateForUI();
    }

    private void UpdateWordStateForUI()
    {
        WordValidationResult currentResult = CheckForWordFormation();
        OnWordStateChanged?.Invoke(currentResult);
    }
}