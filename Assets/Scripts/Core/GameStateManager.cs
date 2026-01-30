using UnityEngine;
using UnityEngine.SceneManagement;

public enum RunState
{
    Playing,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public RunState State { get; private set; } = RunState.Playing;

    [Header("Game Over Controls")]
    public bool allowRestartKey = true;
    public KeyCode restartKey = KeyCode.R;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (State == RunState.GameOver && allowRestartKey && Input.GetKeyDown(restartKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void TriggerGameOver()
    {
        if (State == RunState.GameOver) return;
        State = RunState.GameOver;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("=== GAME OVER ===");
    }
}