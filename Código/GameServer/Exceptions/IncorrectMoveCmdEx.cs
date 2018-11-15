using System;
using System.Runtime.Serialization;

namespace GameServer
{
    [Serializable]
    public class IncorrectMoveCmdEx : Exception
    {
        override public string Message { get; }
        public IncorrectMoveCmdEx()
        {
            Message = "move <*> <*> AND * = <U, D, L, R, UL, UR, DL, DR>";
        }
    }
}