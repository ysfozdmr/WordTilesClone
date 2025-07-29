using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using System; 

public class Letter : MonoBehaviour
{
    public int id;
    public int[] children;

    public List<int> blockedBy;

    public Sprite frontSprite;
    public Sprite backSprite;

    public GameObject contentCanvas;

    public float flipDuration = 0.6f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] public TextMeshProUGUI characterTextMesh;
    [SerializeField] private TextMeshProUGUI pointTextMesh;

    [SerializeField] private BoxCollider2D boxCollider2D;

    private bool isFaceUp = true;
    private bool isAnimating = false;
    public bool isSelected; 

    private Vector3 initialScale;

    private static readonly Dictionary<char, int> letterPoints = new Dictionary<char, int>()
    {
        { 'E', 1 }, { 'A', 1 }, { 'O', 1 }, { 'N', 1 }, { 'R', 1 }, { 'T', 1 }, { 'L', 1 }, { 'S', 1 }, { 'U', 1 },
        { 'I', 1 },
        { 'D', 2 }, { 'G', 2 },
        { 'B', 3 }, { 'C', 3 }, { 'M', 3 }, { 'P', 3 },
        { 'F', 4 }, { 'H', 4 }, { 'V', 4 }, { 'W', 4 }, { 'Y', 4 },
        { 'K', 5 },
        { 'J', 8 }, { 'X', 8 },
        { 'Q', 10 }, { 'Z', 10 }
    };

    private SlotContainerManager slotContainerManager;

    public static int GetPointsForCharacter(char character)
    {
        char upperChar = char.ToUpperInvariant(character);
        if (letterPoints.TryGetValue(upperChar, out int points))
        {
            return points;
        }
        return 0;
    }

    public void SetSlotContainerManager(SlotContainerManager manager)
    {
        slotContainerManager = manager;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Bu objede SpriteRenderer bileşeni bulunamadı!", this);
        }

        if (characterTextMesh == null)
        {
            Debug.LogError("Character TextMesh bileşeni bulunamadı!", this);
        }

        if (pointTextMesh == null)
        {
            Debug.LogError("Point TextMesh bileşeni bulunamadı!", this);
        }

        if (boxCollider2D == null)
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
            if (boxCollider2D == null)
            {
                Debug.LogError("Bu objede BoxCollider2D bileşeni bulunamadı! Tıklama çalışmayabilir.", this);
            }
        }

        initialScale = transform.localScale;
        blockedBy = new List<int>();
        isSelected = false; 
    }

    private void OnMouseDown()
    {
        if (!boxCollider2D.enabled || isAnimating) return;

        if (isSelected) 
        {
            if (slotContainerManager != null)
            {
                slotContainerManager.HandleLetterClickInSlot(this); 
            }
            else
            {
                Debug.LogError("SlotContainerManager referansı Letter'a atanmadı!");
            }
            return; 
        }

        if (!isFaceUp)
        {
            Debug.Log($"Harf {characterTextMesh.text} henüz açık değil, tıklanamaz.");
            return;
        }

        if (blockedBy.Count > 0)
        {
            Debug.LogWarning($"Harf {characterTextMesh.text} henüz alınamaz, {blockedBy.Count} harf tarafından engelleniyor.");
            return;
        }

        if (slotContainerManager != null)
        {
            PlaceSelfInSlot();
        }
        else
        {
            Debug.LogError("SlotContainerManager referansı Letter'a atanmadı!");
        }
    }

    private void PlaceSelfInSlot()
    {
        Transform emptySlotTransform = slotContainerManager.GetEmptySlotTransform();
        if (emptySlotTransform != null)
        {
          
            slotContainerManager.isPlacingLetter = true; 

            if (boxCollider2D != null) boxCollider2D.enabled = false;

            MoveTo(emptySlotTransform.position, 0.5f, () => {
                slotContainerManager.MarkSlotAsOccupied(emptySlotTransform, this);
                isSelected = true;
                if (boxCollider2D != null) boxCollider2D.enabled = true;
            });
        }
        else
        {
            Debug.LogWarning("Hiç boş slot yok! Harf yerleştirilemedi veya başka bir harf yerleştiriliyor.");
        }
    }

    public void Reveal()
    {
        if (!isFaceUp)
        {
            Flip();
        }
    }

    public void Conceal()
    {
        if (isFaceUp)
        {
            Flip();
        }
    }

    private void Flip()
    {
        if (isAnimating)
            return;

        isAnimating = true;
        isFaceUp = !isFaceUp;

        Sequence flipSequence = DOTween.Sequence();

        flipSequence.Append(transform.DORotate(new Vector3(-90f, 0f, 0f), flipDuration / 2f).SetEase(Ease.InBack));

        flipSequence.AppendCallback(() =>
        {
            if (isFaceUp)
            {
                spriteRenderer.sprite = frontSprite;
                contentCanvas.SetActive(true);
            }
            else
            {
                spriteRenderer.sprite = backSprite;
                contentCanvas.SetActive(false);
            }
            if (boxCollider2D != null)
            {
                boxCollider2D.enabled = isFaceUp;
            }
        });

        flipSequence.Append(transform.DORotate(Vector3.zero, flipDuration / 2f).SetEase(Ease.OutBack));

        flipSequence.OnComplete(() => { isAnimating = false; });
    }

    public void SetState(bool showFace)
    {
        isFaceUp = showFace;
        if (isFaceUp)
        {
            spriteRenderer.sprite = frontSprite;
            contentCanvas.SetActive(true);
            if (boxCollider2D != null) boxCollider2D.enabled = true;
        }
        else
        {
            spriteRenderer.sprite = backSprite;
            contentCanvas.SetActive(false);
            if (boxCollider2D != null) boxCollider2D.enabled = false;
        }

        transform.rotation = Quaternion.identity;
        transform.localScale = initialScale;
    }
    
    public void SetSortingOrder(int order)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = order;
        }
    }

    public void SetLetter(string character)
    {
        if (characterTextMesh != null)
        {
            characterTextMesh.text = character;
        }

        if (pointTextMesh != null)
        {
            char upperChar = character.ToUpperInvariant()[0];
            if (letterPoints.TryGetValue(upperChar, out int points))
            {
                pointTextMesh.text = points.ToString();
            }
            else
            {
                pointTextMesh.text = "";
            }
        }
    }

    public void MoveTo(Vector3 targetPosition, float duration, Action onComplete = null, float jumpHeight = 1f, float scaleDownFactor = 0.6f, float rotationAmount = 360f)
    {
        if (isAnimating)
            return;

        isAnimating = true;

        Sequence moveSequence = DOTween.Sequence();

        float jumpDuration = duration * 0.3f;
        moveSequence.Append(transform.DOMove(transform.position + new Vector3(-1, 1, 0) * jumpHeight, jumpDuration).SetEase(Ease.OutQuad));
        moveSequence.Join(transform.DOScale(initialScale * scaleDownFactor, jumpDuration).SetEase(Ease.OutQuad));
        moveSequence.Join(transform.DORotate(new Vector3(0, 0, -rotationAmount / 4f), jumpDuration).SetEase(Ease.OutQuad));


        float moveDuration = duration * 0.7f;
        moveSequence.Append(transform.DOMove(targetPosition, moveDuration).SetEase(Ease.InOutSine));
        moveSequence.Join(transform.DORotate(new Vector3(0, 0, rotationAmount), moveDuration, RotateMode.FastBeyond360).SetEase(Ease.InOutSine));
        moveSequence.Join(transform.DOScale(initialScale, moveDuration).SetEase(Ease.OutBack));

        moveSequence.OnComplete(() =>
        {
            isAnimating = false;
            Debug.Log("Harf hedefe ulaştı: " + targetPosition + " Süre: " + duration + "s");
            onComplete?.Invoke(); 
        });
    }

    public void ReturnToOriginalPosition(Vector3 originalPos, Action onComplete = null)
    {
        isSelected = false;
        if (boxCollider2D != null) boxCollider2D.enabled = false; 
        MoveTo(originalPos, 0.5f, () => {
            onComplete?.Invoke();
        }, jumpHeight: 1f, scaleDownFactor: 1f, rotationAmount: 0f); 
    }

    public void OnParentLetterTaken(int parentId)
    {
        blockedBy.Remove(parentId);
        if (blockedBy.Count == 0)
        {
            Reveal();
            Debug.Log($"Harf {characterTextMesh.text} artık engellenmiyor ve tıklanabilir.");
        }
    }

    public void OnParentLetterReturned(int parentId)
    {
        if (!blockedBy.Contains(parentId))
        {
            blockedBy.Add(parentId);
        }
      
        if (isFaceUp)
        {
            Conceal();
            Debug.Log($"Harf {characterTextMesh.text} tekrar engellendi ve gizlendi.");
        }
    }
    
}