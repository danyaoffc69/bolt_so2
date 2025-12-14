using System;

namespace Axlebolt.Bolt.Security.Axlebolt.Standoff.Common
{
    public class SafeInt
    {
        private readonly int _salt;

        private int _value;

        public int Value
        {
            get => _value - _salt;
            set => _value = value + _salt;
        }

        public SafeInt()
        {
            var random = new Random();
            _salt = random.Next();
            _value = _salt;
        }
    }
}
