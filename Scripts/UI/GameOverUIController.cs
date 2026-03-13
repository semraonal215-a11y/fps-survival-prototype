using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUIController : MonoBehaviour
{
    [Header("Root Panel")]
    public GameObject gameOverRoot;

    [Header("Scene Names")]
    public string startSceneName = "StartScene";

    [Header("Pause")]
    public bool pauseGameOnShow = true;  
    public bool unlockCursorOnShow = true; 

    private void Awake()
    {
        if (gameOverRoot != null)
            gameOverRoot.SetActive(false);
    }

    public void Show()
    {
        if (pauseGameOnShow)
            Time.timeScale = 0f; 

        if (gameOverRoot != null)
            gameOverRoot.SetActive(true);

        if (unlockCursorOnShow)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnRestartButton()
    {
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();
        

        Debug.Log("RESTART BUTTON CLICKED -> loading: " + startSceneName);

       
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(startSceneName);
    }
}