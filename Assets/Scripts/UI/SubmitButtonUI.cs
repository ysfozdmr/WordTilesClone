using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubmitButtonUI : MonoBehaviour
{
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI pointsTextOnButton;
    [SerializeField] private GameObject[] objectsUnderneath;
    [SerializeField] private SlotContainerManager slotContainerManager;

    [Header("Colors")]
    [SerializeField] private Color enabledColor = new Color32(0x71, 0xD9, 0x23, 0xFF); 
    [SerializeField] private Color disabledColor = Color.black;
    

    private void Awake()
    {
        if (submitButton == null) Debug.LogError("Submit Button atanmadı!", this);
        if (pointsTextOnButton == null) Debug.LogError("Points Text On Button atanmadı!", this);
        if (slotContainerManager == null) Debug.LogError("SlotContainerManager atanmadı!", this);


        submitButton.onClick.AddListener(OnSubmitButtonClicked); 
    }

    private void OnEnable()
    {
       
        if (slotContainerManager != null)
        {
            slotContainerManager.OnWordStateChanged += UpdateUIState; 
            UpdateUIState(slotContainerManager.CheckForWordFormation()); 
        }
    }

    private void OnDisable()
    {
        if (slotContainerManager != null)
        {
            slotContainerManager.OnWordStateChanged -= UpdateUIState;
        }
    }

    private void UpdateUIState(WordValidationResult result) 
    {
        pointsTextOnButton.text = result.PotentialBonus > 0 ? result.PotentialBonus.ToString() : "";
        submitButton.interactable = result.IsValid && result.PotentialBonus > 0; 
        submitButton.image.color = submitButton.interactable ? enabledColor : disabledColor;

        bool shouldTextsUnderneathBeActive = !string.IsNullOrEmpty(pointsTextOnButton.text);
        foreach (GameObject _gameObject in objectsUnderneath)
        {
            if (_gameObject != null)
            {
                _gameObject.gameObject.SetActive(shouldTextsUnderneathBeActive);
            }
        }
    }

    private void OnSubmitButtonClicked()
    {
        if (slotContainerManager != null)
        {
            slotContainerManager.SubmitWord();
        }
    }
}