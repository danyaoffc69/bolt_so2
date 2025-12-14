using System;

namespace Axlebolt.Bolt
{
	public class BoltException : Exception
	{
		public BoltException(string message)
			: base(message)
		{
		}

		public BoltException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
