using UnityEngine;
using TMPro; 
using DG.Tweening;
using UnityEngine.UI; 

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI penaltyReasonText; 
    
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private GameObject effectObject;
    
    [SerializeField] private ParticleSystem highScoreEffect;
    [SerializeField] private ParticleSystem[] normalScoreEffect;
    
    [SerializeField] private CanvasGroup canvasGroup;
    

    private void Awake()
    {
        if (finalScoreText == null) Debug.LogError("FinalScore TextMeshPro atanmadı!");
        if (highScoreText == null) Debug.LogError("HighScore TextMeshPro atanmadı!");
        if (penaltyReasonText == null) Debug.LogError("PenaltyReason TextMeshPro atanmadı!"); 
        if (canvasGroup == null) Debug.LogError("CanvasGroup atanmadı!");
        Hide();
    }

    public void Show(int finalScore, int highScore, bool isThisNewHighScore, string penaltyReason = "")
    {
      
        highScoreEffect.gameObject.SetActive(false);
        finalScoreText.text = $"Score: {finalScore}";
        highScoreText.text = $"High Score: {highScore}";

        if (!string.IsNullOrEmpty(penaltyReason))
        {
            penaltyReasonText.text = penaltyReason;
            penaltyReasonText.gameObject.SetActive(true);
        }
        else
        {
            penaltyReasonText.gameObject.SetActive(false);
        }

  
        gameObject.SetActive(true);

  
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

      
        Sequence panelOpenSequence = DOTween.Sequence();
        panelOpenSequence.Append(transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        panelOpenSequence.Join(canvasGroup.DOFade(1f, 0.3f));

  
        foreach (var _effect in normalScoreEffect)
        {
            _effect.Play();
        }


        effectObject.transform.DOKill(); 
        effectObject.transform.rotation = Quaternion.identity;
        effectObject.transform.localScale = Vector3.one;


        effectObject.transform.DORotate(new Vector3(0, 0, 360), 2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        if (isThisNewHighScore)
        {
            highScoreEffect.gameObject.SetActive(true);
            highScoreEffect.Play();
            
            effectObject.transform.DOScale(1.2f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }


    public void Hide()
    {
        effectObject.transform.DOKill();
        effectObject.transform.localScale = Vector3.one;
        effectObject.transform.rotation = Quaternion.identity;

        canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

}