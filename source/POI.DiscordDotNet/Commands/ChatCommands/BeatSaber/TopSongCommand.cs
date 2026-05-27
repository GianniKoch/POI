using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Services.Implementations;
using POI.Persistence.Repositories;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.ScoreSaber.HttpClient;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;

namespace POI.DiscordDotNet.Commands.ChatCommands.BeatSaber
{
	[UsedImplicitly]
	public class TopSongCommand : BaseSongCommand
	{
		public TopSongCommand(ILogger<TopSongCommand> logger, PathProvider pathProvider, ScoreSaberHttpClient scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IBeatSaverClientProvider beatSaverClientProvider)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"))
		{
		}

		[Command("topsong")]
		[Aliases("topscore", "ts")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await GenerateScoreImageAndSendInternal(ctx).ConfigureAwait(false);
		}

		protected override Task<Response6?> FetchScorePage(string playerId, int page)
		{
			return ScoreSaberApiService.PlayerController_getPlayerScores_v2Async(playerId, page, null, Sort2.Top, null, null, null, null, null, null, null, null);
		}
	}
}