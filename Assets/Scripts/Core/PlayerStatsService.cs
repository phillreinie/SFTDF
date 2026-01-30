public class PlayerStatsService
{
    public int Souls { get; private set; }

    public void AddSouls(int amount)
    {
        if (amount <= 0) return;
        Souls += amount;
    }
}