using UnityEngine;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
{
    public int levelToLoad = 1;

    // Unity Editor'da ayarlanabilecek ölçeklendirme ve ofset değerleri
    [Header("Positioning Adjustments")]
    [Tooltip("Adjusts the horizontal scale of the tile positions from JSON.")]
    public float positionScaleX = 0.1f; // JSON'daki X koordinatlarını küçültmek için
    [Tooltip("Adjusts the vertical scale of the tile positions from JSON.")]
    public float positionScaleY = 0.1f; // JSON'daki Y koordinatlarını küçültmek için
    [Tooltip("Adjusts the X offset for all tiles in Unity world space.")]
    public float offsetX = 0f; // Tüm harfleri yatayda kaydırmak için
    [Tooltip("Adjusts the Y offset for all tiles in Unity world space.")]
    public float offsetY = 0f; // Tüm harfleri dikeyde kaydırmak için
    [Tooltip("Adjusts the Z scale of the tile positions from JSON.")]
    public float positionScaleZ = 0.01f; // Z koordinatlarını katmanlama için küçük tutmak için

    private Transform levelContainer;

    [SerializeField] private GameObject letterPrefab;

    private Dictionary<int, Letter> spawnedLetters;

    private void Start()
    {
        LoadLevel(levelToLoad);
    }

    public void LoadLevel(int levelNumber)
    {
        ClearExistingLevel();
        
        string fileName = $"level_{levelNumber}";
        
        // Resources klasöründen TextAsset olarak JSON dosyasını yükle
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName); //

        if (jsonFile != null)
        {
            string jsonString = jsonFile.text; //
            Level levelData = JsonUtility.FromJson<Level>(jsonString); //
            
            Debug.Log($"'{levelData.title}' başlıklı Level {levelNumber} yükleniyor..."); //
            GenerateLevel(levelData); //
        }
        else
        {
            Debug.LogError($"{fileName}.json dosyası Assets/Resources klasöründe bulunamadı!"); //
        }
    }

    private void GenerateLevel(Level levelData)
    {
        levelContainer = new GameObject($"Level_{levelToLoad}_Container").transform; //
        spawnedLetters = new Dictionary<int, Letter>(); // Initialize the dictionary

        // First pass: Create all letter objects and store them in the dictionary
        foreach (Tile tile in levelData.tiles) //
        {
            Letter letterComponent = CreateTileObject(tile); //
            if (letterComponent != null) //
            {
                spawnedLetters.Add(letterComponent.id, letterComponent); //
            }
        }

        // Second pass: Populate the 'blockedBy' list for each letter
        foreach (Tile tile in levelData.tiles) //
        {
            foreach (int childId in tile.children) //
            {
                if (spawnedLetters.TryGetValue(childId, out Letter childLetter)) //
                {
                    childLetter.blockedBy.Add(tile.id); //
                }
                else
                {
                    Debug.LogWarning($"Child letter with ID {childId} not found for parent tile ID {tile.id}."); //
                }
            }
        }
    }
    
    private Letter CreateTileObject(Tile tileData)
    {
        GameObject tileObject = Instantiate(letterPrefab); //

        tileObject.name = $"Tile_{tileData.id}_{tileData.character}"; //

        if (levelContainer != null) //
        {
            tileObject.transform.SetParent(levelContainer); //
        }

        // JSON'daki pozisyonları ölçeklendir ve kaydır
        float newX = tileData.position.x * positionScaleX + offsetX;
        float newY = tileData.position.y * positionScaleY + offsetY;
        float newZ = tileData.position.z * positionScaleZ; // Z koordinatını da ölçeklendir

        tileObject.transform.position = new Vector3(newX, newY, newZ); //
        
        Letter letterComponent = tileObject.GetComponent<Letter>(); //

        if (letterComponent != null) //
        {
            letterComponent.id = tileData.id; //
            letterComponent.children = tileData.children; // Still keep children for consistency
            letterComponent.SetLetter(tileData.character); //

            //TODO:fix it
            // Set initial state based on children count
            if (tileData.children.Length == 0) //
            {
                letterComponent.SetState(true); // If no children, it's face up
            }
            else
            {
                letterComponent.SetState(false); // If has children, it's face down (concealed)
            }
        }
        else
        {
            Debug.LogWarning($"Letter component not found on {tileObject.name}. Character and points might not be displayed."); //
        }
        return letterComponent; // Return the Letter component
    }

    private void ClearExistingLevel()
    {
        if (levelContainer != null) //
        {
            Destroy(levelContainer.gameObject); //
        }
        if (spawnedLetters != null) //
        {
            spawnedLetters.Clear(); //
        }
    }
}