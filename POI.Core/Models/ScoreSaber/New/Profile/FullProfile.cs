using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.New.Profile
{
	public class FullProfile : BasicProfile
	{
		[JsonPropertyName("badges")]
		public List<Badge> Badges { get; }

		[JsonPropertyName("scoreStats")]
		public ScoreStats ScoreStats { get; }

		[JsonConstructor]
		public FullProfile(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw, string role, uint permissions, uint inactive,
			uint banned, List<Badge> badges, ScoreStats scoreStats)
			: base(id, name, profilePicture, country, rank, countryRank, pp, historyRaw, role, permissions, inactive, banned)
		{
			Badges = badges;
			ScoreStats = scoreStats;
		}
	}
}