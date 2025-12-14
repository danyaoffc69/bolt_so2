using System.Threading.Tasks;
using Axlebolt.Bolt.Api;
using Axlebolt.Bolt.Protobuf;

namespace Axlebolt.Bolt.Player
{
	public class BoltGameServerPlayerService : BoltService<BoltGameServerPlayerService>
	{
		private readonly GameServerPlayerRemoteService _gameServerPlayerRemoteService;

		public BoltGameServerPlayerService()
		{
			_gameServerPlayerRemoteService = new GameServerPlayerRemoteService(BoltGameServerApi.Instance.ClientService);
		}

		internal override void Load()
		{
		}

		public Task SetPhotonGameAsync(string playerId, BoltPhotonGame boltPhotonGame)
		{
			return BoltGameServerApi.Instance.Async(delegate
			{
				SetPhotonGame(playerId, boltPhotonGame);
			});
		}

		public void SetPhotonGame(string playerId, BoltPhotonGame boltPhotonGame)
		{
			if (boltPhotonGame == null)
			{
				_gameServerPlayerRemoteService.SetPhotonGame(playerId, null);
				return;
			}
			PhotonGame photonGame = new PhotonGame
			{
				Region = boltPhotonGame.Region,
				RoomId = boltPhotonGame.RoomId,
				AppVersion = boltPhotonGame.AppVersion
			};
			foreach (string key in boltPhotonGame.CustomProperties.Keys)
			{
				photonGame.CustomProperties.Add(key, boltPhotonGame.CustomProperties[key]);
			}
			_gameServerPlayerRemoteService.SetPhotonGame(playerId, photonGame);
		}
	}
}
