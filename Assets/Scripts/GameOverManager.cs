using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class GameOverManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameUIManager gameUIManager;
    public GameObject GameOverPanel;
    public GameObject winPanel;
    public GameObject bonusPanel;
    public GameObject ExtraMoves;
    public GameObject ExtraTime;
    public GameObject ExtraLife;
    public GameObject warningPanel;
    public GameObject pausePanel;
    public CanvasGroup warningCanvas;
    public PlayerMovement player;
    

    public void ShowWarning()
    {
        StopAllCoroutines();
        warningPanel.SetActive(true);
        warningCanvas.alpha = 1f;
        StartCoroutine(HideWarningAfterDelay());
    }

    private IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            warningCanvas.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        warningPanel.SetActive(false);
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
        Time.timeScale = 0f;
        winPanel.SetActive(true);

        player.GetComponent<PlayerInput>().enabled = false;

        string currentName = SceneManager.GetActiveScene().name;
        string numberPart = currentName.Replace("Level", "");
        int levelNumber;
        int.TryParse(numberPart, out levelNumber);

        LevelProgressManager.instance.CompleteLevel(levelNumber);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        GameOverPanel.SetActive(true);

        player.GetComponent<PlayerInput>().enabled = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
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

        if (gameManager.diedByHealth)
        {
            ExtraLife.SetActive(true);
        }
        else if (gameManager.gameMode == GameManager.GameMode.Timer)
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

    public void SkipLevel()
    {
        if (LevelProgressManager.instance.SpendCoins(10))
        {
            NextLevel();
            Debug.Log("10 coin ile sonraki levele geç!");
        }
        else
        {
            ShowWarning();
        }
    }

    public void UseExtraTime()
    {
        if (LevelProgressManager.instance.SpendCoins(2))
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
        if (LevelProgressManager.instance.SpendCoins(2))
        {
            gameManager.movesLeft = 5;
            gameManager.UseMove();
            Debug.Log("2 coin ile 5 hamle eklendi!");
            ReturnGame();
        }
        else
        {
            ShowWarning();
        }
    }

    public void UseExtraLife()
    {
        if (LevelProgressManager.instance.SpendCoins(500))
        {
            gameManager.health = 1;
            gameUIManager.InitHearts(1);
            Debug.Log("5 coin ile kalp eklendi!");
            ReturnGame();
        }
        else
        {
            ShowWarning();
        }
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        string currentName = SceneManager.GetActiveScene().name;
        string numberPart = currentName.Replace("Level", "");
        int levelNumber;

        if (int.TryParse(numberPart, out levelNumber))
        {
            int nextLevelNumber = levelNumber + 1;
            string nextSceneName = "Level" + nextLevelNumber;

            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("Son level bitti: " + currentName);
            }
        }
    }

    public void ReturnGame()
    {
        bonusPanel.SetActive(false);
        Time.timeScale = 1f;
        GameOverPanel.SetActive(false);
        player.GetComponent<PlayerInput>().enabled = true;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        player.GetComponent<PlayerInput>().enabled = false;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        player.GetComponent<PlayerInput>().enabled = true;
    }
    

}