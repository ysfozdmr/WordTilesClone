
using UnityEngine;
using TMPro; 

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText; 
    private SlotContainerManager slotContainerManager; 

    private void Awake()
    {
        slotContainerManager = FindObjectOfType<SlotContainerManager>();
        if (slotContainerManager == null)
        {
            Debug.LogError("Sahneden SlotContainerManager bulunamadı! Puan görüntülenemeyecek.");
        }
        
        if (scoreText == null)
        {
            Debug.LogError("Score Text (TextMeshProUGUI) atanmadı! Lütfen Inspector'dan atayın.", this);
        }
    }
    

    private void Start()
    {

        if (slotContainerManager != null)
        {
            UpdateScoreText(new WordValidationResult()); 
        }
        else if (scoreText != null)
        {
            scoreText.text = "Score: 0"; 
        }
    }


    public void UpdateScoreText(WordValidationResult result)
    {
        if (scoreText != null && slotContainerManager != null)
        {
            scoreText.text = "Score: " + slotContainerManager.TotalScore.ToString();
        }
    }
}