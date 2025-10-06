using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;
    public int totalCoins = 0;

    void Awake()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        Debug.Log("Coin yüklendi: " + totalCoins);
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


