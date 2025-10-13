using UnityEngine;
using UnityEngine.SceneManagement;
public class Start : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Menu");
    }

    public void temizle()
    {
        LevelProgressManager.instance.ResetAllProgress();
    }

}