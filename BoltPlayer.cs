using Axlebolt.Bolt.Avatar;
using System.Collections.Generic;

namespace Axlebolt.Bolt.Player
{
	public class BoltPlayer
	{
		public string Id { get; internal set; }

		public string Uid { get; internal set; }

		public string Name { get; internal set; }

		public BoltAvatar Avatar { get; internal set; }

		public int TimeInGame { get; internal set; }

		public long RegistrationDate { get; internal set; }

		public PlayerStatus PlayerStatus { get; internal set; }

		public Dictionary<string, Attribute> Attributes { get; internal set; }

		public OnlineStatus OnlineStatus => PlayerStatus.OnlineStatus;

		public PlayInGame PlayInGame => PlayerStatus.PlayInGame;

		public BoltPlayer()
		{
			Attributes = new Dictionary<string, Attribute>();
		}

		public BoltPlayer(string id, string uid, string name, BoltAvatar avatar)
		{
			Id = id;
			Uid = uid;
			Name = name;
			Avatar = avatar;
			Attributes = new Dictionary<string, Attribute>();
		}

		protected bool Equals(BoltPlayer other)
		{
			return string.Equals(Id, other.Id);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((BoltPlayer)obj);
		}

		public override int GetHashCode()
		{
			return (Id != null) ? Id.GetHashCode() : 0;
		}
	}
}
