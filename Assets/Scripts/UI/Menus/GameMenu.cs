using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameMenu : Menu
{
    public ScoreDisplay scoreDisplay;
    public SubmittedWordsDisplay wordsDisplay;

    [SerializeField] private TextMeshProUGUI titleText;

    public void UpdateTitleText(string title)
    {
        titleText.text = title;
    }
}
