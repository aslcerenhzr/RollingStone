using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform gridParent;
    public Button nextPageButton;
    public Button prevPageButton;
    public int totalLevels = 100;
    public int levelsPerPage = 20;
    private int currentPage = 0;
    private int totalPages;

    void Start()
    {
        totalPages = Mathf.CeilToInt((float)totalLevels / levelsPerPage);
        ShowPage(0);
    }

    public void ShowPage(int pageIndex)
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        currentPage = Mathf.Clamp(pageIndex, 0, totalPages - 1);
        int startLevel = currentPage * levelsPerPage + 1;
        int endLevel = Mathf.Min(startLevel + levelsPerPage - 1, totalLevels);

        for (int i = startLevel; i <= endLevel; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, gridParent);
            Button btn = newButton.GetComponent<Button>();
            TextMeshProUGUI tmpText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
                tmpText.text = i.ToString();

            int levelIndex = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => LoadLevel(levelIndex));

            // 🔒 Level kilit kontrolü
            int lastUnlockedLevel = PlayerPrefs.GetInt("LastUnlockedLevel", 1);
            if (i <= lastUnlockedLevel)
            {
                btn.interactable = true;
            }
        }
        UpdateNavigationButtons();
    }

    void UpdateNavigationButtons()
    {
        prevPageButton.gameObject.SetActive(currentPage > 0);
        nextPageButton.gameObject.SetActive(currentPage < totalPages - 1);
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
            ShowPage(currentPage + 1);
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
            ShowPage(currentPage - 1);
    }

    void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene("Level" + levelIndex);
    }
    
}