using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class GameUIManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI collectibleText;
    public TextMeshProUGUI levelText;

    [Header("Health UI")]
    public Transform heartPanel;
    public GameObject heartPrefab;     
    private List<Animator> heartAnimators = new List<Animator>();

    void Start()
    {
        UpdateLevelUI();
    }

    public void UpdateLevelUI()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        Match m = Regex.Match(sceneName, @"\d+");
        int parsedLevel = 0;

        if (m.Success && int.TryParse(m.Value, out parsedLevel))
        {
            levelText.text = parsedLevel.ToString("D3");
        }
        else
        {
            levelText.text = "---";
        }
    }   

    public void InitHearts(int health)
    {
        foreach (Transform child in heartPanel)
        {
            Destroy(child.gameObject);
        }
        heartAnimators.Clear();

        for (int i = 0; i < health; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartPanel.transform);
            Animator heartAnimator = newHeart.GetComponent<Animator>();
            heartAnimators.Add(heartAnimator);
        }
    }

    public void UpdateHealthUI(int currentHealth)
    {
        if (currentHealth < heartAnimators.Count && currentHealth >= 0)
        {
            Animator heartToLose = heartAnimators[currentHealth];
            heartToLose.Play("HeartLose", 0, 0f);
        }
    }

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
        movesText.text = movesLeft.ToString();
    }

    public void UpdateCollectibleUI(int collectedCount, int totalCollectibles)
    {
        collectibleText.text = collectedCount.ToString() + "/" + totalCollectibles.ToString();
    }
}