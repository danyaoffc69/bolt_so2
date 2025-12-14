using System;
using Axlebolt.Bolt.Friends;

namespace Axlebolt.Bolt.Matchmaking
{
	public class BoltLobbyInvite
	{
		public string LobbyId { get; internal set; }

		public BoltFriend Friend { get; internal set; }

		public LobbyPlayerType PlayerType { get; internal set; }

		public long Timestamp { get; internal set; }

		public BoltLobbyInvite(string lobbyId, BoltFriend friend, LobbyPlayerType playerType, long timestamp)
		{
			if (lobbyId == null)
			{
				throw new ArgumentNullException("lobbyId");
			}
			if (friend == null)
			{
				throw new ArgumentNullException("friend");
			}
			LobbyId = lobbyId;
			Friend = friend;
			PlayerType = playerType;
			Timestamp = timestamp;
		}
	}
}
