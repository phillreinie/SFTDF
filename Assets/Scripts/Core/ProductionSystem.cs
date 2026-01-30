using UnityEngine;

public class ProductionSystem : MonoBehaviour
{
    [Tooltip("How often production updates. 0.25 is smooth and cheap.")]
    public float tickInterval = 0.25f;

    private float _accum;

    private void Update()
    {
        if (GameServices.Buildings == null) return;

        _accum += Time.deltaTime;
        if (_accum < tickInterval) return;

        float dt = _accum;
        _accum = 0f;

        // Reset "current" net rates each tick
        GameServices.Rates?.Clear();

        TickAll(dt);
    }

    private void TickAll(float dt)
    {
        var all = GameServices.Buildings.All;
        for (int i = 0; i < all.Count; i++)
        {
            var b = all[i];
            if (b == null || b.data == null) continue;

            if (b.data.category == BuildingCategory.Producer && b.data is ProducerData pd)
                TickProducer(b, pd, dt);
        }
    }

    private void TickProducer(BuildingRuntime b, ProducerData pd, float dt)
    {
        // Storage gating
        if (pd.requiresStorageLink && !b.isStorageLinked)
        {
            b.productionState = ProductionState.Inactive;
            return;
        }

        // Power gating
        if (b.data.powerDraw > 0 && !b.isPowered)
        {
            b.productionState = ProductionState.Inactive;
            return;
        }

        // Input gating
        if (!HasAllInputs(pd))
        {
            b.productionState = ProductionState.Starved;
            return;
        }

        // If we reach here, building is in a valid producing condition
        b.productionState = ProductionState.Running;

        // Normal milestone-3 cycle ticking
        b.productionCycleAccum += pd.cyclesPerSecond * dt;

        int requestedCycles = Mathf.FloorToInt(b.productionCycleAccum);
        if (requestedCycles <= 0) return;

        int executed = 0;
        for (int i = 0; i < requestedCycles; i++)
        {
            if (!TryExecuteRecipeCycle(pd)) break;
            executed++;
        }

        if (executed <= 0) return;

        b.productionCycleAccum -= executed;

        float cyclesPerSecondApplied = executed / dt;
        ApplyRates(pd, cyclesPerSecondApplied);
    }


    private bool TryExecuteRecipeCycle(ProducerData pd)
    {
        var inv = GameServices.Inventory;

        var inputs = pd.recipe != null ? pd.recipe.inputs : null;
        var outputs = pd.recipe != null ? pd.recipe.outputs : null;

        // Check inputs
        if (inputs != null)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var ra = inputs[i];
                if (inv.Get(ra.resourceId) < ra.amount)
                    return false;
            }
        }

        // Consume inputs
        if (inputs != null)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var ra = inputs[i];
                if (!inv.TryConsume(ra.resourceId, ra.amount))
                    return false;
            }
        }

        // Produce outputs
        if (outputs != null)
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                var ra = outputs[i];
                inv.Add(ra.resourceId, ra.amount);
            }
        }

        return true;
    }

    private void ApplyRates(ProducerData pd, float cyclesPerSecondApplied)
    {
        if (GameServices.Rates == null) return;

        var inputs = pd.recipe != null ? pd.recipe.inputs : null;
        var outputs = pd.recipe != null ? pd.recipe.outputs : null;

        if (inputs != null)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var ra = inputs[i];
                GameServices.Rates.AddDelta(ra.resourceId, -ra.amount * cyclesPerSecondApplied);
            }
        }

        if (outputs != null)
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                var ra = outputs[i];
                GameServices.Rates.AddDelta(ra.resourceId, ra.amount * cyclesPerSecondApplied);
            }
        }
    }
    
    private bool HasAllInputs(ProducerData pd)
    {
        var inv = GameServices.Inventory;
        var inputs = pd.recipe != null ? pd.recipe.inputs : null;

        if (inputs == null || inputs.Count == 0)
            return true;

        for (int i = 0; i < inputs.Count; i++)
        {
            var ra = inputs[i];
            if (inv.Get(ra.resourceId) < ra.amount)
                return false;
        }

        return true;
    }
}
