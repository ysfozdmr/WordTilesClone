using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class WordValidator : MonoBehaviour
{
    [SerializeField] private TextAsset dictionaryFile;

    private HashSet<string> dictionaryWords = new HashSet<string>();

    private void Awake()
    {
        if (dictionaryFile == null)
        {
            Debug.LogError("Dictionary File not assigned to WordValidator! Please assign it in the Inspector.", this);
            return;
        }

        LoadDictionary();
    }

    private void LoadDictionary()
    {
        if (dictionaryFile == null)
        {
            Debug.LogError("No dictionary file found. Word validation will not work.", this);
            return;
        }

        string[] words = dictionaryFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string word in words)
        {
            dictionaryWords.Add(word.Trim().ToUpperInvariant());
        }

        Debug.Log($"Loaded {dictionaryWords.Count} words from dictionary file.");
    }

    public bool IsValidWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }
        return dictionaryWords.Contains(word.ToUpperInvariant());
    }


    public bool CanFormWordFromCharacters(List<char> availableChars)
    {
        if (availableChars == null || availableChars.Count == 0)
        {
            return false;
        }

        Dictionary<char, int> availableCharCounts = new Dictionary<char, int>();
        foreach (char c in availableChars)
        {
            char upperChar = char.ToUpperInvariant(c);
            if (availableCharCounts.ContainsKey(upperChar))
            {
                availableCharCounts[upperChar]++;
            }
            else
            {
                availableCharCounts.Add(upperChar, 1);
            }
        }

        foreach (string word in dictionaryWords)
        {
            if (CanWordBeFormed(word, availableCharCounts))
            {
                return true;
            }
        }
        return false;
    }

    private bool CanWordBeFormed(string word, Dictionary<char, int> availableCharCounts)
    {
        Dictionary<char, int> tempCharCounts = new Dictionary<char, int>(availableCharCounts);

        foreach (char charInWord in word)
        {
            char upperCharInWord = char.ToUpperInvariant(charInWord);
            if (tempCharCounts.ContainsKey(upperCharInWord) && tempCharCounts[upperCharInWord] > 0)
            {
                tempCharCounts[upperCharInWord]--;
            }
            else
            {
                return false; 
            }
        }
        return true; 
    }
}