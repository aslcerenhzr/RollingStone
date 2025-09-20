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
        // Önce eski butonları temizle
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

            // TextMeshPro kullanıyorsan
            TextMeshProUGUI tmpText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
                tmpText.text = i.ToString();

            int levelIndex = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => LoadLevel(levelIndex));
        }

        UpdateNavigationButtons();
    }

    void UpdateNavigationButtons()
    {
        // İlk sayfadaysa Previous gizle
        prevPageButton.gameObject.SetActive(currentPage > 0);

        // Son sayfadaysa Next gizle
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
        Debug.Log("Level " + levelIndex + " yükleniyor...");
        SceneManager.LoadScene("Level" + levelIndex);
    }
}
