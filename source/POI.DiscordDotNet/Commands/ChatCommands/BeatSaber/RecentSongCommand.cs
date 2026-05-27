using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Services.Implementations;
using POI.Persistence.Repositories;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.ScoreSaber.HttpClient;

namespace POI.DiscordDotNet.Commands.ChatCommands.BeatSaber
{
	[UsedImplicitly]
	public class RecentSongCommand : BaseSongCommand
	{
		public RecentSongCommand(ILogger<RecentSongCommand> logger, PathProvider pathProvider, ScoreSaberHttpClient scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IBeatSaverClientProvider beatSaverClientProvider)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"))
		{
		}

		[Command("recentsong")]
		[Aliases("eris", "recentscore", "rs")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await GenerateScoreImageAndSendInternal(ctx);
		}

		protected override Task<Response6?> FetchScorePage(string playerId, int page)
		{
			return ScoreSaberApiService.PlayerController_getPlayerScores_v2Async(playerId, page, null, Sort2.Recent, null, null, null, null, null, null, null, null);
		}
	}
}