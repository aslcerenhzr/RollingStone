using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager instance;
    public int lastUnlockedLevel = 5; // Oyuncunun ulaşabildiği son level
    public int totalCoins = 0;
    private const string LEVEL_KEY = "LastUnlockedLevel";
    private const string COINS_KEY = "TotalCoins";
    private const string PROGRESS_VERSION_KEY = "ProgressVersion";
    private int progressVersion = 0;

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
        
        progressVersion = PlayerPrefs.GetInt(PROGRESS_VERSION_KEY, 0);

        totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
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

        // ProgressVersion artır: eski "level coin alındı" kayıtları otomatik geçersiz olsun
        progressVersion++;

        PlayerPrefs.SetInt(LEVEL_KEY, lastUnlockedLevel);
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.SetInt(PROGRESS_VERSION_KEY, progressVersion);
        PlayerPrefs.Save();

        Debug.Log("🎯 Tüm ilerleme sıfırlandı! Level ve coin değerleri resetlendi.");
    }

    public void AddCoin(int amount)
    {
        totalCoins += amount;
        PlayerPrefs.SetInt(COINS_KEY, totalCoins); // Kaydet
        PlayerPrefs.Save(); // Diske yaz
        Debug.Log("Toplam Coin: " + totalCoins);
    }

    private string GetCoinsClaimedKey(int levelNumber)
    {
        return $"CoinsClaimed_v{progressVersion}_Level{levelNumber}";
    }

    /// <summary>
    /// Bu level için coin ödülü daha önce verildiyse false döner.
    /// İlk kez veriliyorsa totalCoins'e ekler ve true döner.
    /// </summary>
    public bool AddCoinOnceForLevel(int levelNumber, int amount)
    {
        if (levelNumber <= 0) return false;
        if (amount <= 0) return false;

        string key = GetCoinsClaimedKey(levelNumber);
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            Debug.Log($"[COINS] Level {levelNumber} coin ödülü zaten verildi.");
            return false;
        }

        AddCoin(amount);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log($"[COINS] Level {levelNumber} coin ödülü verildi (+{amount}).");
        return true;
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
