using UnityEngine;

public enum GameMode
{
    Combat,
    Build
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public GameMode Mode { get; private set; } = GameMode.Combat;

    [Header("Refs")]
    public BuildController buildController;
    public PlayerWeapon playerWeapon; // your existing shooting script (or whatever it's called)

    [Header("Input")]
    public bool scrollTogglesMode = true;
    public KeyCode forceBuildKey = KeyCode.B;
    public KeyCode forceCombatKey = KeyCode.Tab;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Apply();
        BuildSelectionEvents.OnModeChanged?.Invoke(Mode);
    }

    private void Update()
    {


        if (Input.GetKeyDown(forceBuildKey))
            SetMode(GameMode.Build);

        if (Input.GetKeyDown(forceCombatKey))
            SetMode(GameMode.Combat);
    }

    public void SetMode(GameMode mode)
    {
        if (Mode == mode) return;

        Mode = mode;
        Apply();
        
        BuildSelectionEvents.OnModeChanged?.Invoke(Mode);
    }

    private void Apply()
    {
        if (playerWeapon != null)
            playerWeapon.enabled = (Mode == GameMode.Combat);

        if (buildController != null)
            buildController.SetBuildMode(Mode == GameMode.Build);
    }
}