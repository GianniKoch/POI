using System.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

	static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
	{
		settings.Converters.Add(new IdConverter());
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

	private class IdConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			if (objectType.Namespace != "POI.ThirdParty.ScoreSaber.HttpClient")
			{
				return false;
			}

			var name = objectType.Name;
			return Regex.IsMatch(name, @"^Id\d+$") ||
			       name.Equals("LeftAverageCut", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("RightAverageCut", StringComparison.OrdinalIgnoreCase);
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			var instance = Activator.CreateInstance(objectType);
			if (reader.TokenType is JsonToken.Integer or JsonToken.String or JsonToken.Float)
			{
				var value = reader.Value;
				var additionalPropertiesProperty = objectType.GetProperty("AdditionalProperties");
				if (additionalPropertiesProperty != null)
				{
					var additionalProperties = (IDictionary<string, object>) additionalPropertiesProperty.GetValue(instance)!;
					additionalProperties["Value"] = value!;
				}

				return instance;
			}

			if (reader.TokenType == JsonToken.StartObject)
			{
				serializer.Populate(reader, instance!);
			}

			return instance;
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			var additionalPropertiesProperty = value.GetType().GetProperty("AdditionalProperties");
			if (additionalPropertiesProperty != null)
			{
				var additionalProperties = (IDictionary<string, object>) additionalPropertiesProperty.GetValue(value)!;
				if (additionalProperties.Count == 1 && additionalProperties.ContainsKey("Value"))
				{
					writer.WriteValue(additionalProperties["Value"]);
					return;
				}
			}

			serializer.Serialize(writer, value);
		}
	}
}