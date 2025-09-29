using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI collectibleText;
    public TextMeshProUGUI healthText;

    public void SetTimerUI()
    {
        timerText.gameObject.SetActive(true);
    }

    public void SetMovesUI()
    {
        movesText.gameObject.SetActive(true);  
    }

    public void UpdateTimerUI(float rTime)
    {
        int minutes = Mathf.FloorToInt(rTime / 60);
        int seconds = Mathf.FloorToInt(rTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateMovesUI(int movesLeft)
    {
        movesText.text = "Moves: " + movesLeft.ToString();
    }

    public void UpdateHealthUI(int health)
    {
        healthText.text = health.ToString();

    }

    public void UpdateCollectibleUI(int collectedCount, int totalCollectibles)
    {
        collectibleText.text = collectedCount.ToString() + "/" + totalCollectibles.ToString();

    }
}
