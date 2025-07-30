using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToMainMenuButton : MonoBehaviour
{
    [SerializeField] private Button button;

    [SerializeField] private BackToMainMenuPopUp backToMainMenuPopUp;

    private void Start()
    {
        button.onClick.AddListener(ButtonListener);
    }

    private void ButtonListener()
    {
        backToMainMenuPopUp.OpenPopup();
    }
}
