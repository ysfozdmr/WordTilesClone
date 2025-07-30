using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI; 

public class SubmittedWordsDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform wordsContainer;
    [SerializeField] private GameObject wordTextPrefab;

    [SerializeField] private float targetAnimationZDepth = 0f;

    private void Awake()
    {
        if (wordsContainer == null) Debug.LogError("Words Container atanmadı! Lütfen Inspector'dan atayın.", this);
        if (wordTextPrefab == null) Debug.LogError("Word Text Prefab atanmadı! Lütfen Inspector'dan atayın.", this);
    }

    public Vector3 GetAnimationTargetPosition()
    {
       

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
        
           
            return wordsContainer.position;
        }

      
        Vector3 screenPos = wordsContainer.position;

    
        Vector3 worldTargetPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, targetAnimationZDepth));

        Debug.Log("target pos :" + worldTargetPosition);
        return worldTargetPosition;
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

           
            LayoutRebuilder.ForceRebuildLayoutImmediate(wordsContainer);

            Sequence displaySequence = DOTween.Sequence();

            displaySequence.Append(newWordTextObj.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            displaySequence.Join(wordText.DOFade(1f, 0.3f));

  


            displaySequence.OnComplete(() =>
            {
                Debug.Log($"Kelime '{word}' tabloda görüntülendi.");
            });
        }
        else
        {
            Debug.LogError("Word Text Prefab üzerinde TextMeshProUGUI bulunamadı!");
            Destroy(newWordTextObj); // Temizlik
        }
    }

    public void ClearWords()
    {
        if (wordsContainer.childCount > 0)
        {
            for (int i = 0; i < wordsContainer.childCount; i++)
            {
                Destroy(wordsContainer.GetChild(i).gameObject);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(wordsContainer);
        }
    }
}