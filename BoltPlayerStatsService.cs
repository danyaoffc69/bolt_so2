using System;
using System.Threading.Tasks;
using Axlebolt.Bolt.Api;
using Axlebolt.Bolt.Protobuf;

namespace Axlebolt.Bolt.Stats
{
	public class BoltPlayerStatsService : BoltService<BoltPlayerStatsService>
	{
		public class StatsRemoteEventListener : IPlayerStatsRemoteEventListener
		{
			private readonly BoltPlayerStatsService _service;

			public StatsRemoteEventListener(BoltPlayerStatsService service)
			{
				_service = service;
			}

			public void OnStatsUpdated(PlayerStat[] updatedStats)
			{
				foreach (PlayerStat playerStat in updatedStats)
				{
					switch (playerStat.Type)
					{
					case StatDefType.Avgrate:
						_service.SetFloatStatToCache(playerStat.Name, playerStat.FloatValue);
						break;
					case StatDefType.Float:
						_service.SetFloatStatToCache(playerStat.Name, playerStat.FloatValue);
						break;
					case StatDefType.Int:
						_service.SetIntStatToCache(playerStat.Name, playerStat.IntValue);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				_service.InvokeOnStatsUpdatedEvent();
			}
		}

		private readonly PlayerStatsRemoteService _statsRemoteService;

		private readonly StatsRemoteEventListener _statsRemoteEventListener;

		public readonly BoltEvent<OnStatsUpdatedEventArgs> OnStatsUpdatedEvent = new BoltEvent<OnStatsUpdatedEventArgs>();

		public int StatCount => Stats.Count;

		public BoltPlayerStats Stats { get; private set; }

		public BoltPlayerStatsService()
		{
			_statsRemoteService = new PlayerStatsRemoteService(BoltPlayerApi.Instance.ClientService);
			_statsRemoteEventListener = new StatsRemoteEventListener(this);
		}

		internal override void BindEvents()
		{
			BoltPlayerApi.Instance.AddEventListener(_statsRemoteEventListener);
		}

		internal override void UnbindEvents()
		{
			BoltPlayerApi.Instance.RemoveEventListener(_statsRemoteEventListener);
		}

        public async Task<BoltPlayerStats> GetPlayerStatsByIdAsync(string playerId)
        {
            Protobuf.Stats protoStats = await GetPlayerStats(playerId);
            return new BoltPlayerStats(protoStats);
        }

        public Task<Axlebolt.Bolt.Protobuf.Stats> GetPlayerStats(string playerId)
		{
			return BoltPlayerApi.Instance.Async(() => _statsRemoteService.GetPlayerStats(playerId));
		}

		public float GetFloatStat(string apiName)
		{
			return Stats.GetFloatStat(apiName);
		}

		public int GetIntStat(string apiName)
		{
			return Stats.GetIntStat(apiName);
		}

		public void SetFloatStat(string apiName, float value)
		{
			Stats.SetFloatStat(apiName, value);
		}

		public void SetIntStat(string apiName, int value)
		{
			Stats.SetIntStat(apiName, value);
		}

		public void SetIntStatToCache(string apiName, int value)
		{
			Stats.SetIntStatToCache(apiName, value);
		}

		public void SetFloatStatToCache(string apiName, float value)
		{
			Stats.SetFloatStatToCache(apiName, value);
		}

		public void IncrementStat(string apiName)
		{
			Stats.IncrementStat(apiName);
		}

		public void UpdateAvgRateStat(string apiName, float countThisSession, double sessionLength)
		{
			Stats.UpdateAvgRateStat(apiName, countThisSession, sessionLength);
		}

		public bool HasChanges()
		{
			return Stats.HasChanges();
		}

		public Task StoreStats()
		{
			return BoltPlayerApi.Instance.Async(delegate
			{
				_statsRemoteService.StoreStats(Stats.GetChanges(), new StoreAchievement[0]);
				Stats.ResetChanges();
			});
		}

		public Task ResetStats()
		{
			return BoltPlayerApi.Instance.Async(delegate
			{
				_statsRemoteService.ResetStats();
			});
		}

		public Task<int> GetNumberOfCurrentPlayers()
		{
			return null;
		}

		internal override void Load()
		{
			Axlebolt.Bolt.Protobuf.Stats stats = _statsRemoteService.GetStats();
			if (Stats == null)
			{
				Stats = new BoltPlayerStats(stats);
			}
			else
			{
				Stats.SetStats(stats);
			}
		}

		public void InvokeOnStatsUpdatedEvent()
		{
			OnStatsUpdatedEventArgs arg = new OnStatsUpdatedEventArgs();
			OnStatsUpdatedEvent.Invoke(arg);
		}
	}
}
