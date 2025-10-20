using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager instance;
    public int lastUnlockedLevel = 5; // Oyuncunun ulaşabildiği son level
    public int totalCoins = 0;
    private const string LEVEL_KEY = "LastUnlockedLevel";

    void Awake()
    {
        // Singleton ayarı
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahne geçişinde yok olmasın
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        Debug.Log("Coin yüklendi: " + totalCoins);

        lastUnlockedLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);
        Debug.Log("Yüklenen son level: " + lastUnlockedLevel);
    }

    public void CompleteLevel(int levelNumber)
    {
        if (levelNumber >= lastUnlockedLevel)
        {
            lastUnlockedLevel = levelNumber + 1; // Bir sonrakini aç
            PlayerPrefs.SetInt(LEVEL_KEY, lastUnlockedLevel);
            PlayerPrefs.Save();
            Debug.Log("Yeni level açıldı: " + lastUnlockedLevel);
        }
    }

    public void ResetAllProgress()
    {
        lastUnlockedLevel = 1;
        totalCoins = 0;

        PlayerPrefs.SetInt("LastUnlockedLevel", lastUnlockedLevel);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log("🎯 Tüm ilerleme sıfırlandı! Level ve coin değerleri resetlendi.");
    }

    public void AddCoin(int amount)
    {
        totalCoins += amount;
        PlayerPrefs.SetInt("TotalCoins", totalCoins); // Kaydet
        PlayerPrefs.Save(); // Diske yaz
        Debug.Log("Toplam Coin: " + totalCoins);
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            PlayerPrefs.SetInt("TotalCoins", totalCoins); // Kaydet
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
}
