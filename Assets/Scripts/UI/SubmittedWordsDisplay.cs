using UnityEngine;
using TMPro;
using DG.Tweening; 

public class SubmittedWordsDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform wordsContainer;
    [SerializeField] private GameObject wordTextPrefab; 

    private void Awake()
    {
        if (wordsContainer == null) Debug.LogError("Words Container atanmadı! Lütfen Inspector'dan atayın.", this);
        if (wordTextPrefab == null) Debug.LogError("Word Text Prefab atanmadı! Lütfen Inspector'dan atayın.", this);
    }

    public Vector3 GetAnimationTargetPosition()
    {
        return wordsContainer.position;
    }


    public void DisplayWord(string word)
    {
        GameObject newWordTextObj = Instantiate(wordTextPrefab, wordsContainer);
        TextMeshProUGUI wordText = newWordTextObj.GetComponent<TextMeshProUGUI>();

        if (wordText != null)
        {
       
            newWordTextObj.transform.localPosition = Vector3.zero;
            newWordTextObj.transform.localRotation = Quaternion.identity;
            newWordTextObj.transform.localScale = Vector3.one; 

            wordText.text = word;

            newWordTextObj.transform.localScale = Vector3.zero;
            wordText.alpha = 0f; 

            Sequence displaySequence = DOTween.Sequence();

            displaySequence.Append(newWordTextObj.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            displaySequence.Join(wordText.DOFade(1f, 0.3f));

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