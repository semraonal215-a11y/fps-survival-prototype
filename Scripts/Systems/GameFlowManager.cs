using UnityEngine;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private float gameOverDelay = 1.2f;

    [Header("References")]
    [SerializeField] private PlayerInputGate inputGate;
    [SerializeField] private GameOverUIController gameOverUI;

    private bool deathHandled;

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDied -= HandlePlayerDied;
    }

    private void Start()
    {
        if (inputGate == null)
            inputGate = FindFirstObjectByType<PlayerInputGate>();

        if (gameOverUI == null)
            gameOverUI = FindFirstObjectByType<GameOverUIController>();
    }

    private void HandlePlayerDied()
    {
        if (deathHandled) return;
        deathHandled = true;

       
        if (inputGate != null)
            inputGate.SetInputEnabled(false);

        
        Time.timeScale = 1f;

        
        StartCoroutine(DeathCameraTiltShort());

        
        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (gameOverUI != null)
            gameOverUI.Show();
        else
            Debug.LogError("GameFlowManager: gameOverUI NULL!");
    }

    private IEnumerator DeathCameraTiltShort()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Transform t = cam.transform;

        float dur = 1.2f;
        float timer = 0f;

        Quaternion startRot = t.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(35f, 0f, 45f);

        while (timer < dur)
        {
            timer += Time.deltaTime;
            float t01 = Mathf.Clamp01(timer / dur);
            float ease = 1f - Mathf.Pow(1f - t01, 3f);

            t.rotation = Quaternion.Slerp(startRot, endRot, ease);
            yield return null;
        }
    }
}
