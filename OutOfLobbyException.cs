namespace Axlebolt.Bolt.Matchmaking
{
	public class OutOfLobbyException : BoltException
	{
		public OutOfLobbyException()
			: base("Unsupported operation. Create/join lobby first!")
		{
		}
	}
}
