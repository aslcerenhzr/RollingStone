using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameOverManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject GameOverPanel;
    public GameObject winPanel;
    public GameObject bonusPanel;
    public GameObject ExtraMoves;
    public GameObject ExtraTime;
    public GameObject warningPanel;

    public CanvasGroup warningCanvas; // ⚡ Fade için kullanıyoruz

    public void ShowWarning()
    {
        StopAllCoroutines(); // eski coroutine varsa sıfırla
        warningPanel.SetActive(true);
        warningCanvas.alpha = 1f; // direkt görünür başlat
        StartCoroutine(HideWarningAfterDelay());
    }

    private IEnumerator HideWarningAfterDelay()
    {
        // 5 saniye bekle
        yield return new WaitForSeconds(5f);

        // fade-out
        float duration = 1f; // yavaşça kaybolma süresi
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            warningCanvas.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        warningPanel.SetActive(false); // tamamen kapanınca disable et
    }


    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

    }
    public void ShowWin()
    {
        Time.timeScale = 0f;  // Oyun dursun
        winPanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;  // Oyun dursun
        GameOverPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Tekrar devam etsin
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
    }

    public void OpenBonusPanel()
    {
        bonusPanel.SetActive(true);
        GameOverPanel.SetActive(false);
        
        if (gameManager.gameMode == GameManager.GameMode.Timer)
        {
            ExtraTime.SetActive(true);
        }
        else if (gameManager.gameMode == GameManager.GameMode.Moves)
        {
            ExtraMoves.SetActive(true);
        }
    }

    public void BacktoGameOver()
    {
        bonusPanel.SetActive(false);
        GameOverPanel.SetActive(true);
    }

// --- Seçenekler ---
    public void SkipLevel()
    {
        if (CoinManager.instance.SpendCoins(10))
        {
            NextLevel();
            Debug.Log("10 coin ile sonraki levele geç!");
        }
    }
    public void UseExtraTime()
    {
        if (CoinManager.instance.SpendCoins(2))
        {
            gameManager.remainingTime = 30f;
            Debug.Log("2 coin ile 30 saniye eklendi!");
            ReturnGame();
        }
        else
        {
            ShowWarning();
        }
    }

    public void UseExtraMoves()
    {
        if (CoinManager.instance.SpendCoins(2))
        {
            gameManager.movesLeft = 6;
            gameManager.UseMove();
            Debug.Log("2 coin ile 5 hamle eklendi!");
            ReturnGame();
        }
        else
        {
            ShowWarning();
        }
    }

    public void NextLevel()
    {
        Time.timeScale = 1f; // Tekrar devam etsin
        string currentName = SceneManager.GetActiveScene().name;

        // "Level" kelimesini çıkar, sadece sayı kısmını al
        string numberPart = currentName.Replace("Level", "");
        int levelNumber;
        if (int.TryParse(numberPart, out levelNumber))
        {
            int nextLevelNumber = levelNumber + 1;
            string nextSceneName = "Level" + nextLevelNumber;

            // O isimli sahne varsa yükle
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("Son level bitti: " + currentName);
                // Burada menüye dönmek istersen:
                // SceneManager.LoadScene("Menu");
            }
        }
    }

    public void ReturnGame()
    {
        bonusPanel.SetActive(false);
        Time.timeScale = 1f;
    }

}