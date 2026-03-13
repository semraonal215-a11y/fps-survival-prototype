using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "GameScene";

    [Header("Start Button (Pressed Lock)")]
    public Button startButton;
    public Sprite pressedSprite;
    private bool isLoading = false;

    public void StartGame()
    {
        if (isLoading) return;
        isLoading = true;

        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();
       
        Time.timeScale = 1f;

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"Game scene not found in Build Settings or name mismatch: '{gameSceneName}'");
            isLoading = false;
            return;
        }

        if (startButton != null)
        {
            startButton.interactable = false;

            Image img = startButton.GetComponent<Image>();
            if (img != null && pressedSprite != null)
                img.sprite = pressedSprite;
        }

        StartCoroutine(LoadGameSceneAsync());
    }

    private IEnumerator LoadGameSceneAsync()
    {
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;
    }

    public void QuitGame()
    {
       
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();
        

        Application.Quit();
    }
}