using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axlebolt.Bolt.Api;
using Axlebolt.Bolt.Friends;
using Axlebolt.Bolt.Friends.Mappers;
using Axlebolt.Bolt.Matchmaking.Events;
using Axlebolt.Bolt.Matchmaking.Mappers;
using Axlebolt.Bolt.Player;
using Axlebolt.Bolt.Protobuf;

namespace Axlebolt.Bolt.Matchmaking
{
    public class BoltMatchmakingService : BoltService<BoltMatchmakingService>
    {
        public class MatchmakingEventListener : IMatchmakingRemoteEventListener
        {
            private readonly BoltMatchmakingService _boltMatchmaking;

            public MatchmakingEventListener(BoltMatchmakingService boltMatchmaking)
            {
                _boltMatchmaking = boltMatchmaking;
            }

            public void OnNewPlayerJoinedLobby(PlayerFriend protoNewPlayer)
            {
                NewPlayerJoinedEventArgs arg;
                lock (Lock)
                {
                    BoltFriend player = FriendMapper.Instance.ToOriginal(protoNewPlayer);
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesOutcoming);
                    if (list.RemoveAll((BoltLobbyInvite item) => item.Friend.Id == player.Id) > 0)
                    {
                        _boltMatchmaking.LobbyInvitesOutcoming = list;
                        _boltMatchmaking.OutcomingInvitesChangedEvent.Invoke();
                    }
                    _boltMatchmaking.CurrentLobby.AddLobbyMember(player);
                    _boltMatchmaking.CurrentLobby.RemoveLobbyMemberInvite(player);
                    arg = new NewPlayerJoinedEventArgs(player);
                }
                _boltMatchmaking.NewPlayerJoinedEvent.Invoke(arg);
            }

            public void OnNewSpectatorJoinedLobby(PlayerFriend protoNewSpectator)
            {
                NewSpectatorJoinedEventArgs arg;
                lock (Lock)
                {
                    BoltFriend spectator = FriendMapper.Instance.ToOriginal(protoNewSpectator);
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesOutcoming);
                    if (list.RemoveAll((BoltLobbyInvite item) => item.Friend.Id == spectator.Id) > 0)
                    {
                        _boltMatchmaking.LobbyInvitesOutcoming = list;
                        _boltMatchmaking.OutcomingInvitesChangedEvent.Invoke();
                    }
                    _boltMatchmaking.CurrentLobby.AddLobbySpectator(spectator);
                    _boltMatchmaking.CurrentLobby.RemoveLobbySpectatorInvite(spectator);
                    arg = new NewSpectatorJoinedEventArgs(spectator);
                }
                _boltMatchmaking.NewSpectatorJoinedEvent.Invoke(arg);
            }

            public void OnLobbyPlayerTypeChanged(string playerId, Axlebolt.Bolt.Protobuf.LobbyPlayerType protoPlayerType)
            {
                LobbyPlayerTypeChangedEventArgs arg;
                lock (Lock)
                {
                    LobbyPlayerType lobbyPlayerType = EnumMapper<Axlebolt.Bolt.Protobuf.LobbyPlayerType, LobbyPlayerType>.ToOriginal(protoPlayerType);
                    BoltFriend boltFriend = null;
                    if (_boltMatchmaking.CurrentLobby.IsLobbyMember(playerId))
                    {
                        boltFriend = _boltMatchmaking.CurrentLobby.GetLobbyMember(playerId);
                    }
                    else if (_boltMatchmaking.CurrentLobby.IsLobbySpectator(playerId))
                    {
                        boltFriend = _boltMatchmaking.CurrentLobby.GetLobbySpectator(playerId);
                    }
                    if (boltFriend != null)
                    {
                        switch (lobbyPlayerType)
                        {
                            case LobbyPlayerType.Member:
                                _boltMatchmaking.CurrentLobby.AddLobbyMember(boltFriend);
                                _boltMatchmaking.CurrentLobby.RemoveLobbySpectator(boltFriend);
                                break;
                            case LobbyPlayerType.Spectator:
                                _boltMatchmaking.CurrentLobby.AddLobbySpectator(boltFriend);
                                _boltMatchmaking.CurrentLobby.RemoveLobbyMember(boltFriend);
                                break;
                        }
                    }
                    arg = new LobbyPlayerTypeChangedEventArgs(playerId, lobbyPlayerType);
                }
                _boltMatchmaking.LobbyPlayerTypeChangedEvent.Invoke(arg);
            }

            public void OnNewPlayerInvitedToLobby(string inviteSenderId, PlayerFriend protoNewPlayer)
            {
                NewPlayerInvitedEventArgs arg;
                lock (Lock)
                {
                    BoltFriend lobbyMember = _boltMatchmaking.CurrentLobby.GetLobbyMember(inviteSenderId);
                    BoltFriend boltFriend = FriendMapper.Instance.ToOriginal(protoNewPlayer);
                    string newPlayerId = protoNewPlayer.Player.Id;
                    if (lobbyMember.IsLocal() && !_boltMatchmaking.LobbyInvitesOutcoming.Exists((BoltLobbyInvite item) => item.Friend.Id == newPlayerId && item.PlayerType == LobbyPlayerType.Member))
                    {
                        BoltLobbyInvite item2 = new BoltLobbyInvite(_boltMatchmaking.CurrentLobby.Id, boltFriend, LobbyPlayerType.Member, GetCurrentMillis());
                        _boltMatchmaking.LobbyInvitesOutcoming = _boltMatchmaking.LobbyInvitesOutcoming.Where((BoltLobbyInvite item) => item.Friend.Id != newPlayerId).ToList();
                        List<BoltLobbyInvite> lobbyInvitesOutcoming = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesOutcoming) { item2 };
                        _boltMatchmaking.LobbyInvitesOutcoming = lobbyInvitesOutcoming;
                        _boltMatchmaking.OutcomingInvitesChangedEvent.Invoke();
                    }
                    _boltMatchmaking.CurrentLobby.RemoveLobbySpectatorInvite(boltFriend);
                    _boltMatchmaking.CurrentLobby.AddLobbyMemberInvite(boltFriend);
                    arg = new NewPlayerInvitedEventArgs(lobbyMember, boltFriend);
                }
                _boltMatchmaking.NewPlayerInvitedEvent.Invoke(arg);
            }

            public void OnNewSpectatorInvitedToLobby(string inviteSenderId, PlayerFriend protoNewPlayer)
            {
                NewSpectatorInvitedEventArgs arg;
                lock (Lock)
                {
                    BoltFriend lobbyMember = _boltMatchmaking.CurrentLobby.GetLobbyMember(inviteSenderId);
                    BoltFriend boltFriend = FriendMapper.Instance.ToOriginal(protoNewPlayer);
                    string newPlayerId = protoNewPlayer.Player.Id;
                    if (lobbyMember.IsLocal() && !_boltMatchmaking.LobbyInvitesOutcoming.Exists((BoltLobbyInvite item) => item.Friend.Id == newPlayerId && item.PlayerType == LobbyPlayerType.Spectator))
                    {
                        BoltLobbyInvite item2 = new BoltLobbyInvite(_boltMatchmaking.CurrentLobby.Id, boltFriend, LobbyPlayerType.Spectator, GetCurrentMillis());
                        _boltMatchmaking.LobbyInvitesOutcoming = _boltMatchmaking.LobbyInvitesOutcoming.Where((BoltLobbyInvite item) => item.Friend.Id != newPlayerId).ToList();
                        List<BoltLobbyInvite> lobbyInvitesOutcoming = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesOutcoming) { item2 };
                        _boltMatchmaking.LobbyInvitesOutcoming = lobbyInvitesOutcoming;
                        _boltMatchmaking.OutcomingInvitesChangedEvent.Invoke();
                    }
                    _boltMatchmaking.CurrentLobby.RemoveLobbyMemberInvite(boltFriend);
                    _boltMatchmaking.CurrentLobby.AddLobbySpectatorInvite(boltFriend);
                    arg = new NewSpectatorInvitedEventArgs(lobbyMember, boltFriend);
                }
                _boltMatchmaking.NewSpectatorInvitedEvent.Invoke(arg);
            }

            public void OnReceivedInviteToLobby(PlayerFriend inviteSender, string lobbyId)
            {
                ReceivedInviteEventArgs arg;
                lock (Lock)
                {
                    BoltLobbyInvite lobbyInvite = new BoltLobbyInvite(lobbyId, FriendMapper.Instance.ToOriginal(inviteSender), LobbyPlayerType.Member, GetCurrentMillis());
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesIncoming);
                    list.RemoveAll((BoltLobbyInvite item) => item.LobbyId == lobbyInvite.LobbyId);
                    list.Add(lobbyInvite);
                    _boltMatchmaking.LobbyInvitesIncoming = list;
                    arg = new ReceivedInviteEventArgs(lobbyInvite);
                }
                _boltMatchmaking.IncomingInvitesChangedEvent.Invoke();
                _boltMatchmaking.ReceivedInviteEvent.Invoke(arg);
            }

            public void OnReceivedSpectatorInviteToLobby(PlayerFriend inviteSender, string lobbyId)
            {
                ReceivedInviteEventArgs arg;
                lock (Lock)
                {
                    BoltLobbyInvite lobbyInvite = new BoltLobbyInvite(lobbyId, FriendMapper.Instance.ToOriginal(inviteSender), LobbyPlayerType.Spectator, GetCurrentMillis());
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesIncoming);
                    list.RemoveAll((BoltLobbyInvite item) => item.LobbyId == lobbyInvite.LobbyId);
                    list.Add(lobbyInvite);
                    _boltMatchmaking.LobbyInvitesIncoming = list;
                    arg = new ReceivedInviteEventArgs(lobbyInvite);
                }
                _boltMatchmaking.IncomingInvitesChangedEvent.Invoke();
                _boltMatchmaking.ReceivedInviteEvent.Invoke(arg);
            }

            public void OnPlayerLeftLobby(string leftPlayerId)
            {
                PlayerLeftEventArgs arg;
                lock (Lock)
                {
                    BoltFriend lobbyAny = _boltMatchmaking.CurrentLobby.GetLobbyAny(leftPlayerId);
                    _boltMatchmaking.CurrentLobby.RemoveLobbyAny(lobbyAny);
                    if (leftPlayerId == BoltService<BoltPlayerService>.Instance.Player.Id)
                    {
                        _boltMatchmaking._currentLobby = null;
                    }
                    arg = new PlayerLeftEventArgs(lobbyAny);
                }
                _boltMatchmaking.PlayerLeftEvent.Invoke(arg);
            }

            public void OnPlayerKickedFromLobby(string kickInitiatorId, string playerId)
            {
                PlayerKickedEventArgs arg;
                lock (Lock)
                {
                    BoltFriend lobbyAny = _boltMatchmaking.CurrentLobby.GetLobbyAny(kickInitiatorId);
                    BoltFriend lobbyAny2 = _boltMatchmaking.CurrentLobby.GetLobbyAny(playerId);
                    _boltMatchmaking.CurrentLobby.RemoveLobbyAny(lobbyAny2);
                    if (lobbyAny2.IsLocal())
                    {
                        _boltMatchmaking._currentLobby = null;
                        _boltMatchmaking.LobbyInvitesOutcoming = new List<BoltLobbyInvite>();
                    }
                    arg = new PlayerKickedEventArgs(lobbyAny, lobbyAny2);
                }
                _boltMatchmaking.PlayerKickedEvent.Invoke(arg);
            }

            public void OnRefuseInviteToLobby(string inviteSenderId, string invitedPlayerId)
            {
                PlayerRefusedEventArgs arg;
                lock (Lock)
                {
                    BoltFriend lobbyAny = _boltMatchmaking.CurrentLobby.GetLobbyAny(inviteSenderId);
                    BoltFriend lobbyAnyInvite = _boltMatchmaking.CurrentLobby.GetLobbyAnyInvite(invitedPlayerId);
                    _boltMatchmaking.CurrentLobby.RemoveLobbyAnyInvite(lobbyAnyInvite);
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesOutcoming);
                    list.RemoveAll((BoltLobbyInvite inv) => inv.Friend.Id == invitedPlayerId);
                    _boltMatchmaking.LobbyInvitesOutcoming = list;
                    arg = new PlayerRefusedEventArgs(lobbyAny, lobbyAnyInvite);
                }
                _boltMatchmaking.OutcomingInvitesChangedEvent.Invoke();
                _boltMatchmaking.PlayerRefusedEvent.Invoke(arg);
            }

            public void OnRevokeInviteToLobby(string inviteSenderId, string invitedPlayerId)
            {
                lock (Lock)
                {
                    if (BoltService<BoltPlayerService>.Instance.Player.Id == invitedPlayerId)
                    {
                        List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(_boltMatchmaking.LobbyInvitesIncoming);
                        list.RemoveAll((BoltLobbyInvite item) => item.Friend.Id == inviteSenderId);
                        _boltMatchmaking.LobbyInvitesIncoming = list;
                        _boltMatchmaking.IncomingInvitesChangedEvent.Invoke();
                    }
                    else
                    {
                        BoltFriend lobbyAny = _boltMatchmaking.CurrentLobby.GetLobbyAny(inviteSenderId);
                        BoltFriend lobbyAnyInvite = _boltMatchmaking.CurrentLobby.GetLobbyAnyInvite(invitedPlayerId);
                        _boltMatchmaking.CurrentLobby.RemoveLobbyAnyInvite(lobbyAnyInvite);
                        PlayerRevokedEventArgs arg = new PlayerRevokedEventArgs(lobbyAny, lobbyAnyInvite);
                        _boltMatchmaking.PlayerRevokedEvent.Invoke(arg);
                    }
                }
            }

            public void OnLobbyOwnerChanged(string lobbyOwnerId)
            {
                LobbyOwnerChangedEventArgs arg;
                lock (Lock)
                {
                    _boltMatchmaking.CurrentLobby.LobbyOwnerId = lobbyOwnerId;
                    BoltFriend lobbyAny = _boltMatchmaking.CurrentLobby.GetLobbyAny(lobbyOwnerId);
                    arg = new LobbyOwnerChangedEventArgs(lobbyAny);
                }
                _boltMatchmaking.LobbyOwnerChangedEvent.Invoke(arg);
            }

            public void OnLobbyNameChanged(string name)
            {
                lock (Lock)
                {
                    _boltMatchmaking.CurrentLobby.Name = name;
                    LobbyNameChangedEventArgs arg = new LobbyNameChangedEventArgs(name);
                    _boltMatchmaking.LobbyNameChangedEvent.Invoke(arg);
                }
            }

            public void OnLobbyTypeChanged(Axlebolt.Bolt.Protobuf.LobbyType protoLobbyType)
            {
                LobbyTypeChangedEventArgs arg;
                lock (Lock)
                {
                    BoltLobby.LobbyType lobbyType = EnumMapper<Axlebolt.Bolt.Protobuf.LobbyType, BoltLobby.LobbyType>.ToOriginal(protoLobbyType);
                    _boltMatchmaking.CurrentLobby.Type = lobbyType;
                    arg = new LobbyTypeChangedEventArgs(lobbyType);
                }
                _boltMatchmaking.LobbyTypeChangedEvent.Invoke(arg);
            }

            public void OnLobbyMaxMembersChanged(byte maxMembers)
            {
                LobbyMaxMembersChangedEventArgs arg;
                lock (Lock)
                {
                    _boltMatchmaking.CurrentLobby.MaxMembers = maxMembers;
                    arg = new LobbyMaxMembersChangedEventArgs(maxMembers);
                }
                _boltMatchmaking.LobbyMaxMembersChangedEvent.Invoke(arg);
            }

            public void OnLobbyMaxSpectatorsChanged(byte maxSpectators)
            {
                LobbyMaxSpectatorsChangedEventArgs arg;
                lock (Lock)
                {
                    _boltMatchmaking.CurrentLobby.MaxSpectators = maxSpectators;
                    arg = new LobbyMaxSpectatorsChangedEventArgs(maxSpectators);
                }
                _boltMatchmaking.LobbyMaxSpectatorsChangedEvent.Invoke(arg);
            }

            public void OnLobbyJoinableChanged(bool joinable)
            {
                LobbyJoinableChangedEventArgs arg;
                lock (Lock)
                {
                    _boltMatchmaking.CurrentLobby.Joinable = joinable;
                    arg = new LobbyJoinableChangedEventArgs(joinable);
                }
                _boltMatchmaking.LobbyJoinableChangedEvent.Invoke(arg);
            }

            public void OnLobbyDataChanged(Dictionary data)
            {
                LobbyDataChangedEventArgs arg;
                lock (Lock)
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(_boltMatchmaking.CurrentLobby.Data);
                    foreach (KeyValuePair<string, string> item in data.Content)
                    {
                        if (string.IsNullOrEmpty(item.Value))
                        {
                            dictionary.Remove(item.Key);
                        }
                        else
                        {
                            dictionary[item.Key] = item.Value;
                        }
                    }
                    _boltMatchmaking.CurrentLobby.Data = dictionary;
                    arg = new LobbyDataChangedEventArgs(new Dictionary<string, string>(data.Content));
                }
                _boltMatchmaking.LobbyDataChangedEvent.Invoke(arg);
            }

            public void OnLobbyGameServerChanged(GameServer protoGameServer)
            {
                LobbyGameServerChangedEventArgs arg;
                lock (Lock)
                {
                    BoltGameServer gameServer = GameServerMapper.Instance.ToOriginal(protoGameServer);
                    _boltMatchmaking.CurrentLobby.GameServer = gameServer;
                    arg = new LobbyGameServerChangedEventArgs(gameServer);
                }
                _boltMatchmaking.LobbyGameServerChangedEvent.Invoke(arg);
            }

            public void OnLobbyPhotonGameChanged(PhotonGame protoPhotonGame)
            {
                LobbyPhotonGameChangedEventArgs arg;
                lock (Lock)
                {
                    BoltPhotonGame photonGame = ((protoPhotonGame != null) ? PhotonGameMapper.Instance.ToOriginal(protoPhotonGame) : null);
                    _boltMatchmaking.CurrentLobby.PhotonGame = photonGame;
                    arg = new LobbyPhotonGameChangedEventArgs(photonGame);
                }
                _boltMatchmaking.LobbyPhotonGameChangedEvent.Invoke(arg);
            }

            public void OnLobbyChatMessage(string playerId, string message)
            {
                LobbyChatMessageEventArgs arg;
                lock (Lock)
                {
                    long date = DateTime.Now.Ticks / 10000;
                    BoltLobbyMessage lobbyMessage = new BoltLobbyMessage(playerId, message, date);
                    arg = new LobbyChatMessageEventArgs(lobbyMessage);
                }
                _boltMatchmaking.LobbyChatMessageEvent.Invoke(arg);
            }

            private long GetCurrentMillis()
            {
                return (long)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            }

            private List<BoltLobbyInvite> AddInvite(List<BoltLobbyInvite> list, BoltLobbyInvite invite)
            {
                if (list.Exists((BoltLobbyInvite item) => item.Friend.Id == invite.Friend.Id))
                {
                    return list;
                }
                return new List<BoltLobbyInvite>(list) { invite };
            }

            private List<BoltLobbyInvite> RemoveInvite(List<BoltLobbyInvite> list, BoltLobbyInvite invite)
            {
                List<BoltLobbyInvite> list2 = new List<BoltLobbyInvite>(list);
                list2.Remove(invite);
                return list2;
            }
        }

        private readonly MatchmakingRemoteService _matchmakingRemoteService;

        private readonly MatchmakingEventListener _remoteEventListener;

        private static readonly object Lock = new object();

        public readonly BoltEvent<NewPlayerJoinedEventArgs> NewPlayerJoinedEvent = new BoltEvent<NewPlayerJoinedEventArgs>();

        public readonly BoltEvent<NewSpectatorJoinedEventArgs> NewSpectatorJoinedEvent = new BoltEvent<NewSpectatorJoinedEventArgs>();

        public readonly BoltEvent<LobbyPlayerTypeChangedEventArgs> LobbyPlayerTypeChangedEvent = new BoltEvent<LobbyPlayerTypeChangedEventArgs>();

        public readonly BoltEvent<NewPlayerInvitedEventArgs> NewPlayerInvitedEvent = new BoltEvent<NewPlayerInvitedEventArgs>();

        public readonly BoltEvent<NewSpectatorInvitedEventArgs> NewSpectatorInvitedEvent = new BoltEvent<NewSpectatorInvitedEventArgs>();

        public readonly BoltEvent<PlayerLeftEventArgs> PlayerLeftEvent = new BoltEvent<PlayerLeftEventArgs>();

        public readonly BoltEvent<PlayerKickedEventArgs> PlayerKickedEvent = new BoltEvent<PlayerKickedEventArgs>();

        public readonly BoltEvent<PlayerRefusedEventArgs> PlayerRefusedEvent = new BoltEvent<PlayerRefusedEventArgs>();

        public readonly BoltEvent<PlayerRevokedEventArgs> PlayerRevokedEvent = new BoltEvent<PlayerRevokedEventArgs>();

        public readonly BoltEvent<LobbyJoinedEventArgs> LobbyJoinedEvent = new BoltEvent<LobbyJoinedEventArgs>();

        public readonly BoltEvent<LobbyLeftEventArgs> LobbyLeftEvent = new BoltEvent<LobbyLeftEventArgs>();

        public readonly BoltEvent IncomingInvitesChangedEvent = new BoltEvent();

        public readonly BoltEvent OutcomingInvitesChangedEvent = new BoltEvent();

        public readonly BoltEvent<ReceivedInviteEventArgs> ReceivedInviteEvent = new BoltEvent<ReceivedInviteEventArgs>();

        public readonly BoltEvent<LobbyNameChangedEventArgs> LobbyNameChangedEvent = new BoltEvent<LobbyNameChangedEventArgs>();

        public readonly BoltEvent<LobbyJoinableChangedEventArgs> LobbyJoinableChangedEvent = new BoltEvent<LobbyJoinableChangedEventArgs>();

        public readonly BoltEvent<LobbyTypeChangedEventArgs> LobbyTypeChangedEvent = new BoltEvent<LobbyTypeChangedEventArgs>();

        public readonly BoltEvent<LobbyOwnerChangedEventArgs> LobbyOwnerChangedEvent = new BoltEvent<LobbyOwnerChangedEventArgs>();

        public readonly BoltEvent<LobbyMaxMembersChangedEventArgs> LobbyMaxMembersChangedEvent = new BoltEvent<LobbyMaxMembersChangedEventArgs>();

        public readonly BoltEvent<LobbyMaxSpectatorsChangedEventArgs> LobbyMaxSpectatorsChangedEvent = new BoltEvent<LobbyMaxSpectatorsChangedEventArgs>();

        public readonly BoltEvent<LobbyDataChangedEventArgs> LobbyDataChangedEvent = new BoltEvent<LobbyDataChangedEventArgs>();

        public readonly BoltEvent<LobbyGameServerChangedEventArgs> LobbyGameServerChangedEvent = new BoltEvent<LobbyGameServerChangedEventArgs>();

        public readonly BoltEvent<LobbyPhotonGameChangedEventArgs> LobbyPhotonGameChangedEvent = new BoltEvent<LobbyPhotonGameChangedEventArgs>();

        public readonly BoltEvent<LobbyChatMessageEventArgs> LobbyChatMessageEvent = new BoltEvent<LobbyChatMessageEventArgs>();

        private BoltLobby _currentLobby;

        public BoltLobby CurrentLobby
        {
            get
            {
                if (_currentLobby == null)
                {
                    throw new OutOfLobbyException();
                }
                return _currentLobby;
            }
        }

        public List<BoltLobbyInvite> LobbyInvitesIncoming { get; internal set; }

        public List<BoltLobbyInvite> LobbyInvitesOutcoming { get; internal set; }

        public BoltMatchmakingService()
        {
            _matchmakingRemoteService = new MatchmakingRemoteService(BoltPlayerApi.Instance.ClientService);
            _remoteEventListener = new MatchmakingEventListener(this);
            LobbyInvitesIncoming = new List<BoltLobbyInvite>();
            LobbyInvitesOutcoming = new List<BoltLobbyInvite>();
        }

        internal override void BindEvents()
        {
            BoltPlayerApi.Instance.AddEventListener(_remoteEventListener);
        }

        internal override void UnbindEvents()
        {
            BoltPlayerApi.Instance.RemoveEventListener(_remoteEventListener);
        }

        internal override void Load()
        {
            LobbyInvite[] invitesToLobby = _matchmakingRemoteService.GetInvitesToLobby();
            LobbyInvitesIncoming = LobbyInviteMapper.Instance.ToOriginalList(invitesToLobby.ToList());
        }

        internal override void Unload()
        {
            _currentLobby = null;
            LobbyInvitesIncoming = new List<BoltLobbyInvite>();
            LobbyInvitesOutcoming = new List<BoltLobbyInvite>();
        }

        public Task<BoltLobby[]> RequestLobbyList(BoltFilter[] filters)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                Filter[] filters2 = FilterMapper.Instance.ToProtoList(filters).ToArray();
                Lobby[] source = _matchmakingRemoteService.RequestLobbyList(LobbyDistanceFilter.Default, filters2);
                return LobbyMapper.Instance.ToOriginalArray(source.ToList());
            });
        }

        public Task<BoltLobby> GetLobby(string lobbyId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                Lobby lobby = _matchmakingRemoteService.GetLobby(lobbyId);
                return LobbyMapper.Instance.ToOriginal(lobby);
            });
        }

        public Task<BoltLobby> CreateLobby(string name, BoltLobby.LobbyType type, int maxMembers, int maxSpectators)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (_currentLobby != null)
                {
                    throw new AlreadyInLobbyException();
                }
                LobbyJoinedEventArgs arg;
                lock (Lock)
                {
                    Axlebolt.Bolt.Protobuf.LobbyType lobbyType = EnumMapper<Axlebolt.Bolt.Protobuf.LobbyType, BoltLobby.LobbyType>.ToProto(type);
                    Lobby proto = _matchmakingRemoteService.CreateLobby(name, lobbyType, maxMembers, maxSpectators);
                    BoltLobby currentLobby = LobbyMapper.Instance.ToOriginal(proto);
                    _currentLobby = currentLobby;
                    arg = new LobbyJoinedEventArgs(CurrentLobby);
                }
                LobbyJoinedEvent.Invoke(arg);
                return _currentLobby;
            });
        }

        public Task<BoltLobby> JoinLobby(string lobbyId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (_currentLobby != null)
                {
                    throw new AlreadyInLobbyException();
                }
                LobbyJoinedEventArgs arg;
                lock (Lock)
                {
                    Lobby proto = _matchmakingRemoteService.JoinLobby(lobbyId);
                    BoltLobby currentLobby = LobbyMapper.Instance.ToOriginal(proto);
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(LobbyInvitesIncoming);
                    if (list.RemoveAll((BoltLobbyInvite item) => item.LobbyId == lobbyId) > 0)
                    {
                        LobbyInvitesIncoming = list;
                        IncomingInvitesChangedEvent.Invoke();
                    }
                    _currentLobby = currentLobby;
                    arg = new LobbyJoinedEventArgs(_currentLobby);
                }
                LobbyJoinedEvent.Invoke(arg);
                return _currentLobby;
            });
        }

        public Task LeaveLobby()
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                LobbyLeftEventArgs arg;
                lock (Lock)
                {
                    _matchmakingRemoteService.LeaveLobby();
                    arg = new LobbyLeftEventArgs(_currentLobby);
                    _currentLobby = null;
                }
                LobbyLeftEvent.Invoke(arg);
            });
        }

        public Task InviteMemberToLobby(string playerId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                _matchmakingRemoteService.InvitePlayerToLobby(playerId, EnumMapper<Axlebolt.Bolt.Protobuf.LobbyPlayerType, LobbyPlayerType>.ToProto(LobbyPlayerType.Member));
            });
        }

        public Task InviteSpectatorToLobby(string playerId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                _matchmakingRemoteService.InvitePlayerToLobby(playerId, EnumMapper<Axlebolt.Bolt.Protobuf.LobbyPlayerType, LobbyPlayerType>.ToProto(LobbyPlayerType.Spectator));
            });
        }

        public Task RevokeInvitationToLobby(string playerId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner() || LobbyInvitesOutcoming.Exists((BoltLobbyInvite item) => item.Friend.Id == playerId))
                {
                    _matchmakingRemoteService.RevokePlayerInvitationToLobby(playerId);
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(LobbyInvitesOutcoming);
                    list.RemoveAll((BoltLobbyInvite item) => item.Friend.Id == playerId);
                    LobbyInvitesOutcoming = list;
                    OutcomingInvitesChangedEvent.Invoke();
                }
            });
        }

        public Task RefuseInvitationToLobby(string lobbyId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (LobbyInvitesIncoming.Exists((BoltLobbyInvite item) => item.LobbyId == lobbyId))
                {
                    _matchmakingRemoteService.RefuseInvitationToLobby(lobbyId);
                    List<BoltLobbyInvite> list = new List<BoltLobbyInvite>(LobbyInvitesIncoming);
                    list.RemoveAll((BoltLobbyInvite item) => item.LobbyId == lobbyId);
                    LobbyInvitesIncoming = list;
                    IncomingInvitesChangedEvent.Invoke();
                }
            });
        }

        public Task KickFromLobby(string playerId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    _matchmakingRemoteService.KickPlayerFromLobby(playerId);
                }
            });
        }

        public Task SetLobbyName(string name)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    _matchmakingRemoteService.SetLobbyName(name);
                }
            });
        }

        public Task SetLobbyType(BoltLobby.LobbyType type)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    _matchmakingRemoteService.SetLobbyType(EnumMapper<Axlebolt.Bolt.Protobuf.LobbyType, BoltLobby.LobbyType>.ToProto(type));
                }
            });
        }

        public Task SetLobbyType(BoltLobby.LobbyType type, int maxMembers, int maxSpectators, List<string> kickPlayerIds, BoltFriend[] kickSpectators)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    string ownerId = CurrentLobby.LobbyOwner.Id;

                    _matchmakingRemoteService.SetLobbyType(EnumMapper<Axlebolt.Bolt.Protobuf.LobbyType, BoltLobby.LobbyType>.ToProto(type));
                    _matchmakingRemoteService.SetLobbyMaxMembers(maxMembers);
                    _matchmakingRemoteService.SetLobbyMaxSpectators(maxSpectators);

                    foreach (string kickPlayerId in kickPlayerIds)
                    {
                        _matchmakingRemoteService.KickPlayerFromLobby(kickPlayerId);
                    }

                    foreach (BoltFriend kickSpectator in kickSpectators)
                    {
                        _matchmakingRemoteService.KickPlayerFromLobby(kickSpectator.Id);
                    }
                }
            });
        }

        public Task SetLobbyOwner(string playerId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    _matchmakingRemoteService.SetLobbyOwner(playerId);
                }
            });
        }

        public Task SetLobbyMaxMembers(int maxMembers)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    _matchmakingRemoteService.SetLobbyMaxMembers(maxMembers);
                }
            });
        }

        public Task SetLobbyMaxSpectators(int maxSpectators)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    _matchmakingRemoteService.SetLobbyMaxSpectators(maxSpectators);
                }
            });
        }

        public Task SetLobbyJoinable(bool isJoinable)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                if (IsLobbyOwner())
                {
                    _matchmakingRemoteService.SetLobbyJoinable(isJoinable);
                }
            });
        }

        public Task SetLobbyData(Dictionary<string, string> data)
        {
            Dictionary dict = new Dictionary();
            dict.Content.Add(data);
            return BoltPlayerApi.Instance.Async(delegate
            {
                _matchmakingRemoteService.SetLobbyData(dict);
            });
        }

        public Task DeleteLobbyData(string[] key)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                _matchmakingRemoteService.DeleteLobbyData(key);
            });
        }

        public Task<BoltPlayer> GetLobbyOwner(string lobbyId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                Axlebolt.Bolt.Protobuf.Player lobbyOwner = _matchmakingRemoteService.GetLobbyOwner(lobbyId);
                return PlayerMapper.Instance.ToOriginal(lobbyOwner);
            });
        }

        public Task<BoltPlayer[]> GetLobbyMembers(string lobbyId)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                Axlebolt.Bolt.Protobuf.Player[] lobbyMembers = _matchmakingRemoteService.GetLobbyMembers(lobbyId);
                return PlayerMapper.Instance.ToOriginalArray(lobbyMembers.ToList());
            });
        }

        public Task SendLobbyChatMsg(string msg)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                _matchmakingRemoteService.SendLobbyChatMsg(msg);
            });
        }

        public bool IsLobbyOwner()
        {
            lock (Lock)
            {
                return IsInLobby() && CurrentLobby.LobbyOwner.IsLocal();
            }
        }

        public bool IsInLobby()
        {
            return _currentLobby != null;
        }

        public Task SetLobbyPhotonGame(BoltPhotonGame photonGame)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                PhotonGame photonGame2 = ((photonGame != null) ? PhotonGameMapper.Instance.ToProto(photonGame) : null);
                _matchmakingRemoteService.SetLobbyPhotonGame(photonGame2);
            });
        }

        public Task ChangeLobbyPlayerType(LobbyPlayerType playerType)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                Axlebolt.Bolt.Protobuf.LobbyPlayerType playerType2 = EnumMapper<Axlebolt.Bolt.Protobuf.LobbyPlayerType, LobbyPlayerType>.ToProto(playerType);
                _matchmakingRemoteService.ChangeLobbyPlayerType(playerType2);
            });
        }

        public Task ChangeLobbyPlayerType(string playerId, LobbyPlayerType playerType)
        {
            return BoltPlayerApi.Instance.Async(delegate
            {
                Axlebolt.Bolt.Protobuf.LobbyPlayerType playerType2 = EnumMapper<Axlebolt.Bolt.Protobuf.LobbyPlayerType, LobbyPlayerType>.ToProto(playerType);
                _matchmakingRemoteService.ChangeLobbyPlayerType(playerId, playerType2);
            });
        }
    }
}
