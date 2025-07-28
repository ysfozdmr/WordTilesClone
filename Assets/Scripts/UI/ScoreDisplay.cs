// ScoreDisplay.cs
using UnityEngine;
using TMPro; // TextMeshPro için gerekli
using System; // Action için gerekli (SlotContainerManager'ın eventi için)

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText; // Puanı gösterecek TextMeshProUGUI objesi
    private SlotContainerManager slotContainerManager; // SlotContainerManager referansı

    private void Awake()
    {
        // Sahnedeki SlotContainerManager'ı bul
        slotContainerManager = FindObjectOfType<SlotContainerManager>();
        if (slotContainerManager == null)
        {
            Debug.LogError("Sahneden SlotContainerManager bulunamadı! Puan görüntülenemeyecek.");
        }

        // scoreText'in Inspector'dan atanıp atanmadığını kontrol et
        if (scoreText == null)
        {
            Debug.LogError("Score Text (TextMeshProUGUI) atanmadı! Lütfen Inspector'dan atayın.", this);
        }
    }
    

    private void Start()
    {
        // Oyun başladığında puanı bir kez güncelle
        // SlotContainerManager'ın Start metodu henüz çalışmamış olabileceği için null kontrolü önemli
        if (slotContainerManager != null)
        {
            // İlk açılışta güncel puanı almak için geçici bir WordValidationResult objesi kullanabiliriz
            // Veya SlotContainerManager'ın sadece TotalScore'unu kullanabiliriz.
            // Burada OnWordStateChanged olayının beklediği parametreye uyum sağlamak için boş bir WordValidationResult gönderiyoruz.
            UpdateScoreText(new WordValidationResult()); 
        }
        else if (scoreText != null)
        {
            scoreText.text = "Score: 0"; // SlotContainerManager yoksa varsayılan metin
        }
    }

    // SlotContainerManager'dan gelen OnWordStateChanged olayı bu metodu tetikler.
    public void UpdateScoreText(WordValidationResult result)
    {
        // result parametresi kullanılmasa bile, olayın imzasına uyması gerekir.
        if (scoreText != null && slotContainerManager != null)
        {
            scoreText.text = "Score: " + slotContainerManager.TotalScore.ToString();
        }
    }
}