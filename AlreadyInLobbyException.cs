namespace Axlebolt.Bolt.Matchmaking
{
	public class AlreadyInLobbyException : BoltException
	{
		public AlreadyInLobbyException()
			: base("Unsupported operation. Leave lobby first!")
		{
		}
	}
}
