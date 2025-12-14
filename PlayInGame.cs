using System;

namespace Axlebolt.Bolt.Player
{
	public class PlayInGame
	{
		public string GameCode { get; }

		public string GameVersion { get; }

		public string LobbyId { get; }

		public BoltPhotonGame PhotonGame { get; }

		public PlayInGame(string gameCode, string gameVersion, string lobbyId, BoltPhotonGame boltPhotonGame)
		{
			if (gameCode == null)
			{
				throw new ArgumentNullException("gameCode");
			}
			if (gameVersion == null)
			{
				throw new ArgumentNullException("gameVersion");
			}
			GameCode = gameCode;
			GameVersion = gameVersion;
			LobbyId = lobbyId;
			PhotonGame = boltPhotonGame;
		}
	}
}
