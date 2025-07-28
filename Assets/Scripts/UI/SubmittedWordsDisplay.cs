// SubmittedWordsDisplay.cs
using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween için

public class SubmittedWordsDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform wordsContainer; // SubmittedWordsContainer GameObject'i
    [SerializeField] private GameObject wordTextPrefab; // Prefab olarak hazırladığınız kelime metin GameObject'i

    private void Awake()
    {
        if (wordsContainer == null) Debug.LogError("Words Container atanmadı! Lütfen Inspector'dan atayın.", this);
        if (wordTextPrefab == null) Debug.LogError("Word Text Prefab atanmadı! Lütfen Inspector'dan atayın.", this);
    }

    /// <summary>
    /// Harflerin hareket edeceği hedefin pozisyonunu döndürür (SubmittedWordsContainer'ın merkezi).
    /// </summary>
    public Vector3 GetAnimationTargetPosition()
    {
        // RectTransform'ın dünya koordinatlarındaki merkezini döndürür
        return wordsContainer.position;
    }

    /// <summary>
    /// Gönderilen kelimeyi tabloda görüntüler ve küçük bir animasyon yapar.
    /// </summary>
    /// <param name="word">Görüntülenecek kelime.</param>
    public void DisplayWord(string word)
    {
        GameObject newWordTextObj = Instantiate(wordTextPrefab, wordsContainer);
        TextMeshProUGUI wordText = newWordTextObj.GetComponent<TextMeshProUGUI>();

        if (wordText != null)
        {
            // Transform Ayarlarını Sıfırla: Prefabın konumlandırma sorunlarını gidermek için
            // Bu, prefabın layout group içinde doğru şekilde başlamasını sağlar.
            newWordTextObj.transform.localPosition = Vector3.zero;
            newWordTextObj.transform.localRotation = Quaternion.identity;
            newWordTextObj.transform.localScale = Vector3.one; // Varsayılan olarak tam boyutta başlat

            wordText.text = word;

            // Animasyon: Kelimeyi biraz yukarı kaydırarak ve şeffaflaşarak görünür yap
            // Animasyon başlangıcı için ölçeği sıfıra ayarla
            newWordTextObj.transform.localScale = Vector3.zero;
            wordText.alpha = 0f; // Başlangıçta şeffaf yap

            Sequence displaySequence = DOTween.Sequence();

            // Biraz büyüyerek ve şeffaflaşarak görün
            displaySequence.Append(newWordTextObj.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            displaySequence.Join(wordText.DOFade(1f, 0.3f));

            // İsteğe bağlı olarak kelimeyi biraz yukarı hareket ettir
            // Layout Group içinde olduğundan DOLocalMoveY kullanmak daha güvenlidir.
            displaySequence.Join(newWordTextObj.transform.DOLocalMoveY(newWordTextObj.transform.localPosition.y + 10f, 0.3f).SetRelative(true));

            displaySequence.OnComplete(() => {
                Debug.Log($"Kelime '{word}' tabloda görüntülendi.");
            });
        }
        else
        {
            Debug.LogError("Word Text Prefab üzerinde TextMeshProUGUI bulunamadı!");
            Destroy(newWordTextObj); // Temizlik
        }
    }
}