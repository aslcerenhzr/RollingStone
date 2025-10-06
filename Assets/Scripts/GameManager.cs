using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GameMode { Default, Timer, Moves }
    public GameMode gameMode = GameMode.Default;
    public float remainingTime;
    public int movesLeft;
    public int health = 3;

    private GameObject[] collectibles;
    private int totalCollectibles;
    private int collectedCount = 0;
    private bool isGameOver = false;

    public GameOverManager gameOverManager;
    public CoinManager coinManager;
    public GameUIManager uIManager;

    void Start()
    {
        Time.timeScale = 1f;

        if (coinManager == null)
        {
            coinManager = FindObjectOfType<CoinManager>();
        }

        if (uIManager == null)
        {
            uIManager = FindObjectOfType<GameUIManager>();
        }

        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
        }

        collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        totalCollectibles = collectibles.Length;

        // ---- Set Game UIs ----
        uIManager.UpdateHealthUI(health);
        uIManager.InitHearts(health);
        uIManager.UpdateCollectibleUI(collectedCount, totalCollectibles);

        if (gameMode == GameMode.Timer)
        {
            uIManager.SetTimerUI();
        }
        else if (gameMode == GameMode.Moves)
        {
            uIManager.SetMovesUI();
            uIManager.UpdateMovesUI(movesLeft);
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

            uIManager.UpdateTimerUI(remainingTime);
        }
    }

    // ---- Moves ----
    public void UseMove()
    {
        if (gameMode != GameMode.Moves) return;

        movesLeft--;
        uIManager.UpdateMovesUI(movesLeft);
    }

    public void NoMovesLeft()
    {
        if (!isGameOver && movesLeft <= 0)
        {
            isGameOver = true;   // artık tekrar çalışmayacak
            gameOverManager.ShowGameOver();
            Time.timeScale = 0f;
        }
    }

    // ---- Collectible ----
    public void UpdateCollectible()
    {
        collectedCount++;
        uIManager.UpdateCollectibleUI(collectedCount, totalCollectibles);

        if (collectedCount >= totalCollectibles)
        {
            gameOverManager.ShowWin();
            coinManager.AddCoin(totalCollectibles);

        }
    }

    // ---- Health ----
    public void UpdateHealth()
    {
        health--;
        uIManager.UpdateHealthUI(health);

        if (health <= 0)
        {
            StartCoroutine(GameOverDelay());
        }
    }
    
    private IEnumerator GameOverDelay()
    {
        // 🔹 Animasyonun oynayıp bitmesi için 0.5 saniye bekle
        yield return new WaitForSeconds(0.5f);

        gameOverManager.ShowGameOver();
        Time.timeScale = 0f;
    }
}