﻿using Microsoft.EntityFrameworkCore;
using POI.Persistence.Domain;
using POI.Persistence.EFCore.Npgsql.Infrastructure;
using POI.Persistence.Repositories;

namespace POI.Persistence.EFCore.Npgsql.Repositories;

internal class ServerSettingsRepository : IServerSettingsRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public ServerSettingsRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}

	public async Task<ServerSettings?> FindOneById(ulong serverId, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.ServerSettings
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.ServerId == serverId, cts)
			.ConfigureAwait(false);
	}

	public async Task<List<ServerSettings>> GetRankUpFeedChannels(CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return context.ServerSettings
			.AsNoTracking()
			.Where(s => s.RankUpFeedChannelId != null)
			.ToList();
	}
}