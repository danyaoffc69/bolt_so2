using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Axlebolt.Bolt.Api;
using Axlebolt.Bolt.Avatar;
using Axlebolt.Bolt.Protobuf;

namespace Axlebolt.Bolt.Player
{
	public class BoltPlayerService : BoltService<BoltPlayerService>
	{
		private readonly PlayerRemoteService _playerRemoteService;

		public BoltPlayer Player { get; private set; }

		internal BoltPlayerService()
		{
			_playerRemoteService = new PlayerRemoteService(BoltPlayerApi.Instance.ClientService);
		}

		public Task SetName(string name)
		{
			return BoltPlayerApi.Instance.Async(delegate
			{
				_playerRemoteService.SetPlayerName(name);
				Player.Name = name;
			});
		}

		public Task SetAvatar(byte[] avatar)
		{
			if (avatar == null)
			{
				throw new ArgumentNullException("avatar");
			}
			return BoltPlayerApi.Instance.Async(delegate
			{
				string avatarId = _playerRemoteService.SetPlayerAvatar(avatar);
				//Player.AvatarId = avatarId;
				//Player.Avatar.Data = avatar;

                Player.Avatar = BoltAvatar.Create(avatarId, avatar);

                BoltService<BoltAvatarService>.Instance.SaveToCache(avatarId, avatar);
			});
		}

		public Task SetFirebaseToken(string token)
		{
			if (token == null)
			{
				throw new ArgumentNullException("token");
			}
			return BoltPlayerApi.Instance.Async(delegate
			{
				_playerRemoteService.SetPlayerFirebaseToken(token);
			});
		}

		public Task SetOnlineStatus()
		{
			return BoltPlayerApi.Instance.Async(SetOnlineStatusInternal);
		}

		public Task SetAwayStatus()
		{
			return BoltPlayerApi.Instance.Async(SetAwayStatusInternal);
		}

		public Task BanMe(int banCode, string reason)
		{
			return BoltPlayerApi.Instance.Async(delegate
			{
				_playerRemoteService.BanMe(banCode, reason);
			});
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void SetAwayStatusInternal()
		{
			_playerRemoteService.SetAwayStatus();
			Player.PlayerStatus.OnlineStatus = OnlineStatus.StateAway;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void SetOnlineStatusInternal()
		{
			_playerRemoteService.SetOnlineStatus();
			Player.PlayerStatus.OnlineStatus = OnlineStatus.StateOnline;
		}

		internal override void Load()
		{
			Axlebolt.Bolt.Protobuf.Player currentPlayer = _playerRemoteService.GetCurrentPlayer();
			Player = PlayerMapper.Instance.ToOriginal(currentPlayer);
			BoltService<BoltAvatarService>.Instance.LoadAvatar(Player.Avatar, BoltCacheMode.Use);
		}
	}
}
