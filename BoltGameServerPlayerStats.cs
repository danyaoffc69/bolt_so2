using System.Collections.Generic;
using System.Linq;
using Axlebolt.Bolt.Stats.Exception;

namespace Axlebolt.Bolt.Stats
{
	public class BoltGameServerPlayerStats
	{
		public string PlayerId { get; set; }

		public BoltGameServerPlayerStat[] Stats { get; set; }

		public BoltGameServerPlayerStat GetStat(string apiName)
		{
			Dictionary<string, BoltGameServerPlayerStat> dictionary = Stats.ToDictionary((BoltGameServerPlayerStat stat) => stat.Name, (BoltGameServerPlayerStat stat) => stat);
			if (dictionary.ContainsKey(apiName))
			{
				return dictionary[apiName];
			}
			throw new BoltStatsException($"stat with apiName {apiName} not found");
		}
	}
}
