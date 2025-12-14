using System;
using System.Threading.Tasks;
using Axlebolt.Bolt.Api;
using Axlebolt.Bolt.Protobuf;
using Axlebolt.Bolt.Stats.Mappers;
using Axlebolt.RpcSupport;

namespace Axlebolt.Bolt.Stats
{
	public class BoltGameServerStatsService : BoltService<BoltGameServerStatsService>
	{
		private readonly GameServerStatsRemoteService _statsRemoteService;

		public BoltGameServerStatsService()
		{
			_statsRemoteService = new GameServerStatsRemoteService(BoltGameServerApi.Instance.ClientService);
		}

		public Task<BoltGameServerPlayerStat[]> GetPlayerStats(string playerId, int timeoutMillis, string[] apiNames)
		{
			return BoltGameServerApi.Instance.Async(delegate
			{
				DateTime now = DateTime.Now;
				while (DateTime.Now.Millisecond - now.Millisecond < timeoutMillis)
				{
					try
					{
						string[] playerIds = new string[1] { playerId };
						PlayerStats[] playersStats = _statsRemoteService.GetPlayersStats(playerIds, apiNames);
						return BoltGameServerPlayerStatsMapper.Instance.ToOriginalArray(playersStats)[0].Stats;
					}
					catch (ConnectionFailedException)
					{
					}
				}
				throw new ConnectionFailedException();
			});
		}

		public Task<BoltGameServerPlayerStats[]> GetPlayersStats(string[] playerIds, int timeoutMillis, string[] apiNames)
		{
			return BoltGameServerApi.Instance.Async(delegate
			{
				DateTime now = DateTime.Now;
				while (DateTime.Now.Millisecond - now.Millisecond < timeoutMillis)
				{
					try
					{
						PlayerStats[] playersStats = _statsRemoteService.GetPlayersStats(playerIds, apiNames);
						return BoltGameServerPlayerStatsMapper.Instance.ToOriginalArray(playersStats);
					}
					catch (ConnectionFailedException)
					{
					}
				}
				throw new ConnectionFailedException();
			});
		}

		public Task StorePlayersStats(BoltGameServerPlayerStats[] playersStats, int timeoutMillis)
		{
			return BoltGameServerApi.Instance.Async(delegate
			{
				DateTime now = DateTime.Now;
				while (DateTime.Now.Millisecond - now.Millisecond < timeoutMillis)
				{
					try
					{
						_statsRemoteService.StoreStats(BoltGameServerPlayerStatsMapper.Instance.ToProtoArray(playersStats));
						return;
					}
					catch (ConnectionFailedException)
					{
					}
				}
				throw new ConnectionFailedException();
			});
		}

		public Task StorePlayerStats(BoltGameServerPlayerStats playerStats, int timeoutMillis)
		{
			return BoltGameServerApi.Instance.Async(delegate
			{
				DateTime now = DateTime.Now;
				while (DateTime.Now.Millisecond - now.Millisecond < timeoutMillis)
				{
					try
					{
						BoltGameServerPlayerStats[] beans = new BoltGameServerPlayerStats[1] { playerStats };
						_statsRemoteService.StoreStats(BoltGameServerPlayerStatsMapper.Instance.ToProtoArray(beans));
						return;
					}
					catch (ConnectionFailedException)
					{
					}
				}
				throw new ConnectionFailedException();
			});
		}

		internal override void Load()
		{
		}
	}
}
