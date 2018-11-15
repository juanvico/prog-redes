using System;
using System.Runtime.Serialization;

namespace GameServer
{
    [Serializable]
    public class NotExistingPlayer : Exception
    {
        override public string Message { get; }
        public NotExistingPlayer()
        {
            Message = "Inexistent player with specified nickname. Try again.";
        }
    }
}