// UndoButtonUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // IPointerDownHandler, IPointerUpHandler için gerekli
using TMPro;

public class UndoButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button undoButton;
    [SerializeField] private SlotContainerManager slotContainerManager;
    [SerializeField] private float holdDuration = 0.5f; // "Tümünü geri al"ı tetiklemek için basılı tutma süresi

    private float pointerDownTimer = 0f;
    private bool isPointerDown = false;
    private bool isHoldTriggered = false; // Basılı tutma sırasında birden fazla çağrıyı önlemek için

    private void Awake()
    {
        if (undoButton == null) Debug.LogError("Undo Button atanmadı!", this);
        if (slotContainerManager == null) Debug.LogError("SlotContainerManager atanmadı!", this);
    }

    private void Update()
    {
        if (isPointerDown && !isHoldTriggered)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= holdDuration)
            {
                // Basılı tutma algılandı, tümünü geri almayı tetikle
                slotContainerManager.UndoAllLetters();
                isHoldTriggered = true; // Tetiklendi olarak işaretle
                Debug.Log("Tümünü geri alma basılı tutma ile tetiklendi.");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;
        isHoldTriggered = false; // Yeni basış için sıfırla
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        // Eğer basılı tutma tetiklenmediyse, bu bir tıklamadır
        if (!isHoldTriggered)
        {
            slotContainerManager.UndoLastLetter();
            Debug.Log("Son harfi geri alma tıklama ile tetiklendi.");
        }

        // Bir sonraki etkileşim için sıfırla
        pointerDownTimer = 0f;
        isHoldTriggered = false;
    }

    // Düğme devre dışı bırakılmamalı, ancak slotlar boşsa işlem yapmamalıdır.
    // Bu nedenle, burada interactable durumu yönetmeye gerek yoktur.
    private void OnEnable()
    {
        // "disabled olmayacak" gereksinimine göre görsel güncellemeye gerek yok
    }
}