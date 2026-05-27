using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace POI.ThirdParty.ScoreSaber.HttpClient;

public partial class ScoreSaberHttpClient
{
	private const int PAGE_SIZE = 100;
	private readonly string[] _countryDefinition = ["BE"];

	private readonly AsyncRetryPolicy _scoreSaberImageRetryPolicy;
	private readonly ILogger<ScoreSaberHttpClient> _logger;

	[ActivatorUtilitiesConstructor]
	public ScoreSaberHttpClient(System.Net.Http.HttpClient httpClient, ILogger<ScoreSaberHttpClient> logger) : this("https://scoresaber.com/", httpClient)
	{
		_logger = logger;
		_scoreSaberImageRetryPolicy = Policy
			.Handle<HttpRequestException>((exception => exception.StatusCode != HttpStatusCode.NotFound))
			.Or<TaskCanceledException>()
			.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(10));
	}

	public async Task<byte[]?> FetchImageFromCdn(string url, CancellationToken cancellationToken = default)
	{
		try
		{
			return await _scoreSaberImageRetryPolicy.ExecuteAsync(ct => _httpClient.GetByteArrayAsync(url, ct), cancellationToken);
		}
		catch (Exception e)
		{
			_logger.LogError("{Exception}", e.ToString());
			return null;
		}
	}

	public async Task<Response2?> GetBelgianPlayers(int page, CancellationToken cancellationToken = default)
	{
		try
		{
			return await PlayerController_getPlayers_v2Async(page, PAGE_SIZE, string.Join(",", _countryDefinition), null, null, "false", Sort.CountryRank,
				SortDirection.Asc, null, cancellationToken);
		}
		catch(Exception e)
		{
			_logger.LogError("{Exception}", e.ToString());
			return null;
		}
	}
}