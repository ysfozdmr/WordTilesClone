
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // IPointerDownHandler, IPointerUpHandler için gerekli
using TMPro;

public class UndoButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button undoButton;
    [SerializeField] private SlotContainerManager slotContainerManager;
    [SerializeField] private float holdDuration = 0.5f; 

    private float pointerDownTimer = 0f;
    private bool isPointerDown = false;
    private bool isHoldTriggered = false; 

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

                slotContainerManager.UndoAllLetters();
                isHoldTriggered = true; 
                Debug.Log("Tümünü geri alma basılı tutma ile tetiklendi.");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;
        isHoldTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        
        if (!isHoldTriggered)
        {
            slotContainerManager.UndoLastLetter();
            Debug.Log("Son harfi geri alma tıklama ile tetiklendi.");
        }

    
        pointerDownTimer = 0f;
        isHoldTriggered = false;
    }
    
}