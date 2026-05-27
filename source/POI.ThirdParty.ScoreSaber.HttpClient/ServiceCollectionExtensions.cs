using Microsoft.Extensions.DependencyInjection;

namespace POI.ThirdParty.ScoreSaber.HttpClient;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddScoreSaberV2(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddHttpClient<ScoreSaberHttpClient>();
		return serviceCollection;
	}
}