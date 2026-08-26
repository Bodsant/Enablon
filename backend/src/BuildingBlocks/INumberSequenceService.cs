namespace Ehsms.BuildingBlocks;

public interface INumberSequenceService
{
    Task<string> NextAsync(string recordType, Guid tenantId, CancellationToken cancellationToken = default);
}
