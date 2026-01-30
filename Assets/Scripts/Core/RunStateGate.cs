public static class RunStateGate
{
    public static bool IsPlaying()
        => GameStateManager.Instance == null || GameStateManager.Instance.State == RunState.Playing;
}