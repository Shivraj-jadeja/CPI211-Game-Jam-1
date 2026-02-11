using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour 
{
    public static bool isPaused = false;

    public GameObject PauseMenuUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (isPaused) {
                Resume();
            } else {
                Pause();
            }
        } else if (Input.GetKeyDown(KeyCode.Escape) && isPaused) {
            Resume();
        }
    }

    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        isPaused = false;
    }

    public void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Time.timeScale = 1.0f;
    }

    public void QuitGame()
    {
        Debug.Log("Qutting Game...");
        Application.Quit();
    }
}