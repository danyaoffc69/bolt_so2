using Axlebolt.Bolt.Protobuf;

namespace Axlebolt.Bolt.Stats.Mappers
{
	public class BoltGameServerPlayerStatsMapper : MessageMapper<PlayerStats, BoltGameServerPlayerStats>
	{
		public static readonly BoltGameServerPlayerStatsMapper Instance = new BoltGameServerPlayerStatsMapper();

		public override BoltGameServerPlayerStats ToOriginal(PlayerStats proto)
		{
			return new BoltGameServerPlayerStats
			{
				PlayerId = proto.PlayerId,
				Stats = BoltGameServerPlayerStatMapper.Instance.ToOriginalArray(proto.Stats)
			};
		}

		public override PlayerStats ToProto(BoltGameServerPlayerStats original)
		{
			PlayerStats playerStats = new PlayerStats
			{
				PlayerId = original.PlayerId
			};
			BoltGameServerPlayerStat[] stats = original.Stats;
			foreach (BoltGameServerPlayerStat original2 in stats)
			{
				playerStats.Stats.Add(BoltGameServerPlayerStatMapper.Instance.ToProto(original2));
			}
			return playerStats;
		}
	}
}
