using System;
using System.Collections.Generic;
using Axlebolt.Bolt.Protobuf;
using Google.Protobuf.Collections;

namespace Axlebolt.Bolt.Player
{
	public class PlayerAttributesMapper : MessageMapper<MapField<string, Axlebolt.Bolt.Protobuf.Attribute>, Dictionary<string, Attribute>>
	{
		public static readonly PlayerAttributesMapper Instance = new PlayerAttributesMapper();

		private Attribute ToOriginal(Axlebolt.Bolt.Protobuf.Attribute proto)
		{
			switch (proto.Type)
			{
			case PropertyType.String:
				return new Attribute(proto.String);
			case PropertyType.Int:
				return new Attribute(proto.Int);
			case PropertyType.Float:
				return new Attribute(proto.Float);
			case PropertyType.Boolean:
				return new Attribute(proto.Boolean);
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public override Dictionary<string, Attribute> ToOriginal(MapField<string, Axlebolt.Bolt.Protobuf.Attribute> proto)
		{
			if (proto == null)
			{
				throw new ArgumentNullException("proto");
			}
			Dictionary<string, Attribute> dictionary = new Dictionary<string, Attribute>();
			foreach (KeyValuePair<string, Axlebolt.Bolt.Protobuf.Attribute> item in proto)
			{
				dictionary.Add(item.Key, ToOriginal(item.Value));
			}
			return dictionary;
		}

		public Dictionary<string, Attribute> ToOriginalMap(MapField<string, Axlebolt.Bolt.Protobuf.Attribute> protoMap)
		{
			Dictionary<string, Attribute> dictionary = new Dictionary<string, Attribute>();
			if (protoMap == null)
			{
				return dictionary;
			}
			foreach (KeyValuePair<string, Axlebolt.Bolt.Protobuf.Attribute> item in protoMap)
			{
				dictionary[item.Key] = ToOriginal(item.Value);
			}
			return dictionary;
		}
	}
}
