using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Axlebolt.Bolt.Friends;
using Axlebolt.Bolt.Player;

namespace Axlebolt.Bolt.Matchmaking
{
	public class BoltLobby
	{
		public enum LobbyType
		{
			Private = 0,
			FriendsOnly = 1,
			Public = 2,
			Invisible = 3
		}

		public string Id { get; internal set; }

		public string LobbyOwnerId
		{
			get
			{
				return LobbyOwner.Id;
			}
			internal set
			{
				LobbyOwner = LobbyMembers.First((BoltFriend bf) => bf.Id == value);
			}
		}

		public BoltFriend LobbyOwner { get; private set; }

		public string Name { get; internal set; }

		public LobbyType Type { get; internal set; }

		public bool Joinable { get; internal set; }

		public int MaxMembers { get; internal set; }

		public int MaxSpectators { get; internal set; }

		public IDictionary<string, string> Data { get; internal set; }

		public BoltFriend[] LobbyMembers { get; private set; }

		public BoltFriend[] LobbySpectators { get; private set; }

		public BoltFriend[] LobbyMemberInvites { get; private set; }

		public BoltFriend[] LobbySpectatorInvites { get; private set; }

		public BoltGameServer GameServer { get; internal set; }

		public BoltPhotonGame PhotonGame { get; internal set; }

		public BoltLobby(string id, string lobbyOwnerId, string name, LobbyType type, bool joinable, int maxMembers, int maxSpectators, ConcurrentDictionary<string, string> data, List<BoltFriend> lobbyMembers, List<BoltFriend> lobbySpectators, List<BoltFriend> lobbyMemberInvites, List<BoltFriend> lobbySpectatorInvites, BoltGameServer gameServer, BoltPhotonGame photonGame)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (lobbyMembers == null)
			{
				throw new ArgumentNullException("lobbyMembers");
			}
			if (lobbySpectators == null)
			{
				throw new ArgumentNullException("lobbySpectators");
			}
			if (lobbyMemberInvites == null)
			{
				throw new ArgumentNullException("lobbyMemberInvites");
			}
			if (lobbySpectatorInvites == null)
			{
				throw new ArgumentNullException("lobbySpectatorInvites");
			}
			Id = id;
			Name = name;
			Type = type;
			Joinable = joinable;
			MaxMembers = maxMembers;
			MaxSpectators = maxSpectators;
			Data = data;
			LobbyMembers = lobbyMembers.ToArray();
			LobbySpectators = lobbySpectators.ToArray();
			LobbyMemberInvites = lobbyMemberInvites.ToArray();
			LobbySpectatorInvites = lobbySpectatorInvites.ToArray();
			LobbyOwnerId = lobbyOwnerId;
			GameServer = gameServer;
			PhotonGame = photonGame;
		}

		public bool IsLobbyMember(BoltFriend friend)
		{
			return LobbyMembers.Any((BoltFriend member) => member.Id == friend.Id);
		}

		public bool IsLobbyMember(string memberId)
		{
			return LobbyMembers.Any((BoltFriend member) => member.Id == memberId);
		}

		public bool IsLobbySpectator(BoltFriend friend)
		{
			return LobbySpectators.Any((BoltFriend member) => member.Id == friend.Id);
		}

		public bool IsLobbySpectator(string spectatorId)
		{
			return LobbySpectators.Any((BoltFriend spectator) => spectator.Id == spectatorId);
		}

		public bool IsMemberInvited(BoltFriend friend)
		{
			return LobbyMemberInvites.Any((BoltFriend invite) => invite.Id == friend.Id);
		}

		public bool IsMemberInvited(string id)
		{
			return LobbyMemberInvites.Any((BoltFriend invite) => invite.Id == id);
		}

		public bool IsSpectatorInvited(BoltFriend friend)
		{
			return LobbySpectatorInvites.Any((BoltFriend invite) => invite.Id == friend.Id);
		}

		public bool IsSpectatorInvited(string id)
		{
			return LobbySpectatorInvites.Any((BoltFriend invite) => invite.Id == id);
		}

		public BoltFriend GetLobbyAny(string id)
		{
			if (IsLobbyMember(id))
			{
				return LobbyMembers.First((BoltFriend member) => member.Id == id);
			}
			if (IsLobbySpectator(id))
			{
				return LobbySpectators.First((BoltFriend member) => member.Id == id);
			}
			return null;
		}

		public BoltFriend GetLobbyMember(string id)
		{
			return LobbyMembers.First((BoltFriend member) => member.Id == id);
		}

		public BoltFriend GetLobbySpectator(string id)
		{
			return LobbySpectators.First((BoltFriend spectator) => spectator.Id == id);
		}

		public BoltFriend GetLobbyAnyInvite(string id)
		{
			if (IsMemberInvited(id))
			{
				return LobbyMemberInvites.First((BoltFriend invite) => invite.Id == id);
			}
			if (IsSpectatorInvited(id))
			{
				return LobbySpectatorInvites.First((BoltFriend invite) => invite.Id == id);
			}
			return null;
		}

		public BoltFriend GetLobbyMemberInvite(string id)
		{
			return LobbyMemberInvites.First((BoltFriend invite) => invite.Id == id);
		}

		public BoltFriend GetLobbySpectatorInvite(string id)
		{
			return LobbySpectatorInvites.First((BoltFriend invite) => invite.Id == id);
		}

		internal void AddLobbyMember(BoltFriend member)
		{
			if (!IsLobbyMember(member))
			{
				LobbyMembers = new List<BoltFriend>(LobbyMembers) { member }.ToArray();
			}
		}

		internal void AddLobbySpectator(BoltFriend spectator)
		{
			if (!IsLobbySpectator(spectator))
			{
				LobbySpectators = new List<BoltFriend>(LobbySpectators) { spectator }.ToArray();
			}
		}

		internal void RemoveLobbyAny(BoltFriend member)
		{
			RemoveLobbyMember(member);
			RemoveLobbySpectator(member);
		}

		internal void RemoveLobbyMember(BoltFriend member)
		{
			if (IsLobbyMember(member))
			{
				List<BoltFriend> list = new List<BoltFriend>(LobbyMembers);
				list.Remove(member);
				LobbyMembers = list.ToArray();
			}
		}

		internal void RemoveLobbySpectator(BoltFriend spectator)
		{
			if (IsLobbySpectator(spectator))
			{
				List<BoltFriend> list = new List<BoltFriend>(LobbySpectators);
				list.Remove(spectator);
				LobbySpectators = list.ToArray();
			}
		}

		internal void AddLobbyMemberInvite(BoltFriend member)
		{
			if (!IsMemberInvited(member))
			{
				LobbyMemberInvites = new List<BoltFriend>(LobbyMemberInvites) { member }.ToArray();
			}
		}

		internal void AddLobbySpectatorInvite(BoltFriend member)
		{
			if (!IsSpectatorInvited(member))
			{
				LobbySpectatorInvites = new List<BoltFriend>(LobbySpectatorInvites) { member }.ToArray();
			}
		}

		internal void RemoveLobbyAnyInvite(BoltFriend member)
		{
			RemoveLobbyMemberInvite(member);
			RemoveLobbySpectatorInvite(member);
		}

		internal void RemoveLobbyMemberInvite(BoltFriend member)
		{
			if (IsMemberInvited(member))
			{
				List<BoltFriend> list = new List<BoltFriend>(LobbyMemberInvites);
				list.Remove(member);
				LobbyMemberInvites = list.ToArray();
			}
		}

		internal void RemoveLobbySpectatorInvite(BoltFriend member)
		{
			if (IsSpectatorInvited(member))
			{
				List<BoltFriend> list = new List<BoltFriend>(LobbySpectatorInvites);
				list.Remove(member);
				LobbySpectatorInvites = list.ToArray();
			}
		}
	}
}
