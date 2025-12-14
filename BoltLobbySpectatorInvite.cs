using System;
using Axlebolt.Bolt.Friends;

namespace Axlebolt.Bolt.Matchmaking
{
	public class BoltLobbySpectatorInvite
	{
		public string LobbyId { get; internal set; }

		public BoltFriend Friend { get; internal set; }

		public long Timestamp { get; internal set; }

		public BoltLobbySpectatorInvite(string lobbyId, BoltFriend friend, long timestamp)
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
			Timestamp = timestamp;
		}
	}
}
