// LevelLoader.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelLoader : MonoBehaviour
{
    public int levelToLoad = 1;

    [Header("Positioning Adjustments")]
    [Tooltip("Adjusts the horizontal scale of the tile positions from JSON.")]
    public float positionScaleX = 0.1f;
    [Tooltip("Adjusts the vertical scale of the tile positions from JSON.")]
    public float positionScaleY = 0.1f;
    [Tooltip("Adjusts the X offset for all tiles in Unity world space.")]
    public float offsetX = 0f;
    [Tooltip("Adjusts the Y offset for all tiles in Unity world space.")]
    public float offsetY = 0f;
    [Tooltip("Adjusts the Z scale of the tile positions from JSON.")]
    public float positionScaleZ = 0.01f;

    private Transform levelContainer;

    [SerializeField] private GameObject letterPrefab;

    private Dictionary<int, Letter> spawnedLetters;
    private Dictionary<int, Vector3> originalLetterPositions; 

    [SerializeField] private SlotContainerManager slotContainerManager; 

    private void Start()
    {
        if (slotContainerManager == null)
        {
            Debug.LogError("SlotContainerManager atanmadı! Lütfen Inspector'dan atayın.");
            return;
        }
        LoadLevel(levelToLoad);
    }

    public void LoadLevel(int levelNumber)
    {
        ClearExistingLevel();

        string fileName = $"level_{levelNumber}";

        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile != null)
        {
            string jsonString = jsonFile.text;
            Level levelData = JsonUtility.FromJson<Level>(jsonString);

            Debug.Log($"'{levelData.title}' başlıklı Level {levelNumber} yükleniyor...");
            GenerateLevel(levelData);
        }
        else
        {
            Debug.LogError($"{fileName}.json dosyası Assets/Resources klasöründe bulunamadı!");
        }
    }

    private void GenerateLevel(Level levelData)
    {
        levelContainer = new GameObject($"Level_{levelToLoad}_Container").transform;
        spawnedLetters = new Dictionary<int, Letter>();
        originalLetterPositions = new Dictionary<int, Vector3>(); 
        foreach (Tile tile in levelData.tiles)
        {
            Letter letterComponent = CreateTileObject(tile);
            if (letterComponent != null)
            {
                spawnedLetters.Add(letterComponent.id, letterComponent);
                originalLetterPositions.Add(letterComponent.id, letterComponent.transform.position);
            }
        }

        foreach (Tile tile in levelData.tiles)
        {
            foreach (int childId in tile.children)
            {
                if (spawnedLetters.TryGetValue(childId, out Letter childLetter))
                {
                    childLetter.blockedBy.Add(tile.id);
                }
                else
                {
                    Debug.LogWarning($"Child letter with ID {childId} not found for parent tile ID {tile.id}.");
                }
            }
        }
        foreach (var entry in spawnedLetters)
        {
            Letter letter = entry.Value;
            letter.SetState(letter.blockedBy.Count == 0);
        }
    }

    private Letter CreateTileObject(Tile tileData)
    {
        GameObject tileObject = Instantiate(letterPrefab);

        tileObject.name = $"Tile_{tileData.id}_{tileData.character}";

        if (levelContainer != null)
        {
            tileObject.transform.SetParent(levelContainer);
        }

        float newX = tileData.position.x * positionScaleX + offsetX;
        float newY = tileData.position.y * positionScaleY + offsetY;
        float newZ = tileData.position.z * positionScaleZ;

        tileObject.transform.position = new Vector3(newX, newY, newZ);

        Letter letterComponent = tileObject.GetComponent<Letter>();

        if (letterComponent != null)
        {
            letterComponent.SetSlotContainerManager(slotContainerManager);

            letterComponent.id = tileData.id;
            letterComponent.children = tileData.children;
            letterComponent.SetLetter(tileData.character);

            // Başlangıç durumu GenerateLevel'ın sonunda ayarlanacak
        }
        else
        {
            Debug.LogWarning($"Letter component not found on {tileObject.name}. Character and points might not be displayed.");
        }
        return letterComponent;
    }

    private void ClearExistingLevel()
    {
        if (levelContainer != null)
        {
            Destroy(levelContainer.gameObject);
        }
        if (spawnedLetters != null)
        {
            spawnedLetters.Clear();
        }
        if (originalLetterPositions != null)
        {
            originalLetterPositions.Clear();
        }
    }

    public void OnLetterPlacedInSlot(Letter placedLetter)
    {
        foreach (int childId in placedLetter.children)
        {
            if (spawnedLetters.TryGetValue(childId, out Letter childLetter))
            {
                childLetter.OnParentLetterTaken(placedLetter.id);
            }
        }
    }
    public Vector3 GetLetterOriginalPosition(int letterId)
    {
        if (originalLetterPositions.ContainsKey(letterId))
        {
            return originalLetterPositions[letterId];
        }
        Debug.LogError($"Letter ID {letterId} için orijinal pozisyon bulunamadı!");
        return Vector3.zero;
    }

    public void OnLetterReturnedToOriginalPosition(Letter returnedLetter)
    {
        returnedLetter.isSelected = false;
        foreach (int childId in returnedLetter.children)
        {
            if (spawnedLetters.TryGetValue(childId, out Letter childLetter))
            {
                childLetter.OnParentLetterReturned(returnedLetter.id);
            }
        }
        returnedLetter.SetState(returnedLetter.blockedBy.Count == 0);
    }
}