// CHANGED: V2-0
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

        TickAll(dt);
    }

    private void TickAll(float dt)
    {
        var all = GameServices.Buildings.All;
        for (int i = 0; i < all.Count; i++)
        {
            var b = all[i];
            if (b == null || b.data == null) continue;

            // V2-0: tick ANY ProducerData, regardless of category (Power buildings can now produce too).
            if (b.data is ProducerData pd)
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

        // Power gating (still a gate for now; later power becomes resource consumption)
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

        // Output capacity gating (1-cycle feasibility)
        if (!HasOutputSpaceForOneCycle(pd))
        {
            b.productionState = ProductionState.Blocked;
            return;
        }

        // Valid producing condition
        b.productionState = ProductionState.Running;

        b.productionCycleAccum += pd.cyclesPerSecond * dt;

        int requestedCycles = Mathf.FloorToInt(b.productionCycleAccum);
        if (requestedCycles <= 0) return;

        int executed = 0;
        bool blockedByCap = false;

        for (int i = 0; i < requestedCycles; i++)
        {
            var result = TryExecuteRecipeCycle(pd);
            if (result == CycleResult.Success)
            {
                executed++;
                continue;
            }

            if (result == CycleResult.Blocked)
                blockedByCap = true;

            break;
        }

        if (executed <= 0)
        {
            // If we couldn't execute at all and it's due to cap => Blocked
            if (blockedByCap || !HasOutputSpaceForOneCycle(pd))
                b.productionState = ProductionState.Blocked;

            return;
        }

        b.productionCycleAccum -= executed;

        float cyclesPerSecondApplied = executed / dt;
        ApplyRates(pd, cyclesPerSecondApplied);

        // If we executed some cycles but then hit cap, we can optionally show Blocked.
        // Keeping it as Running is also acceptable. For clarity, we flip to Blocked if now full.
        if (!HasOutputSpaceForOneCycle(pd))
            b.productionState = ProductionState.Blocked;
    }

    private enum CycleResult { Success, Starved, Blocked }

    private CycleResult TryExecuteRecipeCycle(ProducerData pd)
    {
        var inv = GameServices.Inventory;
        if (inv == null) return CycleResult.Starved;

        var inputs = pd.recipe != null ? pd.recipe.inputs : null;
        var outputs = pd.recipe != null ? pd.recipe.outputs : null;

        // Check inputs
        if (inputs != null)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var ra = inputs[i];
                if (inv.Get(ra.resourceId) < ra.amount)
                    return CycleResult.Starved;
            }
        }

        // Check output space BEFORE consuming inputs (important!)
        if (outputs != null)
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                var ra = outputs[i];
                if (!inv.CanAdd(ra.resourceId, ra.amount))
                    return CycleResult.Blocked;
            }
        }

        // Consume inputs
        if (inputs != null)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var ra = inputs[i];
                if (!inv.TryConsume(ra.resourceId, ra.amount))
                    return CycleResult.Starved;
            }
        }

        // Produce outputs (Inventory clamps, but we already ensured space)
        if (outputs != null)
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                var ra = outputs[i];
                inv.Add(ra.resourceId, ra.amount);
            }
        }

        return CycleResult.Success;
    }

    private void ApplyRates(ProducerData pd, float cyclesPerSecondApplied)
    {

        var inputs = pd.recipe != null ? pd.recipe.inputs : null;
        var outputs = pd.recipe != null ? pd.recipe.outputs : null;

        if (inputs != null)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var ra = inputs[i];
            }
        }

        if (outputs != null)
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                var ra = outputs[i];
            }
        }
    }

    private bool HasAllInputs(ProducerData pd)
    {
        var inv = GameServices.Inventory;
        if (inv == null) return false;

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

    private bool HasOutputSpaceForOneCycle(ProducerData pd)
    {
        var inv = GameServices.Inventory;
        if (inv == null) return false;

        var outputs = pd.recipe != null ? pd.recipe.outputs : null;
        if (outputs == null || outputs.Count == 0) return true;

        for (int i = 0; i < outputs.Count; i++)
        {
            var ra = outputs[i];
            if (!inv.CanAdd(ra.resourceId, ra.amount))
                return false;
        }

        return true;
    }
}
