using UnityEngine;
using TMPro; // TextMeshPro için
using System;
using UnityEngine.UI; // Action için

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI penaltyReasonText; // Yeni eklendi
    
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private GameObject effectObject;
    

    private void Awake()
    {
        // UI elemanlarını atadığınızdan emin olun
        if (finalScoreText == null) Debug.LogError("FinalScore TextMeshPro atanmadı!");
        if (highScoreText == null) Debug.LogError("HighScore TextMeshPro atanmadı!");
        if (penaltyReasonText == null) Debug.LogError("PenaltyReason TextMeshPro atanmadı!"); // Yeni kontrol
        Hide();
    }

    public void Show(int finalScore, int highScore, string penaltyReason = "") // penaltyReason eklendi
    {
        finalScoreText.text = $"Score: {finalScore}";
        highScoreText.text = $"High Score: {highScore}";

        if (!string.IsNullOrEmpty(penaltyReason))
        {
            penaltyReasonText.text = penaltyReason;
            penaltyReasonText.gameObject.SetActive(true); // Mesaj varsa göster
        }
        else
        {
            penaltyReasonText.gameObject.SetActive(false); // Mesaj yoksa gizle
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}