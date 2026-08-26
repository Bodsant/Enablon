namespace Ehsms.BuildingBlocks;

public interface IClock
{
    DateTime UtcNow { get; }
}

public class UtcClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
