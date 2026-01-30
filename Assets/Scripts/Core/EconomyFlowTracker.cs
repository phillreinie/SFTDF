using System.Collections.Generic;

public class EconomyFlowTracker
{
    private readonly Dictionary<string, float> _netPerSecond = new();

    public IReadOnlyDictionary<string, float> Net => _netPerSecond;

    public float GetNetPerSecond(string resourceId)
        => (resourceId != null && _netPerSecond.TryGetValue(resourceId, out var v)) ? v : 0f;

    public void RebuildFromBuildings(IReadOnlyList<BuildingRuntime> buildings)
    {
        _netPerSecond.Clear();
        if (buildings == null) return;

        for (int i = 0; i < buildings.Count; i++)
        {
            var br = buildings[i];
            if (br == null || br.data == null) continue;

            // Only count producers that are actually running
            if (br.data is not ProducerData pd) continue;
            if (br.productionState != ProductionState.Running) continue;

            float cps = pd.cyclesPerSecond;
            if (cps <= 0f) continue;

            // Inputs: negative
            if (pd.recipe != null && pd.recipe.inputs != null)
            {
                for (int n = 0; n < pd.recipe.inputs.Count; n++)
                {
                    var input = pd.recipe.inputs[n];
                    if (string.IsNullOrWhiteSpace(input.resourceId) || input.amount <= 0) continue;
                    Add(input.resourceId, -input.amount * cps);
                }
            }

            // Outputs: positive
            if (pd.recipe != null && pd.recipe.outputs != null)
            {
                for (int n = 0; n < pd.recipe.outputs.Count; n++)
                {
                    var output = pd.recipe.outputs[n];
                    if (string.IsNullOrWhiteSpace(output.resourceId) || output.amount <= 0) continue;
                    Add(output.resourceId, output.amount * cps);
                }
            }
        }
    }

    private void Add(string resourceId, float deltaPerSecond)
    {
        if (string.IsNullOrWhiteSpace(resourceId)) return;
        _netPerSecond[resourceId] = GetNetPerSecond(resourceId) + deltaPerSecond;
    }
}
