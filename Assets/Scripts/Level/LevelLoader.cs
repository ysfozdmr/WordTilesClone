using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Tooltip("Yüklenmesi istenen levelin numarası. (Örn: 1 -> level_1.json)")]
    public int levelToLoad = 1;

    private Transform levelContainer; // Oluşturulan tile'ları sahnede düzenli tutmak için.

    void Start()
    {
        // Başlangıçta Inspector'da belirtilen leveli yükle.
        LoadLevel(levelToLoad);
    }

    /// <summary>
    /// Belirtilen numaraya sahip leveli yükler ve sahnede oluşturur.
    /// </summary>
    /// <param name="levelNumber">Yüklenecek levelin numarası.</param>
    public void LoadLevel(int levelNumber)
    {
        // Önceki levelden kalan objeleri temizle
        ClearExistingLevel();

        // Dosya adını dinamik olarak oluştur. Örn: "level_1"
        string fileName = $"level_{levelNumber}";

        // Belirtilen level dosyasını Resources klasöründen yükle.
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile != null)
        {
            // JSON metnini string olarak al ve Level nesnesine dönüştür.
            string jsonString = jsonFile.text;
            Level levelData = JsonUtility.FromJson<Level>(jsonString);

            // Verinin doğru yüklendiğini kontrol et ve leveli oluştur.
            Debug.Log($"'{levelData.title}' başlıklı Level {levelNumber} yükleniyor...");
            GenerateLevel(levelData);
        }
        else
        {
            Debug.LogError($"{fileName}.json dosyası Assets/Resources klasöründe bulunamadı!");
        }
    }

    /// <summary>
    /// Yüklenen level verisine göre oyun objelerini sahnede oluşturur.
    /// </summary>
    private void GenerateLevel(Level levelData)
    {
        // Tile objelerini sahnede düzenli tutmak için boş bir container obje oluştur.
        levelContainer = new GameObject($"Level_{levelToLoad}_Container").transform;

        foreach (Tile tile in levelData.tiles)
        {
            CreateTileObject(tile);
        }
    }

    /// <summary>
    /// Verilen tile bilgisine göre bir oyun objesi oluşturur.
    /// </summary>
    private void CreateTileObject(Tile tileData)
    {
        // Örnek: Basit bir GameObject ve TextMesh ile tile oluşturma.
        // Daha gelişmiş bir sistem için prefab kullanmak daha iyidir.
        GameObject tileObject = new GameObject($"Tile_{tileData.id}_{tileData.character}");

        // Oluşturulan objeyi container'ın altına al ki hiyerarşi düzenli kalsın.
        if (levelContainer != null)
        {
            tileObject.transform.SetParent(levelContainer);
        }

        // Pozisyonu JSON verisinden ayarla
        tileObject.transform.position = new Vector3(tileData.position.x, tileData.position.y, tileData.position.z);

        // Harfi göstermek için bir TextMesh ekle
        TextMesh textMesh = tileObject.AddComponent<TextMesh>();
        textMesh.text = tileData.character;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 20;
        textMesh.characterSize = 1f; // Boyutu sahnenize göre ayarlayın
    }

    /// <summary>
    /// Sahnede daha önceden oluşturulmuş level objelerini siler.
    /// </summary>
    private void ClearExistingLevel()
    {
        if (levelContainer != null)
        {
            Destroy(levelContainer.gameObject);
        }
    }
}