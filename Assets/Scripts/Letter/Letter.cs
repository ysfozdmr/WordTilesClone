using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

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

    [SerializeField] private TextMeshProUGUI characterTextMesh;
    [SerializeField] private TextMeshProUGUI pointTextMesh;

    private bool isFaceUp = true;
    private bool isAnimating = false;

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


    private void Awake()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Bu objede SpriteRenderer bileşeni bulunamadı!", this);
        }

        if (characterTextMesh == null) // Check if character text mesh is assigned
        {
            Debug.LogError("Character TextMesh bileşeni bulunamadı!", this);
        }

        if (pointTextMesh == null) // Check if point text mesh is assigned
        {
            Debug.LogError("Point TextMesh bileşeni bulunamadı!", this);
        }

        initialScale = transform.localScale;
        blockedBy = new List<int>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Reveal();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Conceal();
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
        }
        else
        {
            spriteRenderer.sprite = backSprite;
            contentCanvas.SetActive(false);
        }

        transform.rotation = Quaternion.identity;
        transform.localScale = initialScale;
    }

    public void SetLetter(string character)
    {
        if (characterTextMesh != null)
        {
            characterTextMesh.text = character;
        }

        if (pointTextMesh != null)
        {
            char upperChar = character.ToUpperInvariant()[0]; // Convert to uppercase for point lookup
            if (letterPoints.TryGetValue(upperChar, out int points))
            {
                pointTextMesh.text = points.ToString();
            }
            else
            {
                pointTextMesh.text = ""; // Or display "0" or "?" for unknown characters
            }
        }
    }
}