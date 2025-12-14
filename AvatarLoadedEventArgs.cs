using Axlebolt.Bolt.Avatar;
using System;
using System.Collections.Generic;

namespace Axlebolt.Bolt.Player.Events
{
	public class AvatarLoadedEventArgs
	{
		public HashSet<BoltAvatar> Avatars { get; }

		public Exception Exception { get; }

		public bool Success => Exception == null;

		public AvatarLoadedEventArgs(HashSet<BoltAvatar> avatar, Exception ex = null)
		{
            Avatars = avatar;
			Exception = ex;
		}
	}
}
