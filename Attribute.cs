using Axlebolt.Bolt.Avatar;
using Google.Protobuf.WellKnownTypes;
using System;

namespace Axlebolt.Bolt.Player
{
	public class Attribute
	{
		public string stringValue { get; internal set; }

		public int intValue { get; internal set; }

		public float floatValue { get; internal set; }

		public bool booleanValue { get; internal set; }

        public BoltAvatar avatarValue { get; internal set; }

        public Attribute(string value)
		{
			stringValue = value;
		}

		public Attribute(int value)
		{
			intValue = value;
		}

		public Attribute(float value)
		{
			floatValue = value;
		}

		public Attribute(bool value)
		{
			booleanValue = value;
		}

        public Attribute(BoltAvatar avatar)
        {
            avatarValue = avatar;
        }

        public bool IsEmpty()
        {
            return stringValue != null && stringValue.Length == 0;
        }
    }
}
