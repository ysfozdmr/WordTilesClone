
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SlotContainerManager : MonoBehaviour
{
    [SerializeField]
    private List<Slot> allSlots = new List<Slot>();

    private Dictionary<int, Letter> occupiedLetters = new Dictionary<int, Letter>();

    private int totalScore = 0;

    public int TotalScore => totalScore;

    private LevelLoader levelLoader;

    [Header("Word Validation")]
    [SerializeField] private WordValidator wordValidator; 
  

  
    private HashSet<string> awardedWordsThisSequence = new HashSet<string>();

    private void Awake()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
        if (levelLoader == null)
        {
            Debug.LogError("Sahneden LevelLoader bulunamadý! Lütfen atandýðýndan emin olun.");
        }

        if (wordValidator == null)
        {
            Debug.LogError("WordValidator atanmadý! Lütfen Inspector'dan atayýn.");
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

        awardedWordsThisSequence.Clear();
    }

    public Transform GetEmptySlotTransform()
    {
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

            Debug.Log($"Slot {slotIndex} dolduruldu: Harf '{letter.characterTextMesh.text}' ile. Güncel Toplam Puan: {totalScore}");

            levelLoader?.OnLetterPlacedInSlot(letter);

            CheckForWordFormation();
        }
        else
        {
            Debug.LogError("Belirtilen Transform'a sahip slot bulunamadý!");
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

            Debug.Log($"Slot {slotIndex} boþaltýldý. Güncel Toplam Puan: {totalScore}");

            awardedWordsThisSequence.Clear();
        }
        else
        {
            Debug.LogError("Belirtilen Transform'a sahip slot bulunamadý!");
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
            Debug.LogWarning("Týklanan harf dolu slotlarda bulunamadý.");
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
        awardedWordsThisSequence.Clear();
    }

    private void CheckForWordFormation()
    {
        if (wordValidator == null) return;

        StringBuilder currentWordBuilder = new StringBuilder();
        for (int i = 0; i < allSlots.Count; i++)
        {
          
            if (allSlots[i].currentLetter != null)
            {
                currentWordBuilder.Append(allSlots[i].currentLetter.characterTextMesh.text[0]);
            
            }
           
        }

        string currentWord = currentWordBuilder.ToString();

        if (string.IsNullOrEmpty(currentWord))
        {
            return; 
        }

        if (wordValidator.IsValidWord(currentWord))
        {
            if (!awardedWordsThisSequence.Contains(currentWord))
            {
               // totalScore += wordBonusScore;
                awardedWordsThisSequence.Add(currentWord);
                Debug.Log($"GEÇERLÝ KELÝME OLUÞTU: '{currentWord}'!. Yeni Toplam Puan: {totalScore}");
            }
            else
            {
                Debug.Log($"Kelime '{currentWord}' zaten bonus aldý bu dizilimde.");
            }
        }
        else
        {
         
            Debug.Log($"Kelime '{currentWord}' geçerli bir kelime deðil.");
        }
    }
}