using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Axlebolt.Bolt.Avatar;
using Axlebolt.Bolt.Protobuf;
using Google.Protobuf.Collections;

namespace Axlebolt.Bolt.Player
{
	public class PlayerMapper : MessageMapper<Axlebolt.Bolt.Protobuf.Player, BoltPlayer>
	{
		public static readonly PlayerMapper Instance = new PlayerMapper();

		private static readonly PlayerAttributesMapper PlayerAttributesMapper = new PlayerAttributesMapper();

		public T ToOriginal<T>(Axlebolt.Bolt.Protobuf.Player proto, T boltPlayer) where T : BoltPlayer
		{
			if (proto == null)
			{
				throw new ArgumentNullException("proto");
			}
			if (boltPlayer == null)
			{
				throw new ArgumentNullException("boltPlayer");
			}
			boltPlayer.Id = proto.Id;
			boltPlayer.Uid = proto.Uid;
			boltPlayer.Name = proto.Name;
			boltPlayer.Avatar = BoltAvatar.Create(proto.AvatarId, null);
			//boltPlayer.AvatarId = proto.AvatarId;
			boltPlayer.TimeInGame = proto.TimeInGame;
			boltPlayer.RegistrationDate = proto.RegistrationDate;
			if (proto.PlayerStatus != null)
			{
				boltPlayer.PlayerStatus = PlayerStatusMapper.Instance.ToOriginal(proto.PlayerStatus);
			}
			if (proto.Attributes != null)
			{
				boltPlayer.Attributes = PlayerAttributesMapper.ToOriginalMap(proto.Attributes.Map);
			}
			return boltPlayer;
		}

		public override BoltPlayer ToOriginal(Axlebolt.Bolt.Protobuf.Player proto)
		{
			return ToOriginal(proto, new BoltPlayer());
		}

		public ConcurrentDictionary<string, BoltPlayer> ToOriginalMap(MapField<string, Axlebolt.Bolt.Protobuf.Player> protoMap)
		{
			ConcurrentDictionary<string, BoltPlayer> concurrentDictionary = new ConcurrentDictionary<string, BoltPlayer>();
			if (protoMap == null)
			{
				return concurrentDictionary;
			}
			foreach (KeyValuePair<string, Axlebolt.Bolt.Protobuf.Player> item in protoMap)
			{
				concurrentDictionary[item.Key] = ToOriginal(item.Value);
			}
			return concurrentDictionary;
		}
	}
}
