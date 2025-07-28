// SubmitButtonUI.cs
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
        }
        UpdateUIState(slotContainerManager.CheckForWordFormation()); // Başlangıç durumunu ayarla
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
        // Buton üzerindeki puanı güncelle
        // Kullanıcının isteği doğrultusunda metin formatı değiştirildi:
        // Puan alınamıyorsa hiçbir şey yazma, alabiliyorsa sadece puanı yaz.
        pointsTextOnButton.text = result.PotentialBonus > 0 ? result.PotentialBonus.ToString() : "";

        // Butonun etkileşimini ve rengini ayarla
        submitButton.interactable = result.IsValid && result.PotentialBonus > 0; // Sadece geçerli ve puanı olan kelimeler için etkileşimli
        submitButton.image.color = submitButton.interactable ? enabledColor : disabledColor;

        // Buton altındaki metinlerin aktifliğini ayarla
        // pointsTextOnButton boşsa, bu metinler de devre dışı bırakılmalı
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