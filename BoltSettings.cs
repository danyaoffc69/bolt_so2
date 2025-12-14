using UnityEngine;

namespace Axlebolt.Bolt.Unity
{
	[CreateAssetMenu(fileName = "BoltSettings", menuName = "Bolt/BoltOptions", order = 0)]
	public class BoltSettings : ScriptableObject
	{
		private static BoltSettings _instance;

		[SerializeField]
		private string _gameId;

		[SerializeField]
		private string _gameCode;

		public static BoltSettings Instance => _instance ?? (_instance = Resources.Load<BoltSettings>("BoltSettings"));

		public string GameId => _gameId;

		public string GameCode => _gameCode;
	}
}
