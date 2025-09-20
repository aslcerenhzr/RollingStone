using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameMode { Timer, Moves }
    public GameMode gameMode = GameMode.Timer;
    public GameOverManager gameOverManager;

    public int health = 3;
    private GameObject[] collectibles;
    private int totalCollectibles;
    private int collectedCount = 0;

    public TextMeshProUGUI timerText;
    public float remainingTime;

    public int movesLeft;
    public TextMeshProUGUI movesText;

    public TextMeshProUGUI collectibleText;
    public TextMeshProUGUI healthText;

    void Start()
    {
        Time.timeScale = 1f;

        collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        totalCollectibles = collectibles.Length;

        healthText.text = health.ToString();
        collectibleText.text = "0/" + totalCollectibles.ToString();

        if (gameMode == GameMode.Timer && timerText != null)
        {
            if (timerText != null) timerText.gameObject.SetActive(true);
            UpdateTimerUI();
        }
        else if (gameMode == GameMode.Moves && movesText != null)
        {
            if (movesText != null) movesText.gameObject.SetActive(true);
            UpdateMovesUI();
        }

    }

    void Update() 
    {
       if (gameMode == GameMode.Timer)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                gameOverManager.ShowGameOver();
                Time.timeScale = 0f;
            }

            UpdateTimerUI();
        }
    }

     // ---- UI Update helpers ----
    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds); 
    }

    void UpdateMovesUI()
    {
        movesText.text = "Moves: " + movesLeft.ToString();
    }

    // ---- Moves ----
    public void UseMove()
    {
        if (gameMode != GameMode.Moves) return;

        movesLeft--;
        UpdateMovesUI();
    }

    public void NoMovesLeft()
    {
        if (movesLeft <= 0)
        {
            gameOverManager.ShowGameOver();
            Time.timeScale = 0f;
        }
    }

    // ---- Collectible ----
    public void UpdateCollectible()
    {
        collectedCount++;
        collectibleText.text = collectedCount.ToString() + "/" + totalCollectibles.ToString();

        if (collectedCount >= totalCollectibles)
        {
            gameOverManager.ShowWin();
        }
    }

    // ---- Health ----
    public void UpdateHealth()
    {
        health--;
        healthText.text = health.ToString();

        if (health <= 0)
        {
            gameOverManager.ShowGameOver();
            Time.timeScale = 0f;
        }
    }
}