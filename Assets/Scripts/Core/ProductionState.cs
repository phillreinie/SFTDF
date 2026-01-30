public enum ProductionState
{
    Inactive,   // Not powered / not linked / disabled by gating
    Running,    // Producing (or able to produce)
    Starved,    // Missing required input resources
    Blocked     // Reserved for later (output blocked, paused, etc.)
}