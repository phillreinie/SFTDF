using UnityEngine;

public class GameOverFreezeSystem : MonoBehaviour
{
    [Header("Disable these on GameOver")]
    public MonoBehaviour[] toDisable;

    private RunState _lastState;

    private void Update()
    {
        if (GameStateManager.Instance == null) return;

        var state = GameStateManager.Instance.State;
        if (state == _lastState) return;
        _lastState = state;

        if (state == RunState.GameOver)
        {
            for (int i = 0; i < toDisable.Length; i++)
            {
                if (toDisable[i] != null)
                    toDisable[i].enabled = false;
            }
        }
    }
}