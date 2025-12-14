using System.Collections.Generic;
using Axlebolt.Bolt.Api;
using Axlebolt.Bolt.Protobuf;

namespace Axlebolt.Bolt.Settings
{
	public class BoltGameServerSettingsService : BoltService<BoltGameServerSettingsService>
	{
		private readonly GameSettingsRemoteService _gameSettingsRemoteService;

		private readonly Dictionary<string, GameSetting> _settings;

		public BoltGameServerSettingsService()
		{
			_settings = new Dictionary<string, GameSetting>();
			_gameSettingsRemoteService = new GameSettingsRemoteService(BoltGameServerApi.Instance.ClientService);
		}

		internal override void Load()
		{
			_settings.Clear();
			GameSetting[] gameSettings = _gameSettingsRemoteService.GetGameSettings();
			GameSetting[] array = gameSettings;
			foreach (GameSetting gameSetting in array)
			{
				_settings.Add(gameSetting.Key, gameSetting);
			}
		}

		internal override void Unload()
		{
		}

		private bool ContainsSetting(string setting)
		{
			return _settings.ContainsKey(setting);
		}

		public Dictionary<string, GameSetting> GetSettings()
		{
			return _settings;
		}

		public string GetStringSetting(string setting)
		{
			return ContainsSetting(setting) ? _settings[setting].Value : null;
		}

		public int GetIntSetting(string setting, int defaultValue = 0)
		{
			return ContainsSetting(setting) ? _settings[setting].IntValue : defaultValue;
		}

		public float GetFloatSetting(string setting, float defaultValue = 0f)
		{
			return ContainsSetting(setting) ? _settings[setting].FloatValue : defaultValue;
		}

		public bool GetBoolSetting(string setting, bool defaultValue = false)
		{
			return ContainsSetting(setting) ? _settings[setting].BoolValue : defaultValue;
		}
	}
}
