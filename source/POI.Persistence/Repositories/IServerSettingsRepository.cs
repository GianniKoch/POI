using POI.Persistence.Domain;

namespace POI.Persistence.Repositories;

public interface IServerSettingsRepository
{
	Task<ServerSettings?> FindOneById(ulong serverId, CancellationToken cts = default);
	Task<List<ServerSettings>> GetRankUpFeedChannels(CancellationToken cts = default);
}