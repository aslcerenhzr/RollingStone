using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject GameOverPanel;
    public GameObject winPanel;

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

}
