using UnityEngine;
using System.Collections.Generic;
using System.IO;

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
}