// SlotContainerManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening; // DOTween için
using System; // Action için

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
    private List<Letter> placedLettersOrder = new List<Letter>(); // Harflerin yerleştirilme sırasını takip eder

    private int totalScore = 0;

    public int TotalScore => totalScore;
    public bool HasLettersInSlots => placedLettersOrder.Count > 0; // Slotlarda harf olup olmadığını kontrol eder
    
    public bool isPlacingLetter = false;

    private LevelLoader levelLoader;

    [Header("Word Validation")]
    [SerializeField] private WordValidator wordValidator;

    // SubmittedWordsDisplay referansı
    [SerializeField] private SubmittedWordsDisplay submittedWordsDisplay;
    
    [SerializeField] private ScoreDisplay scoreDisplay;

    public event System.Action<WordValidationResult> OnWordStateChanged;

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

    public Transform GetEmptySlotTransform()
    {
        if (isPlacingLetter) // Eğer bir harf zaten yerleştiriliyorsa, yeni bir yerleştirmeyi engelle
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
                if (occupiedLetters[slotIndex] != null)
                {
                    totalScore -= Letter.GetPointsForCharacter(occupiedLetters[slotIndex].characterTextMesh.text[0]);
                }
                occupiedLetters[slotIndex] = letter;
            }
            else
            {
                occupiedLetters.Add(slotIndex, letter);
            }

            totalScore += Letter.GetPointsForCharacter(letter.characterTextMesh.text[0]);
            placedLettersOrder.Add(letter); // Harfi sıraya ekle

            Debug.Log($"Slot {slotIndex} dolduruldu: Harf '{letter.characterTextMesh.text}' ile. Güncel Toplam Puan: {totalScore}");

            levelLoader?.OnLetterPlacedInSlot(letter);
            UpdateWordStateForUI();

            isPlacingLetter = false; // Harf başarıyla yerleştirildi, bayrağı sıfırla
        }
        else
        {
            Debug.LogError("Belirtilen Transform'a sahip slot bulunamadı!");
            isPlacingLetter = false; // Hata durumunda da bayrağı sıfırla
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
                totalScore -= Letter.GetPointsForCharacter(occupiedLetters[slotIndex].characterTextMesh.text[0]);
                occupiedLetters.Remove(slotIndex);
            }

            targetSlot.IsEmpty = true;
            targetSlot.currentLetter = null;

            Debug.Log($"Slot {slotIndex} boşaltıldı. Güncel Toplam Puan: {totalScore}");
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
        foreach (var entry in occupiedLetters)
        {
            if (entry.Value == clickedLetter)
            {
                clickedSlotIndex = entry.Key;
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
            if (occupiedLetters.ContainsKey(i) && occupiedLetters[i] != null)
            {
                lettersToReturn.Add(occupiedLetters[i]);
            }
        }

        foreach(Letter letter in lettersToReturn) // Geri alınan harfleri sıradan çıkar
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
            
            letter.ReturnToOriginalPosition(originalPos, () => {
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
            UpdateWordStateForUI();

            // Harfleri animasyonla tabloya gönder ve slotları temizle
            List<Letter> lettersInSlots = new List<Letter>();
            for (int i = 0; i < allSlots.Count; i++)
            {
                if (allSlots[i].currentLetter != null)
                {
                    lettersInSlots.Add(allSlots[i].currentLetter);
                }
            }
            AnimateAndDisplaySubmittedWord(lettersInSlots, result.FormedWord);
            scoreDisplay.UpdateScoreText(new WordValidationResult());
        }
        else
        {
            Debug.Log($"Gönderilemedi: '{result.FormedWord}' geçerli bir kelime değil.");
        }
    }

    /// <summary>
    /// Slotlara en son eklenen harfi geri alır ve orijinal konumuna gönderir.
    /// </summary>
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
        lastLetter.ReturnToOriginalPosition(originalPos, () => {
            levelLoader.OnLetterReturnedToOriginalPosition(lastLetter);
        });

        Debug.Log($"Son harf '{lastLetter.characterTextMesh.text}' geri alındı. Yeni Toplam Puan: {totalScore}");
        UpdateWordStateForUI();
    }

    /// <summary>
    /// Tüm slotlardaki harfleri boşaltır ve orijinal konumlarına geri gönderir.
    /// </summary>
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
            letter.ReturnToOriginalPosition(originalPos, () => {
                levelLoader.OnLetterReturnedToOriginalPosition(letter);
            });
        }
        Debug.Log("Tüm harflere geri alındı. Yeni Toplam Puan: " + totalScore);
        UpdateWordStateForUI();
    }

    /// <summary>
    /// Harfleri animasyonla gönderilen kelimeler tablosuna doğru hareket ettirir ve sonra yok eder.
    /// Bu versiyon, animasyonların tamamlanmasını daha basit bir gecikme mekanizmasıyla takip eder.
    /// </summary>
    private void AnimateAndDisplaySubmittedWord(List<Letter> letters, string word)
    {
        if (letters.Count == 0)
        {
            submittedWordsDisplay.DisplayWord(word);
            ClearSlotsAfterSubmission();
            return;
        }

        // Animasyon hedef pozisyonunu SubmittedWordsDisplay'den al
        Vector3 targetCenter = submittedWordsDisplay.GetAnimationTargetPosition();
        float animationDuration = 0.5f; // Her harfin hareket animasyon süresi
        float delayBetweenLetters = 0.05f; // Harfler arasında küçük bir gecikme

        // Tüm harflerin animasyonunun tamamlanması için gereken yaklaşık toplam süreyi hesapla.
        // Bu süre, son harfin animasyonu bittikten sonra da küçük bir tampon içerir.
        float totalLetterAnimationTime = animationDuration + (letters.Count - 1) * delayBetweenLetters;
        float finalCleanupDelay = totalLetterAnimationTime + 0.1f; // Tüm animasyonlar bittikten sonraki ek gecikme

        // Belirtilen gecikmenin ardından kelimeyi gösterme ve slotları temizleme işlemini zamanla.
        // Bu, her harfin tamamlanmasını ayrı ayrı takip etme ihtiyacını ortadan kaldırır.
        DOVirtual.DelayedCall(finalCleanupDelay, () => {
            submittedWordsDisplay.DisplayWord(word); // Kelimeyi tabloya ekle
            ClearSlotsAfterSubmission(); // Slotları temizle
        });

        // Her harf için animasyonu gecikmeli olarak başlat
        for (int i = 0; i < letters.Count; i++)
        {
            Letter letter = letters[i];
            if (letter != null)
            {
                float currentLetterDelay = i * delayBetweenLetters;
                DOVirtual.DelayedCall(currentLetterDelay, () => {
                    // Harfi hedefe doğru hareket ettir ve animasyon sonunda yok et
                    // DİKKAT: MoveTo parametre sırası burada güncellendi
                    letter.MoveTo(targetCenter, animationDuration, () => {
                        Destroy(letter.gameObject); // Hedefe varınca harfi yok et
                    }, 0.5f, 0.2f, 720f);
                });
            }
        }
    }

    /// <summary>
    /// Kelime gönderildikten sonra slotları temizler.
    /// </summary>
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