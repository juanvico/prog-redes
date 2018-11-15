using System;
using System.Runtime.Serialization;

namespace GameServer
{
    [Serializable]
    public class ExistsPlayerForMoveEx : Exception
    {
        override public string Message { get; }
        public ExistsPlayerForMoveEx()
        {
            Message = "Can't move. There's a player there. Try again.";
        }
    }
}