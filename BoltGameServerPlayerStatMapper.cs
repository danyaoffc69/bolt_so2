using Axlebolt.Bolt.Protobuf;

namespace Axlebolt.Bolt.Stats.Mappers
{
	public class BoltGameServerPlayerStatMapper : MessageMapper<StorePlayerStat, BoltGameServerPlayerStat>
	{
		public static readonly BoltGameServerPlayerStatMapper Instance = new BoltGameServerPlayerStatMapper();

		public override BoltGameServerPlayerStat ToOriginal(StorePlayerStat proto)
		{
			return new BoltGameServerPlayerStat
			{
				Name = proto.Name,
				FloatValue = proto.StoreFloat,
				IntValue = proto.StoreInt
			};
		}

		public override StorePlayerStat ToProto(BoltGameServerPlayerStat original)
		{
			return new StorePlayerStat
			{
				Name = original.Name,
				StoreFloat = original.FloatValue,
				StoreInt = original.IntValue
			};
		}
	}
}
