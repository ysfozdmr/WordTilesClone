using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BackToMainMenuPopUp : MonoBehaviour
{
    [SerializeField] private float animationDuration = 0.3f;

    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [SerializeField] private GameController gameController;

    private void Start()
    {
        noButton.onClick.AddListener(NoButtonListener);
        yesButton.onClick.AddListener(YesButtonListener);
        gameObject.transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
    public void OpenPopup()
    {
        gameObject.SetActive(true);
        gameObject.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
    }

    public void ClosePopup()
    {
        gameObject.transform.DOScale(0f, animationDuration).SetEase(Ease.InBack).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    private void YesButtonListener()
    {
        ClosePopup();
        gameController.BackToMainMenu();
    }

    private void NoButtonListener()
    {
        ClosePopup();
    }
}
