using System;
using UnityEngine;

public static class BuildSelectionEvents
{
    public static Action<BuildingData> OnSelectedChanged;
    public static Action<GameMode> OnModeChanged;
}